using System.Linq;

using ff14bot.Objects;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using Vector3 = SlimDX.Vector3;
using YamLikeCommand = YamLikeConfig.Command;

namespace FFXIVOverlay.Command
{
    public class GatherHotspotList : ComplexDrawCommand, IDrawCommandCreator
    {
        public System.Drawing.Color Color = System.Drawing.Color.FromArgb(0xE0, 0x00, 0xD0, 0xD0);

        public GatherHotspotList() : base()
        {

        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            Vector3[] points = this.Commands
                .Where(p => ((p as GatherHotspot) != null))
                .Select(p => (p as GatherHotspot).Center).ToArray();

            if (points != null && points.Length > 1)
            {
                Vector3 from = points[0];
                for (int i = 1;  i < points.Length; ++i)
                {
                    ctx.DrawLine(from, points[i], Color);
                    from = points[i];
                }

                if (points.Length > 2)
                {
                    ctx.DrawLine(from, points[0], Color);
                }
            }

            base.Drawing(ctx, obj);
        }

        public override bool Init(YamLikeCommand cmd)
        {
            if (cmd.has("color"))
            {
                this.Color = cmd["color"].ParseColor(this.Color);
            }

            return base.Init(cmd);
        }
    }
}
