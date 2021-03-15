using ff14bot.Objects;
using FFXIVOverlay.Overlay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector3 = SlimDX.Vector3;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using System.Windows.Media;
using System.Text.RegularExpressions;
using ff14bot;
using System.Globalization;

namespace FFXIVOverlay.Command
{
    class Title : IDrawCommand
    {
        public string Text;
        public float FontSize = 12;
        public System.Drawing.Color TextColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);

        public void Drawing(DrawingContext ctx, GameObject obj)
        {
            var vecCenter = obj.Location.Convert() + new Vector3(0, 1, 0);

            string outString = prepareString(obj);

            ctx.Draw3DText(outString, vecCenter, FontSize);
        }

        string prepareString(GameObject obj)
        {
            Regex rgx = new Regex(@"(%.+?%)");
            Character c = obj as Character;
            //BattleCharacter bc = obj as BattleCharacter;

            string resultString = rgx.Replace(Text, new MatchEvaluator((Match m) =>
            {
                string token = m.Value.ToLower();
                switch (token)
                {
                    case "%name%":
                        return obj.EnglishName;

                    case "%objtype%":
                        return string.Format("{0}", (uint)obj.Type);

                    case "%npcid%":
                        return string.Format("{0}", obj.NpcId);

                    case "%pos%":
                        return obj.Location.ToString();

                    case "%distance%":
                        return Core.Me.Location.Distance(obj.Location).ToString("F2", CultureInfo.InvariantCulture);

                    case "%distance2d%":
                        return Core.Me.Location.Distance2D(obj.Location).ToString("F2", CultureInfo.InvariantCulture);

                    case "%font%":
                        return string.Format("{0}", FontSize);

                    case "%hp%":
                        return string.Format("{0}", obj.CurrentHealth);

                    case "%hppct%":
                        return string.Format("{0:F2}", obj.CurrentHealthPercent);

                    case "%maxhp%":
                        return string.Format("{0}", obj.MaxHealth);

                    case "%spellid%":
                        {
                            if (c == null || !c.IsCasting)
                            {
                                return string.Empty;
                            }

                            return string.Format("{0}", c.CastingSpellId);
                        }

                    case "%spellname%":
                        {
                            if (c == null || !c.IsCasting)
                            {
                                return string.Empty;
                            }

                            return c.SpellCastInfo.Name ?? "";
                        }
                }

                return m.Value;
            }));

            return resultString;
        }
    }
}
