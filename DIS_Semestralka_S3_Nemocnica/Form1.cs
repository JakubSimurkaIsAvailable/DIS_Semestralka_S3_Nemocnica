using Agents.AgentZdrojov;
using DIS_Semestralka_S3_Nemocnica.Collectors;
using OSPAnimator;
using Simulation;
using System.Data;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
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
        private StreamWriter? _csvWriter;
        private string? _csvPath;
        private StreamWriter? _finalCsvWriter;
        private string? _finalCsvPath;
        private KonfigForm _konfigForm = new();

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
            StopCsvLogging();
            StopFinalCsvLogging();
            _sim = new MySimulation();

            if (cbNahodny.Checked)
                _sim.NastavNahodny();
            else
                _sim.NastavSeed((int)nudSeed.Value);

            _sim.KonfSestry       = (int)nudSestry.Value;
            _sim.KonfLekari       = (int)nudLekari.Value;
            _sim.KonfMiestnostiA  = (int)nudMiestnostiA.Value;
            _sim.KonfMiestnostiB  = (int)nudMiestnostiB.Value;
            _sim.KonfZahrievanie  = (double)nudZahrievanie.Value * 3600.0;
            _sim.PreferVV           = _konfigForm.PreferVV;
            _sim.RezervaLekarPreA   = _konfigForm.RezervaLekarPreA;
            _sim.RezervaSestraPreVV = _konfigForm.RezervaSestraPreVV;
            _sim.MinPohybPersonalu  = _konfigForm.MinPohybPersonalu;
            _sim.RadABPreferA       = _konfigForm.RadABPreferA;
            _sim.PrefRadBEnabled    = _konfigForm.PrefRadBEnabled;
            _sim.PrefRadBPrah       = _konfigForm.PrefRadBPrah;

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
                if (!_maxSpeed) WriteCsvSnapshot(s);
                if (_maxSpeed) return;
                long now = DateTime.UtcNow.Ticks;
                if (now - _lastRefreshTicks < 1_500_000) return;
                _lastRefreshTicks = now;
                try { Invoke(RefreshUI); } catch { }
            };
            _sim.ReplicationFinished += _ => BeginInvoke(() =>
            {
                AktualizujStatus();
                if (_maxSpeed) AktualizujStatistikyTab();
            });
            _replikacieForm?.AttachSim(_sim);
            _vysledkyForm?.AttachSim(_sim);

            int reps      = (int)nudReplikacie.Value;
            double duration = ((double)nudTrvanie.Value + (double)nudZahrievanie.Value) * 3600.0;

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

        private void BtnKonfig_Click(object sender, EventArgs e)
        {
            _konfigForm.Show(this);
            _konfigForm.BringToFront();
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
            _sim.MaxSpeed = _maxSpeed;
            if (_maxSpeed)
            {
                _sim.GuiInterval = trkInterval.Value;
                _sim.GuiDurationMs = 0;
                _sim.SetMaxSimSpeed();
                UpdateFinalCsvLogging();
                UpdateCsvLogging();
                return;
            }
            int ms  = trkDuration.Value;
            int sec = trkInterval.Value;
            _sim.GuiDurationMs = ms;
            _sim.GuiInterval   = sec;
            double dur = ms == 0 ? 0.001 : ms / 1000.0;
            _sim.SetSimSpeed(sec, dur);
            UpdateFinalCsvLogging();
            UpdateCsvLogging();
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

            WriteFinalCsvSummary();
            StopCsvLogging();
            StopFinalCsvLogging();
            RefreshUI();
        }

        private void UpdateCsvLogging()
        {
            if (_sim == null || _maxSpeed || !_konfigForm.CsvLogEnabled)
            {
                StopCsvLogging();
                return;
            }

            if (_csvWriter != null) return;

            string dir = Path.Combine(AppContext.BaseDirectory, "csv");
            Directory.CreateDirectory(dir);
            _csvPath = Path.Combine(dir, $"statistiky_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            _csvWriter = new StreamWriter(_csvPath, false, new UTF8Encoding(false)) { AutoFlush = true };
            _csvWriter.WriteLine(
                "TimeSec,Replication,Pacienti,PacientiPeso,PacientiSanitka," +
                "DobaVSystemeSec,DobaVSystemePesoSec,DobaVSystemeSanitkaSec," +
                "DobaVVSec,DobaVVPesoSec,DobaVVSanitkaSec," +
                "DobaOsetrenieSec,DobaOsetrenieASec,DobaOsetrenieABSec,DobaOsetrenieBSec," +
                "DobaPrichodDoOsetreniaSec,DobaPrichodDoOsetreniaPesoSec,DobaPrichodDoOsetreniaSanitkaSec," +
                "VytazenostLekari,VytazenostSestry,VytazenostMiestnostiA,VytazenostMiestnostiB," +
                "DlzkaRadVV,DlzkaRadA,DlzkaRadAB,DlzkaRadB");
        }

        private void StopCsvLogging()
        {
            _csvWriter?.Dispose();
            _csvWriter = null;
            _csvPath = null;
        }


        private void UpdateFinalCsvLogging()
        {
            if (_sim == null || !_maxSpeed || !_konfigForm.CsvFinalEnabled)
            {
                StopFinalCsvLogging();
                return;
            }

            if (_finalCsvWriter != null) return;

            string dir = Path.Combine(AppContext.BaseDirectory, "csv");
            Directory.CreateDirectory(dir);

            string? appendPath = _konfigForm.AppendFinalCsvPath;
            bool append = appendPath != null && File.Exists(appendPath);
            _finalCsvPath = append
                ? appendPath!
                : Path.Combine(dir, $"vysledky_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            _finalCsvWriter = new StreamWriter(_finalCsvPath, append: append, new UTF8Encoding(false)) { AutoFlush = true };

            if (!append)
                _finalCsvWriter.WriteLine(
                    "ReplicationCount,Pacienti,PacientiPeso,PacientiSanitka," +
                    "DobaVSystemeSec,DobaVSystemePesoSec,DobaVSystemeSanitkaSec," +
                    "DobaVVSec,DobaVVPesoSec,DobaVVSanitkaSec," +
                    "DobaOsetrenieSec,DobaOsetrenieASec,DobaOsetrenieABSec,DobaOsetrenieBSec," +
                    "DobaPrichodDoOsetreniaSec,DobaPrichodDoOsetreniaPesoSec,DobaPrichodDoOsetreniaSanitkaSec," +
                    "VytazenostLekari,VytazenostSestry,VytazenostMiestnostiA,VytazenostMiestnostiB," +
                    "DlzkaRadVV,DlzkaRadA,DlzkaRadAB,DlzkaRadB," +
                    "CI_Low_Pacienti,CI_Low_PacientiPeso,CI_Low_PacientiSanitka," +
                    "CI_Low_DobaVSystemeSec,CI_Low_DobaVSystemePesoSec,CI_Low_DobaVSystemeSanitkaSec," +
                    "CI_Low_DobaVVSec,CI_Low_DobaVVPesoSec,CI_Low_DobaVVSanitkaSec," +
                    "CI_Low_DobaOsetrenieSec,CI_Low_DobaOsetrenieASec,CI_Low_DobaOsetrenieABSec,CI_Low_DobaOsetrenieBSec," +
                    "CI_Low_DobaPrichodDoOsetreniaSec,CI_Low_DobaPrichodDoOsetreniaPesoSec,CI_Low_DobaPrichodDoOsetreniaSanitkaSec," +
                    "CI_Low_VytazenostLekari,CI_Low_VytazenostSestry,CI_Low_VytazenostMiestnostiA,CI_Low_VytazenostMiestnostiB," +
                    "CI_Low_DlzkaRadVV,CI_Low_DlzkaRadA,CI_Low_DlzkaRadAB,CI_Low_DlzkaRadB," +
                    "CI_High_Pacienti,CI_High_PacientiPeso,CI_High_PacientiSanitka," +
                    "CI_High_DobaVSystemeSec,CI_High_DobaVSystemePesoSec,CI_High_DobaVSystemeSanitkaSec," +
                    "CI_High_DobaVVSec,CI_High_DobaVVPesoSec,CI_High_DobaVVSanitkaSec," +
                    "CI_High_DobaOsetrenieSec,CI_High_DobaOsetrenieASec,CI_High_DobaOsetrenieABSec,CI_High_DobaOsetrenieBSec," +
                    "CI_High_DobaPrichodDoOsetreniaSec,CI_High_DobaPrichodDoOsetreniaPesoSec,CI_High_DobaPrichodDoOsetreniaSanitkaSec," +
                    "CI_High_VytazenostLekari,CI_High_VytazenostSestry,CI_High_VytazenostMiestnostiA,CI_High_VytazenostMiestnostiB," +
                    "CI_High_DlzkaRadVV,CI_High_DlzkaRadA,CI_High_DlzkaRadAB,CI_High_DlzkaRadB");
        }

        private void StopFinalCsvLogging()
        {
            _finalCsvWriter?.Dispose();
            _finalCsvWriter = null;
            _finalCsvPath = null;
        }

        private void WriteFinalCsvSummary()
        {
            if (_finalCsvWriter == null || _sim == null || !_maxSpeed || !_konfigForm.CsvFinalEnabled) return;
            var s = _sim;
            var fmt = CultureInfo.InvariantCulture;

            static string F(StatisticsCollector c, IFormatProvider p)
                => c.ValueCounter > 0 ? c.Average.ToString("G", p) : "";
            static string FCI(double? v, IFormatProvider p)
                => v.HasValue ? v.Value.ToString("G", p) : "";

            StatisticsCollector[] collectors =
            {
                s.PocetPacienti, s.PocetPeso, s.PocetSanitka,
                s.DobaVSysteme, s.DobaVSystemePeso, s.DobaVSystemeSanitka,
                s.DobaVV, s.DobaVVPeso, s.DobaVVSanitka,
                s.DobaOsetrenie, s.DobaOsetrenieA, s.DobaOsetrenieAB, s.DobaOsetrenieB,
                s.DobaPrichodDoOsetrenia, s.DobaPrichodDoOsetreniaPeso, s.DobaPrichodDoOsetreniaSanitka,
                s.VytazenostLekari, s.VytazenostSestry, s.VytazenostMiestnostiA, s.VytazenostMiestnostiB,
                s.DlzkaRadVV, s.DlzkaRadA, s.DlzkaRadAB, s.DlzkaRadB,
            };

            var intervals = collectors.Select(c => c.GetConfidenceInterval()).ToArray();

            var parts = new List<string> { s.ReplicationCount.ToString(fmt) };
            foreach (var c  in collectors) parts.Add(F(c, fmt));
            foreach (var iv in intervals)  parts.Add(FCI(iv?.Lower, fmt));
            foreach (var iv in intervals)  parts.Add(FCI(iv?.Upper, fmt));

            _finalCsvWriter.WriteLine(string.Join(",", parts));
        }

        private void WriteCsvSnapshot(MySimulation s)
        {
            if (_csvWriter == null) return;
            var o = s.AgentOkolia;
            var z = s.AgentZdrojov;
            var ci = CultureInfo.InvariantCulture;

            static double? Avg(StatisticsCollector c) => c.ValueCounter > 0 ? c.Average : null;
            static double? WAvg(WeightedStatisticsCollector c) => c.TotalWeight > 0 ? c.WeightedAverage : null;
            static string F(double? v, IFormatProvider p) => v.HasValue ? v.Value.ToString("G", p) : "0";

            string[] values =
            {
                s.CurrentTime.ToString("F3", ci),
                s.CurrentReplication.ToString(ci),
                o.LocPocetPacienti.ToString(ci),
                o.LocPocetPeso.ToString(ci),
                o.LocPocetSanitka.ToString(ci),
                F(Avg(o.LocDobaVSysteme), ci),
                F(Avg(o.LocDobaVSystemePeso), ci),
                F(Avg(o.LocDobaVSystemeSanitka), ci),
                F(Avg(z.LocDobaVV), ci),
                F(Avg(z.LocDobaVVPeso), ci),
                F(Avg(z.LocDobaVVSanitka), ci),
                F(Avg(z.LocDobaOsetrenie), ci),
                F(Avg(z.LocDobaOsetrenieA), ci),
                F(Avg(z.LocDobaOsetrenieAB), ci),
                F(Avg(z.LocDobaOsetrenieB), ci),
                F(Avg(z.LocDobaPrichodDoOsetrenia), ci),
                F(Avg(z.LocDobaPrichodDoOsetreniaPeso), ci),
                F(Avg(z.LocDobaPrichodDoOsetreniaSanitka), ci),
                F(WAvg(z.LocVytazenostLekari), ci),
                F(WAvg(z.LocVytazenostSestry), ci),
                F(WAvg(z.LocVytazenostMiestnostiA), ci),
                F(WAvg(z.LocVytazenostMiestnostiB), ci),
                F(WAvg(z.LocDlzkaRaduVV), ci),
                F(WAvg(z.LocDlzkaRaduA), ci),
                F(WAvg(z.LocDlzkaRaduAB), ci),
                F(WAvg(z.LocDlzkaRaduB), ci),
            };

            _csvWriter.WriteLine(string.Join(",", values));
        }

        private static string FormatCas(double sec)
        {
            var ts = TimeSpan.FromSeconds(sec);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        private void AktualizujStatus()
        {
            if (_sim == null) return;
            lblReplikacia.Text = $"Replikácia: {_sim.CurrentReplication} / {_sim.ReplicationCount}";
            lblSimCas.Text = $"Čas: {FormatCas(_sim.CurrentTime)}";
        }

        private void RefreshUI()
        {
            if (_sim == null) return;
            lblSimCas.Text = $"Čas: {FormatCas(_sim.CurrentTime)}";
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
                    FormatCas(p.CasPrichodu),
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
            lblRadBCount.Text = $"Rad B – priorita 3-5 ({bItems.Count})";
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
            ("DLZKY RADOV",                  true),
            ("Rad VV – vstupné vyšetrenie",  false),
            ("Rad A – priorita 1-2",         false),
            ("Rad A/B – priorita 3-4",       false),
            ("Rad B – priorita 5",         false),
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
                row.Cells["colReplikacia"].Value = isHeader ? "" : "0";
                row.Cells["colPriemer"].Value  = isHeader ? "" : "0";
                if (isHeader)
                    row.DefaultCellStyle = headerStyle;
            }
        }

        private void AktualizujStatistikyTab()
        {
            if (_sim == null) return;
            var s = _sim;
            var o = s.AgentOkolia;
            var z = s.AgentZdrojov;

            static string Cas(double sec) => sec < 0 ? "00:00:00" : FormatCas(sec);
            static string Pct(double v) => $"{v * 100:F1} %";
            static string Pocet(double v) => $"{v:F2}";

            // Stĺpec „Replikácia" — živé hodnoty z lokálnych kolektorov agentov.
            // V spomalenom móde sa aktualizujú priebežne, v rýchlom po každej replikácii.
            static string LiveCas(StatisticsCollector loc)
                => loc.ValueCounter > 0 ? Cas(loc.Average) : "00:00:00";
            static string LiveCount(int val)
                => $"{val}";
            static string LivePct(WeightedStatisticsCollector loc)
                => loc.TotalWeight > 0 ? Pct(loc.WeightedAverage) : "0.0 %";
            static string LivePocet(WeightedStatisticsCollector loc)
                => loc.TotalWeight > 0 ? Pocet(loc.WeightedAverage) : "0.00";

            static string AggStat(StatisticsCollector c)
            {
                if (c.ValueCounter == 0) return "00:00:00";
                var ci = c.GetConfidenceInterval();
                string avg = Cas(c.Average);
                if (ci == null) return $"{avg}  (n<30)";
                return $"{avg}  [{Cas(ci.Value.Lower)}, {Cas(ci.Value.Upper)}]";
            }
            static string AggCount(StatisticsCollector c)
            {
                if (c.ValueCounter == 0) return "0.0";
                var ci = c.GetConfidenceInterval();
                string avg = $"{c.Average:F1}";
                if (ci == null) return $"{avg}  (n<30)";
                return $"{avg}  [{ci.Value.Lower:F1}, {ci.Value.Upper:F1}]";
            }
            static string AggPct(StatisticsCollector c)
            {
                if (c.ValueCounter == 0) return "0.0 %";
                var ci = c.GetConfidenceInterval();
                string avg = Pct(c.Average);
                if (ci == null) return $"{avg}  (n<30)";
                return $"{avg}  [{Pct(ci.Value.Lower)}, {Pct(ci.Value.Upper)}]";
            }
            static string AggPocet(StatisticsCollector c)
            {
                if (c.ValueCounter == 0) return "0.00";
                var ci = c.GetConfidenceInterval();
                string avg = Pocet(c.Average);
                if (ci == null) return $"{avg}  (n<30)";
                return $"{avg}  [{Pocet(ci.Value.Lower)}, {Pocet(ci.Value.Upper)}]";
            }

            void Set(int row, string cur, string agg)
            {
                dgvStat.Rows[row].Cells["colReplikacia"].Value = cur;
                dgvStat.Rows[row].Cells["colPriemer"].Value    = agg;
            }

            // Pacienti (rows 1-3)
            Set(1, LiveCount(o.LocPocetPacienti), AggCount(s.PocetPacienti));
            Set(2, LiveCount(o.LocPocetPeso),     AggCount(s.PocetPeso));
            Set(3, LiveCount(o.LocPocetSanitka),  AggCount(s.PocetSanitka));

            // Čas v systéme (rows 5-7)
            Set(5, LiveCas(o.LocDobaVSysteme),        AggStat(s.DobaVSysteme));
            Set(6, LiveCas(o.LocDobaVSystemePeso),    AggStat(s.DobaVSystemePeso));
            Set(7, LiveCas(o.LocDobaVSystemeSanitka), AggStat(s.DobaVSystemeSanitka));

            // Čakanie na VV (rows 9-11)
            Set(9,  LiveCas(z.LocDobaVV),        AggStat(s.DobaVV));
            Set(10, LiveCas(z.LocDobaVVPeso),    AggStat(s.DobaVVPeso));
            Set(11, LiveCas(z.LocDobaVVSanitka), AggStat(s.DobaVVSanitka));

            // Čakanie na ošetrenie (rows 13-16)
            Set(13, LiveCas(z.LocDobaOsetrenie),   AggStat(s.DobaOsetrenie));
            Set(14, LiveCas(z.LocDobaOsetrenieA),  AggStat(s.DobaOsetrenieA));
            Set(15, LiveCas(z.LocDobaOsetrenieAB), AggStat(s.DobaOsetrenieAB));
            Set(16, LiveCas(z.LocDobaOsetrenieB),  AggStat(s.DobaOsetrenieB));

            // Čas do začiatku ošetrenia (rows 18-20)
            Set(18, LiveCas(z.LocDobaPrichodDoOsetrenia),         AggStat(s.DobaPrichodDoOsetrenia));
            Set(19, LiveCas(z.LocDobaPrichodDoOsetreniaPeso),     AggStat(s.DobaPrichodDoOsetreniaPeso));
            Set(20, LiveCas(z.LocDobaPrichodDoOsetreniaSanitka),  AggStat(s.DobaPrichodDoOsetreniaSanitka));

            // Vyťaženie zdrojov (rows 22-25)
            Set(22, LivePct(z.LocVytazenostLekari),      AggPct(s.VytazenostLekari));
            Set(23, LivePct(z.LocVytazenostSestry),      AggPct(s.VytazenostSestry));
            Set(24, LivePct(z.LocVytazenostMiestnostiA), AggPct(s.VytazenostMiestnostiA));
            Set(25, LivePct(z.LocVytazenostMiestnostiB), AggPct(s.VytazenostMiestnostiB));

            // Dĺžky radov (rows 27-30)
            Set(27, LivePocet(z.LocDlzkaRaduVV),  AggPocet(s.DlzkaRadVV));
            Set(28, LivePocet(z.LocDlzkaRaduA),   AggPocet(s.DlzkaRadA));
            Set(29, LivePocet(z.LocDlzkaRaduAB),  AggPocet(s.DlzkaRadAB));
            Set(30, LivePocet(z.LocDlzkaRaduB),   AggPocet(s.DlzkaRadB));
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
            StopCsvLogging();
            StopFinalCsvLogging();
            base.OnFormClosing(e);
        }
    }
}
