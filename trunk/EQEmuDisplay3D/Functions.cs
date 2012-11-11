using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace EQEmuDisplay3D
{
    public static class Functions
    {
        public static ImageBrush OverlayImage(ImageSource overlay, ImageSource baseImage)
        {
            ImageBrush brush = null;
            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();
            var rect = new System.Windows.Rect(new System.Windows.Size(baseImage.Width,baseImage.Height));
            dc.DrawImage(baseImage,rect);
            dc.DrawImage(overlay, rect);

            var bmp = new RenderTargetBitmap( (int)rect.Width, (int)rect.Height, 120, 96, PixelFormats.Pbgra32);
            dc.Close();
            bmp.Render(dv);
            brush = new ImageBrush(bmp);

            return brush;
        }

        public static BitmapImage BitmapToImageSource(System.Drawing.Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Position = 0;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                return bi;
            }
        }
    }
}
