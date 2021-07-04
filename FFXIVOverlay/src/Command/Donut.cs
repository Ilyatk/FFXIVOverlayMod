using ff14bot.Objects;
using System.Drawing;
using Vector3 = SlimDX.Vector3;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;

namespace FFXIVOverlay.Command
{

    public class Donut : DrawItemBase
    {
        public Donut() : base()
        { }

        public Color InColor { set { this.C1 = value; }  get { return this.C1; } }

        public Color OutColor { set { this.C2 = value; } get { return this.C2; } }

        public float InRadius { set { this.R1 = value; } get { return this.R1; } }

        public float OutRadius { set { this.R2 = value; } get { return this.R2; } }

        private void internalDraw(DrawingContext ctx, Vector3 loc)
        {
            if (OutColor.A > 0)
            {
                ctx.DrawDonut(loc, InRadius, OutRadius, OutColor);
            }

            if (InColor.A > 0)
            {
                ctx.DrawCircle(loc, InRadius, InColor);
            }
        }

        public override bool CanDrawing(DrawingContext ctx, GameObject obj)
        {
            if (!FixCenter && obj == null)
                return false;

            if (InColor.A == 0 && OutColor.A == 0)
                return false;

            return true;
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            if (!FixCenter && obj == null)
                return;

            internalDraw(ctx, CalcLocation(obj));
        }

        public override void internalDrawingCache(DrawingContext ctx, DrawItemState state)
        {
            internalDraw(ctx, state.Center);
        }
    }

}
