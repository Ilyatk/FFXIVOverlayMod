using System.Collections.Generic;
using ff14bot.Objects;
using System.ComponentModel;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using Vector3 = SlimDX.Vector3;

namespace FFXIVOverlay.Command
{
    public class ApproachGatherSpot : DrawItemBase
    {
        public ApproachGatherSpot() : base()
        {
            this.R1 = 1.0f;
        }

        //[DefaultValue(System.Drawing.Color.FromArgb(0,10,0,0))]
        [DefaultValue(typeof(System.Drawing.Color), "Navy")]
        public System.Drawing.Color Color { set { this.C1 = value; } get { return this.C1; } }
        //public System.Drawing.Color ApproachColor { set { this.C2 = value; } get { return this.C2; } }


        public Clio.Utilities.Vector3 NodeLocation { get; set; }
        public Clio.Utilities.Vector3 ApproachLocation { get; set; }

        [DefaultValue(1.0f)]
        public float Height { set { this.R1 = value; } get { return this.R1; } }

        private void internalDraw(DrawingContext ctx, Vector3 loc)
        {
            /*
                Node[y-h/2] ----- Approach[y-h/2]
                    |                   |
                    |                   |
                    |                   |
                    |                   |
                Node[y+h/2] ----- Approach[y+h/2]
             */

            //ctx.DrawLineList
            //var vecCenter = loc + new Vector3(0, 1, 0);
            //ctx.DrawOutlinedBox(vecCenter, new Vector3(1.0f * 1.0f), NodeColor);

            //ctx.DrawCircleWithPoint(
            //    vecCenter,
            //    0.0f,
            //    Radius,
            //    RangeColor, RangeColor);

            Clio.Utilities.Vector3 diffVec = new Clio.Utilities.Vector3(0, Height / 2, 0);

            List<Clio.Utilities.Vector3> lines = new List<Clio.Utilities.Vector3>
            {
                NodeLocation - diffVec, ApproachLocation - diffVec,
                NodeLocation - diffVec, NodeLocation + diffVec,
                ApproachLocation - diffVec, ApproachLocation + diffVec,
                NodeLocation + diffVec, ApproachLocation + diffVec,
            };

            ctx.DrawLineList(lines, 4, this.Color);
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            internalDraw(ctx, Center);
        }

        public override void internalDrawingCache(DrawingContext ctx, DrawItemState state)
        {
            internalDraw(ctx, state.Center);
        }

        public override bool Init(YamLikeConfig.Command cmd)
        {
            if (!base.Init(cmd))
            {
                return false;
            }

            if (!cmd.has("ApproachLocation") || !cmd.has("NodeLocation"))
                return false;

            Clio.Utilities.Vector3 vec3;
            if (!cmd["ApproachLocation"].TryParse(out vec3))
            {
                return false;
            }

            ApproachLocation = vec3;

            if (!cmd["NodeLocation"].TryParse(out vec3))
            {
                return false;
            }

            NodeLocation = vec3;

            return true;
        }
    }
}
