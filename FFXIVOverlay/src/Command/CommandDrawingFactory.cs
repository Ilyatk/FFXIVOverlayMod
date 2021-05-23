using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YamLikeCommand = YamLikeConfig.Command;

namespace FFXIVOverlay.Command
{
    public class CommandDrawingFactory
    {
        public static IDrawCommand create(YamLikeCommand cmd)
        {
            string cmdName = cmd.Name ?? "";
            cmdName = cmdName.ToLower();

            switch (cmdName)
            {
                case "title":
                    return createTitle(cmd);
                case "box":
                    return createBox(cmd);
                case "lineattack":
                    return createLineAttack(cmd);
                case "circleattack":
                    return createCircleAttack(cmd);
                case "sideattack":
                    return createSideAttack(cmd);
                case "spell":
                    return createSpellFilter(cmd);

                case "hotspots":
                    return createGatherHotSpotList(cmd);

                case "hotspot":
                case "<hotspot":
                    return createGatherHotSpot(cmd);
            }

            return null;
        }

        static IDrawCommand createTitle(YamLikeCommand cmd)
        {
            string format;
            if (cmd.UnnamedParams != null && cmd.UnnamedParams.Count > 0)
                format = cmd[0];
            else
                format = cmd["format"];

            if (string.IsNullOrWhiteSpace(format))
                return null;

            Title t = new Title
            {
                Text = format
            };

            float sizeAttr = 12;
            if (float.TryParse(cmd["size"], NumberStyles.Float, CultureInfo.InvariantCulture, out sizeAttr))
            {
                t.FontSize = sizeAttr;
            }

            return t;
        }

        static IDrawCommand createBox(YamLikeCommand cmd)
        {
            OutLineBox box = new OutLineBox { };

            float sizeAttr = 0;
            if (float.TryParse(cmd["size"], NumberStyles.Float, CultureInfo.InvariantCulture, out sizeAttr))
            {
                box.SizeFactor = sizeAttr;
            }

            if (cmd.Params.ContainsKey("color"))
            {
                box.BoxColor = cmd["color"].ParseColor(box.BoxColor);
            }

            return box;
        }

        static IDrawCommand createLineAttack(YamLikeCommand cmd)
        {
            LineAttack lineAttack = new LineAttack { };

            float tmp = 0;
            if (cmd.tryGet("width", out tmp))
            {
                lineAttack.Width = tmp;
            }

            if (cmd.tryGet("length", out tmp))
            {
                lineAttack.Length = tmp;
            }

            if (cmd.tryGet("offset", out tmp))
            {
                lineAttack.HeadingOffset = tmp;
            }

            System.Drawing.Color tmpColor;
            if (cmd.tryGet("color", out tmpColor))
            {
                lineAttack.Color = tmpColor;
            }

            if (cmd.tryGet("point", out tmpColor))
            {
                lineAttack.PointColor = tmpColor;
            }

            return lineAttack;
        }

        static IDrawCommand createCircleAttack(YamLikeCommand cmd)
        {
            CircleAttack circleAttack = new CircleAttack { };

            float tmp = 0;
            if (cmd.tryGet("radius", out tmp))
            {
                circleAttack.Radius = tmp;
            }

            if (cmd.tryGet("offset", out tmp))
            {
                circleAttack.HeadingOffset = tmp;
            }

            System.Drawing.Color tmpColor;
            if (cmd.tryGet("color", out tmpColor))
            {
                circleAttack.Color = tmpColor;
            }

            if (cmd.tryGet("point", out tmpColor))
            {
                circleAttack.PointColor = tmpColor;
            }

            return circleAttack;
        }

        static IDrawCommand createSideAttack(YamLikeCommand cmd)
        {
            SideAttack sideAttack = new SideAttack { };

            float tmp = 0;
            if (cmd.tryGet("width", out tmp))
            {
                sideAttack.Width = tmp;
            }

            if (cmd.tryGet("length", out tmp))
            {
                sideAttack.Length = tmp;
            }

            if (cmd.tryGet("offset", out tmp))
            {
                sideAttack.HeadingOffset = tmp;
            }

            System.Drawing.Color tmpColor;
            if (cmd.tryGet("color", out tmpColor))
            {
                sideAttack.Color = tmpColor;
            }

            return sideAttack;
        }

        static IDrawCommand createSpellFilter(YamLikeCommand cmd)
        {
            if (cmd.SubCommand == null || cmd.SubCommand.Count == 0)
                return null;

            if (cmd.UnnamedParams == null || cmd.UnnamedParams.Count == 0)
                return null;

            List<uint> spells = new List<uint>();

            foreach (var q in cmd.UnnamedParams)
            {
                uint spellId = 0;
                if (uint.TryParse(q, out spellId))
                {
                    spells.Add(spellId);
                }
            }

            if (spells.Count == 0)
                return null;

            List<IDrawCommand> tmp = new List<IDrawCommand>();
            foreach (var c in cmd.SubCommand)
            {
                IDrawCommand drawingCmd = CommandDrawingFactory.create(c);
                if (drawingCmd == null)
                    continue;

                tmp.Add(drawingCmd);
            }

            if (tmp.Count == 0)
                return null;

            SpellFilter spellFilter = new SpellFilter();

            foreach (var s in spells)
                spellFilter.AddSpell(s);

            foreach (var d in tmp)
                spellFilter.AddDrawItem(d);

            return spellFilter;
        }

        static IDrawCommand createGatherHotSpotList(YamLikeCommand cmd)
        {
            if (cmd.SubCommand == null || cmd.SubCommand.Count == 0)
                return null;

            GatherHotspotList hotspotList = new GatherHotspotList();

            if (cmd.Params.ContainsKey("color"))
            {
                hotspotList.Color = cmd["color"].ParseColor(hotspotList.Color);
            }

            foreach (var c in cmd.SubCommand)
            {
                IDrawCommand drawingCmd = CommandDrawingFactory.create(c);
                if (drawingCmd == null)
                    continue;

                hotspotList.AddDrawItem(drawingCmd);
            }

            return hotspotList;
        }

        static IDrawCommand createGatherHotSpot(YamLikeCommand cmd)
        {
            if (!cmd.has("XYZ"))
                return null;

            string xyzString = cmd["XYZ"];

            SlimDX.Vector3 xyz;
            if (!xyzString.TryParse(out xyz))
                return null;

            GatherHotspot h = new GatherHotspot();
            h.XYZ = xyz;
            if (cmd.has("Radius"))
            {
                float tmp;
                if (cmd.tryGet("Radius", out tmp))
                {
                    h.Radius = tmp;
                }
            }

            if (cmd.Params.ContainsKey("color"))
            {
                h.NodeColor = cmd["color"].ParseColor(h.NodeColor);
            }

            if (cmd.Params.ContainsKey("rangecolor"))
            {
                h.RangeColor = cmd["rangecolor"].ParseColor(h.RangeColor);
            }

            return h;
            
        }
    }
}
