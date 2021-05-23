using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using ff14bot.Objects;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using Vector3 = SlimDX.Vector3;

namespace FFXIVOverlay.Command
{
    public class GatherHotspot : IDrawCommand
    {
        public Vector3 XYZ;
        public System.Drawing.Color NodeColor = System.Drawing.Color.FromArgb(0xE0, 0x00, 0xD0, 0xD0);
        public System.Drawing.Color RangeColor = System.Drawing.Color.FromArgb(0x20, 0x00, 0xD0, 0x00);

        public float Radius = 25;

        public void Drawing(DrawingContext ctx, GameObject obj)
        {
            var vecCenter = XYZ + new Vector3(0, 1, 0);
            ctx.DrawOutlinedBox(vecCenter, new Vector3(1.0f * 1.0f), NodeColor);
            
            ctx.DrawCircleWithPoint(
                vecCenter,
                0.0f,
                Radius,
                RangeColor, RangeColor);
        }
    }
}
