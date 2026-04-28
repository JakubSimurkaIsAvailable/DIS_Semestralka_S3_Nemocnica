using Agents.AgentZdrojov;
using Simulation;
using System.Data;

namespace DIS_Semestralka_S3_Nemocnica
{
    public partial class Form1 : Form
    {
        // ── Animator helper types ─────────────────────────────────────
        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel() { DoubleBuffered = true; }
        }

        private sealed class AnimEntity
        {
            public float X, Y, TgtX, TgtY;
            public Color Color;
            public string Label = "";
        }

        private enum RoomActivity { Free, VV, Osetrenie }

        // ── Animator anchor positions (normalized 0..1) ───────────────
        private static readonly PointF AP_PesiVstup    = new(0.08f, 0.87f);
        private static readonly PointF AP_SanVstup    = new(0.90f, 0.08f);
        private static readonly PointF AP_IdleSestra  = new(0.04f, 0.06f);
        private static readonly PointF AP_IdleLekar   = new(0.04f, 0.17f);
        // Room bands: B rooms at y≈0.28, A rooms at y≈0.50
        private const float AP_BRoomsY = 0.28f;
        private const float AP_ARoomsY = 0.50f;
        private static readonly PointF AP_VVRad  = new(0.22f, 0.63f);
        private static readonly PointF AP_OseRad = new(0.65f, 0.63f);
        private static readonly PointF AP_Vystup = new(0.44f, 0.87f);

        private static readonly Color AC_Pesi    = Color.SteelBlue;
        private static readonly Color AC_Sanitka = Color.OrangeRed;
        private static readonly Color AC_Sestra  = Color.MediumSeaGreen;
        private static readonly Color AC_Lekar   = Color.Goldenrod;
        private static readonly Font  AF_Small   = new("Segoe UI", 6.5f);

        // ── Animator state ────────────────────────────────────────────
        private readonly Dictionary<int, AnimEntity> _pacientiAnim = new();
        private AnimEntity[] _sestryAnim = [];
        private AnimEntity[] _lekariAnim = [];
        private readonly System.Windows.Forms.Timer _animTimer = new() { Interval = 33 };
        // Room slot tracking: patientId → (isTypeA, slotIndex)
        private readonly Dictionary<int, (bool IsTypeA, int Idx)> _patientRooms = new();
        private RoomActivity[] _roomAActivity = [];
        private RoomActivity[] _roomBActivity = [];

        // ── Existing fields ───────────────────────────────────────────
        private MySimulation? _sim;
        private Thread? _simThread;
        private readonly DataTable _pacientTable = new();
        private long _lastRefreshTicks;
        private readonly ManualResetEventSlim _pauseEvent = new(true);

        public Form1()
        {
            InitializeComponent();
            InitPacientTable();
            _animTimer.Tick += AnimTimer_Tick;
            _animTimer.Start();
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

            _sim.KonfSestry     = (int)nudSestry.Value;
            _sim.KonfLekari     = (int)nudLekari.Value;
            _sim.KonfMiestnostiA = (int)nudMiestnostiA.Value;
            _sim.KonfMiestnostiB = (int)nudMiestnostiB.Value;

            _pauseEvent.Set(); // ensure not paused from a previous run
            _lastRefreshTicks = 0;
            _sim.OnRefreshUI(s =>
            {
                _pauseEvent.Wait(); // blocks here when paused; resumes on Set()
                long now = DateTime.UtcNow.Ticks;
                if (now - _lastRefreshTicks < 1_500_000) return; // throttle to ~6 fps
                _lastRefreshTicks = now;
                try { Invoke(RefreshUI); } catch { }
            });
            _sim.OnReplicationDidFinish(s => BeginInvoke(AktualizujStatus));

            int reps     = (int)nudReplikacie.Value;
            double duration = (double)nudTrvanie.Value * 3600.0;

            btnSpustit.Enabled = false;
            btnZastavit.Enabled = true;
            btnPauza.Enabled = true;
            btnPauza.Text = "⏸  Pauza";
            btnPauza.BackColor = System.Drawing.Color.LightYellow;

            // Apply slowdown from UI thread (reads UI controls safely here).
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
            _pauseEvent.Set();       // unblock if currently paused so interrupt can fire
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

        // Always called from UI thread — reads controls directly.
        private void AplukujSpomalenie()
        {
            if (_sim == null) return;
            int ms  = trkDuration.Value;
            int sec = trkInterval.Value;
            _sim.GuiDurationMs = ms;
            _sim.GuiInterval   = sec;
            // Always use SetSimSpeed (never SetMaxSimSpeed) so that OnRefreshUI fires
            // after every message — required for pause/resume to work mid-replication.
            // At ms==0, a 1ms sleep per interval is negligible but keeps the scheduler alive.
            _sim.SetSimSpeed(sec, ms == 0 ? 0.001 : ms / 1000.0);
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

        // ── UI refresh (called via Invoke from OSPABA's OnRefreshUI) ──

        private void RefreshUI()
        {
            if (_sim == null) return;
            lblSimCas.Text = $"Čas: {TimeSpan.FromSeconds(_sim.CurrentTime):hh\\:mm\\:ss}";
            lblReplikacia.Text = $"Replikácia: {_sim.CurrentReplication} / {_sim.ReplicationCount}";
            AktualizujPacientiTab();
            AktualizujZdrojeTab();
            AktualizujRadyTab();
            AktualizujAnimator();
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

            pbSestry.Maximum    = z.TotalSestry;
            pbLekari.Maximum    = z.TotalLekari;
            pbMiestnostiA.Maximum = z.TotalMiestnostiA;
            pbMiestnostiB.Maximum = z.TotalMiestnostiB;

            lblSestryVal.Text       = $"{volSestry} / {z.TotalSestry} voľných";
            lblLekariVal.Text       = $"{volLekari} / {z.TotalLekari} voľných";
            lblMiestnostiAVal.Text  = $"{volA} / {z.TotalMiestnostiA} voľných";
            lblMiestnostiBVal.Text  = $"{volB} / {z.TotalMiestnostiB} voľných";

            pbSestry.Value      = Math.Max(0, Math.Min(z.TotalSestry    - volSestry, pbSestry.Maximum));
            pbLekari.Value      = Math.Max(0, Math.Min(z.TotalLekari    - volLekari, pbLekari.Maximum));
            pbMiestnostiA.Value = Math.Max(0, Math.Min(z.TotalMiestnostiA - volA,   pbMiestnostiA.Maximum));
            pbMiestnostiB.Value = Math.Max(0, Math.Min(z.TotalMiestnostiB - volB,   pbMiestnostiB.Maximum));
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
            _animTimer.Stop();
            if (_sim != null) _sim.Zastavit = true;
            _simThread?.Interrupt();
            base.OnFormClosing(e);
        }

        // ── Animator logic ────────────────────────────────────────────

        private void AnimTimer_Tick(object? sender, EventArgs e)
        {
            foreach (var ent in _pacientiAnim.Values) ALerp(ent);
            foreach (var ent in _sestryAnim)          ALerp(ent);
            foreach (var ent in _lekariAnim)          ALerp(ent);
            pnlAnimator.Invalidate();
        }

        private static void ALerp(AnimEntity ent)
        {
            const float A = 0.12f;
            ent.X += (ent.TgtX - ent.X) * A;
            ent.Y += (ent.TgtY - ent.Y) * A;
        }

        private static bool AIsRoomState(string stav) =>
            stav is "Presun sestry" or "VV prebieha" or "Presun personálu" or "Ošetrenie prebieha";

        private void AktualizujAnimator()
        {
            if (_sim == null) return;
            var z = _sim.AgentZdrojov;

            // Sync room arrays with current config (resets on size change or new replication)
            if (_roomAActivity.Length != z.TotalMiestnostiA || _roomBActivity.Length != z.TotalMiestnostiB)
            {
                _roomAActivity = new RoomActivity[z.TotalMiestnostiA];
                _roomBActivity = new RoomActivity[z.TotalMiestnostiB];
                _patientRooms.Clear();
            }

            var alive = _sim.Pacienti.Keys.ToHashSet();

            // Free rooms for patients who left or changed out of ambulance state
            foreach (var id in _patientRooms.Keys
                .Where(k => !alive.Contains(k) || !AIsRoomState(_sim.Pacienti[k].Stav))
                .ToList())
            {
                var (isA, idx) = _patientRooms[id];
                (isA ? _roomAActivity : _roomBActivity)[idx] = RoomActivity.Free;
                _patientRooms.Remove(id);
            }

            // Remove departed patients from animation
            foreach (var id in _pacientiAnim.Keys.Where(k => !alive.Contains(k)).ToList())
                _pacientiAnim.Remove(id);

            // Update/create patient entities; assign specific rooms when in room-state
            var slotCounter = new Dictionary<string, int>();
            foreach (var (id, info) in _sim.Pacienti.OrderBy(x => x.Key))
            {
                float tx, ty;
                if (AIsRoomState(info.Stav))
                {
                    // Assign to a specific room slot if not yet done
                    if (!_patientRooms.ContainsKey(id))
                        ATryAssignRoom(id, info);

                    if (_patientRooms.TryGetValue(id, out var room))
                    {
                        (tx, ty) = ARoomPatientPos(room.IsTypeA, room.Idx);
                    }
                    else (tx, ty) = (0.5f, AP_BRoomsY); // fallback
                }
                else
                {
                    string area = info.Stav switch
                    {
                        "Príchod"           => "Vstup",
                        "Presun na VV"      => "VVRad",
                        "Čaká na VV"        => "VVRad",
                        "Čaká na ošetrenie" => "OseRad",
                        "Odchod"            => "Vystup",
                        _                   => "Vstup"
                    };
                    int slot = slotCounter.GetValueOrDefault(area);
                    slotCounter[area] = slot + 1;
                    (tx, ty) = area switch
                    {
                        "VVRad"  => AStagger(AP_VVRad,  slot, 4),
                        "OseRad" => AStagger(AP_OseRad, slot, 4),
                        "Vystup" => (AP_Vystup.X, AP_Vystup.Y),
                        _ => info.PrisielSanitkou
                            ? (AP_SanVstup.X, AP_SanVstup.Y)
                            : (AP_PesiVstup.X, AP_PesiVstup.Y),
                    };
                }

                if (_pacientiAnim.TryGetValue(id, out var ent))
                {
                    ent.TgtX = tx; ent.TgtY = ty;
                }
                else
                {
                    var sp = info.PrisielSanitkou ? AP_SanVstup : AP_PesiVstup;
                    _pacientiAnim[id] = new AnimEntity
                    {
                        X = sp.X, Y = sp.Y, TgtX = tx, TgtY = ty,
                        Color = info.PrisielSanitkou ? AC_Sanitka : AC_Pesi,
                        Label = id.ToString()
                    };
                }
            }

            // Staff placement at specific room centers
            var vvRooms = _patientRooms
                .Where(kvp => alive.Contains(kvp.Key) &&
                    _sim.Pacienti[kvp.Key].Stav is "Presun sestry" or "VV prebieha")
                .OrderBy(kvp => kvp.Value.Idx).ToList();
            var oseRooms = _patientRooms
                .Where(kvp => alive.Contains(kvp.Key) &&
                    _sim.Pacienti[kvp.Key].Stav is "Presun personálu" or "Ošetrenie prebieha")
                .OrderBy(kvp => kvp.Value.IsTypeA ? 0 : 1).ThenBy(kvp => kvp.Value.Idx).ToList();

            AEnsureStaff(ref _sestryAnim, z.TotalSestry, AC_Sestra, "S");
            int sIdx = 0;
            foreach (var (_, room) in vvRooms)
            {
                if (sIdx >= _sestryAnim.Length) break;
                (float rx, float ry) = ARoomStaffPos(room.IsTypeA, room.Idx, 0);
                _sestryAnim[sIdx].TgtX = rx; _sestryAnim[sIdx].TgtY = ry; sIdx++;
            }
            foreach (var (_, room) in oseRooms)
            {
                if (sIdx >= _sestryAnim.Length) break;
                (float rx, float ry) = ARoomStaffPos(room.IsTypeA, room.Idx, 0);
                _sestryAnim[sIdx].TgtX = rx; _sestryAnim[sIdx].TgtY = ry; sIdx++;
            }
            for (int i = 0; sIdx < _sestryAnim.Length; i++, sIdx++)
                (_sestryAnim[sIdx].TgtX, _sestryAnim[sIdx].TgtY) = AStagger(AP_IdleSestra, i, 4);

            AEnsureStaff(ref _lekariAnim, z.TotalLekari, AC_Lekar, "L");
            int lIdx = 0;
            foreach (var (_, room) in oseRooms)
            {
                if (lIdx >= _lekariAnim.Length) break;
                (float rx, float ry) = ARoomStaffPos(room.IsTypeA, room.Idx, 1);
                _lekariAnim[lIdx].TgtX = rx; _lekariAnim[lIdx].TgtY = ry; lIdx++;
            }
            for (int i = 0; lIdx < _lekariAnim.Length; i++, lIdx++)
                (_lekariAnim[lIdx].TgtX, _lekariAnim[lIdx].TgtY) = AStagger(AP_IdleLekar, i, 4);
        }

        private void ATryAssignRoom(int patientId, PacientInfo info)
        {
            bool toA = (info.Stav is "Presun personálu" or "Ošetrenie prebieha") && info.PouzilaMiestnostA;
            var activity = (info.Stav is "Presun sestry" or "VV prebieha")
                ? RoomActivity.VV : RoomActivity.Osetrenie;
            var arr = toA ? _roomAActivity : _roomBActivity;
            int slot = Array.IndexOf(arr, RoomActivity.Free);
            if (slot < 0) return;
            arr[slot] = activity;
            _patientRooms[patientId] = (toA, slot);
        }

        // Normalized position for patient circle in a room (slightly left of center)
        private (float X, float Y) ARoomPatientPos(bool isTypeA, int idx)
        {
            var (cx, cy) = AGetRoomCenter(isTypeA, idx);
            return (cx - 0.025f, cy);
        }

        // Normalized position for staff in a room: staffCol=0→sestra, 1→lekár
        private (float X, float Y) ARoomStaffPos(bool isTypeA, int idx, int staffCol)
        {
            var (cx, cy) = AGetRoomCenter(isTypeA, idx);
            return (cx + staffCol * 0.028f + 0.005f, cy);
        }

        // Compute normalized center of a specific room box
        private (float X, float Y) AGetRoomCenter(bool isTypeA, int idx)
        {
            if (_sim == null) return (0.5f, isTypeA ? AP_ARoomsY : AP_BRoomsY);
            int total = isTypeA ? _sim.AgentZdrojov.TotalMiestnostiA : _sim.AgentZdrojov.TotalMiestnostiB;
            const int COLS = 10;
            int col = idx % COLS, row = idx / COLS;
            float baseY = isTypeA ? AP_ARoomsY : AP_BRoomsY;
            float y = baseY + row * 0.15f;
            float cols = Math.Min(total, COLS);
            float step = Math.Min(0.130f, 0.84f / cols);
            float x = 0.08f + (0.84f - cols * step) / 2f + (col + 0.5f) * step;
            return (x, y);
        }

        private static (float X, float Y) AStagger(PointF center, int idx, int cols)
        {
            const float step = 0.04f;
            int row = idx / cols, col = idx % cols;
            float dx = (col - (cols - 1) * 0.5f) * step;
            float dy = row * step;
            return (center.X + dx, center.Y + dy);
        }

        private void AEnsureStaff(ref AnimEntity[] arr, int count, Color color, string prefix)
        {
            if (arr.Length == count) return;
            var next = new AnimEntity[count];
            for (int i = 0; i < count; i++)
            {
                if (i < arr.Length) { next[i] = arr[i]; continue; }
                next[i] = new AnimEntity
                {
                    X = AP_SanVstup.X, Y = AP_SanVstup.Y,
                    TgtX = AP_SanVstup.X, TgtY = AP_SanVstup.Y,
                    Color = color, Label = $"{prefix}{i + 1}"
                };
            }
            arr = next;
        }

        // ── Animator painting ─────────────────────────────────────────

        private void PnlAnimator_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int W = pnlAnimator.ClientSize.Width;
            int H = pnlAnimator.ClientSize.Height;
            if (W < 10 || H < 10) return;

            // Static areas
            ADrawArea(g, W, H, AP_PesiVstup, "Vstup\n(pešo)",    Color.LightBlue,           70, 44);
            ADrawArea(g, W, H, AP_SanVstup,  "Vstup\n(sanitka)", Color.LightSalmon,          76, 44);
            ADrawArea(g, W, H, AP_VVRad,     "Rad VV",           Color.LightGoldenrodYellow,  92, 36);
            ADrawArea(g, W, H, AP_OseRad,    "Rad ošetrenia",    Color.LightGoldenrodYellow, 110, 36);
            ADrawArea(g, W, H, AP_Vystup,    "Výstup",           Color.LightGray,             66, 28);

            // Individual ambulance rooms
            ADrawRooms(g, W, H);

            // Flow arrows (use band midpoints for source/target of room bands)
            using var arrowPen = new Pen(Color.Silver, 1.2f)
                { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            void Arrow(float ax, float ay, float bx, float by) =>
                g.DrawLine(arrowPen, ax * W, ay * H, bx * W, by * H);

            Arrow(AP_PesiVstup.X, AP_PesiVstup.Y, AP_VVRad.X, AP_VVRad.Y);
            Arrow(AP_SanVstup.X,  AP_SanVstup.Y,  AP_VVRad.X, AP_VVRad.Y);
            Arrow(AP_VVRad.X,  AP_VVRad.Y,  0.30f, AP_BRoomsY);
            Arrow(0.30f, AP_BRoomsY, AP_OseRad.X, AP_OseRad.Y);
            Arrow(AP_OseRad.X, AP_OseRad.Y, 0.60f, AP_BRoomsY);
            Arrow(AP_OseRad.X, AP_OseRad.Y, 0.70f, AP_ARoomsY);
            Arrow(0.60f, AP_BRoomsY, AP_Vystup.X, AP_Vystup.Y);
            Arrow(0.70f, AP_ARoomsY, AP_Vystup.X, AP_Vystup.Y);

            // Staff (drawn before patients so patients appear on top)
            foreach (var ent in _sestryAnim)          ADrawEntity(g, W, H, ent, 7);
            foreach (var ent in _lekariAnim)          ADrawEntity(g, W, H, ent, 7);
            foreach (var ent in _pacientiAnim.Values) ADrawEntity(g, W, H, ent, 9);

            ADrawLegend(g, H);
        }

        private void ADrawRooms(Graphics g, int W, int H)
        {
            if (_sim == null) return;
            var z = _sim.AgentZdrojov;
            float s = H / 400f;

            // Band labels
            int bLabelY = (int)((AP_BRoomsY - 0.07f) * H);
            int aLabelY = (int)((AP_ARoomsY - 0.07f) * H);
            g.DrawString("Ambulancie B  (VV + ošetrenie)", AF_Small, Brushes.DimGray, 6, bLabelY);
            g.DrawString("Ambulancie A  (ošetrenie, vysoká priorita)", AF_Small, Brushes.DimGray, 6, aLabelY);

            for (int i = 0; i < z.TotalMiestnostiB; i++)
            {
                var act = i < _roomBActivity.Length ? _roomBActivity[i] : RoomActivity.Free;
                var (cx, cy) = AGetRoomCenter(false, i);
                ADrawRoom(g, W, H, s, cx, cy, $"B{i + 1}", act, z.TotalMiestnostiB);
            }
            for (int i = 0; i < z.TotalMiestnostiA; i++)
            {
                var act = i < _roomAActivity.Length ? _roomAActivity[i] : RoomActivity.Free;
                var (cx, cy) = AGetRoomCenter(true, i);
                ADrawRoom(g, W, H, s, cx, cy, $"A{i + 1}", act, z.TotalMiestnostiA);
            }
        }

        private static void ADrawRoom(Graphics g, int W, int H, float s,
                                       float cx, float cy, string label,
                                       RoomActivity activity, int totalRooms)
        {
            float cols = Math.Min(totalRooms, 10f);
            float step = Math.Min(0.130f, 0.84f / cols);  // same formula as AGetRoomCenter
            int bw = Math.Max(20, (int)(step * 0.72f * W));
            int bh = Math.Max(24, (int)(50 * s));
            int icx = (int)(cx * W), icy = (int)(cy * H);
            var r = new Rectangle(icx - bw / 2, icy - bh / 2, bw, bh);

            Color fill = activity switch
            {
                RoomActivity.VV        => Color.LightGreen,
                RoomActivity.Osetrenie => Color.LightCoral,
                _                      => Color.WhiteSmoke
            };
            using var fb = new SolidBrush(Color.FromArgb(activity == RoomActivity.Free ? 55 : 145, fill));
            g.FillRectangle(fb, r);

            Color border = activity switch
            {
                RoomActivity.VV        => Color.ForestGreen,
                RoomActivity.Osetrenie => Color.Crimson,
                _                      => Color.LightGray
            };
            float bw2 = activity == RoomActivity.Free ? 1f : 2f;
            using var pen = new Pen(border, bw2);
            g.DrawRectangle(pen, r);

            using var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(label, AF_Small, Brushes.Black, r, sf);
        }

        private static void ADrawArea(Graphics g, int W, int H, PointF c,
                                      string text, Color col, int bw, int bh)
        {
            float s = H / 400f;
            int sw = (int)(bw * s), sh = (int)(bh * s);
            int cx = (int)(c.X * W), cy = (int)(c.Y * H);
            var r = new Rectangle(cx - sw / 2, cy - sh / 2, sw, sh);
            using var b = new SolidBrush(Color.FromArgb(110, col));
            g.FillRectangle(b, r);
            g.DrawRectangle(Pens.DarkGray, r);
            using var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, AF_Small, Brushes.Black, r, sf);
        }

        private static void ADrawEntity(Graphics g, int W, int H, AnimEntity ent, int baseR)
        {
            float s = H / 400f;
            int r  = Math.Max(4, (int)(baseR * s));
            int cx = (int)(ent.X * W);
            int cy = (int)(ent.Y * H);
            using var b = new SolidBrush(ent.Color);
            g.FillEllipse(b, cx - r, cy - r, 2 * r, 2 * r);
            g.DrawEllipse(Pens.DimGray, cx - r, cy - r, 2 * r, 2 * r);
            using var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
            g.DrawString(ent.Label, AF_Small, Brushes.Black,
                new RectangleF(cx - 14, cy - r - 13, 28, 13), sf);
        }

        private static void ADrawLegend(Graphics g, int H)
        {
            int x = 8, y = H - 60;
            (Color c, string t)[] items =
            [
                (AC_Pesi,    "Pacient pešo"),
                (AC_Sanitka, "Pacient sanitka"),
                (AC_Sestra,  "Sestra  (v izbe VV alebo pri ošetrení)"),
                (AC_Lekar,   "Lekár  (pri ošetrení)"),
            ];
            foreach (var (c, t) in items)
            {
                using var b = new SolidBrush(c);
                g.FillEllipse(b, x, y + 1, 10, 10);
                g.DrawEllipse(Pens.DimGray, x, y + 1, 10, 10);
                g.DrawString(t, AF_Small, Brushes.Black, x + 14, y);
                y += 14;
            }
            // Room color legend
            y += 4;
            (Color c, string t)[] rooms =
            [
                (Color.LightGreen, "Izba B – VV prebieha"),
                (Color.LightCoral, "Izba – ošetrenie prebieha"),
            ];
            foreach (var (c, t) in rooms)
            {
                using var b = new SolidBrush(Color.FromArgb(145, c));
                g.FillRectangle(b, x, y + 1, 14, 10);
                g.DrawRectangle(Pens.DimGray, x, y + 1, 14, 10);
                g.DrawString(t, AF_Small, Brushes.Black, x + 18, y);
                y += 14;
            }
        }
    }
}
