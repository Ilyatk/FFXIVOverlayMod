using ff14bot.Objects;
using System.Collections.Generic;

using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using YamLikeCommand = YamLikeConfig.Command;

namespace FFXIVOverlay.Command
{
    public interface IDrawCommand
    {
        void Drawing(DrawingContext ctx, GameObject obj);
    }

    public interface IDrawCommandCreator
    {
        bool Init(YamLikeCommand cmd);
    }

    public class ComplexDrawCommand : IDrawCommand, IDrawCommandCreator
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

        public virtual bool Init(YamLikeCommand cmd)
        {
            if (cmd.SubCommand == null || cmd.SubCommand.Count == 0)
                return false;

            List<IDrawCommand> tmp = new List<IDrawCommand>();
            foreach (var c in cmd.SubCommand)
            {
                IDrawCommand drawingCmd = CommandDrawingFactoryEx.create(c);
                if (drawingCmd == null)
                    continue;

                tmp.Add(drawingCmd);
            }

            if (tmp.Count == 0)
                return false;

            this.Commands.AddRange(tmp);

            return true;
        }
    }

}
