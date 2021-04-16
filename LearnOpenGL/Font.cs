using Silk.NET.OpenGL;
using System.Drawing;
using System.Runtime.InteropServices;
using SysFont = System.Drawing.Font;

namespace LearnOpenGL
{
    class Font
    {
        uint _handle;
        GL _gl;

        public Font(string text, string fontname, int fontsize, GL gl)
        {
            _gl = gl;

            Bitmap bmp = CreateTexture(text, fontname, fontsize);
            CreateGLTexture(bmp);
        }

        private Bitmap CreateTexture(string text, string fontname, int fontsize)
        {
            Bitmap bmp = new Bitmap(500, 500);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (FontFamily fontFamily = new FontFamily("Arial"))
                {
                    using (SysFont font = new SysFont(fontFamily, 475, FontStyle.Regular, GraphicsUnit.Pixel))
                    {
                        using (SolidBrush solidBrush = new SolidBrush(Color.White))
                        {
                            g.DrawString("A", font, solidBrush, new PointF(10, 10));
                        }
                    }
                }
            }

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }

        private unsafe void CreateGLTexture(Bitmap bmp)
        {
            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var length = bitmapData.Stride * bitmapData.Height;

            byte[] bytes = new byte[length];

            Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
            bmp.UnlockBits(bitmapData);

            fixed (void* d = &bytes[0])
            {
                Load(d, (uint)bmp.Width, (uint)bmp.Height);
            }
        }

        private unsafe void Load(void* data, uint width, uint height)
        {
            _handle = _gl.GenTexture();
            Bind();

            _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }      

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            _gl.ActiveTexture(textureSlot);
            _gl.BindTexture(TextureTarget.Texture2D, _handle);
        }
    }
}
