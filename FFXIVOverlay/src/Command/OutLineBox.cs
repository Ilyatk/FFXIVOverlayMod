using ff14bot.Objects;
using Vector3 = SlimDX.Vector3;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;

namespace FFXIVOverlay.Command
{
    public class OutLineBox : DrawItemBase
    {
        public System.Drawing.Color BoxColor { set { this.C1 = value; } get { return this.C1; } }
        public float SizeFactor { set { this.R1 = value; } get { return this.R1; } }

        public OutLineBox() : base()
        {
            this.SizeFactor = 1.0f;
        }

        private void internalDraw(DrawingContext ctx, Vector3 loc)
        {
            var vecCenter = loc + new Vector3(0, 1, 0);
            ctx.DrawOutlinedBox(vecCenter, new Vector3(1f * SizeFactor), BoxColor);
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            internalDraw(ctx, CalcLocation(obj));
        }

        public override void internalDrawingCache(DrawingContext ctx, DrawItemState state)
        {
            internalDraw(ctx, state.Center);
        }
    }
}
