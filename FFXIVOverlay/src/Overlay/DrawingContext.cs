using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using ff14bot;
using System.Threading.Tasks;
using ff14bot.Managers;
using ff14bot.Overlay3D;
using SlimDX;
using SlimDX.Direct2D;
using SlimDX.Direct3D9;
using Font = SlimDX.Direct3D9.Font;
using Mesh = SlimDX.Direct3D9.Mesh;
using Vector3 = SlimDX.Vector3;
using Triangle = ff14bot.Managers.Triangle;
using System.Runtime.Caching;


namespace FFXIVOverlay.Overlay
{
    public class DrawingContext : IDisposable, I3DDrawer
    {
        private Vector3 Camera;

        internal void UpdateCameraLocation()
        {
            Camera = FF14Camera.CameraLocation;
        }

        private Sprite _fontSprite;
        private CacheItemPolicy CachePolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(3) };


        private VertexDeclaration _coloredVertexDecl;
        private ColoredVertex[] _vertexBuffer = new ColoredVertex[1000];
        private readonly int[] _indexBuffer = new int[1000];

        private static readonly ushort[] s_boxIndices =
{
            // front
            0, 1, 2, 2, 3, 0,
            // right
            1, 5, 6, 6, 2, 1,
            // back
            5, 4, 7, 7, 6, 5,
            // left
            4, 0, 3, 3, 7, 4,
            // top
            4, 5, 1, 1, 0, 4,
            // bottom
            3, 2, 6, 6, 7, 3
        };

        private static readonly ushort[] s_boxOutlineIndices =
        {
            //Top           [_]
            0, 1, 1, 3,
            3, 2, 2, 0,
            //Bottom        [_]
            6, 7, 7, 5,
            5, 4, 4, 6,
            // Back         | |
            0, 6, 1, 7,
            // Front        | |
            2, 4, 3, 5,
            // Left         | |
            0, 6, 2, 4,
            // Right        | |
            1, 7, 3, 5
        };


        public DrawingContext(Device device)
        {
            Device = device;
        }

        public Device Device { get; }

        public void Dispose()
        {
            _coloredVertexDecl?.Dispose();
        }

        private void SetDeclaration()
        {
            if (_coloredVertexDecl == null)
                _coloredVertexDecl = ColoredVertex.GetDecl(Device);

            Device.VertexDeclaration = _coloredVertexDecl;
            Device.VertexFormat = ColoredVertex.Format;
        }

        public void DrawCircleWithPoint(Clio.Utilities.Vector3 center, float heading, float radius, Color color, Color pointColor)
        {
            int slices = 30;
            var radsPerSlice = (float)(Math.PI * 2 / slices);

            var newCenter = new Vector3(center.X, center.Y, center.Z);

            _vertexBuffer[0] = new ColoredVertex(newCenter, color);
            _vertexBuffer[1] = new ColoredVertex(newCenter + new Vector3(radius, 0, 0), color);

            for (int i = 0; i < slices; i++)
            {
                double h = ((Math.PI * 2) - heading) + (Math.PI / 2);
                if (h > (Math.PI * 2))
                    h = h - (Math.PI * 2);

                bool watchAt = (i * radsPerSlice) < h && h < ((i + 1) * radsPerSlice);

                var sine = (float)Math.Sin((i + 1) * radsPerSlice);
                var cosine = (float)Math.Cos((i + 1) * radsPerSlice);

                _vertexBuffer[2 + i] =
                    new ColoredVertex(newCenter + new Vector3(cosine * radius, 0, sine * radius),
                        watchAt ? pointColor.ToArgb() : color.ToArgb());
            }

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleFan, slices, _vertexBuffer);
        }

        public void DrawCircleWithPoint(SlimDX.Vector3 center, float heading, float radius, Color color, Color pointColor)
        {
            int slices = 30;
            var radsPerSlice = (float)(Math.PI * 2 / slices);

            var newCenter = new Vector3(center.X, center.Y, center.Z);

            _vertexBuffer[0] = new ColoredVertex(newCenter, color);
            _vertexBuffer[1] = new ColoredVertex(newCenter + new Vector3(radius, 0, 0), color);

            for (int i = 0; i < slices; i++)
            {
                double h = ((Math.PI * 2) - heading) + (Math.PI / 2);
                if (h > (Math.PI * 2))
                    h = h - (Math.PI * 2);

                bool watchAt = (i * radsPerSlice) < h && h < ((i + 1) * radsPerSlice);

                var sine = (float)Math.Sin((i + 1) * radsPerSlice);
                var cosine = (float)Math.Cos((i + 1) * radsPerSlice);

                _vertexBuffer[2 + i] =
                    new ColoredVertex(newCenter + new Vector3(cosine * radius, 0, sine * radius),
                        watchAt ? pointColor.ToArgb() : color.ToArgb());
            }

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleFan, slices, _vertexBuffer);
        }

        public void DrawAgroLine(Clio.Utilities.Vector3 center, float heading, float width, float height, Color color, Color pointColor)
        {
            var newCenter = new Vector3(center.X, center.Y, center.Z);

            float heightBack = width;

            float diag = (float)Math.Sqrt(height * height + (width * width)/4);
            float diagBack = (float)Math.Sqrt(heightBack * heightBack + width * width) / 2;

            float subangle = (float)Math.Atan2(width / 2, height / 2);


            float h = (float)(((Math.PI * 2) - heading) + (Math.PI / 2));

            float r1 = h - subangle;
            float r2 = h + ((float)Math.PI / 4) + (float)Math.PI;
            float r3 = h - ((float)Math.PI / 4) + (float)Math.PI;
            float r4 = h + subangle;

            _vertexBuffer[0] = new ColoredVertex(newCenter, pointColor);
            _vertexBuffer[1] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r1) * diag, 0, (float)Math.Sin(r1) * diag), pointColor);
            _vertexBuffer[2] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r2) * diagBack, 0, (float)Math.Sin(r2) * diagBack), color);
            _vertexBuffer[3] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r3) * diagBack, 0, (float)Math.Sin(r3) * diagBack), color);
            _vertexBuffer[4] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r4) * diag, 0, (float)Math.Sin(r4) * diag), pointColor);
            _vertexBuffer[5] = _vertexBuffer[1];

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleFan, 4, _vertexBuffer);
        }

        public void DrawSideAttackAgroLine(Clio.Utilities.Vector3 center, float heading, float width, float height, Color color)
        {
            var newCenter = new Vector3(center.X, center.Y, center.Z);

            float diag = (float)Math.Sqrt(height * height + width * width) / 2;
            float subangle = (float)Math.Atan2(width / 2, height / 2);


            float h = (float)(((Math.PI * 2) - heading));

            float r1 = h - subangle;
            float r2 = h + subangle + (float)Math.PI;
            float r3 = h - subangle + (float)Math.PI;
            float r4 = h + subangle;

            _vertexBuffer[0] = new ColoredVertex(newCenter, color);
            _vertexBuffer[1] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r1) * diag, 0, (float)Math.Sin(r1) * diag), color);
            _vertexBuffer[2] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r2) * diag, 0, (float)Math.Sin(r2) * diag), color);
            _vertexBuffer[3] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r3) * diag, 0, (float)Math.Sin(r3) * diag), color);
            _vertexBuffer[4] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r4) * diag, 0, (float)Math.Sin(r4) * diag), color);
            _vertexBuffer[5] = _vertexBuffer[1];

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleFan, 4, _vertexBuffer);
        }
        public void DrawOutlinedBox(Vector3 center, Vector3 extents, Color color)
        {
            Vector3 min = center - extents;
            Vector3 max = center + extents;

            _vertexBuffer[0] = new ColoredVertex(new Vector3(min.X, max.Y, max.Z), color);
            _vertexBuffer[1] = new ColoredVertex(new Vector3(max.X, max.Y, max.Z), color);
            _vertexBuffer[2] = new ColoredVertex(new Vector3(min.X, min.Y, max.Z), color);
            _vertexBuffer[3] = new ColoredVertex(new Vector3(max.X, min.Y, max.Z), color);
            _vertexBuffer[4] = new ColoredVertex(new Vector3(min.X, min.Y, min.Z), color);
            _vertexBuffer[5] = new ColoredVertex(new Vector3(max.X, min.Y, min.Z), color);
            _vertexBuffer[6] = new ColoredVertex(new Vector3(min.X, max.Y, min.Z), color);
            _vertexBuffer[7] = new ColoredVertex(new Vector3(max.X, max.Y, min.Z), color);

            SetDeclaration();
            Device.DrawIndexedUserPrimitives(PrimitiveType.LineList, 0, 8, 12, s_boxOutlineIndices,
                Format.Index16, _vertexBuffer, 16);
        }

        public void DrawText(string text, int x, int y, Color color, float emSize = 12f,
    FontStyle fontStyle = FontStyle.Regular, bool begin = true)
        {
            if (_fontSprite == null)
            {
                _fontSprite = new Sprite(Device);
            }
            if (begin)
                _fontSprite.Begin(SpriteFlags.AlphaBlend);


            var key = $"{emSize} : {fontStyle}";
            Lazy<Font> fontValue = new Lazy<Font>(() => new Font(Device,
                new System.Drawing.Font(FontFamily.GenericSansSerif, emSize,
                    fontStyle)));

            var fnt = MemoryCache.Default.AddOrGetExisting(key, fontValue, CachePolicy);

            Font font;
            if (fnt == null)
                font = fontValue.Value;
            else
                font = ((Lazy<Font>)fnt).Value;

            var rows = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var cols = rows.Select(i => i.Length);

            var fontRectKey = $"{rows.Length} : {cols} : {key}";

            Lazy<Rectangle> stringRectLookup = new Lazy<Rectangle>(() => font.MeasureString(_fontSprite, text, DrawTextFormat.Left));

            var rec = MemoryCache.Default.AddOrGetExisting(fontRectKey, stringRectLookup, CachePolicy);

            Rectangle stringRect;
            if (rec == null)
                stringRect = stringRectLookup.Value;
            else
                stringRect = ((Lazy<Rectangle>)rec).Value;


            Rectangle rect = new Rectangle(x, y, stringRect.Width + 1, stringRect.Height + 1);

            font.DrawString(_fontSprite, text, rect, DrawTextFormat.Left, color.ToArgb());
            if (begin)
                _fontSprite.End();
        }

        public void Draw3DText(string text, Vector3 textPos, float emSize = 12f,
            FontStyle fontStyle = FontStyle.Regular)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException(nameof(text));
            if (text == Core.Me.Name)
                text = "ME";
            var cam = Camera;
            var up = new Vector3(0, 1, 0);
            var fwd = new Vector3(0, 0, 1);

            var mtx = Conversions.BillboardLh(ref textPos, ref cam, ref up, ref fwd);

            //mtx.Invert();
            Device.SetTransform(TransformState.World, mtx);

            var meshKey = $"mesh : {text}";

            Lazy<Mesh> meshLookup = new Lazy<Mesh>(() => Mesh.CreateText(Device,
                new System.Drawing.Font("Verdana", emSize, fontStyle), text,
                0.001f, 0.01f));

            var rec = MemoryCache.Default.AddOrGetExisting(meshKey, meshLookup, CachePolicy);

            Mesh m;
            if (rec == null)
            {
                m = meshLookup.Value;
            }
            else
            {
                m = ((Lazy<Mesh>)rec).Value;
            }

            m.DrawSubset(0);

            Device.SetTransform(TransformState.World, Matrix.Identity);
        }

        public void DrawOutlinedText(string text, int x, int y, Color color, Color shadowColor,
            float emSize = 12f, FontStyle fontStyle = FontStyle.Regular)
        {
            if (_fontSprite == null)
            {
                _fontSprite = new Sprite(Device);
            }
            _fontSprite.Begin(SpriteFlags.AlphaBlend);
            for (int yo = -1; yo <= 1; yo++)
            {
                for (int xo = -1; xo <= 1; xo++)
                {
                    if (xo == 0 && yo == 0)
                        continue;

                    DrawText(text, x + xo, y + yo, shadowColor, emSize, fontStyle, false);
                }
            }

            DrawText(text, x, y, color, emSize, fontStyle, false);

            _fontSprite.End();
        }

        public void DrawLine(Clio.Utilities.Vector3 start, Clio.Utilities.Vector3 end, Color color)
        {
            DrawLine(start.Convert(), end.Convert(), color);
        }
        public void DrawLine(Vector3 start, Vector3 end, Color color, float width = 0.025f)
        {
            Vector3 dir = end - start;
            dir.Z = 0;

            Vector3 extDir1;
            Vector3 extDir2;
            if (dir.LengthSquared() > 0.0001f)
            {
                dir.Normalize();

                extDir1.X = -dir.Y;
                extDir1.Y = dir.X;
                extDir1.Z = 0;

                extDir2 = Vector3.Cross(dir, extDir1);
            }
            else
            {
                extDir1 = Vector3.UnitX;
                extDir2 = Vector3.UnitY;
            }

            _vertexBuffer[0] = new ColoredVertex(start + extDir1 * (width / 2), color);
            _vertexBuffer[1] = new ColoredVertex(start - extDir1 * (width / 2), color);
            _vertexBuffer[2] = new ColoredVertex(end + extDir1 * (width / 2), color);
            _vertexBuffer[3] = new ColoredVertex(end - extDir1 * (width / 2), color);

            _vertexBuffer[4] = new ColoredVertex(start + extDir2 * (width / 2), color);
            _vertexBuffer[5] = new ColoredVertex(start - extDir2 * (width / 2), color);
            _vertexBuffer[6] = new ColoredVertex(end + extDir2 * (width / 2), color);
            _vertexBuffer[7] = new ColoredVertex(end - extDir2 * (width / 2), color);

            _indexBuffer[0] = 0;
            _indexBuffer[1] = 1;
            _indexBuffer[2] = 2;

            _indexBuffer[3] = 1;
            _indexBuffer[4] = 2;
            _indexBuffer[5] = 3;

            _indexBuffer[6] = 4;
            _indexBuffer[7] = 5;
            _indexBuffer[8] = 6;

            _indexBuffer[9] = 5;
            _indexBuffer[10] = 6;
            _indexBuffer[11] = 7;

            SetDeclaration();
            Device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0, 8, 4, _indexBuffer, Format.Index32, _vertexBuffer, ColoredVertex.Stride);
        }

        public void DrawTriangles(Clio.Utilities.Vector3[] verts, Color color)
        {
            if (verts.Length == 0)
                return;

            if (verts.Length > _vertexBuffer.Length)
                Array.Resize(ref _vertexBuffer, verts.Length);

            if (verts.Length > 10000)
            {
                Parallel.For(0, verts.Length, i => _vertexBuffer[i] = new ColoredVertex(verts[i].Convert(), color));
            }
            else
            {
                for (int i = 0; i < verts.Length; i++)
                    _vertexBuffer[i] = new ColoredVertex(verts[i].Convert(), color);
            }

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleList, verts.Length / 3, _vertexBuffer);
        }

        public void DrawTriangles(List<Clio.Utilities.Vector3> verts, Color color)
        {
            if (verts.Count == 0)
                return;

            if (verts.Count > _vertexBuffer.Length)
                Array.Resize(ref _vertexBuffer, verts.Count); ;

            if (verts.Count > 10000)
            {
                Parallel.For(0, verts.Count, i => _vertexBuffer[i] = new ColoredVertex(verts[i].Convert(), color));
            }
            else
            {
                for (int i = 0; i < verts.Count; i++)
                    _vertexBuffer[i] = new ColoredVertex(verts[i].Convert(), color);
            }



            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleList, verts.Count / 3, _vertexBuffer);
        }

        public void DrawTriangleFan(Clio.Utilities.Vector3[] poly, int index, int count, Color color)
        {
            for (int i = 0; i < count; i++)
                _vertexBuffer[i] = new ColoredVertex(poly[index + i].Convert(), color);

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleFan, count - 2, _vertexBuffer);
        }

        public void DrawBox(Clio.Utilities.Vector3 center, Clio.Utilities.Vector3 extents, Color color)
        {
            DrawBox(center.Convert(), extents.Convert(), color);
        }

        public void DrawBoxOutline(Clio.Utilities.Vector3 center, Clio.Utilities.Vector3 extents, Color color)
        {
            DrawOutlinedBox(center.Convert(), extents.Convert(), color);
        }

        public void DrawCircle(Clio.Utilities.Vector3 center, float radius, Color color)
        {
            int slices = 30;
            var radsPerSlice = (float)(Math.PI * 2 / slices);

            var newCenter = new Vector3(center.X, center.Y, center.Z);

            _vertexBuffer[0] = new ColoredVertex(newCenter, color);
            _vertexBuffer[1] = new ColoredVertex(newCenter + new Vector3(radius, 0, 0), color);

            for (int i = 0; i < slices; i++)
            {
                var sine = (float)Math.Sin((i + 1) * radsPerSlice);
                var cosine = (float)Math.Cos((i + 1) * radsPerSlice);

                _vertexBuffer[2 + i] =
                    new ColoredVertex(newCenter + new Vector3(cosine * radius, 0, sine * radius),
                        color.ToArgb());
            }

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleFan, slices, _vertexBuffer);
        }
        public void DrawCircleOutline(Clio.Utilities.Vector3 center, float radius, Color color)
        {
            int slices = 30;
            var radsPerSlice = (float)(Math.PI * 2 / slices);

            var newCenter = new Vector3(center.X, center.Y, center.Z);

            for (int i = 0; i < slices; i++)
            {
                var sine = (float)Math.Sin((i + 1) * radsPerSlice);
                var cosine = (float)Math.Cos((i + 1) * radsPerSlice);

                _vertexBuffer[i] =
                    new ColoredVertex(newCenter + new Vector3(cosine * radius, 0, sine * radius),
                        color.ToArgb());
            }

            _vertexBuffer[slices] = _vertexBuffer[0];

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.LineStrip, slices, _vertexBuffer);
        }

        public void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            _vertexBuffer[0] = new ColoredVertex(a, color);
            _vertexBuffer[1] = new ColoredVertex(b, color);
            _vertexBuffer[2] = new ColoredVertex(c, color);

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, _vertexBuffer);
        }

        public void DrawBox(Vector3 center, Vector3 extents, Color color)
        {
            Vector3 min = center - extents;
            Vector3 max = center + extents;

            _vertexBuffer[0] = new ColoredVertex(new Vector3(min.X, max.Y, max.Z), color);
            _vertexBuffer[1] = new ColoredVertex(new Vector3(max.X, max.Y, max.Z), color);
            _vertexBuffer[2] = new ColoredVertex(new Vector3(max.X, min.Y, max.Z), color);
            _vertexBuffer[3] = new ColoredVertex(new Vector3(min.X, min.Y, max.Z), color);
            _vertexBuffer[4] = new ColoredVertex(new Vector3(min.X, max.Y, min.Z), color);
            _vertexBuffer[5] = new ColoredVertex(new Vector3(max.X, max.Y, min.Z), color);
            _vertexBuffer[6] = new ColoredVertex(new Vector3(max.X, min.Y, min.Z), color);
            _vertexBuffer[7] = new ColoredVertex(new Vector3(min.X, min.Y, min.Z), color);

            SetDeclaration();
            Device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0, 8, 12,
                s_boxIndices,
                Format.Index16, _vertexBuffer, 16);
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct ColoredVertex
        {
            public ColoredVertex(Vector3 position, int color)
            {
                Position = position;
                Color = color;
            }

            public ColoredVertex(Vector3 position, Color color)
                : this(position, color.ToArgb())
            {
            }

            public Vector3 Position;
            public int Color;

            public static int Stride => sizeof(ColoredVertex);
            public static VertexFormat Format => VertexFormat.Position | VertexFormat.Diffuse;

            public static VertexDeclaration GetDecl(Device device)
            {
                return new VertexDeclaration(device, new[]
                {
                    new VertexElement(0, 0,
                        DeclarationType.Float3,
                        DeclarationMethod.Default,
                        DeclarationUsage.Position,
                        0),
                    new VertexElement(0, 12,
                        DeclarationType.Color,
                        DeclarationMethod.Default,
                        DeclarationUsage.Color, 0),
                    VertexElement.VertexDeclarationEnd
                });
            }
        }

    }



    internal static class Conversions
    {
        public static Vector3 Convert(this Clio.Utilities.Vector3 v) => new Vector3(v.X, v.Y, v.Z);
        public static Vector3 Convert(this System.Numerics.Vector3 v) => new Vector3(v.X, v.Y, v.Z);
        public static System.Numerics.Vector3 ConvertNumerics(this Vector3 v) => new System.Numerics.Vector3(v.X, v.Y, v.Z);

        public static Clio.Utilities.Vector3 Convert(this Vector3 v)
            => new Clio.Utilities.Vector3(v.X, v.Y, v.Z);

        public static Color Convert(this System.Windows.Media.Color c)
            => Color.FromArgb(c.A, c.R, c.G, c.B);

        public const float ZeroTolerance = 1e-6f;

        /// <summary>
        /// Creates a right-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        public static Matrix BillboardLh(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 cameraUpVector, ref Vector3 cameraForwardVector)
        {
            var result = new Matrix();
            Vector3 crossed;
            Vector3 final;
            Vector3 difference = cameraPosition - objectPosition;

            float lengthSq = difference.LengthSquared();
            if (IsZero(lengthSq))
                difference = -cameraForwardVector;
            else
                difference *= (float)(1.0 / Math.Sqrt(lengthSq));

            Vector3.Cross(ref cameraUpVector, ref difference, out crossed);
            crossed.Normalize();
            Vector3.Cross(ref difference, ref crossed, out final);

            result.M11 = crossed.X;
            result.M12 = crossed.Y;
            result.M13 = crossed.Z;
            result.M14 = 0.0f;

            result.M21 = final.X;
            result.M22 = final.Y;
            result.M23 = final.Z;
            result.M24 = 0.0f;

            result.M31 = difference.X;
            result.M32 = difference.Y;
            result.M33 = difference.Z;
            result.M34 = 0.0f;

            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1.0f;

            return result;

        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to zero (0.0f); otherwise, <c>false</c>.</returns>
        public static bool IsZero(float a)
        {
            return Math.Abs(a) < ZeroTolerance;
        }

    }
}