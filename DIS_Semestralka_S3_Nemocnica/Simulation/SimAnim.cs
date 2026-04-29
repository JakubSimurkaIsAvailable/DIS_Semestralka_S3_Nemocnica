namespace Simulation
{
    internal static class SimAnim
    {
        public const float W = 1200f, H = 700f;

        public static readonly (float X, float Y) PesiVstup  = (96f,   609f);
        public static readonly (float X, float Y) SanVstup   = (1080f,  56f);
        public static readonly (float X, float Y) IdleSestra = (1050f,  42f);
        public static readonly (float X, float Y) IdleLekar  = (1050f, 119f);
        public static readonly (float X, float Y) VVRad      = (264f,  441f);
        public static readonly (float X, float Y) OseRad     = (780f,  441f);
        public static readonly (float X, float Y) Vystup     = (528f,  609f);

        private const float BRoomsY     = 196f;
        private const float ARoomsY     = 350f;
        private const float RoomRowStep = 105f;
        private const int   MaxCols     = 10;

        private static float RoomStep(int total)
            => Math.Min(156f, 1008f / Math.Min(total, MaxCols));

        public static (float X, float Y) RoomCenter(bool isA, int idx, int totalA, int totalB)
        {
            int   total  = isA ? totalA : totalB;
            float step   = RoomStep(total);
            float cols   = Math.Min(total, MaxCols);
            float startX = 96f + (1008f - cols * step) / 2f;
            float x      = startX + (idx % MaxCols + 0.5f) * step;
            float y      = (isA ? ARoomsY : BRoomsY) + (idx / MaxCols) * RoomRowStep;
            return (x, y);
        }

        public static (float X, float Y) RoomPatientPos(bool isA, int idx, int totalA, int totalB)
        {
            var (cx, cy) = RoomCenter(isA, idx, totalA, totalB);
            return (cx - 25f, cy);
        }

        public static (float X, float Y) RoomStaffPos(bool isA, int idx, int staffCol, int totalA, int totalB)
        {
            var (cx, cy) = RoomCenter(isA, idx, totalA, totalB);
            return (cx + staffCol * 28f + 5f, cy);
        }

        public static (float X, float Y) QueuePos((float X, float Y) center, int slot)
        {
            const float step = 48f;
            return (center.X + (slot % 4) * step, center.Y + (slot / 4) * step);
        }

        public static (float X, float Y) IdlePos((float X, float Y) center, int idx)
        {
            const float step = 48f;
            return (center.X + (idx % 4) * step, center.Y + (idx / 4) * step);
        }
    }
}
