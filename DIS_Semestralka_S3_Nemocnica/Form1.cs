using Agents.AgentZdrojov;
using Simulation;
using System.Data;

namespace DIS_Semestralka_S3_Nemocnica
{
    public partial class Form1 : Form
    {
        private MySimulation? _sim;
        private Thread? _simThread;
        private readonly DataTable _pacientTable = new();

        // ── controls ──────────────────────────────────────────────────
        private NumericUpDown nudTrvanie = null!;
        private NumericUpDown nudReplikacie = null!;
        private NumericUpDown nudSeed = null!;
        private CheckBox cbNahodny = null!;
        private Button btnSpustit = null!;
        private Button btnZastavit = null!;
        private Label lblSimCas = null!;
        private Label lblReplikacia = null!;
        private Label lblPocetPacientov = null!;
        private DataGridView dgvPacienti = null!;
        private ProgressBar pbSestry = null!, pbLekari = null!, pbMiestnostiA = null!, pbMiestnostiB = null!;
        private Label lblSestryVal = null!, lblLekariVal = null!, lblMiestnostiAVal = null!, lblMiestnostiBVal = null!;
        private ListBox lbRadVV = null!, lbRadOsetrenie = null!;
        private Label lblRadVVCount = null!, lblRadOsetrenieCount = null!;

        // slowdown sliders
        private TrackBar trkDuration = null!;
        private TrackBar trkInterval = null!;
        private Label lblDurationVal = null!;
        private Label lblIntervalVal = null!;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            InitPacientTable();
        }

        // ── UI construction ───────────────────────────────────────────

        private void SetupUI()
        {
            SuspendLayout();

            var pnlTop = BuildTopPanel();
            var pnlSpeed = BuildSpeedPanel();
            var pnlStatus = BuildStatusPanel();
            var tabControl = BuildTabControl();

            Controls.Add(tabControl);
            Controls.Add(pnlStatus);
            Controls.Add(pnlSpeed);
            Controls.Add(pnlTop);

            ResumeLayout();
        }

        private FlowLayoutPanel BuildTopPanel()
        {
            var pnl = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(6, 8, 6, 0),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = SystemColors.ControlLight
            };

            nudTrvanie = AddLabeled(pnl, "Trvanie (hod):", new NumericUpDown { Minimum = 1, Maximum = 24, Value = 8, Width = 55 });
            nudReplikacie = AddLabeled(pnl, "Replikácie:", new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 1, Width = 70 });
            nudSeed = AddLabeled(pnl, "Seed:", new NumericUpDown { Minimum = 0, Maximum = 999999, Value = 42, Width = 70 });

            cbNahodny = new CheckBox { Text = "Náhodný", AutoSize = true, Margin = new Padding(0, 6, 12, 0) };
            cbNahodny.CheckedChanged += CbNahodny_CheckedChanged;
            pnl.Controls.Add(cbNahodny);

            btnSpustit = new Button { Text = "▶  Spustiť", Width = 95, Height = 28, Margin = new Padding(4, 4, 4, 0), BackColor = Color.LightGreen };
            btnSpustit.Click += BtnSpustit_Click;
            pnl.Controls.Add(btnSpustit);

            btnZastavit = new Button { Text = "⏹  Zastaviť", Width = 95, Height = 28, Margin = new Padding(4, 4, 4, 0), BackColor = Color.Salmon, Enabled = false };
            btnZastavit.Click += BtnZastavit_Click;
            pnl.Controls.Add(btnZastavit);

            return pnl;
        }

        private Panel BuildSpeedPanel()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = SystemColors.ControlLight,
                Padding = new Padding(8, 0, 8, 0)
            };

            // Duration slider  (0 – 2000 ms)
            var lblD = new Label { Text = "Trvanie pauzy:", AutoSize = true, Location = new Point(8, 8) };
            trkDuration = new TrackBar
            {
                Minimum = 0, Maximum = 2000, Value = 0, TickFrequency = 200,
                SmallChange = 10, LargeChange = 100,
                Location = new Point(115, 2), Width = 220, Height = 45
            };
            lblDurationVal = new Label { Text = "0 ms  (max rýchlosť)", AutoSize = true, Location = new Point(340, 8) };
            trkDuration.ValueChanged += SliderChanged;

            // Interval slider (1 – 3600 sim-sekúnd)
            var lblI = new Label { Text = "Interval (sim. sek.):", AutoSize = true, Location = new Point(500, 8) };
            trkInterval = new TrackBar
            {
                Minimum = 1, Maximum = 3600, Value = 60, TickFrequency = 300,
                SmallChange = 1, LargeChange = 60,
                Location = new Point(628, 2), Width = 220, Height = 45
            };
            lblIntervalVal = new Label { Text = "60 s", AutoSize = true, Location = new Point(853, 8) };
            trkInterval.ValueChanged += SliderChanged;

            pnl.Controls.AddRange(new Control[] { lblD, trkDuration, lblDurationVal, lblI, trkInterval, lblIntervalVal });
            return pnl;
        }

        private Panel BuildStatusPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 26, BackColor = SystemColors.Info };

            lblSimCas = new Label { Text = "Čas: —", AutoSize = true, Location = new Point(6, 5) };
            lblReplikacia = new Label { Text = "Replikácia: —", AutoSize = true, Location = new Point(130, 5) };
            lblPocetPacientov = new Label { Text = "Pacienti v systéme: 0", AutoSize = true, Location = new Point(320, 5) };
            pnl.Controls.AddRange(new Control[] { lblSimCas, lblReplikacia, lblPocetPacientov });
            return pnl;
        }

        private TabControl BuildTabControl()
        {
            var tab = new TabControl { Dock = DockStyle.Fill };
            tab.TabPages.Add(BuildPacientiTab());
            tab.TabPages.Add(BuildZdrojeTab());
            tab.TabPages.Add(BuildRadyTab());
            return tab;
        }

        private TabPage BuildPacientiTab()
        {
            var tp = new TabPage("Pacienti");
            dgvPacienti = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };
            tp.Controls.Add(dgvPacienti);
            return tp;
        }

        private TabPage BuildZdrojeTab()
        {
            var tp = new TabPage("Zdroje");

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                Padding = new Padding(12)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddHeaderRow(tlp);

            pbSestry = new ProgressBar { Minimum = 0, Maximum = AgentZdrojov.TotalSestry, Dock = DockStyle.Fill };
            pbLekari = new ProgressBar { Minimum = 0, Maximum = AgentZdrojov.TotalLekari, Dock = DockStyle.Fill };
            pbMiestnostiA = new ProgressBar { Minimum = 0, Maximum = AgentZdrojov.TotalMiestnostiA, Dock = DockStyle.Fill };
            pbMiestnostiB = new ProgressBar { Minimum = 0, Maximum = AgentZdrojov.TotalMiestnostiB, Dock = DockStyle.Fill };

            lblSestryVal = new Label { Text = "—", AutoSize = true, Anchor = AnchorStyles.Left };
            lblLekariVal = new Label { Text = "—", AutoSize = true, Anchor = AnchorStyles.Left };
            lblMiestnostiAVal = new Label { Text = "—", AutoSize = true, Anchor = AnchorStyles.Left };
            lblMiestnostiBVal = new Label { Text = "—", AutoSize = true, Anchor = AnchorStyles.Left };

            AddZdrojRow(tlp, "Sestry", lblSestryVal, pbSestry);
            AddZdrojRow(tlp, "Lekári", lblLekariVal, pbLekari);
            AddZdrojRow(tlp, "Miestnosti A", lblMiestnostiAVal, pbMiestnostiA);
            AddZdrojRow(tlp, "Miestnosti B", lblMiestnostiBVal, pbMiestnostiB);

            tp.Controls.Add(tlp);
            return tp;
        }

        private TabPage BuildRadyTab()
        {
            var tp = new TabPage("Rady");
            var sc = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };

            lblRadVVCount = new Label { Text = "Rad VV (0)", Dock = DockStyle.Top, Height = 22, Font = new Font(Font, FontStyle.Bold) };
            lbRadVV = new ListBox { Dock = DockStyle.Fill };
            sc.Panel1.Controls.Add(lbRadVV);
            sc.Panel1.Controls.Add(lblRadVVCount);

            lblRadOsetrenieCount = new Label { Text = "Rad ošetrenie (0)", Dock = DockStyle.Top, Height = 22, Font = new Font(Font, FontStyle.Bold) };
            lbRadOsetrenie = new ListBox { Dock = DockStyle.Fill };
            sc.Panel2.Controls.Add(lbRadOsetrenie);
            sc.Panel2.Controls.Add(lblRadOsetrenieCount);

            tp.Controls.Add(sc);
            return tp;
        }

        private static T AddLabeled<T>(FlowLayoutPanel parent, string labelText, T control) where T : Control
        {
            parent.Controls.Add(new Label { Text = labelText, AutoSize = true, Margin = new Padding(0, 7, 0, 0) });
            control.Margin = new Padding(2, 4, 10, 0);
            parent.Controls.Add(control);
            return control;
        }

        private static void AddHeaderRow(TableLayoutPanel tlp)
        {
            var bold = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
            tlp.Controls.Add(new Label { Text = "Zdroj", Font = bold, AutoSize = true }, 0, 0);
            tlp.Controls.Add(new Label { Text = "Voľné / Celkom", Font = bold, AutoSize = true }, 1, 0);
            tlp.Controls.Add(new Label { Text = "Vyťaženie", Font = bold, AutoSize = true }, 2, 0);
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        }

        private static void AddZdrojRow(TableLayoutPanel tlp, string name, Label valLabel, ProgressBar bar)
        {
            int row = tlp.RowStyles.Count;
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tlp.Controls.Add(new Label { Text = name, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            tlp.Controls.Add(valLabel, 1, row);
            tlp.Controls.Add(bar, 2, row);
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

            // OSPABA callbacks (fired on simulation thread → BeginInvoke to UI thread)
            _sim.OnRefreshUI(s => BeginInvoke(RefreshUI));
            _sim.OnReplicationDidFinish(s => BeginInvoke(AktualizujStatus));
            _sim.OnSimulationDidFinish(s => BeginInvoke(SimulaciaSkoncila));

            int reps = (int)nudReplikacie.Value;
            double duration = (double)nudTrvanie.Value * 3600.0;

            btnSpustit.Enabled = false;
            btnZastavit.Enabled = true;

            _simThread = new Thread(() =>
            {
                try
                {
                    AplukujSpomalenie();
                    _sim.Simulate(reps, duration);
                }
                catch (ThreadInterruptedException) { }
            }) { IsBackground = true };
            _simThread.Start();
        }

        private void BtnZastavit_Click(object sender, EventArgs e)
        {
            if (_sim != null) _sim.Zastavit = true;
            _simThread?.Interrupt();
        }

        private void CbNahodny_CheckedChanged(object sender, EventArgs e)
        {
            nudSeed.Enabled = !cbNahodny.Checked;
        }

        private void SliderChanged(object? sender, EventArgs e)
        {
            int durationMs = trkDuration.Value;
            int intervalSec = trkInterval.Value;

            lblDurationVal.Text = durationMs == 0 ? "0 ms  (max rýchlosť)" : $"{durationMs} ms";
            lblIntervalVal.Text = $"{intervalSec} s";

            if (_sim != null && _sim.IsRunning())
                AplukujSpomalenie();
        }

        private void AplukujSpomalenie()
        {
            if (_sim == null) return;
            int durationMs = trkDuration.Value;
            if (durationMs == 0)
            {
                _sim.SetMaxSimSpeed();
            }
            else
            {
                // SetSimSpeed(speed, tick):
                //   speed = reálne sekundy pauzy
                //   tick  = sim. sekundy medzi pauzami
                double speed = durationMs / 1000.0;
                double tick = trkInterval.Value;
                _sim.SetSimSpeed(speed, tick);
            }
        }

        private void SimulaciaSkoncila()
        {
            RefreshUI();
            btnSpustit.Enabled = true;
            btnZastavit.Enabled = false;
            lblReplikacia.Text = $"Replikácia: {_sim?.CurrentReplication} / {_sim?.ReplicationCount}  (hotovo)";
        }

        private void AktualizujStatus()
        {
            if (_sim == null) return;
            lblReplikacia.Text = $"Replikácia: {_sim.CurrentReplication} / {_sim.ReplicationCount}";
        }

        // ── UI refresh (volaný z OSPABA OnRefreshUI) ──────────────────

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
        }

        private void AktualizujZdrojeTab()
        {
            if (_sim == null) return;
            var z = _sim.AgentZdrojov;

            int volSestry = z.VolneSestry;
            int volLekari = z.VolneLekari;
            int volA = z.VolneMiestnostiA;
            int volB = z.VolneMiestnostiB;

            lblSestryVal.Text = $"{volSestry} / {AgentZdrojov.TotalSestry} voľných";
            lblLekariVal.Text = $"{volLekari} / {AgentZdrojov.TotalLekari} voľných";
            lblMiestnostiAVal.Text = $"{volA} / {AgentZdrojov.TotalMiestnostiA} voľných";
            lblMiestnostiBVal.Text = $"{volB} / {AgentZdrojov.TotalMiestnostiB} voľných";

            pbSestry.Value = Math.Min(AgentZdrojov.TotalSestry - volSestry, pbSestry.Maximum);
            pbLekari.Value = Math.Min(AgentZdrojov.TotalLekari - volLekari, pbLekari.Maximum);
            pbMiestnostiA.Value = Math.Min(AgentZdrojov.TotalMiestnostiA - volA, pbMiestnostiA.Maximum);
            pbMiestnostiB.Value = Math.Min(AgentZdrojov.TotalMiestnostiB - volB, pbMiestnostiB.Maximum);
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
