using Agents.AgentZdrojov;
using DIS_Semestralka_S3_Nemocnica.Collectors;
using OSPAnimator;
using Simulation;
using System.Data;
using System.IO;
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
        private volatile bool _maxSpeed;
        private readonly ElementHost _animHost = new() { Dock = DockStyle.Fill };
        private Animator? _animator;
        private TextWriter? _savedConsoleOut;
        private ReplikacieForm? _replikacieForm;
        private VysledkyForm? _vysledkyForm;

        public Form1()
        {
            InitializeComponent();
            InitPacientTable();
            InitStatGrid();
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

            // Animator runs only when not in max-speed mode (Dispatcher.Invoke per patient is too slow)
            if (!_maxSpeed)
            {
                _animator = new Animator(_sim);
                _animator.SynchronizedTime = false;
                _animHost.Child = _animator.MyCanvas;
                _sim.Animator = _animator;
            }
            else
            {
                _sim.Animator = null;
            }

            // Suppress console output at max speed (Console.WriteLine is I/O-bound and slow)
            if (_maxSpeed && _savedConsoleOut == null)
            {
                _savedConsoleOut = Console.Out;
                Console.SetOut(TextWriter.Null);
            }

            _pauseEvent.Set();
            _lastRefreshTicks = 0;
            _sim.GuiTick += s =>
            {
                _pauseEvent.Wait();
                if (_maxSpeed) return;
                long now = DateTime.UtcNow.Ticks;
                if (now - _lastRefreshTicks < 1_500_000) return;
                _lastRefreshTicks = now;
                try { Invoke(RefreshUI); } catch { }
            };
            _sim.ReplicationFinished += _ => BeginInvoke(AktualizujStatus);

            _replikacieForm?.AttachSim(_sim);
            _vysledkyForm?.AttachSim(_sim);

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
            if (_maxSpeed)
            {
                // Also update GuiInterval/GuiDurationMs so PrepareReplication() doesn't override us
                _sim.GuiInterval   = 3_600_000;
                _sim.GuiDurationMs = 0;
                _sim.SetSimSpeed(3_600_000, 0.001);
                return;
            }
            int ms  = trkDuration.Value;
            int sec = trkInterval.Value;
            _sim.GuiDurationMs = ms;
            _sim.GuiInterval   = sec;
            double dur = ms == 0 ? 0.001 : ms / 1000.0;
            _sim.SetSimSpeed(sec, dur);
        }

        private void BtnMaxSpeed_Click(object sender, EventArgs e)
        {
            _maxSpeed = !_maxSpeed;
            if (_maxSpeed)
            {
                btnMaxSpeed.Text      = "⚡ Max Speed: ZAP";
                btnMaxSpeed.BackColor = System.Drawing.Color.LightGreen;

                // Detach animator so Dispatcher.Invoke calls stop
                if (_sim != null)
                    _sim.Animator = null;

                // Suppress console output
                if (_savedConsoleOut == null)
                {
                    _savedConsoleOut = Console.Out;
                    Console.SetOut(TextWriter.Null);
                }
            }
            else
            {
                btnMaxSpeed.Text      = "⚡ Max Speed: VYP";
                btnMaxSpeed.BackColor = System.Drawing.Color.LightGray;

                // Restore console output
                if (_savedConsoleOut != null)
                {
                    Console.SetOut(_savedConsoleOut);
                    _savedConsoleOut = null;
                }

                // Re-attach animator if simulation is running
                if (_sim != null && _animator != null)
                    _sim.Animator = _animator;
            }
            AplukujSpomalenie();
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

            // Restore console so post-simulation output is visible
            if (_savedConsoleOut != null)
            {
                Console.SetOut(_savedConsoleOut);
                _savedConsoleOut = null;
            }

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
            AktualizujStatistikyTab();
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
            lblRadVVCount.Text = $"Rad VV – vstupné vyšetrenie ({vvIds.Count})";

            var aItems = z.RadAItems.ToList();
            lbRadA.BeginUpdate();
            lbRadA.Items.Clear();
            foreach (var (id, prio) in aItems)
                lbRadA.Items.Add($"Pacient #{id}  [P{prio}]");
            lbRadA.EndUpdate();
            lblRadACount.Text = $"Rad A – priorita 1-2 ({aItems.Count})";

            var abItems = z.RadABItems.ToList();
            lbRadAB.BeginUpdate();
            lbRadAB.Items.Clear();
            foreach (var (id, prio) in abItems)
                lbRadAB.Items.Add($"Pacient #{id}  [P{prio}]");
            lbRadAB.EndUpdate();
            lblRadABCount.Text = $"Rad A/B – priorita 3-4 ({abItems.Count})";

            var bItems = z.RadBItems.ToList();
            lbRadB.BeginUpdate();
            lbRadB.Items.Clear();
            foreach (var (id, prio) in bItems)
                lbRadB.Items.Add($"Pacient #{id}  [P{prio}]");
            lbRadB.EndUpdate();
            lblRadBCount.Text = $"Rad B – priorita 5 ({bItems.Count})";
        }

        // ── Statistics tab ────────────────────────────────────────────

        private static readonly (string Name, bool IsHeader)[] StatDefs =
        {
            ("PACIENTI",                     true),
            ("Celkovo",                      false),
            ("Pešo (samostatne)",            false),
            ("Sanitkou",                     false),
            ("ČAS V SYSTÉME",                true),
            ("Celkovo",                      false),
            ("Pešo (samostatne)",            false),
            ("Sanitkou",                     false),
            ("ČAKANIE NA VV",                true),
            ("Celkovo",                      false),
            ("Pešo (samostatne)",            false),
            ("Sanitkou",                     false),
            ("ČAKANIE NA OŠETRENIE",         true),
            ("Celkovo",                      false),
            ("Rad A – priorita 1–2",         false),
            ("Rad A/B – priorita 3–4",       false),
            ("Rad B – priorita 5",           false),
            ("ČAS DO ZAČIATKU OŠETRENIA",    true),
            ("Celkovo",                      false),
            ("Pešo (samostatne)",            false),
            ("Sanitkou",                     false),
            ("VYŤAŽENIE ZDROJOV",            true),
            ("Lekári",                       false),
            ("Sestry",                       false),
            ("Miestnosti A",                 false),
            ("Miestnosti B",                 false),
        };

        private void InitStatGrid()
        {
            var headerStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(210, 210, 220),
                Font = new System.Drawing.Font(dgvStat.Font, System.Drawing.FontStyle.Bold),
                SelectionBackColor = System.Drawing.Color.FromArgb(180, 180, 200),
            };

            foreach (var (name, isHeader) in StatDefs)
            {
                int idx = dgvStat.Rows.Add();
                var row = dgvStat.Rows[idx];
                row.Cells["colNazov"].Value    = isHeader ? name : "  " + name;
                row.Cells["colReplikacia"].Value = isHeader ? "" : "—";
                row.Cells["colPriemer"].Value  = isHeader ? "" : "—";
                if (isHeader)
                    row.DefaultCellStyle = headerStyle;
            }
        }

        private void AktualizujStatistikyTab()
        {
            if (_sim == null) return;
            var s = _sim;

            static string Cas(double sec) =>
                sec <= 0 ? "—" : TimeSpan.FromSeconds(sec).ToString(@"mm\:ss");
            static string Pct(double v) => $"{v * 100:F1} %";

            // Read last completed replication's value from the aggregate collector.
            // This avoids the race where PrepareReplication() resets local collectors
            // before RefreshUI() runs on the UI thread.
            static string LastCas(StatisticsCollector c)
            {
                var vals = c.GetValues();
                return vals.Length == 0 ? "—" : Cas(vals[vals.Length - 1]);
            }
            static string LastCount(StatisticsCollector c)
            {
                var vals = c.GetValues();
                return vals.Length == 0 ? "—" : $"{vals[vals.Length - 1]:F0}";
            }
            static string LastPct(StatisticsCollector c)
            {
                var vals = c.GetValues();
                return vals.Length == 0 ? "—" : Pct(vals[vals.Length - 1]);
            }

            static string AggStat(StatisticsCollector c)
            {
                if (c.ValueCounter == 0) return "—";
                var ci = c.GetConfidenceInterval();
                string avg = Cas(c.Average);
                if (ci == null) return $"{avg}  (n<30)";
                return $"{avg}  ±{Cas(c.Average - ci.Value.Lower)}";
            }
            static string AggCount(StatisticsCollector c)
            {
                if (c.ValueCounter == 0) return "—";
                var ci = c.GetConfidenceInterval();
                string avg = $"{c.Average:F1}";
                if (ci == null) return $"{avg}  (n<30)";
                return $"{avg}  ±{c.Average - ci.Value.Lower:F1}";
            }
            static string AggPct(StatisticsCollector c)
            {
                if (c.ValueCounter == 0) return "—";
                var ci = c.GetConfidenceInterval();
                string avg = Pct(c.Average);
                if (ci == null) return $"{avg}  (n<30)";
                return $"{avg}  ±{(c.Average - ci.Value.Lower) * 100:F1} %";
            }

            void Set(int row, string cur, string agg)
            {
                dgvStat.Rows[row].Cells["colReplikacia"].Value = cur;
                dgvStat.Rows[row].Cells["colPriemer"].Value    = agg;
            }

            // Pacienti (rows 1-3)
            Set(1, LastCount(s.PocetPacienti), AggCount(s.PocetPacienti));
            Set(2, LastCount(s.PocetPeso),     AggCount(s.PocetPeso));
            Set(3, LastCount(s.PocetSanitka),  AggCount(s.PocetSanitka));

            // Čas v systéme (rows 5-7)
            Set(5, LastCas(s.DobaVSysteme),        AggStat(s.DobaVSysteme));
            Set(6, LastCas(s.DobaVSystemePeso),    AggStat(s.DobaVSystemePeso));
            Set(7, LastCas(s.DobaVSystemeSanitka), AggStat(s.DobaVSystemeSanitka));

            // Čakanie na VV (rows 9-11)
            Set(9,  LastCas(s.DobaVV),        AggStat(s.DobaVV));
            Set(10, LastCas(s.DobaVVPeso),    AggStat(s.DobaVVPeso));
            Set(11, LastCas(s.DobaVVSanitka), AggStat(s.DobaVVSanitka));

            // Čakanie na ošetrenie (rows 13-16)
            Set(13, LastCas(s.DobaOsetrenie),   AggStat(s.DobaOsetrenie));
            Set(14, LastCas(s.DobaOsetrenieA),  AggStat(s.DobaOsetrenieA));
            Set(15, LastCas(s.DobaOsetrenieAB), AggStat(s.DobaOsetrenieAB));
            Set(16, LastCas(s.DobaOsetrenieB),  AggStat(s.DobaOsetrenieB));

            // Čas do začiatku ošetrenia (rows 18-20)
            Set(18, LastCas(s.DobaPrichodDoOsetrenia),         AggStat(s.DobaPrichodDoOsetrenia));
            Set(19, LastCas(s.DobaPrichodDoOsetreniaPeso),     AggStat(s.DobaPrichodDoOsetreniaPeso));
            Set(20, LastCas(s.DobaPrichodDoOsetreniaSanitka),  AggStat(s.DobaPrichodDoOsetreniaSanitka));

            // Vyťaženie zdrojov (rows 22-25)
            Set(22, LastPct(s.VytazenostLekari),       AggPct(s.VytazenostLekari));
            Set(23, LastPct(s.VytazenostSestry),       AggPct(s.VytazenostSestry));
            Set(24, LastPct(s.VytazenostMiestnostiA),  AggPct(s.VytazenostMiestnostiA));
            Set(25, LastPct(s.VytazenostMiestnostiB),  AggPct(s.VytazenostMiestnostiB));
        }

        private void BtnReplikacie_Click(object sender, EventArgs e)
        {
            bool isNew = _replikacieForm == null || _replikacieForm.IsDisposed;
            if (isNew)
            {
                _replikacieForm = new ReplikacieForm(() => _maxSpeed);
                if (_sim != null) _replikacieForm.AttachSim(_sim);
            }
            _replikacieForm!.Show(this);
            _replikacieForm.BringToFront();
        }

        private void BtnVysledky_Click(object sender, EventArgs e)
        {
            bool isNew = _vysledkyForm == null || _vysledkyForm.IsDisposed;
            if (isNew)
            {
                _vysledkyForm = new VysledkyForm();
                if (_sim != null) _vysledkyForm.AttachSim(_sim);
            }
            _vysledkyForm!.Show(this);
            _vysledkyForm.BringToFront();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_sim != null) _sim.Zastavit = true;
            _simThread?.Interrupt();
            base.OnFormClosing(e);
        }
    }
}
