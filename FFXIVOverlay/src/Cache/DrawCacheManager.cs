using System;
using System.Collections.Generic;

using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using Vector3 = SlimDX.Vector3;

namespace FFXIVOverlay.Command
{
    public class DrawCacheManager
    {
        public class CacheItem
        {
            public ICacheDrawItem cmd;
            public object state;

            public DateTime StartDrawTime;
            public DateTime EndDrawTime;
            public DateTime ClearTime;
        }

        private Dictionary<string, CacheItem> data = new Dictionary<string, CacheItem>();

        public bool HasKey(string key)
        {
            return data.ContainsKey(key);
        }

        public void Add(string key, ICacheDrawItem cmd, object state, float duration = 5.0f, float before = 0f, float after = 0f)
        {
            if (duration < 0 || after < 0 || before < 0)
            {
                return;
            }

            if (HasKey(key))
                return;

            float killTime = before + duration + after;
            DateTime n = DateTime.Now;
            CacheItem item = new CacheItem
            {
                cmd = cmd,
                state = state,
                StartDrawTime = n.AddSeconds(before),
                EndDrawTime = n.AddSeconds(before + duration),
                ClearTime = n.AddSeconds(killTime)
            };

            data[key] = item;
        }

        public void Draw(DrawingContext ctx)
        {
            List<string> toRemove = new List<string>();

            DateTime now = DateTime.Now;
            foreach (var p in data)
            {
                string k = p.Key;
                CacheItem i = p.Value;

                if (now > i.ClearTime)
                {
                    toRemove.Add(k);
                    continue;
                }

                if (now > i.StartDrawTime && now < i.EndDrawTime)
                {
                    i.cmd.DrawCache(ctx, i.state);
                }
            }

            foreach (var k in toRemove)
            {
                data.Remove(k);
            }
        }
    }
}
