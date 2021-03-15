using ff14bot.Objects;
using FFXIVOverlay.Overlay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector3 = SlimDX.Vector3;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using System.Windows.Media;

namespace FFXIVOverlay.Command
{
    public class LineAttack : IDrawCommand
    {
        public float HeadingOffset = 0f; // -360+360
        public float Width = 5.0f;
        public float Length = 100f;

        public System.Drawing.Color Color = System.Drawing.Color.FromArgb(255, 255, 0, 0);
        public System.Drawing.Color PointColor = System.Drawing.Color.FromArgb(255, 0, 255, 0);
        public void Drawing(DrawingContext ctx, GameObject obj)
        {
            ctx.DrawAgroLine(
                obj.Location,
                obj.Heading + (((float)Math.PI / 180f) * HeadingOffset),
                Width, Length, 
                Color, PointColor);
        }
    }
}
