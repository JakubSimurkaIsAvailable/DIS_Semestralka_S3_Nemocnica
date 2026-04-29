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

        public StatisticsCollector LocDobaVV { get; private set; } = new();
        public StatisticsCollector LocDobaOsetrenie { get; private set; } = new();

        public StatisticsCollector DobaVV { get; } = new();
        public StatisticsCollector DobaOsetrenie { get; } = new();

        // ── Animation state (sim thread only) ─────────────────────────
        private readonly Dictionary<int, AnimShapeItem> _animPacienti = new();
        private AnimShapeItem[] _animSestry = [];
        private AnimShapeItem[] _animLekari = [];

        private readonly Queue<int> _animVolneSlotB  = new();
        private readonly Queue<int> _animVolneSlotA  = new();
        private readonly Queue<int> _animVolnaSestra = new();
        private readonly Queue<int> _animVolnyLekar  = new();

        private readonly Dictionary<int, (bool IsA, int Slot)>   _animPacientMiestnost = new();
        private readonly Dictionary<int, (int Sestra, int Lekar)> _animPacientStaff    = new();

        private readonly Queue<int>              _animVolneSlotVV  = new();
        private readonly Dictionary<int, int>    _animPacientVVSlot = new();
        private int _animVVSlotMax = 0;

        private readonly Queue<int>              _animVolneSlotOse  = new();
        private readonly Dictionary<int, int>    _animPacientOseSlot = new();
        private int _animOseSlotMax = 0;

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

        public MySimulation()
        {
            Init();
            OnReplicationDidFinish(_ =>
            {
                if (LocDobaVV.ValueCounter > 0)        DobaVV.AddValue(LocDobaVV.Average);
                if (LocDobaOsetrenie.ValueCounter > 0) DobaOsetrenie.AddValue(LocDobaOsetrenie.Average);
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
            LocDobaVV = new StatisticsCollector();
            LocDobaOsetrenie = new StatisticsCollector();
            AnimPriprav();
        }

        // ── Animation helpers ──────────────────────────────────────────

        private void AnimPriprav()
        {
            var anim = AnimCore;
            if (anim == null) return;

            anim.PrepareReplication(); // resets InternalSimTime → 0

            var oldPatients = _animPacienti.Values.ToList();
            if (oldPatients.Count > 0)
                anim.MyCanvas.Dispatcher.Invoke(() =>
                {
                    foreach (var item in oldPatients)
                        try { anim.Remove(item); } catch { }
                });
            _animPacienti.Clear();
            _animPacientMiestnost.Clear();
            _animPacientStaff.Clear();

            _animVolneSlotB.Clear();
            for (int i = 0; i < KonfMiestnostiB; i++) _animVolneSlotB.Enqueue(i);
            _animVolneSlotA.Clear();
            for (int i = 0; i < KonfMiestnostiA; i++) _animVolneSlotA.Enqueue(i);

            _animVolneSlotVV.Clear();  _animPacientVVSlot.Clear();  _animVVSlotMax = 0;
            _animVolneSlotOse.Clear(); _animPacientOseSlot.Clear(); _animOseSlotMax = 0;

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
            _animVolnaSestra.Clear();
            for (int i = 0; i < KonfSestry; i++)
                _animVolnaSestra.Enqueue(i);
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
            _animVolnyLekar.Clear();
            for (int i = 0; i < KonfLekari; i++)
                _animVolnyLekar.Enqueue(i);
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
        public void AnimAllocVVRoom(int pacientId)
        {
            if (_animPacientVVSlot.TryGetValue(pacientId, out var qslot))
            {
                _animVolneSlotVV.Enqueue(qslot);
                _animPacientVVSlot.Remove(pacientId);
            }
            if (_animVolneSlotB.Count == 0) return;
            int slot = _animVolneSlotB.Dequeue();
            _animPacientMiestnost[pacientId] = (false, slot);
            var (px, py) = SimAnim.RoomPatientPos(false, slot, KonfMiestnostiA, KonfMiestnostiB);
            if (_animPacienti.TryGetValue(pacientId, out var item))
                item.SetPosition(CurrentTime, px, py);
            int si = _animVolnaSestra.Count > 0 ? _animVolnaSestra.Dequeue() : 0;
            _animPacientStaff[pacientId] = (si, -1);
        }

        // Nurse walks to VV room over 'trvanie' sim-seconds
        public void AnimSestryPohybDoMiestnosti(int pacientId, double trvanie)
        {
            if (!_animPacientMiestnost.TryGetValue(pacientId, out var room)) return;
            if (!_animPacientStaff.TryGetValue(pacientId, out var staff) || staff.Sestra < 0) return;
            if (staff.Sestra >= _animSestry.Length) return;
            var (sx, sy) = SimAnim.RoomStaffPos(room.IsA, room.Slot, 0, KonfMiestnostiA, KonfMiestnostiB);
            _animSestry[staff.Sestra].MoveTo(CurrentTime, trvanie, sx, sy);
        }

        // Nurse and doctor walk to osetrenie room; each has its own duration
        public void AnimStaffPohybDoMiestnosti(int pacientId, double trvanieSestra, double trvanieLekar)
        {
            if (!_animPacientMiestnost.TryGetValue(pacientId, out var room)) return;
            if (!_animPacientStaff.TryGetValue(pacientId, out var staff)) return;
            if (staff.Sestra >= 0 && staff.Sestra < _animSestry.Length)
            {
                var (sx, sy) = SimAnim.RoomStaffPos(room.IsA, room.Slot, 0, KonfMiestnostiA, KonfMiestnostiB);
                _animSestry[staff.Sestra].MoveTo(CurrentTime, trvanieSestra, sx, sy);
            }
            if (staff.Lekar >= 0 && staff.Lekar < _animLekari.Length)
            {
                var (lx, ly) = SimAnim.RoomStaffPos(room.IsA, room.Slot, 1, KonfMiestnostiA, KonfMiestnostiB);
                _animLekari[staff.Lekar].MoveTo(CurrentTime, trvanieLekar, lx, ly);
            }
        }

        // VV done: release room + nurse slot, patient goes to osetrenie queue
        public void AnimUvolniVV(int pacientId)
        {
            if (_animPacientMiestnost.TryGetValue(pacientId, out var room))
            {
                _animVolneSlotB.Enqueue(room.Slot);
                _animPacientMiestnost.Remove(pacientId);
            }
            if (_animPacientStaff.TryGetValue(pacientId, out var staff))
            {
                if (staff.Sestra >= 0 && staff.Sestra < _animSestry.Length)
                    _animVolnaSestra.Enqueue(staff.Sestra);  // return slot; nurse stays where she is
                _animPacientStaff.Remove(pacientId);
            }
            // Patient moves to osetrenie queue
            int slot = _animVolneSlotOse.Count > 0 ? _animVolneSlotOse.Dequeue() : _animOseSlotMax++;
            _animPacientOseSlot[pacientId] = slot;
            var (ox, oy) = SimAnim.QueuePos(SimAnim.OseRad, slot);
            if (_animPacienti.TryGetValue(pacientId, out var item))
                item.SetPosition(CurrentTime, ox, oy);
        }

        // Osetrenie resources allocated: patient moves to room, nurse+doctor slots reserved
        public void AnimAllocOsetrenieRoom(int pacientId, bool isA)
        {
            if (_animPacientOseSlot.TryGetValue(pacientId, out var qslot))
            {
                _animVolneSlotOse.Enqueue(qslot);
                _animPacientOseSlot.Remove(pacientId);
            }
            var pool = isA ? _animVolneSlotA : _animVolneSlotB;
            if (pool.Count == 0) return;
            int slot = pool.Dequeue();
            _animPacientMiestnost[pacientId] = (isA, slot);
            var (px, py) = SimAnim.RoomPatientPos(isA, slot, KonfMiestnostiA, KonfMiestnostiB);
            if (_animPacienti.TryGetValue(pacientId, out var item))
                item.SetPosition(CurrentTime, px, py);
            int si = _animVolnaSestra.Count > 0 ? _animVolnaSestra.Dequeue() : 0;
            int li = _animVolnyLekar.Count > 0 ? _animVolnyLekar.Dequeue() : 0;
            _animPacientStaff[pacientId] = (si, li);
        }

        // Osetrenie done: release room + staff slots; staff stays in the room
        public void AnimUvolniOsetrenie(int pacientId)
        {
            if (_animPacientMiestnost.TryGetValue(pacientId, out var room))
            {
                if (room.IsA) _animVolneSlotA.Enqueue(room.Slot);
                else          _animVolneSlotB.Enqueue(room.Slot);
                _animPacientMiestnost.Remove(pacientId);
            }
            if (_animPacientStaff.TryGetValue(pacientId, out var staff))
            {
                if (staff.Sestra >= 0 && staff.Sestra < _animSestry.Length)
                    _animVolnaSestra.Enqueue(staff.Sestra);  // return slot; nurse stays where she is
                if (staff.Lekar >= 0 && staff.Lekar < _animLekari.Length)
                    _animVolnyLekar.Enqueue(staff.Lekar);    // return slot; doctor stays where he is
                _animPacientStaff.Remove(pacientId);
            }
        }

        // Patient left the system
        public void AnimPacientOdsiel(int id)
        {
            var anim = AnimCore;
            if (anim == null) return;
            if (!_animPacienti.TryGetValue(id, out var item)) return;
            anim.MyCanvas.Dispatcher.Invoke(() => anim.Remove(item));
            _animPacienti.Remove(id);
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
