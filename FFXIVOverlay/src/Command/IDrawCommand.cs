using ff14bot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;

namespace FFXIVOverlay.Command
{
    public interface IDrawCommand
    {
        void Drawing(DrawingContext ctx, GameObject obj);
    }

    public class ComplexDrawCommand : IDrawCommand
    {
        public ComplexDrawCommand()
        {
            Commands = new List<IDrawCommand>();
        }

        public virtual void AddDrawItem(IDrawCommand cmd)
        {
            Commands.Add(cmd);
        }

        public List<IDrawCommand> Commands { get; internal set; }
        public virtual void Drawing(DrawingContext ctx, GameObject obj)
        {
            foreach(var c in Commands)
            {
                c.Drawing(ctx, obj);
            }
        }
    }

}
