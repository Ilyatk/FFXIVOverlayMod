using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ff14bot.Objects;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;

namespace FFXIVOverlay.Command
{
    public class GatherNode : ComplexDrawCommand
    {
        public int zoneId = -1;

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            if (zoneId != -1)
            {
                if (zoneId != ff14bot.Managers.WorldManager.ZoneId)
                    return;
            }

            base.Drawing(ctx, obj);
        }
    }
}
