using System.Linq;

using ff14bot.Objects;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using Vector3 = SlimDX.Vector3;
using YamLikeCommand = YamLikeConfig.Command;

namespace FFXIVOverlay.Command
{
    class ApproachGatherSpotList : ComplexDrawCommand, IDrawCommandCreator
    {
        public ApproachGatherSpotList() : base()
        {
        }
    }
}
