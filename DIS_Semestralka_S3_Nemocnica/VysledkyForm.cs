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
        private record PlotEntry(FormsPlot Fp, Func<MySimulation, StatisticsCollector> GetColl, Func<double, double> Transform);

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

            Add("Čas – celkovo",    "Čas v systéme – celkovo",        "minúty",      s => s.DobaVSysteme,          v => v / 60.0);
            Add("Čas – pešo",       "Čas v systéme – pešo",           "minúty",      s => s.DobaVSystemePeso,      v => v / 60.0);
            Add("Čas – sanitkou",   "Čas v systéme – sanitkou",       "minúty",      s => s.DobaVSystemeSanitka,   v => v / 60.0);
            Add("VV – celkovo",     "Čakanie na VV – celkovo",        "minúty",      s => s.DobaVV,                v => v / 60.0);
            Add("VV – pešo",        "Čakanie na VV – pešo",           "minúty",      s => s.DobaVVPeso,            v => v / 60.0);
            Add("VV – sanitkou",    "Čakanie na VV – sanitkou",       "minúty",      s => s.DobaVVSanitka,         v => v / 60.0);
            Add("Ošetr. – celkovo", "Čakanie na ošetrenie – celkovo", "minúty",      s => s.DobaOsetrenie,         v => v / 60.0);
            Add("Ošetr. – Rad A",   "Čakanie na ošetrenie – Rad A",   "minúty",      s => s.DobaOsetrenieA,        v => v / 60.0);
            Add("Ošetr. – Rad A/B", "Čakanie na ošetrenie – Rad A/B", "minúty",      s => s.DobaOsetrenieAB,       v => v / 60.0);
            Add("Ošetr. – Rad B",   "Čakanie na ošetrenie – Rad B",   "minúty",      s => s.DobaOsetrenieB,           v => v / 60.0);
            Add("Príchod → ošetr.", "Čas od príchodu do začiatku ošetrenia – celkovo",  "minúty", s => s.DobaPrichodDoOsetrenia,         v => v / 60.0);
            Add("Príchod → ošetr. (pešo)",    "Čas od príchodu do začiatku ošetrenia – pešo",    "minúty", s => s.DobaPrichodDoOsetreniaPeso,    v => v / 60.0);
            Add("Príchod → ošetr. (sanitka)", "Čas od príchodu do začiatku ošetrenia – sanitkou","minúty", s => s.DobaPrichodDoOsetreniaSanitka, v => v / 60.0);
            Add("Lekári",           "Vyťaženie lekárov",              "percent (%)", s => s.VytazenostLekari,         v => v * 100.0);
            Add("Sestry",           "Vyťaženie sestier",              "percent (%)", s => s.VytazenostSestry,      v => v * 100.0);
            Add("Miestnosti A",     "Vyťaženie miestností A",         "percent (%)", s => s.VytazenostMiestnostiA, v => v * 100.0);
            Add("Miestnosti B",     "Vyťaženie miestností B",         "percent (%)", s => s.VytazenostMiestnostiB, v => v * 100.0);

            _onRepFinished = _ =>
            {
                if (IsDisposed || _pendingUpdate) return;
                _pendingUpdate = true;
                try { BeginInvoke(() => { RedrawAll(); _pendingUpdate = false; }); }
                catch { _pendingUpdate = false; }
            };

            Shown += (_, _) => RedrawAll();
            tabControl.SelectedIndexChanged += (_, _) =>
            {
                int i = tabControl.SelectedIndex;
                if (i >= 0 && i < _entries.Count) Redraw(_entries[i], _currentSim);
            };
        }

        public void AttachSim(MySimulation sim)
        {
            if (_currentSim != null)
                _currentSim.ReplicationFinished -= _onRepFinished;
            _currentSim = sim;
            sim.ReplicationFinished += _onRepFinished;
            if (IsHandleCreated)
                try { BeginInvoke(RedrawAll); } catch { }
        }

        private void RedrawAll()
        {
            var sim = _currentSim;
            foreach (var e in _entries) Redraw(e, sim);
        }

        private static void Redraw(PlotEntry e, MySimulation? sim)
        {
            var plt = e.Fp.Plot;
            plt.Clear();

            if (sim == null)
            {
                plt.Add.Annotation("Čakajte na spustenie simulácie...");
                e.Fp.Refresh();
                return;
            }

            var coll = e.GetColl(sim);
            double[] vals = coll.GetValues();
            if (vals.Length == 0)
            {
                plt.Add.Annotation("Čakajte na dokončenie replikácií...");
                e.Fp.Refresh();
                return;
            }

            int n = vals.Length;
            double[] xs      = new double[n];
            double[] ys      = new double[n];
            double[] cumAvgs = new double[n];
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                xs[i]      = i + 1;
                ys[i]      = e.Transform(vals[i]);
                sum       += vals[i];
                cumAvgs[i] = e.Transform(sum / (i + 1));
            }

            var sc = plt.Add.Scatter(xs, ys);
            sc.Color      = ScottPlot.Color.FromHex("#90CAF9");
            sc.MarkerSize = 5;
            sc.LineWidth  = 0;
            sc.LegendText = "hodnota replikácie";

            var scAvg = plt.Add.Scatter(xs, cumAvgs);
            scAvg.Color      = ScottPlot.Color.FromHex("#1565C0");
            scAvg.MarkerSize = 0;
            scAvg.LineWidth  = 2f;
            scAvg.LegendText = "kumulatívny priemer";

            var ci = coll.GetConfidenceInterval();
            if (ci.HasValue)
            {
                double mean = e.Transform(coll.Average);
                double lo   = e.Transform(ci.Value.Lower);
                double hi   = e.Transform(ci.Value.Upper);

                var hMean = plt.Add.HorizontalLine(mean);
                hMean.Color      = ScottPlot.Color.FromHex("#E53935");
                hMean.LineWidth  = 2;
                hMean.LegendText = $"priemer: {mean:F2}";

                var hLo = plt.Add.HorizontalLine(lo);
                hLo.Color        = ScottPlot.Color.FromHex("#E53935").WithAlpha(0.5);
                hLo.LineWidth    = 1.5f;
                hLo.LinePattern  = ScottPlot.LinePattern.Dashed;
                hLo.LegendText   = $"95% CI: [{lo:F2}, {hi:F2}]";

                var hHi = plt.Add.HorizontalLine(hi);
                hHi.Color        = ScottPlot.Color.FromHex("#E53935").WithAlpha(0.5);
                hHi.LineWidth    = 1.5f;
                hHi.LinePattern  = ScottPlot.LinePattern.Dashed;
            }

            plt.ShowLegend();
            plt.Axes.AutoScale();
            e.Fp.Refresh();
        }
    }
}
