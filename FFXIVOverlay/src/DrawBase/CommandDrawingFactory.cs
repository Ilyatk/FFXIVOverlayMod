using System;
using System.Collections.Generic;

using YamLikeCommand = YamLikeConfig.Command;

namespace FFXIVOverlay.Command
{
    public class CommandFactory
    {
        public void Reg<T>(string name) where T : new()
        {
            string shortName = name.Trim().ToLower();

            this._table[shortName] = (YamLikeCommand cmd) =>
            {
                T t = new T();
                IDrawCommandCreator creator = t as IDrawCommandCreator;
                IDrawCommand drawItem = t as IDrawCommand;

                if (creator == null || drawItem == null)
                {
                    return null;
                }

                if (!creator.Init(cmd))
                {
                    return null;
                }

                return drawItem;
            };
        }

        public void Reg(string name, Func<YamLikeCommand, IDrawCommand> fn)
        {
            string shortName = name.Trim().ToLower();
            this._table[shortName] = fn;
        }

        public IDrawCommand create(YamLikeCommand cmd)
        {
            if (cmd == null || string.IsNullOrWhiteSpace(cmd.Name))
            {
                return null;
            }

            string shortName = cmd.Name.Trim().ToLower();
            if (!_table.ContainsKey(shortName))
            {
                return null;
            }

            return _table[shortName](cmd);
        }

        private Dictionary<string, Func<YamLikeCommand, IDrawCommand>> _table = new Dictionary<string, Func<YamLikeCommand, IDrawCommand>>();
    }

    public class CommandDrawingFactoryEx
    {
        public static DrawCacheManager drawCacheManager = null;

        private static bool initDone = false;
        private static CommandFactory factory = new CommandFactory();

        public static IDrawCommand create(YamLikeCommand cmd)
        {
            if (!initDone)
            {
                init();
                initDone = true;
            }

            return factory.create(cmd);
        }

        private static void init()
        {
            factory.Reg<Donut>("donut");
            factory.Reg<Sector>("sector");
            factory.Reg<CircleAttack>("circleattack");

            factory.Reg<LineAttack>("lineattack");

            factory.Reg<GatherHotspot>("hotspot");
            factory.Reg<GatherHotspot>("<hotspot");

            factory.Reg<GatherNode>("gather");

            //factory.Reg("gather", (cmd) =>
            //{
            //    GatherNode n = new GatherNode();
            //    if (!n.Init(cmd))
            //    {
            //        return null;
            //    }

            //    return n;
            //});

            factory.Reg<Title>("title");
            factory.Reg<SideAttack>("sideattack");

            factory.Reg<OutLineBox>("box");

            factory.Reg<SpellFilter>("spell");

            factory.Reg<GatherHotspotList>("hotspots");

            factory.Reg("cache", (cmd) =>
            {
                if (CommandDrawingFactoryEx.drawCacheManager == null)
                {
                    return null;
                }

                DrawCacheCommand n = new DrawCacheCommand(drawCacheManager);
                if (!n.Init(cmd))
                {
                    return null;
                }

                return n;
            });

            factory.Reg<PathShadow>("path");
        }
    }
}
