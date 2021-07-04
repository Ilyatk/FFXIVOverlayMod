using System.Drawing;
using Vector3 = SlimDX.Vector3;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using ff14bot.Objects;

namespace FFXIVOverlay.Command
{
    public class Sector : DrawItemBase
    {
        public Sector() : base()
        {
        }

        public Color Color { set { this.C1 = value; } get { return this.C1; } }

        public float Radius { set { this.R1 = value; } get { return this.R1; } }

        private void internalDraw(DrawingContext ctx, Vector3 center, float heading)
        {
            ctx.DrawSector(center, Color, Radius, Angle, heading);
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
