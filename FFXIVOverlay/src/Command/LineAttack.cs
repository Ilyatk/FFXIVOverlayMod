using ff14bot.Objects;
using Vector3 = SlimDX.Vector3;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;

namespace FFXIVOverlay.Command
{
    public class LineAttack : DrawItemBase
    {
        public LineAttack() : base()
        { 
        }

        public float Width { set { this.R1 = value; } get { return this.R1; } }
        public float Length { set { this.R2 = value; } get { return this.R2; } }

        public System.Drawing.Color Color { set { this.C1 = value; } get { return this.C1; } }
        public System.Drawing.Color PointColor { set { this.C2 = value; } get { return this.C2; } }

        private void internalDraw(DrawingContext ctx, Vector3 loc, float heading)
        {
            ctx.DrawAgroLine(loc, heading, Width, Length, Color, PointColor);
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            internalDraw(ctx, CalcLocation(obj), CalcHeading(obj));
        }
        public override void internalDrawingCache(DrawingContext ctx, DrawItemState state)
        {
            internalDraw(ctx, state.Center, state.Heading);
        }
    }
}
