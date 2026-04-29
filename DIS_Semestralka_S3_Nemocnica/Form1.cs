using Agents.AgentZdrojov;
using OSPAnimator;
using Simulation;
using System.Data;
using System.Windows.Forms.Integration;

namespace DIS_Semestralka_S3_Nemocnica
{
    public partial class Form1 : Form
    {
        private MySimulation? _sim;
        private Thread? _simThread;
        private readonly DataTable _pacientTable = new();
        private long _lastRefreshTicks;
        private readonly ManualResetEventSlim _pauseEvent = new(true);
        private readonly ElementHost _animHost = new() { Dock = DockStyle.Fill };

        public Form1()
        {
            InitializeComponent();
            InitPacientTable();
            pnlAnimator.Controls.Add(_animHost);
        }

        private void InitPacientTable()
        {
            _pacientTable.Columns.Add("ID", typeof(int));
            _pacientTable.Columns.Add("Príchod", typeof(string));
            _pacientTable.Columns.Add("Sanitka", typeof(string));
            _pacientTable.Columns.Add("Priorita", typeof(string));
            _pacientTable.Columns.Add("Stav", typeof(string));
            dgvPacienti.DataSource = _pacientTable;
            dgvPacienti.Columns["ID"]!.Width = 45;
            dgvPacienti.Columns["Príchod"]!.Width = 75;
            dgvPacienti.Columns["Sanitka"]!.Width = 60;
            dgvPacienti.Columns["Priorita"]!.Width = 65;
            dgvPacienti.Columns["Stav"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        // ── event handlers ────────────────────────────────────────────

        private void BtnSpustit_Click(object sender, EventArgs e)
        {
            _sim = new MySimulation();

            if (cbNahodny.Checked)
                _sim.NastavNahodny();
            else
                _sim.NastavSeed((int)nudSeed.Value);

            _sim.KonfSestry      = (int)nudSestry.Value;
            _sim.KonfLekari      = (int)nudLekari.Value;
            _sim.KonfMiestnostiA = (int)nudMiestnostiA.Value;
            _sim.KonfMiestnostiB = (int)nudMiestnostiB.Value;

            // Create and attach OSPAnimator
            var animator = new Animator(_sim);
            animator.SynchronizedTime = false;
            _animHost.Child = animator.MyCanvas;
            _sim.Animator = animator;

            _pauseEvent.Set();
            _lastRefreshTicks = 0;
            _sim.OnRefreshUI(s =>
            {
                _pauseEvent.Wait();
                long now = DateTime.UtcNow.Ticks;
                if (now - _lastRefreshTicks < 1_500_000) return;
                _lastRefreshTicks = now;
                try { Invoke(RefreshUI); } catch { }
            });
            _sim.OnReplicationDidFinish(s => BeginInvoke(AktualizujStatus));

            int reps      = (int)nudReplikacie.Value;
            double duration = (double)nudTrvanie.Value * 3600.0;

            btnSpustit.Enabled = false;
            btnZastavit.Enabled = true;
            btnPauza.Enabled = true;
            btnPauza.Text = "⏸  Pauza";
            btnPauza.BackColor = System.Drawing.Color.LightYellow;

            AplukujSpomalenie();

            _simThread = new Thread(() =>
            {
                try   { _sim.Simulate(reps, duration); }
                catch (ThreadInterruptedException) { }
                catch (Exception ex) { BeginInvoke(() => MessageBox.Show(ex.ToString(), "Chyba simulácie")); }
                finally { BeginInvoke(SimulaciaSkoncila); }
            }) { IsBackground = true };
            _simThread.Start();
        }

        private void BtnZastavit_Click(object sender, EventArgs e)
        {
            if (_sim == null) return;
            _sim.Zastavit = true;
            _pauseEvent.Set();
            _simThread?.Interrupt();
        }

        private void BtnPauza_Click(object sender, EventArgs e)
        {
            if (_pauseEvent.IsSet)
            {
                _pauseEvent.Reset();
                btnPauza.Text = "▶  Pokračovať";
                btnPauza.BackColor = System.Drawing.Color.LightBlue;
            }
            else
            {
                _pauseEvent.Set();
                btnPauza.Text = "⏸  Pauza";
                btnPauza.BackColor = System.Drawing.Color.LightYellow;
            }
        }

        private void CbNahodny_CheckedChanged(object sender, EventArgs e)
        {
            nudSeed.Enabled = !cbNahodny.Checked;
        }

        private void SliderChanged(object? sender, EventArgs e)
        {
            int ms  = trkDuration.Value;
            int sec = trkInterval.Value;

            lblDurationVal.Text = ms == 0 ? "0 ms  (max rýchlosť)" : $"{ms} ms";
            lblIntervalVal.Text = $"{sec} s";

            if (_sim != null && _sim.IsRunning())
                AplukujSpomalenie();
        }

        private void AplukujSpomalenie()
        {
            if (_sim == null) return;
            int ms  = trkDuration.Value;
            int sec = trkInterval.Value;
            _sim.GuiDurationMs = ms;
            _sim.GuiInterval   = sec;
            double dur = ms == 0 ? 0.001 : ms / 1000.0;
            _sim.SetSimSpeed(sec, dur);
        }

        private void SimulaciaSkoncila()
        {
            _pauseEvent.Set();
            btnSpustit.Enabled = true;
            btnZastavit.Enabled = false;
            btnPauza.Enabled = false;
            btnPauza.Text = "⏸  Pauza";
            btnPauza.BackColor = System.Drawing.Color.LightYellow;
            lblReplikacia.Text = $"Replikácia: {_sim?.CurrentReplication} / {_sim?.ReplicationCount}  (hotovo)";
            RefreshUI();
        }

        private void AktualizujStatus()
        {
            if (_sim == null) return;
            lblReplikacia.Text = $"Replikácia: {_sim.CurrentReplication} / {_sim.ReplicationCount}";
            lblSimCas.Text = $"Čas: {TimeSpan.FromSeconds(_sim.CurrentTime):hh\\:mm\\:ss}";
        }

        private void RefreshUI()
        {
            if (_sim == null) return;
            lblSimCas.Text = $"Čas: {TimeSpan.FromSeconds(_sim.CurrentTime):hh\\:mm\\:ss}";
            lblReplikacia.Text = $"Replikácia: {_sim.CurrentReplication} / {_sim.ReplicationCount}";
            AktualizujPacientiTab();
            AktualizujZdrojeTab();
            AktualizujRadyTab();
        }

        private void AktualizujPacientiTab()
        {
            if (_sim == null) return;
            var snapshot = _sim.Pacienti.Values.OrderBy(p => p.Id).ToList();
            _pacientTable.BeginLoadData();
            _pacientTable.Clear();
            foreach (var p in snapshot)
            {
                _pacientTable.Rows.Add(
                    p.Id,
                    TimeSpan.FromSeconds(p.CasPrichodu).ToString(@"hh\:mm\:ss"),
                    p.PrisielSanitkou ? "Áno" : "Nie",
                    p.Priorita > 0 ? p.Priorita.ToString() : "—",
                    p.Stav
                );
            }
            _pacientTable.EndLoadData();
            lblPocetPacientov.Text = $"Pacienti v systéme: {snapshot.Count}";
            lblPocetVybavenych.Text = $"Vybavených: {_sim!.PocetVybavenych}";
        }

        private void AktualizujZdrojeTab()
        {
            if (_sim == null) return;
            var z = _sim.AgentZdrojov;

            int volSestry = z.VolneSestry;
            int volLekari = z.VolneLekari;
            int volA = z.VolneMiestnostiA;
            int volB = z.VolneMiestnostiB;

            pbSestry.Maximum      = z.TotalSestry;
            pbLekari.Maximum      = z.TotalLekari;
            pbMiestnostiA.Maximum = z.TotalMiestnostiA;
            pbMiestnostiB.Maximum = z.TotalMiestnostiB;

            lblSestryVal.Text      = $"{volSestry} / {z.TotalSestry} voľných";
            lblLekariVal.Text      = $"{volLekari} / {z.TotalLekari} voľných";
            lblMiestnostiAVal.Text = $"{volA} / {z.TotalMiestnostiA} voľných";
            lblMiestnostiBVal.Text = $"{volB} / {z.TotalMiestnostiB} voľných";

            pbSestry.Value      = Math.Max(0, Math.Min(z.TotalSestry     - volSestry, pbSestry.Maximum));
            pbLekari.Value      = Math.Max(0, Math.Min(z.TotalLekari     - volLekari, pbLekari.Maximum));
            pbMiestnostiA.Value = Math.Max(0, Math.Min(z.TotalMiestnostiA - volA,     pbMiestnostiA.Maximum));
            pbMiestnostiB.Value = Math.Max(0, Math.Min(z.TotalMiestnostiB - volB,     pbMiestnostiB.Maximum));
        }

        private void AktualizujRadyTab()
        {
            if (_sim == null) return;
            var z = _sim.AgentZdrojov;

            var vvIds = z.RadVVIds.ToList();
            lbRadVV.BeginUpdate();
            lbRadVV.Items.Clear();
            foreach (var id in vvIds)
                lbRadVV.Items.Add($"Pacient #{id}");
            lbRadVV.EndUpdate();
            lblRadVVCount.Text = $"Rad VV ({vvIds.Count})";

            var oseItems = z.RadOsetreniaItems.OrderBy(x => x.Priorita).ToList();
            lbRadOsetrenie.BeginUpdate();
            lbRadOsetrenie.Items.Clear();
            foreach (var (id, prio) in oseItems)
                lbRadOsetrenie.Items.Add($"Pacient #{id}  [P{prio}]");
            lbRadOsetrenie.EndUpdate();
            lblRadOsetrenieCount.Text = $"Rad ošetrenie ({oseItems.Count})";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_sim != null) _sim.Zastavit = true;
            _simThread?.Interrupt();
            base.OnFormClosing(e);
        }
    }
}
