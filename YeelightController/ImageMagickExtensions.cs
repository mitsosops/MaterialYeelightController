using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightController
{
    public static class ImageMagickExtensions
    {
        public static MagickImage ToMagickImage(this Bitmap bmp)
        {
            MagickImage img = null;
            MagickFactory f = new MagickFactory();
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;
                img = new MagickImage(f.Image.Create(ms));
            }
            return img;
        }
    }
}
