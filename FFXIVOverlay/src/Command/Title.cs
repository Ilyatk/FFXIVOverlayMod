using ff14bot.Objects;
using Vector3 = SlimDX.Vector3;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using System.Text.RegularExpressions;
using ff14bot;
using System.Globalization;

namespace FFXIVOverlay.Command
{
    class Title : DrawItemBase
    {
        class TextCacheState
        {
            public Vector3 Location;
            public string Text;
        }

        public Title() : base()
        {
        }

        public float FontSize { set { this.R1 = value; } get { return this.R1; } }
        public System.Drawing.Color TextColor { set { this.C1 = value; } get { return this.C1; } }

        public override object State(DrawingContext ctx, GameObject obj)
        {
            return new TextCacheState
            {
                Location = CalcLocation(obj),
                Text = prepareString(obj)
            };
        }

        private void internalDraw(DrawingContext ctx, Vector3 loc, string text)
        {
            Vector3 newVec = loc + new Vector3(0, 1, 0);
            ctx.Draw3DText(text, newVec, FontSize);
        }

        public override void DrawCache(DrawingContext ctx, object state)
        {
            TextCacheState drawItemState = state as TextCacheState;
            if (drawItemState == null)
                return;

            internalDraw(ctx, drawItemState.Location, drawItemState.Text);
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            string outString = prepareString(obj);
            ctx.Draw3DText(outString, CalcLocation(obj), FontSize);
        }
        public override bool Init(YamLikeConfig.Command cmd)
        {
            if (!base.Init(cmd))
            {
                return false;
            }

            if (cmd.UnnamedParams != null && cmd.UnnamedParams.Count > 0)
            {
                this.Text = cmd[0];
            }

            return true;
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

                    case "%zoneid%":
                        return ff14bot.Managers.WorldManager.ZoneId.ToString();
                }

                return m.Value;
            }));

            return resultString;
        }
    }
}
