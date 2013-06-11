using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace MingPluginInterfaces
{
    public static class Utilities
    {
        public static BitmapImage BitmapImageFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.StreamSource = ms;
                bmi.EndInit();
                bmi.Freeze();
                
                return bmi;
            }
        }

        public static string LogException(Exception ex)
        {
            try
            {
                var tempPath = Path.GetTempFileName();

                using (StreamWriter sw = new StreamWriter(tempPath))
                {
                    var e = ex;
                    while (e != null)
                    {
                        sw.WriteLine(e.ToString());
                        sw.WriteLine();
                        e = e.InnerException;
                    }
                    sw.Close();
                }

                return tempPath;
            }
            catch 
            {
            }

            return null;
        }
    }
}
