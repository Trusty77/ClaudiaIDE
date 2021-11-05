using System;
using System.IO;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using ClaudiaIDE.Helpers;

namespace ClaudiaIDE.ImageProvider
{
    public class SingleImageProvider : IImageProvider
    {
        private BitmapImage _bitmap;
        private Setting _setting;

        public SingleImageProvider(Setting setting, string solutionfile = null)
        {
            SolutionConfigFile = solutionfile;
            _setting = setting;
            _setting.OnChanged.AddEventHandler(ReloadSettings);

            LoadImage();
        }

        ~SingleImageProvider()
        {
            if (_setting != null)
            {
                _setting.OnChanged.RemoveEventHandler(ReloadSettings);
            }
        }

        private void ReloadSettings(object sender, System.EventArgs e)
        {
            LoadImage();
            this.NewImageAvailable?.Invoke(this, EventArgs.Empty);
        }


        public BitmapSource GetBitmap()
        {
            BitmapSource ret_bitmap = _bitmap;
            if (_setting.ImageStretch == ImageStretch.None &&
                    (_bitmap.Width != _bitmap.PixelWidth || _bitmap.Height != _bitmap.PixelHeight)
                )
            {
                ret_bitmap = Utils.ConvertToDpi96(_bitmap);
            }
            
            if (_setting.SoftEdgeX > 0 || _setting.SoftEdgeY > 0)
            {
                ret_bitmap = Utils.SoftenEdges(ret_bitmap, _setting.SoftEdgeX, _setting.SoftEdgeY);
            }

            return ret_bitmap;
        }

        private void LoadImage()
        {
            // Try to remplace the constant string $profileFolder by the directory of the current settings file.
            // Works as usual if this string is present in the filename.
            string filename = this._setting.BackgroundImageAbsolutePath;
            if (this._setting.SolutionConfigFilePath != null)
            {
                int pos = filename.IndexOf("$profileFolder");
                if (pos != -1)
                {
                    filename = filename.Remove(0, pos + 14);
                    filename = Path.GetDirectoryName(this._setting.SolutionConfigFilePath) + filename;
                }
            }

            var fileUri = new Uri(filename, UriKind.RelativeOrAbsolute);
            var fileInfo = new FileInfo(filename);

            if (fileInfo.Exists)
            {
                _bitmap = new BitmapImage();
                _bitmap.BeginInit();
                _bitmap.CacheOption = BitmapCacheOption.OnLoad;
                _bitmap.CreateOptions = BitmapCreateOptions.None;
                _bitmap.UriSource = fileUri;
                _bitmap.EndInit();
                _bitmap.Freeze();

                if (_setting.ImageStretch == ImageStretch.None)
                {
                    _bitmap = Utils.EnsureMaxWidthHeight(_bitmap, _setting.MaxWidth, _setting.MaxHeight);
                }
            }
            else
            {
                _bitmap = null;
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
