using ff14bot.Objects;
using FFXIVOverlay.Overlay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVOverlay.Command
{
    public interface ICacheDrawItem
    {
        bool CanDrawing(DrawingContext ctx, GameObject obj);
        string Key(DrawingContext ctx, GameObject obj);
        object State(DrawingContext ctx, GameObject obj);

        void DrawCache(DrawingContext ctx, object state);
    }
}
