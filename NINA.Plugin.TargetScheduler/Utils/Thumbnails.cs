using NINA.Plugin.TargetScheduler.Shared.Utility;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NINA.Plugin.TargetScheduler.Util {

    public class Thumbnails {

        /// <summary>
        /// Create a thumbnail of an acquired image for persisting with AcquiredImage rows.
        ///
        /// Cribbed from Lightbucket plugin (https://github.com/lightbucket-co/lightbucket-nina-plugin)
        /// </summary>
        /// <param name="imageSource"></param>
        /// <returns></returns>
        public static (int, int, byte[]) CreateThumbnail(BitmapSource imageSource) {
            try {
                double scaleFactor = 192 / imageSource.Height;
                BitmapSource resizedBitmap = new TransformedBitmap(imageSource, new ScaleTransform(scaleFactor, scaleFactor));
                int width = (int)resizedBitmap.Width;
                int height = (int)resizedBitmap.Height;
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 100;
                encoder.Frames.Add(BitmapFrame.Create(resizedBitmap));

                using (MemoryStream ms = new MemoryStream()) {
                    encoder.Save(ms);
                    return (width, height, ms.ToArray());
                }
            } catch (Exception ex) {
                TSLogger.Error($"error creating image thumbnail: {ex.Message}\n{ex.StackTrace}");
                return (0, 0, null);
            }
        }

        /// <summary>
        /// Convert image data to a form suitable for WPF display.
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public static BitmapImage RestoreThumbnail(byte[] imageData) {
            try {
                using (MemoryStream ms = new MemoryStream(imageData)) {
                    BitmapImage btm = new BitmapImage();
                    btm.BeginInit();
                    btm.StreamSource = ms;
                    btm.CacheOption = BitmapCacheOption.OnLoad;
                    btm.EndInit();
                    btm.Freeze();
                    return btm;
                }
            } catch (Exception ex) {
                TSLogger.Error($"error restoring image thumbnail: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private Thumbnails() {
        }
    }
}