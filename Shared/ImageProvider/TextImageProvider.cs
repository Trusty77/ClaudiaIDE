using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;

using Color = System.Windows.Media.Color;
using Point = System.Drawing.Point;

namespace ClaudiaIDE.ImageProvider
{
    public class TextImageProvider : IImageProvider
    {
        private BitmapImage _bitmap;
        private Setting _setting;

        public TextImageProvider(Setting setting, string solutionfile = null)
        {
            this.SolutionConfigFile = solutionfile;
            this._setting = setting;
            this._setting.OnChanged.AddEventHandler(this.ReloadSettings);

            this.LoadImage();
        }

        ~TextImageProvider()
        {
            if (this._setting != null)
            {
                this._setting.OnChanged.RemoveEventHandler(this.ReloadSettings);
            }
        }

        private void ReloadSettings(object sender, System.EventArgs e)
        {
            this.LoadImage();
            NewImageAvailable?.Invoke(this, EventArgs.Empty);
        }

        public BitmapSource GetBitmap()
        {
            if (this._bitmap == null)
                return null;
            BitmapSource ret_bitmap = this._bitmap;
            if (this._setting.ImageStretch == ImageStretch.None &&
                (this._bitmap.Width != this._bitmap.PixelWidth || this._bitmap.Height != this._bitmap.PixelHeight)
            )
            {
                ret_bitmap = Utils.ConvertToDpi96(this._bitmap);
            }

            if (this._setting.SoftEdgeX > 0 || this._setting.SoftEdgeY > 0)
            {
                ret_bitmap = Utils.SoftenEdges(ret_bitmap, this._setting.SoftEdgeX, this._setting.SoftEdgeY);
            }

            return ret_bitmap;
        }

        private void LoadImage()
        {
            var text = new FormattedText(_setting.BackgroundText,
                new CultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(new System.Windows.Media.FontFamily("Calibri"), FontStyles.Normal, FontWeights.Bold, new FontStretch()),
                _setting.TextSize,
                System.Windows.Media.Brushes.LightGray/* _setting.Foreground*/,
                1.00);

            var drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawText(text, new System.Windows.Point(0, 0));
            drawingContext.Close();

            if (text.Width > 0 && text.Height > 0)
            {

                var rtBmp = new RenderTargetBitmap((int)text.Width, (int)text.Height, 96, 96, PixelFormats.Pbgra32);
                rtBmp.Render(drawingVisual);

                var bitmapEncoder = new PngBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(rtBmp));

                using (var stream = new MemoryStream())
                {
                    bitmapEncoder.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    this._bitmap = new BitmapImage();
                    this._bitmap.BeginInit();
                    this._bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    this._bitmap.StreamSource = stream;
                    this._bitmap.EndInit();
                }
            }
        }

        public event EventHandler NewImageAvailable;

        public ImageBackgroundType ProviderType
        {
            get
            {
                return ImageBackgroundType.Single;
            }
        }

        public string SolutionConfigFile { get; private set; }
    }
}
