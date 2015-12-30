using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Musick.Musick_Classes
{
    class ImageConvert
    {
        public static BitmapSource convert (System.Drawing.Bitmap imageToConvert)
        {
            var bitmap = new System.Drawing.Bitmap(imageToConvert);
            BitmapSource convertedImage = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmap.Dispose();
            return convertedImage;        
        }
    }
}
