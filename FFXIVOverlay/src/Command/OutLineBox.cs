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
    public class OutLineBox : IDrawCommand
    {
        public System.Drawing.Color BoxColor = System.Drawing.Color.FromArgb(255, 255, 0, 0);
        public float SizeFactor = 1.0f;
        public void Drawing(DrawingContext ctx, GameObject obj)
        {
            var vecCenter = obj.Location.Convert() + new Vector3(0, 1, 0);
            ctx.DrawOutlinedBox(vecCenter, new Vector3(1f * SizeFactor), BoxColor);
        }
    }
}
