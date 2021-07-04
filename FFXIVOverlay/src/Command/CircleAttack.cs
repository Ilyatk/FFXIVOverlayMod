using ff14bot.Objects;
using System;
using Vector3 = SlimDX.Vector3;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;

namespace FFXIVOverlay.Command
{
    public class CircleAttack : DrawItemBase
    {
        public CircleAttack()
            : base()
        {
        }

        public float Radius { get { return this.R1; } set { this.R1 = value; } }

        public System.Drawing.Color Color { get { return this.C1; } set { this.C1 = value; } }
        public System.Drawing.Color PointColor { get { return this.C2; } set { this.C2 = value; } }

        private void internalDraw(DrawingContext ctx, Vector3 loc, float heading)
        {
            ctx.DrawCircleWithPoint(loc, heading, Radius, Color, PointColor);
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            if (!FixCenter && obj == null)
                return;

            internalDraw(
                ctx, 
                CalcLocation(obj),
                CalcHeading(obj));
        }

        public override void internalDrawingCache(DrawingContext ctx, DrawItemState state)
        {
            internalDraw(ctx, state.Center, state.Heading);
        }
    }
}
