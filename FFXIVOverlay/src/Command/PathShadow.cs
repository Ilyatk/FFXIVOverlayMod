using ff14bot.Objects;
using System;
using System.Drawing;
using System.Collections.Generic;
using Vector3 = SlimDX.Vector3;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;

namespace FFXIVOverlay.Command
{
    public class PathShadow : DrawItemBase
    {
        public PathShadow() : base()
        {
            path = new List<Vector3>();
        }

        int pointIntervalMsc = 100;
        float distanceSquare = 5.0f;
        int maxPointCount = 10;
        bool useFadeOut = false;

        List<Vector3> path;
        int pointCount = 0;
        
        DateTime lastAdd = DateTime.Now;
        Vector3 lastPoint;

        private void internalDraw(DrawingContext ctx, Vector3 loc)
        {
            bool shouldAddPoint = (DateTime.Now - lastAdd).TotalMilliseconds > pointIntervalMsc;
            if (pointCount > 5)
            {
                shouldAddPoint = shouldAddPoint && (SlimDX.Vector3.DistanceSquared(loc, lastPoint) > distanceSquare);
            }

            if (shouldAddPoint)
            {
                lastAdd = DateTime.Now;
                lastPoint = loc;

                if (pointCount < maxPointCount)
                {
                    pointCount++;
                    path.Add(loc);
                } 
                else
                {
                    path.RemoveAt(0);
                    path.Add(loc);
                }
            }

            if (path == null || pointCount < 3)
            {
                return;
            }

            if (useFadeOut)
            {
                float startA = Math.Max(C1.A, (byte)0x79);
                float endA = 10f;
                float stepA = (startA - endA) / maxPointCount;
                Vector3 a = path[0];
                float currentA = endA - stepA;

                foreach (var b in path)
                {
                    currentA += stepA;
                    if (a == b) continue;

                    Color c = Color.FromArgb((int)Math.Floor(currentA), this.C1);
                    ctx.DrawLine(a, b, c);
                    a = b;
                }
            }
            else
            {
                Vector3 a = path[0];
                foreach (var b in path)
                {
                    if (a == b) continue;

                    ctx.DrawLine(a, b, this.C1);
                    a = b;
                }
            }
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            if (!FixCenter && obj == null)
                return;

            internalDraw(ctx, CalcLocation(obj));
        }

        public override void internalDrawingCache(DrawingContext ctx, DrawItemState state)
        {
            // No cache support
            //internalDraw(ctx, state.Center);
        }

        public override bool Init(YamLikeConfig.Command cmd)
        {
            if (!base.Init(cmd))
            {
                return false;
            }

            if (cmd.has("fadeout") && cmd["fadeout"] == "1")
            {
                useFadeOut = true;
            }

            if (cmd.has("length"))
            {
                int tmp;
                if (cmd.tryGet("length", out tmp))
                {
                    maxPointCount = tmp;
                }
            }

            if (cmd.has("interval"))
            {
                int tmp;
                if (cmd.tryGet("interval", out tmp))
                {
                    pointIntervalMsc = tmp;
                }
            }

            if (cmd.has("distance"))
            {
                float tmp;
                if (cmd.tryGet("distance", out tmp))
                {
                    distanceSquare = tmp;
                }
            }

            return true;
        }
    }

}
