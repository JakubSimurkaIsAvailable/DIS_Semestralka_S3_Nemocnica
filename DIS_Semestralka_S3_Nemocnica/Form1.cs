using Agents.AgentZdrojov;
using DIS_Semestralka_S3_Nemocnica.Collectors;
using OSPAnimator;
using Simulation;
using System.Data;
using System.Globalization;
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

        private void CbCsvLog_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCsvLogging();
        }

        private void CbCsvFinal_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFinalCsvLogging();
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
                _sim.GuiInterval = 1;
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
            if (_sim == null || _maxSpeed || !cbCsvLog.Checked)
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
            if (_sim == null || !_maxSpeed || !cbCsvFinal.Checked)
            {
                StopFinalCsvLogging();
                return;
            }

            if (_finalCsvWriter != null) return;

            string dir = Path.Combine(AppContext.BaseDirectory, "csv");
            Directory.CreateDirectory(dir);
            _finalCsvPath = Path.Combine(dir, $"vysledky_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            _finalCsvWriter = new StreamWriter(_finalCsvPath, false, new UTF8Encoding(false)) { AutoFlush = true };
            _finalCsvWriter.WriteLine(
                "ReplicationCount,Pacienti,PacientiPeso,PacientiSanitka," +
                "DobaVSystemeSec,DobaVSystemePesoSec,DobaVSystemeSanitkaSec," +
                "DobaVVSec,DobaVVPesoSec,DobaVVSanitkaSec," +
                "DobaOsetrenieSec,DobaOsetrenieASec,DobaOsetrenieABSec,DobaOsetrenieBSec," +
                "DobaPrichodDoOsetreniaSec,DobaPrichodDoOsetreniaPesoSec,DobaPrichodDoOsetreniaSanitkaSec," +
                "VytazenostLekari,VytazenostSestry,VytazenostMiestnostiA,VytazenostMiestnostiB," +
                "DlzkaRadVV,DlzkaRadA,DlzkaRadAB,DlzkaRadB");
        }

        private void StopFinalCsvLogging()
        {
            _finalCsvWriter?.Dispose();
            _finalCsvWriter = null;
            _finalCsvPath = null;
        }

        private void WriteFinalCsvSummary()
        {
            if (_finalCsvWriter == null || _sim == null || !_maxSpeed || !cbCsvFinal.Checked) return;
            var s = _sim;
            var ci = CultureInfo.InvariantCulture;

            static string F(StatisticsCollector c, IFormatProvider p)
                => c.ValueCounter > 0 ? c.Average.ToString("G", p) : "";

            string[] values =
            {
                s.ReplicationCount.ToString(ci),
                F(s.PocetPacienti, ci),
                F(s.PocetPeso, ci),
                F(s.PocetSanitka, ci),
                F(s.DobaVSysteme, ci),
                F(s.DobaVSystemePeso, ci),
                F(s.DobaVSystemeSanitka, ci),
                F(s.DobaVV, ci),
                F(s.DobaVVPeso, ci),
                F(s.DobaVVSanitka, ci),
                F(s.DobaOsetrenie, ci),
                F(s.DobaOsetrenieA, ci),
                F(s.DobaOsetrenieAB, ci),
                F(s.DobaOsetrenieB, ci),
                F(s.DobaPrichodDoOsetrenia, ci),
                F(s.DobaPrichodDoOsetreniaPeso, ci),
                F(s.DobaPrichodDoOsetreniaSanitka, ci),
                F(s.VytazenostLekari, ci),
                F(s.VytazenostSestry, ci),
                F(s.VytazenostMiestnostiA, ci),
                F(s.VytazenostMiestnostiB, ci),
                F(s.DlzkaRadVV, ci),
                F(s.DlzkaRadA, ci),
                F(s.DlzkaRadAB, ci),
                F(s.DlzkaRadB, ci),
            };

            _finalCsvWriter.WriteLine(string.Join(",", values));
        }

        private void WriteCsvSnapshot(MySimulation s)
        {
            if (_csvWriter == null) return;
            var o = s.AgentOkolia;
            var z = s.AgentZdrojov;
            var ci = CultureInfo.InvariantCulture;

            static double? Avg(StatisticsCollector c) => c.ValueCounter > 0 ? c.Average : null;
            static double? WAvg(WeightedStatisticsCollector c) => c.TotalWeight > 0 ? c.WeightedAverage : null;
            static string F(double? v, IFormatProvider p) => v.HasValue ? v.Value.ToString("G", p) : "";

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
            ("Rad B – priorita 3-5",         false),
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
            var o = s.AgentOkolia;
            var z = s.AgentZdrojov;

            static string Cas(double sec) =>
                sec <= 0 ? "—" : TimeSpan.FromSeconds(sec).ToString(@"hh\:mm\:ss");
            static string Pct(double v) => $"{v * 100:F1} %";

            // Stĺpec „Replikácia" — živé hodnoty z lokálnych kolektorov agentov.
            // V spomalenom móde sa aktualizujú priebežne, v rýchlom po každej replikácii.
            static string LiveCas(StatisticsCollector loc)
                => loc.ValueCounter > 0 ? Cas(loc.Average) : "—";
            static string LiveCount(int val)
                => val > 0 ? $"{val}" : "—";
            static string LivePct(WeightedStatisticsCollector loc)
                => loc.TotalWeight > 0 ? Pct(loc.WeightedAverage) : "—";

            static string AggStat(StatisticsCollector c)
            {
                if (c.ValueCounter == 0) return "—";
                var ci = c.GetConfidenceInterval();
                string avg = Cas(c.Average);
                if (ci == null) return $"{avg}  (n<30)";
                return $"{avg}  [{Cas(ci.Value.Lower)}, {Cas(ci.Value.Upper)}]";
            }
            static string AggCount(StatisticsCollector c)
            {
                if (c.ValueCounter == 0) return "—";
                var ci = c.GetConfidenceInterval();
                string avg = $"{c.Average:F1}";
                if (ci == null) return $"{avg}  (n<30)";
                return $"{avg}  [{ci.Value.Lower:F1}, {ci.Value.Upper:F1}]";
            }
            static string AggPct(StatisticsCollector c)
            {
                if (c.ValueCounter == 0) return "—";
                var ci = c.GetConfidenceInterval();
                string avg = Pct(c.Average);
                if (ci == null) return $"{avg}  (n<30)";
                return $"{avg}  [{Pct(ci.Value.Lower)}, {Pct(ci.Value.Upper)}]";
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
            Set(27, LivePct(z.LocDlzkaRaduVV),  AggPct(s.DlzkaRadVV));
            Set(28, LivePct(z.LocDlzkaRaduA),   AggPct(s.DlzkaRadA));
            Set(29, LivePct(z.LocDlzkaRaduAB),  AggPct(s.DlzkaRadAB));
            Set(30, LivePct(z.LocDlzkaRaduB),   AggPct(s.DlzkaRadB));
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
