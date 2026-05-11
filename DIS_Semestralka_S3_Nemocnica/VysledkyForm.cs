using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ScottPlot.WinForms;
using DIS_Semestralka_S3_Nemocnica.Collectors;
using Simulation;

namespace DIS_Semestralka_S3_Nemocnica
{
    public partial class VysledkyForm : Form
    {
        private sealed class PlotEntry
        {
            public FormsPlot Fp { get; }
            public Func<MySimulation, StatisticsCollector> GetColl { get; }
            public Func<double, double> Transform { get; }

            private readonly List<double> _xs = new();
            private readonly List<double> _ys = new();

            public PlotEntry(FormsPlot fp, Func<MySimulation, StatisticsCollector> getColl, Func<double, double> transform)
            {
                Fp = fp; GetColl = getColl; Transform = transform;
            }

            public void Reset()
            {
                _xs.Clear();
                _ys.Clear();
                Fp.Plot.Clear();
                Fp.Plot.Add.Annotation("Čakajte na dokončenie replikácií...");
                Fp.Refresh();
            }

            public void AppendPoint(int rep, double avg)
            {
                _xs.Add(rep);
                _ys.Add(avg);

                var plt = Fp.Plot;
                plt.Clear();

                var sc = plt.Add.Scatter(_xs.ToArray(), _ys.ToArray());
                sc.Color      = ScottPlot.Color.FromHex("#1565C0");
                sc.MarkerSize = 3;
                sc.LineWidth  = 2f;

                plt.Axes.AutoScale();
                Fp.Refresh();
            }
        }

        private readonly List<PlotEntry> _entries = new();
        private volatile bool _pendingUpdate;
        private MySimulation? _currentSim;
        private readonly Action<MySimulation> _onRepFinished;

        public VysledkyForm()
        {
            InitializeComponent();

            void Add(string tab, string title, string yUnit,
                     Func<MySimulation, StatisticsCollector> getColl, Func<double, double> t)
            {
                var fp = new FormsPlot { Dock = DockStyle.Fill };
                fp.Plot.Title(title);
                fp.Plot.XLabel("Replikácia");
                fp.Plot.YLabel(yUnit);
                var tp = new TabPage(tab);
                tp.Controls.Add(fp);
                tabControl.TabPages.Add(tp);
                _entries.Add(new PlotEntry(fp, getColl, t));
            }

            Add("Čas – celkovo",    "Čas v systéme – celkovo",        "minúty",          s => s.DobaVSysteme,              v => v / 60.0);
            Add("Čas – pešo",       "Čas v systéme – pešo",           "minúty",          s => s.DobaVSystemePeso,          v => v / 60.0);
            Add("Čas – sanitkou",   "Čas v systéme – sanitkou",       "minúty",          s => s.DobaVSystemeSanitka,       v => v / 60.0);
            Add("VV – celkovo",     "Čakanie na VV – celkovo",        "minúty",          s => s.DobaVV,                    v => v / 60.0);
            Add("VV – pešo",        "Čakanie na VV – pešo",           "minúty",          s => s.DobaVVPeso,                v => v / 60.0);
            Add("VV – sanitkou",    "Čakanie na VV – sanitkou",       "minúty",          s => s.DobaVVSanitka,             v => v / 60.0);
            Add("Ošetr. – celkovo", "Čakanie na ošetrenie – celkovo", "minúty",          s => s.DobaOsetrenie,             v => v / 60.0);
            Add("Ošetr. – Rad A",   "Čakanie na ošetrenie – Rad A",   "minúty",          s => s.DobaOsetrenieA,            v => v / 60.0);
            Add("Ošetr. – Rad A/B", "Čakanie na ošetrenie – Rad A/B", "minúty",          s => s.DobaOsetrenieAB,           v => v / 60.0);
            Add("Ošetr. – Rad B",   "Čakanie na ošetrenie – Rad B",   "minúty",          s => s.DobaOsetrenieB,            v => v / 60.0);
            Add("Príchod → ošetr.", "Čas od príchodu do začiatku ošetrenia – celkovo",   "minúty", s => s.DobaPrichodDoOsetrenia,         v => v / 60.0);
            Add("Príchod → ošetr. (pešo)",    "Čas od príchodu do začiatku ošetrenia – pešo",    "minúty", s => s.DobaPrichodDoOsetreniaPeso,    v => v / 60.0);
            Add("Príchod → ošetr. (sanitka)", "Čas od príchodu do začiatku ošetrenia – sanitkou","minúty", s => s.DobaPrichodDoOsetreniaSanitka, v => v / 60.0);
            Add("Lekári",           "Vyťaženie lekárov",              "percent (%)",     s => s.VytazenostLekari,          v => v * 100.0);
            Add("Sestry",           "Vyťaženie sestier",              "percent (%)",     s => s.VytazenostSestry,          v => v * 100.0);
            Add("Miestnosti A",     "Vyťaženie miestností A",         "percent (%)",     s => s.VytazenostMiestnostiA,     v => v * 100.0);
            Add("Miestnosti B",     "Vyťaženie miestností B",         "percent (%)",     s => s.VytazenostMiestnostiB,     v => v * 100.0);
            Add("Dĺžka radu A",     "Dĺžka radu A",                  "počet pacientov", s => s.DlzkaRadA,                 v => v);
            Add("Dĺžka radu A/B",   "Dĺžka radu A/B",                "počet pacientov", s => s.DlzkaRadAB,                v => v);
            Add("Dĺžka radu B",     "Dĺžka radu B",                  "počet pacientov", s => s.DlzkaRadB,                 v => v);

            _onRepFinished = sim =>
            {
                if (IsDisposed || _pendingUpdate) return;

                // capture values on sim thread
                var updates = new (int Rep, double Avg)?[_entries.Count];
                for (int i = 0; i < _entries.Count; i++)
                {
                    var coll = _entries[i].GetColl(sim);
                    if (coll.ValueCounter == 0) continue;
                    updates[i] = (coll.ValueCounter, _entries[i].Transform(coll.Average));
                }

                _pendingUpdate = true;
                try
                {
                    BeginInvoke(() =>
                    {
                        for (int i = 0; i < _entries.Count; i++)
                            if (updates[i].HasValue)
                                _entries[i].AppendPoint(updates[i]!.Value.Rep, updates[i]!.Value.Avg);
                        _pendingUpdate = false;
                    });
                }
                catch { _pendingUpdate = false; }
            };
        }

        public void AttachSim(MySimulation sim)
        {
            if (_currentSim != null)
                _currentSim.ReplicationFinished -= _onRepFinished;
            _currentSim = sim;
            foreach (var e in _entries) e.Reset();
            sim.ReplicationFinished += _onRepFinished;
        }
    }
}
