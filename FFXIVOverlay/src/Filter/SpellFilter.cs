using ff14bot.Objects;
using System.Collections.Generic;

using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using YamLikeCommand = YamLikeConfig.Command;

namespace FFXIVOverlay.Command
{
    public class SpellFilter : ComplexDrawCommand
    {
        HashSet<uint> spells;

        public SpellFilter()
            : base()
        {
            Commands = new List<IDrawCommand>();
            spells = new HashSet<uint>();
        }

        public void AddSpell(uint s)
        {
            spells.Add(s);
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            Character c = obj as Character;
            if (c == null)
                return;

            if (!c.IsCasting)
                return;

            if (!spells.Contains(c.CastingSpellId))
                return;

            base.Drawing(ctx, obj);
        }

        public override bool Init(YamLikeCommand cmd)
        {
            if (cmd.SubCommand == null || cmd.SubCommand.Count == 0)
                return false;

            if (cmd.UnnamedParams == null || cmd.UnnamedParams.Count == 0)
                return false;

            List<uint> tmpSpells = new List<uint>();

            foreach (var q in cmd.UnnamedParams)
            {
                uint spellId = 0;
                if (uint.TryParse(q, out spellId))
                {
                    tmpSpells.Add(spellId);
                }
            }

            if (tmpSpells.Count == 0)
                return false;

            foreach(var s in tmpSpells)
            {
                spells.Add(s);
            }

            return base.Init(cmd);
        }
    }

}
