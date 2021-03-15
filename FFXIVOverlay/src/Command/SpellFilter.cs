using ff14bot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;

namespace FFXIVOverlay.Command
{
    public class SpellFilter : IDrawCommand
    {
        public uint SpellId = 0xFFFFFFFF;
        HashSet<uint> spells;

        public SpellFilter()
        {
            Commands = new List<IDrawCommand>();
            spells = new HashSet<uint>();
        }

        public void AddSpell(uint s)
        {
            spells.Add(s);
        }

        public void AddDrawItem(IDrawCommand cmd)
        {
            Commands.Add(cmd);
        }
        public List<IDrawCommand> Commands { get; internal set; }

        public void Drawing(DrawingContext ctx, GameObject obj)
        {
            Character c = obj as Character;
            if (c == null)
                return;

            if (!c.IsCasting)
                return;

            if (!spells.Contains(c.CastingSpellId))
                return;

            foreach (var dc in Commands)
            {
                dc.Drawing(ctx, obj);
            }
        }
    }

}
