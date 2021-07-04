using ff14bot.Objects;
using FFXIVOverlay.Overlay;
using System;
using System.Drawing;
using Vector3 = SlimDX.Vector3;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;

namespace FFXIVOverlay.Command
{
    public class DrawItemState
    {
        public Vector3 Center;
        public float Heading = 0f;
    }

    public class DrawItemBase : IDrawCommand, ICacheDrawItem, IDrawCommandCreator
    {
        public bool FixCenter = false;
        public Vector3 Center;

        public bool FixHeading = false;
        public float Heading = 0f;
        public float HeadingOffset = 0f;

        public float R1 = 3f;
        public float R2 = 10f;

        public Color C1 = DefaultColors.GreenColor;
        public Color C2 = DefaultColors.RedColor;

        public float Angle = 90f;
        public float AngleOffset = 0f; // unused
        public string Text = string.Empty;

        public bool UseSpellLocation = false;
        public bool UseTargetLocation = false;

        string cacheKey;
        public DrawItemBase()
        {
            cacheKey = Guid.NewGuid().ToString();
        }

        public virtual bool CanDrawing(DrawingContext ctx, GameObject obj)
        {
            if (!FixCenter && obj == null)
            {
                return false;
            }

            return true;
        }

        public virtual string Key(DrawingContext ctx, GameObject obj)
        {
            return string.Format(
                "{0}{1}{2}", cacheKey
                , (FixCenter ? Center : obj.Location.Convert()).ToShortXYZString()
                , (FixHeading ? Heading : obj.Heading).ToString("F4")
            );
        }

        public virtual float CalcHeading(GameObject obj)
        {
            return FixHeading
                    ? Heading
                    : (obj.Heading + (((float)Math.PI / 180f) * HeadingOffset));
        }

        public virtual Vector3 CalcLocation(GameObject obj)
        {
            if (FixCenter)
            {
                return Center;
            }

            if (UseSpellLocation)
            {
                Character c = obj as Character;
                if (c != null)
                {
                    if (c.IsCasting && c.SpellCastInfo != null)
                    {
                        return c.SpellCastInfo.CastLocation.Convert();
                    }
                }
            }

            return obj.Location.Convert();
        }

        public virtual object State(DrawingContext ctx, GameObject obj)
        {
            return new DrawItemState
            {
                Center = CalcLocation(obj)
                , Heading = CalcHeading(obj)
            };
        }
        public virtual void DrawCache(DrawingContext ctx, object state)
        {
            DrawItemState drawItemState = state as DrawItemState;
            if (drawItemState == null)
                return;

            internalDrawingCache(ctx, drawItemState);
        }

        public virtual void Drawing(DrawingContext ctx, GameObject obj)
        {
        }

        public virtual void internalDrawingCache(DrawingContext ctx, DrawItemState state)
        {
        }

        public virtual bool Init(YamLikeConfig.Command cmd)
        {
            Vector3 vec3;
            float tmpF;

            if (cmd.has("XYZ"))
            {
                if (cmd["XYZ"].TryParse(out vec3))
                {
                    this.Center = vec3;
                    this.FixCenter = true;
                }
            }

            if (cmd.has("xyz"))
            {
                if (cmd["xyz"].TryParse(out vec3))
                {
                    this.Center = vec3;
                    this.FixCenter = true;
                }
            }

            if (cmd.has("center"))
            {
                if (cmd["center"].TryParse(out vec3))
                {
                    this.Center = vec3;
                    this.FixCenter = true;
                }
            }

            if (cmd.tryGet("heading", out tmpF))
            {
                this.Heading = tmpF;
                this.FixHeading = true;
            }

            if (cmd.tryGet("headingOffset", out tmpF))
            {
                this.HeadingOffset = tmpF;
            }

            if (cmd.tryGet("offset", out tmpF))
            {
                this.HeadingOffset = tmpF;
            }

            if (cmd.tryGet("r1", out tmpF))
            {
                this.R1 = tmpF;
            }

            if (cmd.tryGet("radius", out tmpF))
            {
                this.R1 = tmpF;
            }

            if (cmd.tryGet("Radius", out tmpF))
            {
                this.R1 = tmpF;
            }

            if (cmd.tryGet("radiusIn", out tmpF))
            {
                this.R1 = tmpF;
            }

            if (cmd.tryGet("width", out tmpF))
            {
                this.R1 = tmpF;
            }

            if (cmd.tryGet("r2", out tmpF))
            {
                this.R2 = tmpF;
            }

            if (cmd.tryGet("radiusOut", out tmpF))
            {
                this.R2 = tmpF;
            }

            if (cmd.tryGet("length", out tmpF))
            {
                this.R2 = tmpF;
            }

            System.Drawing.Color tmpColor;
            if (cmd.tryGet("c1", out tmpColor))
            {
                this.C1 = tmpColor;
            }

            if (cmd.tryGet("color", out tmpColor))
            {
                this.C1 = tmpColor;
            }

            if (cmd.tryGet("colorIn", out tmpColor))
            {
                this.C1 = tmpColor;
            }

            if (cmd.tryGet("c2", out tmpColor))
            {
                this.C2 = tmpColor;
            }

            if (cmd.tryGet("colorOut", out tmpColor))
            {
                this.C2 = tmpColor;
            }

            if (cmd.tryGet("rangecolor", out tmpColor))
            {
                this.C2 = tmpColor;
            }

            if (cmd.tryGet("point", out tmpColor))
            {
                this.C2 = tmpColor;
            }

            if (cmd.tryGet("angle", out tmpF))
            {
                this.Angle = tmpF;
            }

            if (cmd.tryGet("angleOffset", out tmpF))
            {
                this.AngleOffset = tmpF;
            }

            if (cmd.has("format"))
            {
                this.Text = cmd["format"];
            }

            if (cmd.has("spell_location") && cmd["spell_location"] == "1")
            {
                this.UseSpellLocation = true;
            }

            if (cmd.has("target_location") && cmd["target_location"] == "1")
            {
                this.UseTargetLocation = true;
            }

            return true;
        }
    }
}
