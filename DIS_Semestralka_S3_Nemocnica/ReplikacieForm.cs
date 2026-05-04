using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ScottPlot.WinForms;
using Simulation;
using DIS_Semestralka_S3_Nemocnica.Collectors;

namespace DIS_Semestralka_S3_Nemocnica
{
    public partial class ReplikacieForm : Form
    {
        private class PlotEntry
        {
            public required FormsPlot Fp;
            public required Func<MySimulation, double> Snapshot;
            public required Func<double, double> Transform;
            public readonly List<double> Times = new();
            public readonly List<double> Vals  = new();
            public readonly object Lock = new();
        }

        private readonly List<PlotEntry> _entries = new();
        private volatile bool _pendingUpdate;
        private MySimulation? _currentSim;
        private readonly Action<MySimulation> _onGuiTick;
        private readonly Action<MySimulation> _onRepStart;

        public ReplikacieForm(Func<bool> isMaxSpeed)
        {
            InitializeComponent();

            void Add(string tab, string title, string yUnit,
                     Func<MySimulation, double> snap, Func<double, double> t)
            {
                var fp = new FormsPlot { Dock = DockStyle.Fill };
                fp.Plot.Title(title);
                fp.Plot.XLabel("Čas simulácie [min]");
                fp.Plot.YLabel(yUnit);
                var tp = new TabPage(tab);
                tp.Controls.Add(fp);
                tabControl.TabPages.Add(tp);
                _entries.Add(new PlotEntry { Fp = fp, Snapshot = snap, Transform = t });
            }

            Add("Čas – celkovo",    "Čas v systéme – celkovo",        "minúty",
                s => s.AgentOkolia.LocDobaVSysteme.ValueCounter        > 0 ? s.AgentOkolia.LocDobaVSysteme.Average        : double.NaN, v => v / 60.0);
            Add("Čas – pešo",       "Čas v systéme – pešo",           "minúty",
                s => s.AgentOkolia.LocDobaVSystemePeso.ValueCounter    > 0 ? s.AgentOkolia.LocDobaVSystemePeso.Average    : double.NaN, v => v / 60.0);
            Add("Čas – sanitkou",   "Čas v systéme – sanitkou",       "minúty",
                s => s.AgentOkolia.LocDobaVSystemeSanitka.ValueCounter > 0 ? s.AgentOkolia.LocDobaVSystemeSanitka.Average : double.NaN, v => v / 60.0);
            Add("VV – celkovo",     "Čakanie na VV – celkovo",        "minúty",
                s => s.AgentZdrojov.LocDobaVV.ValueCounter        > 0 ? s.AgentZdrojov.LocDobaVV.Average        : double.NaN, v => v / 60.0);
            Add("VV – pešo",        "Čakanie na VV – pešo",           "minúty",
                s => s.AgentZdrojov.LocDobaVVPeso.ValueCounter    > 0 ? s.AgentZdrojov.LocDobaVVPeso.Average    : double.NaN, v => v / 60.0);
            Add("VV – sanitkou",    "Čakanie na VV – sanitkou",       "minúty",
                s => s.AgentZdrojov.LocDobaVVSanitka.ValueCounter > 0 ? s.AgentZdrojov.LocDobaVVSanitka.Average : double.NaN, v => v / 60.0);
            Add("Ošetr. – celkovo", "Čakanie na ošetrenie – celkovo", "minúty",
                s => s.AgentZdrojov.LocDobaOsetrenie.ValueCounter    > 0 ? s.AgentZdrojov.LocDobaOsetrenie.Average    : double.NaN, v => v / 60.0);
            Add("Ošetr. – Rad A",   "Čakanie na ošetrenie – Rad A",   "minúty",
                s => s.AgentZdrojov.LocDobaOsetrenieA.ValueCounter   > 0 ? s.AgentZdrojov.LocDobaOsetrenieA.Average   : double.NaN, v => v / 60.0);
            Add("Ošetr. – Rad A/B", "Čakanie na ošetrenie – Rad A/B", "minúty",
                s => s.AgentZdrojov.LocDobaOsetrenieAB.ValueCounter  > 0 ? s.AgentZdrojov.LocDobaOsetrenieAB.Average  : double.NaN, v => v / 60.0);
            Add("Ošetr. – Rad B",   "Čakanie na ošetrenie – Rad B",   "minúty",
                s => s.AgentZdrojov.LocDobaOsetrenieB.ValueCounter   > 0 ? s.AgentZdrojov.LocDobaOsetrenieB.Average   : double.NaN, v => v / 60.0);
            Add("Príchod → ošetr.", "Čas od príchodu do začiatku ošetrenia – celkovo", "minúty",
                s => s.AgentZdrojov.LocDobaPrichodDoOsetrenia.ValueCounter        > 0 ? s.AgentZdrojov.LocDobaPrichodDoOsetrenia.Average        : double.NaN, v => v / 60.0);
            Add("Príchod → ošetr. (pešo)", "Čas od príchodu do začiatku ošetrenia – pešo", "minúty",
                s => s.AgentZdrojov.LocDobaPrichodDoOsetreniaPeso.ValueCounter    > 0 ? s.AgentZdrojov.LocDobaPrichodDoOsetreniaPeso.Average    : double.NaN, v => v / 60.0);
            Add("Príchod → ošetr. (sanitka)", "Čas od príchodu do začiatku ošetrenia – sanitkou", "minúty",
                s => s.AgentZdrojov.LocDobaPrichodDoOsetreniaSanitka.ValueCounter > 0 ? s.AgentZdrojov.LocDobaPrichodDoOsetreniaSanitka.Average : double.NaN, v => v / 60.0);
            Add("Lekári",           "Vyťaženie lekárov",              "percent (%)",
                s => s.AgentZdrojov.LocVytazenostLekari.TotalWeight      > 0 ? s.AgentZdrojov.LocVytazenostLekari.WeightedAverage      : double.NaN, v => v * 100.0);
            Add("Sestry",           "Vyťaženie sestier",              "percent (%)",
                s => s.AgentZdrojov.LocVytazenostSestry.TotalWeight      > 0 ? s.AgentZdrojov.LocVytazenostSestry.WeightedAverage      : double.NaN, v => v * 100.0);
            Add("Miestnosti A",     "Vyťaženie miestností A",         "percent (%)",
                s => s.AgentZdrojov.LocVytazenostMiestnostiA.TotalWeight > 0 ? s.AgentZdrojov.LocVytazenostMiestnostiA.WeightedAverage : double.NaN, v => v * 100.0);
            Add("Miestnosti B",     "Vyťaženie miestností B",         "percent (%)",
                s => s.AgentZdrojov.LocVytazenostMiestnostiB.TotalWeight > 0 ? s.AgentZdrojov.LocVytazenostMiestnostiB.WeightedAverage : double.NaN, v => v * 100.0);
            Add("Počty – celkovo",  "Počet pacientov – celkovo",      "počet",
                s => (double)s.AgentOkolia.LocPocetPacienti, v => v);
            Add("Počty – pešo",     "Počet pacientov – pešo",         "počet",
                s => (double)s.AgentOkolia.LocPocetPeso,     v => v);
            Add("Počty – sanitkou", "Počet pacientov – sanitkou",     "počet",
                s => (double)s.AgentOkolia.LocPocetSanitka,  v => v);

            _onGuiTick = s =>
            {
                if (IsDisposed || isMaxSpeed()) return;
                double t = s.CurrentTime / 60.0;
                foreach (var e in _entries)
                {
                    double v = e.Snapshot(s);
                    if (!double.IsNaN(v))
                        lock (e.Lock) { e.Times.Add(t); e.Vals.Add(v); }
                }
                if (_pendingUpdate) return;
                _pendingUpdate = true;
                try { BeginInvoke(() => { RedrawCurrent(); _pendingUpdate = false; }); }
                catch { _pendingUpdate = false; }
            };

            _onRepStart = _ =>
            {
                if (IsDisposed) return;
                foreach (var e in _entries)
                    lock (e.Lock) { e.Times.Clear(); e.Vals.Clear(); }
                if (!isMaxSpeed())
                    try { BeginInvoke(RedrawAll); } catch { }
            };

            Shown += (_, _) => RedrawAll();
            tabControl.SelectedIndexChanged += (_, _) =>
            {
                int i = tabControl.SelectedIndex;
                if (i >= 0 && i < _entries.Count) Redraw(_entries[i]);
            };
        }

        public void AttachSim(MySimulation sim)
        {
            if (_currentSim != null)
            {
                _currentSim.GuiTick -= _onGuiTick;
                _currentSim.ReplicationDidStart -= _onRepStart;
            }
            _currentSim = sim;
            foreach (var e in _entries)
                lock (e.Lock) { e.Times.Clear(); e.Vals.Clear(); }
            sim.GuiTick += _onGuiTick;
            sim.ReplicationDidStart += _onRepStart;
            if (IsHandleCreated)
                try { BeginInvoke(RedrawAll); } catch { }
        }

        private void RedrawAll()
        {
            foreach (var e in _entries) Redraw(e);
        }

        private void RedrawCurrent()
        {
            int i = tabControl.SelectedIndex;
            if (i >= 0 && i < _entries.Count) Redraw(_entries[i]);
        }

        private void Redraw(PlotEntry e)
        {
            double[] xs, ys;
            lock (e.Lock) { xs = e.Times.ToArray(); ys = e.Vals.ToArray(); }

            var plt = e.Fp.Plot;
            plt.Clear();

            if (xs.Length == 0)
            {
                string msg = _currentSim == null
                    ? "Čakajte na spustenie simulácie..."
                    : "Čakajte na dáta replikácie...";
                plt.Add.Annotation(msg);
                e.Fp.Refresh();
                return;
            }

            double[] transformed = Array.ConvertAll(ys, y => e.Transform(y));

            var sc = plt.Add.Scatter(xs, transformed);
            sc.Color      = ScottPlot.Color.FromHex("#2196F3");
            sc.MarkerSize = 5;
            sc.LineWidth  = 1.5f;
            sc.LegendText = "bežiaci priemer";

            plt.ShowLegend();
            plt.Axes.AutoScale();
            e.Fp.Refresh();
        }
    }
}
