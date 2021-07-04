using ff14bot.Objects;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using Vector3 = SlimDX.Vector3;

namespace FFXIVOverlay.Command
{
    public class GatherHotspot : DrawItemBase
    {
        public GatherHotspot() : base()
        {
        }

        public System.Drawing.Color NodeColor { set { this.C1 = value; } get { return this.C1; } }
        public System.Drawing.Color RangeColor { set { this.C2 = value; } get { return this.C2; } }

        public float Radius { set { this.R1 = value; } get { return this.R1; } }

        private void internalDraw(DrawingContext ctx, Vector3 loc)
        {
            var vecCenter = loc + new Vector3(0, 1, 0);
            ctx.DrawOutlinedBox(vecCenter, new Vector3(1.0f * 1.0f), NodeColor);

            ctx.DrawCircleWithPoint(
                vecCenter,
                0.0f,
                Radius,
                RangeColor, RangeColor);
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            internalDraw(ctx, Center);
        }

        public override void internalDrawingCache(DrawingContext ctx, DrawItemState state)
        {
            internalDraw(ctx, state.Center);
        }

    }
}
