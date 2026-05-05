using Agents.AgentOkolia;
using OSPABA;
using Agents.AgentModelu;
using Agents.AgentUrgentu;
using Agents.AgentPresunov;
using Agents.AgentVstupnehoVysetrenia;
using Agents.AgentZdrojov;
using Agents.AgentOsetrenia;
using DIS_Semestralka_S3_Nemocnica.Collectors;
using System.Collections.Concurrent;
using OSPAnimator;
using System.Windows.Media;
using Simulation.Resources;

namespace Simulation
{
    public class MySimulation : OSPABA.Simulation
    {
        public Random SeedRandom { get; private set; } = new Random();

        public ConcurrentDictionary<int, PacientInfo> Pacienti { get; } = new();
        public volatile bool Zastavit;
        public volatile int PocetVybavenych;

        public int GuiInterval { get; set; } = 60;
        public int GuiDurationMs { get; set; } = 0;

        public int KonfSestry { get; set; } = 3;
        public int KonfLekari { get; set; } = 2;
        public int KonfMiestnostiA { get; set; } = 5;
        public int KonfMiestnostiB { get; set; } = 7;
        public double KonfZahrievanie { get; set; } = 0;
        public bool WarmupSkoncilo { get; set; } = false;

        // ── Aggregate stats (across replications) ─────────────────────
        public StatisticsCollector PocetPacienti  { get; } = new(true);
        public StatisticsCollector PocetPeso      { get; } = new(true);
        public StatisticsCollector PocetSanitka   { get; } = new(true);
        public StatisticsCollector DobaVV              { get; } = new(true);
        public StatisticsCollector DobaOsetrenie       { get; } = new(true);
        public StatisticsCollector DobaVSysteme        { get; } = new(true);
        public StatisticsCollector DobaVSystemePeso    { get; } = new(true);
        public StatisticsCollector DobaVSystemeSanitka { get; } = new(true);
        public StatisticsCollector DobaVVPeso          { get; } = new(true);
        public StatisticsCollector DobaVVSanitka       { get; } = new(true);
        public StatisticsCollector DobaOsetrenieA      { get; } = new(true);
        public StatisticsCollector DobaOsetrenieAB     { get; } = new(true);
        public StatisticsCollector DobaOsetrenieB      { get; } = new(true);
        public StatisticsCollector DobaPrichodDoOsetrenia       { get; } = new(true);
        public StatisticsCollector DobaPrichodDoOsetreniaPeso    { get; } = new(true);
        public StatisticsCollector DobaPrichodDoOsetreniaSanitka { get; } = new(true);
        public StatisticsCollector VytazenostLekari       { get; } = new(true);
        public StatisticsCollector VytazenostSestry       { get; } = new(true);
        public StatisticsCollector VytazenostMiestnostiA  { get; } = new(true);
        public StatisticsCollector VytazenostMiestnostiB  { get; } = new(true);

        // ── Animation state (sim thread only) ─────────────────────────
        private readonly Dictionary<int, AnimShapeItem> _animPacienti = new();
        private AnimShapeItem[] _animSestry = [];
        private AnimShapeItem[] _animLekari = [];


        private readonly Dictionary<int, (bool IsA, int Slot)>   _animPacientMiestnost = new();
        private readonly Dictionary<int, (int Sestra, int Lekar)> _animPacientStaff    = new();

        private readonly Queue<int>              _animVolneSlotVV  = new();
        private readonly Dictionary<int, int>    _animPacientVVSlot = new();
        private int _animVVSlotMax = 0;

        private readonly Queue<int>              _animVolneSlotOse   = new();
        private readonly Dictionary<int, int>    _animPacientOseSlot = new();

        private readonly Dictionary<int, (bool IsA, int Slot)> _sestraRoom  = new();
        private readonly Dictionary<int, (bool IsA, int Slot)> _lekarRoom   = new();
        private readonly Dictionary<int, (bool IsA, int Slot)> _pacientRoom = new();

        private List<AnimShapeItem> _animMiestnosti = new();

        private Animator? AnimCore => Animator as Animator;

        // ──────────────────────────────────────────────────────────────

        public void NastavSeed(int seed) => SeedRandom = new Random(seed);
        public void NastavNahodny() => SeedRandom = new Random();

        public void AktualizujStavPacienta(int id, string stav)
        {
            if (Pacienti.TryGetValue(id, out var info))
                info.Stav = stav;
        }

        public void AktualizujPriorituPacienta(int id, int priorita)
        {
            if (Pacienti.TryGetValue(id, out var info))
                info.Priorita = priorita;
        }

        public void AktualizujMiestnostPacienta(int id, bool pouzilaMiestnostA)
        {
            if (Pacienti.TryGetValue(id, out var info))
                info.PouzilaMiestnostA = pouzilaMiestnostA;
        }

        // C# events — use += so multiple subscribers can coexist.
        // OSPABA's On* callbacks use = (replaces), so we bridge through these events.
        public event Action<MySimulation>? ReplicationFinished;
        public event Action<MySimulation>? ReplicationDidStart;
        public event Action<MySimulation>? GuiTick;

        public MySimulation()
        {
            Init();
            OnRefreshUI(_ => GuiTick?.Invoke(this));
            // Single OSPABA registration: collect stats first, then notify subscribers.
            OnReplicationDidFinish(_ =>
            {
                // Finalize weighted utilization at simulation end time
                var z = AgentZdrojov;
                double t = CurrentTime;
                if (z.TotalLekari > 0)
                    z.LocVytazenostLekari.AddWeightedValue((double)(z.TotalLekari - z.VolneLekari) / z.TotalLekari, t);
                if (z.TotalSestry > 0)
                    z.LocVytazenostSestry.AddWeightedValue((double)(z.TotalSestry - z.VolneSestry) / z.TotalSestry, t);
                if (z.TotalMiestnostiA > 0)
                    z.LocVytazenostMiestnostiA.AddWeightedValue((double)(z.TotalMiestnostiA - z.VolneMiestnostiA) / z.TotalMiestnostiA, t);
                if (z.TotalMiestnostiB > 0)
                    z.LocVytazenostMiestnostiB.AddWeightedValue((double)(z.TotalMiestnostiB - z.VolneMiestnostiB) / z.TotalMiestnostiB, t);

                // Zbieranie priemerov replikácie do agregovaných kolektorov
                var o = AgentOkolia;
                PocetPacienti.AddValue(o.LocPocetPacienti);
                PocetPeso.AddValue(o.LocPocetPeso);
                PocetSanitka.AddValue(o.LocPocetSanitka);
                if (o.LocDobaVSysteme.ValueCounter > 0)        DobaVSysteme.AddValue(o.LocDobaVSysteme.Average);
                if (o.LocDobaVSystemePeso.ValueCounter > 0)    DobaVSystemePeso.AddValue(o.LocDobaVSystemePeso.Average);
                if (o.LocDobaVSystemeSanitka.ValueCounter > 0) DobaVSystemeSanitka.AddValue(o.LocDobaVSystemeSanitka.Average);
                if (z.LocDobaVV.ValueCounter > 0)              DobaVV.AddValue(z.LocDobaVV.Average);
                if (z.LocDobaVVPeso.ValueCounter > 0)          DobaVVPeso.AddValue(z.LocDobaVVPeso.Average);
                if (z.LocDobaVVSanitka.ValueCounter > 0)       DobaVVSanitka.AddValue(z.LocDobaVVSanitka.Average);
                if (z.LocDobaOsetrenie.ValueCounter > 0)       DobaOsetrenie.AddValue(z.LocDobaOsetrenie.Average);
                if (z.LocDobaOsetrenieA.ValueCounter > 0)      DobaOsetrenieA.AddValue(z.LocDobaOsetrenieA.Average);
                if (z.LocDobaOsetrenieAB.ValueCounter > 0)     DobaOsetrenieAB.AddValue(z.LocDobaOsetrenieAB.Average);
                if (z.LocDobaOsetrenieB.ValueCounter > 0)      DobaOsetrenieB.AddValue(z.LocDobaOsetrenieB.Average);
                if (z.LocDobaPrichodDoOsetrenia.ValueCounter > 0)        DobaPrichodDoOsetrenia.AddValue(z.LocDobaPrichodDoOsetrenia.Average);
                if (z.LocDobaPrichodDoOsetreniaPeso.ValueCounter > 0)    DobaPrichodDoOsetreniaPeso.AddValue(z.LocDobaPrichodDoOsetreniaPeso.Average);
                if (z.LocDobaPrichodDoOsetreniaSanitka.ValueCounter > 0) DobaPrichodDoOsetreniaSanitka.AddValue(z.LocDobaPrichodDoOsetreniaSanitka.Average);
                VytazenostLekari.AddValue(z.LocVytazenostLekari.WeightedAverage);
                VytazenostSestry.AddValue(z.LocVytazenostSestry.WeightedAverage);
                VytazenostMiestnostiA.AddValue(z.LocVytazenostMiestnostiA.WeightedAverage);
                VytazenostMiestnostiB.AddValue(z.LocVytazenostMiestnostiB.WeightedAverage);

                ReplicationFinished?.Invoke(this);
            });
        }

        override public void PrepareReplication()
        {
            base.PrepareReplication();
            SetSimSpeed(GuiInterval, GuiDurationMs > 0 ? GuiDurationMs / 1000.0 : 0.001);
            if (Zastavit)
            {
                StopSimulation();
                return;
            }
            Pacienti.Clear();
            PocetVybavenych = 0;
            WarmupSkoncilo = false;
            AnimPriprav();
            // Re-register OnRefreshUI here in case base.PrepareReplication() resets it
            OnRefreshUI(_ => GuiTick?.Invoke(this));
            ReplicationDidStart?.Invoke(this);
        }

        public void ResetLocalStatsAtWarmupEnd()
        {
            AgentOkolia.ResetLocalStats();
            AgentZdrojov.ResetLocalStatsAtWarmupEnd(CurrentTime);
            WarmupSkoncilo = true;
        }

        // ── Animation helpers ──────────────────────────────────────────

        private void AnimPriprav()
        {
            var anim = AnimCore;
            if (anim == null) return;

            anim.PrepareReplication(); // resets InternalSimTime → 0

            var oldPatients = _animPacienti.Values.ToList();
            var oldMiestnosti = _animMiestnosti.ToList();
            anim.MyCanvas.Dispatcher.Invoke(() =>
            {
                foreach (var item in oldPatients)
                    try { anim.Remove(item); } catch { }
                foreach (var item in oldMiestnosti)
                    try { anim.Remove(item); } catch { }
            });
            _animPacienti.Clear();
            _animPacientMiestnost.Clear();
            _animPacientStaff.Clear();
            _animMiestnosti = new List<AnimShapeItem>();

            _animVolneSlotVV.Clear();  _animPacientVVSlot.Clear();  _animVVSlotMax = 0;
            _animVolneSlotOse.Clear(); _animPacientOseSlot.Clear();
            _sestraRoom.Clear();
            _lekarRoom.Clear();
            _pacientRoom.Clear();

            AnimInitMiestnosti(anim);
            AnimInitSestry(anim);
            AnimInitLekari(anim);
        }

        private void AnimInitSestry(Animator anim)
        {
            bool needCreate = _animSestry.Length != KonfSestry;
            var old    = needCreate ? _animSestry : Array.Empty<AnimShapeItem>();
            var target = needCreate ? new AnimShapeItem[KonfSestry] : _animSestry;
            anim.MyCanvas.Dispatcher.Invoke(() =>
            {
                if (needCreate)
                {
                    foreach (var s in old)
                        try { anim.Remove(s); } catch { }
                    for (int i = 0; i < KonfSestry; i++)
                    {
                        target[i] = new AnimShapeItem(AnimShape.CIRCLE, Colors.MediumSeaGreen, 7);
                        target[i].Label = $"S{i + 1}";
                        anim.Register(target[i]);
                    }
                }
                for (int i = 0; i < KonfSestry; i++)
                {
                    var (x, y) = SimAnim.IdlePos(SimAnim.IdleSestra, i);
                    target[i].Clear();
                    target[i].SetPosition(0.0, x, y);
                }
            });
            if (needCreate) _animSestry = target;
        }

        private void AnimInitLekari(Animator anim)
        {
            bool needCreate = _animLekari.Length != KonfLekari;
            var old    = needCreate ? _animLekari : Array.Empty<AnimShapeItem>();
            var target = needCreate ? new AnimShapeItem[KonfLekari] : _animLekari;
            anim.MyCanvas.Dispatcher.Invoke(() =>
            {
                if (needCreate)
                {
                    foreach (var l in old)
                        try { anim.Remove(l); } catch { }
                    for (int i = 0; i < KonfLekari; i++)
                    {
                        target[i] = new AnimShapeItem(AnimShape.CIRCLE, Colors.Goldenrod, 7);
                        target[i].Label = $"L{i + 1}";
                        anim.Register(target[i]);
                    }
                }
                for (int i = 0; i < KonfLekari; i++)
                {
                    var (x, y) = SimAnim.IdlePos(SimAnim.IdleLekar, i);
                    target[i].Clear();
                    target[i].SetPosition(0.0, x, y);
                }
            });
            if (needCreate) _animLekari = target;
        }

        private void AnimInitMiestnosti(Animator anim)
        {
            const float roomH = 70f;
            const float pad   = 20f;

            anim.MyCanvas.Dispatcher.Invoke(() =>
            {
                void Pridaj(float cx, float cy, float w, float h, System.Windows.Media.Color color, string label)
                {
                    var item = new AnimShapeItem(AnimShape.RECTANGLE_EMPTY, color, h);
                    item.Width  = w;
                    item.Height = h;
                    item.Label  = label;
                    anim.Register(item);
                    item.SetPosition(0.0, cx, cy);
                    _animMiestnosti.Add(item);
                }

                // Ambulance rooms — A (CornflowerBlue) vs B (MediumAquamarine)
                void PridajMiestnosti(bool isA, int total)
                {
                    float step  = SimAnim.RoomStep(total);
                    float roomW = step * 0.88f;
                    var   color = isA ? Colors.CornflowerBlue : Colors.MediumAquamarine;
                    for (int i = 0; i < total; i++)
                    {
                        var (cx, cy) = SimAnim.RoomCenter(isA, i, KonfMiestnostiA, KonfMiestnostiB);
                        Pridaj(cx, cy, roomW, roomH, color, (isA ? "A" : "B") + (i + 1));
                    }
                }
                PridajMiestnosti(true,  KonfMiestnostiA);
                PridajMiestnosti(false, KonfMiestnostiB);

                // Patient entrance areas
                Pridaj(SimAnim.PesiVstup.X, SimAnim.PesiVstup.Y, 84f, 44f, Colors.DarkOrange, "Vstup (peší)");
                Pridaj(SimAnim.SanVstup.X,  SimAnim.SanVstup.Y,  84f, 44f, Colors.Crimson,    "Vstup (sanitka)");

                // Staff idle areas — sized by config
                (float W, float H) StaffSize(int n)
                {
                    int cols = Math.Min(n, 4);
                    int rows = (n + 3) / 4;
                    return (cols * 48f + pad, rows * 48f + pad);
                }
                (float CX, float CY) StaffCenter((float X, float Y) origin, int n)
                {
                    int cols = Math.Min(n, 4);
                    int rows = (n + 3) / 4;
                    return (origin.X + (cols - 1) * 24f,
                            origin.Y + (rows - 1) * 24f);
                }

                var (wS, hS) = StaffSize(KonfSestry);
                var (cxS, cyS) = StaffCenter(SimAnim.IdleSestra, KonfSestry);
                Pridaj(cxS, cyS, wS, hS, Colors.MediumSeaGreen, "Sestry");

                var (wL, hL) = StaffSize(KonfLekari);
                var (cxL, cyL) = StaffCenter(SimAnim.IdleLekar, KonfLekari);
                Pridaj(cxL, cyL, wL, hL, Colors.Goldenrod, "Lekári");
            });
        }

        // ── Public animation methods (called from managers/processes) ──

        public void AnimPacientPrisiel(int id, bool sanitka)
        {
            var anim = AnimCore;
            if (anim == null) return;
            var (x, y) = sanitka ? SimAnim.SanVstup : SimAnim.PesiVstup;
            var color = sanitka ? Colors.OrangeRed : Colors.SteelBlue;
            AnimShapeItem item = null!;
            anim.MyCanvas.Dispatcher.Invoke(() =>
            {
                item = new AnimShapeItem(AnimShape.CIRCLE, color, 9);
                item.Label = id.ToString();
                anim.Register(item);
            });
            item.SetPosition(CurrentTime, x, y);
            _animPacienti[id] = item;
        }

        // Called from ProcessPresunutiaPacienta before Hold — animates physical movement
        public void AnimPacientPohyb(int id, double trvanie, float tx, float ty)
        {
            if (!_animPacienti.TryGetValue(id, out var item)) return;
            item.MoveTo(CurrentTime, trvanie, tx, ty);
        }

        // Patient placed in VV queue (after arrival movement finishes)
        public void AnimPacientDoVVRadu(int id)
        {
            int slot = _animVolneSlotVV.Count > 0 ? _animVolneSlotVV.Dequeue() : _animVVSlotMax++;
            _animPacientVVSlot[id] = slot;
            var (x, y) = SimAnim.QueuePos(SimAnim.VVRad, slot);
            if (_animPacienti.TryGetValue(id, out var item))
                item.SetPosition(CurrentTime, x, y);
        }

        // VV resources allocated: patient moves to room, nurse slot reserved
        public void AnimAllocVVRoom(int pacientId, Sestra sestra, MiestnostB miestnost)
        {
            if (_animPacientVVSlot.TryGetValue(pacientId, out var qslot))
            {
                _animVolneSlotVV.Enqueue(qslot);
                _animPacientVVSlot.Remove(pacientId);
            }
            _animPacientMiestnost[pacientId] = (false, miestnost.Id);
            var (px, py) = SimAnim.RoomPatientPos(false, miestnost.Id, KonfMiestnostiA, KonfMiestnostiB);
            if (_animPacienti.TryGetValue(pacientId, out var item))
                item.SetPosition(CurrentTime, px, py);
            _animPacientStaff[pacientId] = (sestra.Id, -1);
        }

        // Nurse walks to VV room over 'trvanie' sim-seconds (0 = already there)
        public void AnimSestryPohybDoMiestnosti(int pacientId, double trvanie)
        {
            if (!_animPacientMiestnost.TryGetValue(pacientId, out var room)) return;
            if (!_animPacientStaff.TryGetValue(pacientId, out var staff) || staff.Sestra < 0) return;
            if (staff.Sestra >= _animSestry.Length) return;
            _sestraRoom[staff.Sestra] = (room.IsA, room.Slot);
            if (trvanie <= 0) return;
            var (sx, sy) = SimAnim.RoomStaffPos(room.IsA, room.Slot, 0, KonfMiestnostiA, KonfMiestnostiB);
            _animSestry[staff.Sestra].MoveTo(CurrentTime, trvanie, sx, sy);
        }

        // Nurse and doctor walk to osetrenie room; each has its own duration (0 = already there)
        public void AnimStaffPohybDoMiestnosti(int pacientId, double trvanieSestra, double trvanieLekar)
        {
            if (!_animPacientMiestnost.TryGetValue(pacientId, out var room)) return;
            if (!_animPacientStaff.TryGetValue(pacientId, out var staff)) return;
            if (staff.Sestra >= 0 && staff.Sestra < _animSestry.Length)
            {
                _sestraRoom[staff.Sestra] = (room.IsA, room.Slot);
                if (trvanieSestra > 0)
                {
                    var (sx, sy) = SimAnim.RoomStaffPos(room.IsA, room.Slot, 0, KonfMiestnostiA, KonfMiestnostiB);
                    _animSestry[staff.Sestra].MoveTo(CurrentTime, trvanieSestra, sx, sy);
                }
            }
            if (staff.Lekar >= 0 && staff.Lekar < _animLekari.Length)
            {
                _lekarRoom[staff.Lekar] = (room.IsA, room.Slot);
                if (trvanieLekar > 0)
                {
                    var (lx, ly) = SimAnim.RoomStaffPos(room.IsA, room.Slot, 1, KonfMiestnostiA, KonfMiestnostiB);
                    _animLekari[staff.Lekar].MoveTo(CurrentTime, trvanieLekar, lx, ly);
                }
            }
        }

        // VV done: release room + nurse slot; patient waits in front of their VV room
        public void AnimUvolniVV(int pacientId)
        {
            if (_animPacientMiestnost.TryGetValue(pacientId, out var room))
            {
                _animPacientMiestnost.Remove(pacientId);
                if (_animPacienti.TryGetValue(pacientId, out var item))
                {
                    var (cx, cy) = SimAnim.RoomCenter(false, room.Slot, KonfMiestnostiA, KonfMiestnostiB);
                    item.SetPosition(CurrentTime, cx, cy + 50f);
                }
            }
            _animPacientStaff.Remove(pacientId);
        }

        // Osetrenie resources allocated: nurse+doctor slots reserved; patient walks separately via AnimPacientPohybDoOsetrenia
        public void AnimAllocOsetrenieRoom(int pacientId, Miestnost miestnost, Sestra sestra, Lekar lekar)
        {
            if (_animPacientOseSlot.TryGetValue(pacientId, out var qslot))
            {
                _animVolneSlotOse.Enqueue(qslot);
                _animPacientOseSlot.Remove(pacientId);
            }
            _animPacientMiestnost[pacientId] = (miestnost.IsA, miestnost.Id);
            _animPacientStaff[pacientId] = (sestra.Id, lekar.Id);
        }

        // Patient walks to osetrenie room over 'trvanie' sim-seconds (0 = already there)
        public void AnimPacientPohybDoOsetrenia(int pacientId, double trvanie)
        {
            if (!_animPacienti.TryGetValue(pacientId, out var item)) return;
            if (!_animPacientMiestnost.TryGetValue(pacientId, out var room)) return;
            _pacientRoom[pacientId] = (room.IsA, room.Slot);
            if (trvanie <= 0) return;
            var (px, py) = SimAnim.RoomPatientPos(room.IsA, room.Slot, KonfMiestnostiA, KonfMiestnostiB);
            item.MoveTo(CurrentTime, trvanie, px, py);
        }

        public bool SestraJeUzVMiestnosti(Sestra sestra, Miestnost miestnost)
            => _sestraRoom.TryGetValue(sestra.Id, out var r) && r.IsA == miestnost.IsA && r.Slot == miestnost.Id;

        public bool LekarJeUzVMiestnosti(Lekar lekar, Miestnost miestnost)
            => _lekarRoom.TryGetValue(lekar.Id, out var r) && r.IsA == miestnost.IsA && r.Slot == miestnost.Id;

        public bool PacientJeUzVMiestnosti(int pacientId, Miestnost miestnost)
            => _pacientRoom.TryGetValue(pacientId, out var r) && r.IsA == miestnost.IsA && r.Slot == miestnost.Id;

        // Osetrenie done: release room + staff slots; patient waits in front of their ambulance
        public void AnimUvolniOsetrenie(int pacientId)
        {
            if (_animPacientMiestnost.TryGetValue(pacientId, out var room))
            {
                _animPacientMiestnost.Remove(pacientId);
                _pacientRoom[pacientId] = (room.IsA, room.Slot);
                if (_animPacienti.TryGetValue(pacientId, out var item))
                {
                    var (cx, cy) = SimAnim.RoomCenter(room.IsA, room.Slot, KonfMiestnostiA, KonfMiestnostiB);
                    item.SetPosition(CurrentTime, cx, cy + 50f);
                }
            }
            _animPacientStaff.Remove(pacientId);
        }

        public bool SestraJeVAmbulancii(Sestra sestra)
            => _sestraRoom.ContainsKey(sestra.Id);

        public bool LekarJeVAmbulancii(Lekar lekar)
            => _lekarRoom.ContainsKey(lekar.Id);

        // Patient left the system
        public void AnimPacientOdsiel(int id)
        {
            var anim = AnimCore;
            if (anim == null) return;
            if (!_animPacienti.TryGetValue(id, out var item)) return;
            anim.MyCanvas.Dispatcher.Invoke(() => anim.Remove(item));
            _animPacienti.Remove(id);
            _pacientRoom.Remove(id);
        }

        //meta! userInfo="Generated code: do not modify", tag="begin"
        private void Init()
        {
            AgentModelu = new AgentModelu(SimId.AgentModelu, this, null);
            AgentOkolia = new AgentOkolia(SimId.AgentOkolia, this, AgentModelu);
            AgentUrgentu = new AgentUrgentu(SimId.AgentUrgentu, this, AgentModelu);
            AgentPresunov = new AgentPresunov(SimId.AgentPresunov, this, AgentUrgentu);
            AgentVstupnehoVysetrenia = new AgentVstupnehoVysetrenia(SimId.AgentVstupnehoVysetrenia, this, AgentUrgentu);
            AgentOsetrenia = new AgentOsetrenia(SimId.AgentOsetrenia, this, AgentUrgentu);
            AgentZdrojov = new AgentZdrojov(SimId.AgentZdrojov, this, AgentUrgentu);
        }
        public AgentModelu AgentModelu
        { get; set; }
        public AgentOkolia AgentOkolia
        { get; set; }
        public AgentUrgentu AgentUrgentu
        { get; set; }
        public AgentPresunov AgentPresunov
        { get; set; }
        public AgentVstupnehoVysetrenia AgentVstupnehoVysetrenia
        { get; set; }
        public AgentOsetrenia AgentOsetrenia
        { get; set; }
        public AgentZdrojov AgentZdrojov
        { get; set; }
        //meta! tag="end"
    }
}
