using System;

using ff14bot.Objects;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using Vector3 = SlimDX.Vector3;

namespace FFXIVOverlay.Command
{
    public class DrawCacheCommand : ComplexDrawCommand
    {
        DrawCacheManager cacheManager;
        private string baseKey;

        public float Before = 0.0f;
        public float Duration = 5.0f;
        public float After = 0.0f;

        public DrawCacheCommand(DrawCacheManager cache, float duration = 5.0f, float before = 0.0f, float after = 0.0f)
            : base()
        {
            cacheManager = cache;
            Before = before;
            Duration = duration;
            After = after;

            baseKey = Guid.NewGuid().ToString();
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            foreach (var c in Commands)
            {
                ICacheDrawItem cacheDrawItem = c as ICacheDrawItem;
                if (cacheDrawItem == null)
                {
                    c.Drawing(ctx, obj);
                    continue;
                }

                HandleCacheCommand(cacheDrawItem, ctx, obj);
            }
        }

        public void HandleCacheCommand(ICacheDrawItem cmd, DrawingContext ctx, GameObject obj)
        {
            if (!cmd.CanDrawing(ctx, obj))
                return;

            string subKey = cmd.Key(ctx, obj);
            string k = baseKey + subKey;
            if (cacheManager.HasKey(k))
                return;

            object s = cmd.State(ctx, obj);
            cacheManager.Add(k, cmd, s, Duration, Before, After);
        }

        public override bool Init(YamLikeConfig.Command cmd)
        {
            cmd.tryGet("before", out this.Before);
            cmd.tryGet("duration", out this.Duration);
            cmd.tryGet("after", out this.After);

            return base.Init(cmd);
        }
    }
}
