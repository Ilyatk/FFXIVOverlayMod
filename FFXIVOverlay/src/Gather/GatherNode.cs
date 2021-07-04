using ff14bot.Objects;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using YamLikeCommand = YamLikeConfig.Command;

namespace FFXIVOverlay.Command
{
    public class GatherNode : ComplexDrawCommand
    {
        public GatherNode(): base()
        {
        }

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

        public override bool Init(YamLikeCommand cmd)
        {
            if (!cmd.tryGet("zone", out this.zoneId))
            {
                return false;
            }

            return base.Init(cmd);
        }
    }
}
