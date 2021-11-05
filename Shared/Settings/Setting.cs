using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace ClaudiaIDE.Settings
{
    public class Setting
    {
        private static readonly List<Setting> _instance = new List<Setting>();
        private static readonly string CONFIGFILE = "config.txt";
        private const string DefaultBackgroundImage = "Images\\background.png";
        private const string DefaultBackgroundFolder = "Images";

        internal DTE ServiceProvider { get; set; }

        [IgnoreDataMember]
        public WeakEvent<EventArgs> OnChanged = new WeakEvent<EventArgs>();

        [IgnoreDataMember]
        public string SolutionConfigFilePath { get; set; }

        public static Setting Instance
        {
            get
            {
                var solfile = VisualStudioUtility.GetSolutionSettingsFileFullPath();
                var i = _instance?.FirstOrDefault(x => x.SolutionConfigFilePath == solfile);
                if (i == null)
                {
                    i = new Setting
                    {
                        SolutionConfigFilePath = solfile
                    };
                    _instance.Add(i);
                }
                return  i;
            }
        }

        public static Setting DefaultInstance
        {
            get
            {
                var i = _instance?.FirstOrDefault(x => x.SolutionConfigFilePath == null);
                if (i == null)
                {
                    i = new Setting();
                    _instance.Add(i);
                }
                return i;
            }
        }


        public Setting()
        {
            var assemblylocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            BackgroundImagesDirectoryAbsolutePath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, DefaultBackgroundFolder);
            BackgroundImageAbsolutePath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, DefaultBackgroundImage);
            BackgroundText = string.Empty;
            TextSize = 108;
            Opacity = 0.35;
            PositionHorizon = PositionH.Right;
            PositionVertical = PositionV.Bottom;
            ImageStretch = ImageStretch.None;
            UpdateImageInterval = TimeSpan.FromMinutes(30);
            ImageFadeAnimationInterval = TimeSpan.FromSeconds(0);
            Extensions = ".png, .jpg";
            ImageBackgroundType = ImageBackgroundType.Single;
            LoopSlideshow = true;
            ShuffleSlideshow = false;
            MaxWidth = 0;
            MaxHeight = 0;
            SoftEdgeX = 0;
            SoftEdgeY = 0;
            ExpandToIDE = false;
            ViewBoxPointX = 0;
            ViewBoxPointY = 0;
            IsLimitToMainlyEditorWindow = false;
        }

        public ImageBackgroundType ImageBackgroundType { get; set; }
        public string BackgroundText { get; set; }
        public int TextSize { get; set; }
        public double Opacity { get; set; }
        public PositionV PositionVertical { get; set; }
        public PositionH PositionHorizon { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public int SoftEdgeX { get; set; }
        public int SoftEdgeY { get; set; }
        
        public string BackgroundImageAbsolutePath { get; set; }

        public TimeSpan UpdateImageInterval { get; set; }
        public TimeSpan ImageFadeAnimationInterval { get; set; }
        public string BackgroundImagesDirectoryAbsolutePath { get; set; }
        public string Extensions { get; set; }
        public bool LoopSlideshow { get; set; }
        public bool ShuffleSlideshow { get; set; }
        public ImageStretch ImageStretch { get; set; }
        public bool ExpandToIDE { get; set; }
        public double ViewBoxPointX { get; set; }
        public double ViewBoxPointY { get; set; }
        public bool IsLimitToMainlyEditorWindow { get; set; }

        public void Serialize()
        {
            var config = JsonSerializer<Setting>.Serialize(this);

            var assemblylocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var configpath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, CONFIGFILE);

            using (var s = new StreamWriter(configpath, false, Encoding.UTF8))
            {
                s.Write(config);
                s.Close();
            }
        }

        public void Serialize(string solutionConfigPath)
        {
            var config = JsonSerializer<Setting>.Serialize(this);

            using (var s = new StreamWriter(solutionConfigPath, false, Encoding.UTF8))
            {
                s.Write(config);
                s.Close();
            }
        }

        public static Setting Initialize(DTE serviceProvider)
        {
            var settings = Setting.Instance;
            if (settings.ServiceProvider == null || settings.ServiceProvider != serviceProvider)
            {
                settings.ServiceProvider = serviceProvider;
            }
            try
            {
                var solfile = VisualStudioUtility.GetSolutionSettingsFileFullPath();
                if (!string.IsNullOrWhiteSpace(solfile))
                {
                    var solconf = Deserialize(solfile);
                    settings.BackgroundImageAbsolutePath = solconf.BackgroundImageAbsolutePath;
                    settings.BackgroundImagesDirectoryAbsolutePath = solconf.BackgroundImagesDirectoryAbsolutePath;
                    settings.BackgroundText = solconf.BackgroundText;
                    settings.TextSize = solconf.TextSize;
                    settings.ExpandToIDE = solconf.ExpandToIDE;
                    settings.Extensions = solconf.Extensions;
                    settings.ImageBackgroundType = solconf.ImageBackgroundType;
                    settings.ImageFadeAnimationInterval = solconf.ImageFadeAnimationInterval;
                    settings.ImageStretch = solconf.ImageStretch;
                    settings.LoopSlideshow = solconf.LoopSlideshow;
                    settings.MaxHeight = solconf.MaxHeight;
                    settings.MaxWidth = solconf.MaxWidth;
                    settings.Opacity = solconf.Opacity;
                    settings.PositionHorizon = solconf.PositionHorizon;
                    settings.PositionVertical = solconf.PositionVertical;
                    settings.ShuffleSlideshow = solconf.ShuffleSlideshow;
                    settings.SoftEdgeX = solconf.SoftEdgeX;
                    settings.SoftEdgeY = solconf.SoftEdgeY;
                    settings.UpdateImageInterval = solconf.UpdateImageInterval;
                    settings.ViewBoxPointX = solconf.ViewBoxPointX;
                    settings.ViewBoxPointY = solconf.ViewBoxPointY;
                    settings.SolutionConfigFilePath = solfile;
                    settings.IsLimitToMainlyEditorWindow = solconf.IsLimitToMainlyEditorWindow;
                }
                else
                {
                    settings.Load();
                }
            }
            catch
            {
                return Setting.Deserialize();
            }
            return settings;
        }

        private void Load(Properties props)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            BackgroundImagesDirectoryAbsolutePath = Setting.ToFullPath((string)props.Item("BackgroundImageDirectoryAbsolutePath").Value, DefaultBackgroundFolder);
            BackgroundImageAbsolutePath = Setting.ToFullPath((string)props.Item("BackgroundImageAbsolutePath").Value, DefaultBackgroundImage);
            BackgroundText = (string)props.Item("BackgroundText").Value;
            TextSize = (int)props.Item("TextSize").Value;
            Opacity = (double)props.Item("Opacity").Value;
            PositionHorizon = (PositionH)props.Item("PositionHorizon").Value;
            PositionVertical = (PositionV)props.Item("PositionVertical").Value;
            ImageStretch = (ImageStretch)props.Item("ImageStretch").Value;
            UpdateImageInterval = (TimeSpan)props.Item("UpdateImageInterval").Value;
            Extensions = (string)props.Item("Extensions").Value;
            ImageBackgroundType = (ImageBackgroundType)props.Item("ImageBackgroundType").Value;
            LoopSlideshow = (bool)props.Item("LoopSlideshow").Value;
            ShuffleSlideshow = (bool)props.Item("ShuffleSlideshow").Value;
            MaxWidth = (int)props.Item("MaxWidth").Value;
            MaxHeight = (int)props.Item("MaxHeight").Value;
            SoftEdgeX = (int)props.Item("SoftEdgeX").Value;
            SoftEdgeY = (int)props.Item("SoftEdgeY").Value;
            ExpandToIDE = (bool)props.Item("ExpandToIDE").Value;
            ViewBoxPointX = (double)props.Item("ViewBoxPointX").Value;
            ViewBoxPointY = (double)props.Item("ViewBoxPointY").Value;
            IsLimitToMainlyEditorWindow = (bool)props.Item("IsLimitToMainlyEditorWindow").Value;
        }

        public void Load()
        {
            var _DTE2 = (DTE2)ServiceProvider;
            var props = _DTE2.Properties["ClaudiaIDE", "General"];

            Load(props);
        }

        public void Load(string solutionConfigFile)
        {
            if (string.IsNullOrEmpty(solutionConfigFile))
            {
                Load();
                return;
            }
            else
            {
                var solconf = Deserialize(solutionConfigFile);
                BackgroundImageAbsolutePath = solconf.BackgroundImageAbsolutePath;
                BackgroundImagesDirectoryAbsolutePath = solconf.BackgroundImagesDirectoryAbsolutePath;
                BackgroundText = solconf.BackgroundText;
                TextSize = solconf.TextSize;
                ExpandToIDE = solconf.ExpandToIDE;
                Extensions = solconf.Extensions;
                ImageBackgroundType = solconf.ImageBackgroundType;
                ImageFadeAnimationInterval = solconf.ImageFadeAnimationInterval;
                ImageStretch = solconf.ImageStretch;
                LoopSlideshow = solconf.LoopSlideshow;
                MaxHeight = solconf.MaxHeight;
                MaxWidth = solconf.MaxWidth;
                Opacity = solconf.Opacity;
                PositionHorizon = solconf.PositionHorizon;
                PositionVertical = solconf.PositionVertical;
                ShuffleSlideshow = solconf.ShuffleSlideshow;
                SoftEdgeX = solconf.SoftEdgeX;
                SoftEdgeY = solconf.SoftEdgeY;
                UpdateImageInterval = solconf.UpdateImageInterval;
                ViewBoxPointX = solconf.ViewBoxPointX;
                ViewBoxPointY = solconf.ViewBoxPointY;
                IsLimitToMainlyEditorWindow = solconf.IsLimitToMainlyEditorWindow;
            }
        }

        public void OnApplyChanged()
        {
            try
            {
                Load(this.SolutionConfigFilePath);
                if (string.IsNullOrEmpty(this.SolutionConfigFilePath))
                {
                    Load();
                    OnChanged?.RaiseEvent(this, EventArgs.Empty);
                }
            }
            catch
            {

            }
        }

        public static Setting Deserialize()
        {
            var assemblylocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var configpath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, CONFIGFILE);
            string config = "";

            using (var s = new StreamReader(configpath, Encoding.UTF8, false))
            {
                config = s.ReadToEnd();
                s.Close();
            }
            var ret = JsonSerializer<Setting>.DeSerialize(config);
            ret.BackgroundImageAbsolutePath = ToFullPath(ret.BackgroundImageAbsolutePath, DefaultBackgroundImage);
            ret.BackgroundImagesDirectoryAbsolutePath = ToFullPath(ret.BackgroundImagesDirectoryAbsolutePath, DefaultBackgroundFolder);
            return ret;
        }

        public static Setting Deserialize(string solutionConfigFilePath)
        {
            string config = "";

            using (var s = new StreamReader(solutionConfigFilePath, Encoding.UTF8, false))
            {
                config = s.ReadToEnd();
                s.Close();
            }
            var ret = JsonSerializer<Setting>.DeSerialize(config);
            ret.BackgroundImageAbsolutePath = ToFullPath(ret.BackgroundImageAbsolutePath, DefaultBackgroundImage);
            ret.BackgroundImagesDirectoryAbsolutePath = ToFullPath(ret.BackgroundImagesDirectoryAbsolutePath, DefaultBackgroundFolder);
            return ret;
        }

        public static string ToFullPath(string path, string defaultPath)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = defaultPath;
            }
            var assemblylocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, path);
            }
            return path;
        }

    }

    [ComVisible(true)]
    [Guid("12d9a45f-ec0b-4a96-88dc-b0cba1f4789a")]
    public enum PositionV
    {
        Top,
        Bottom,
        Center
    }

    [ComVisible(true)]
    [Guid("8b2e3ece-fbf7-43ba-b369-3463726b828d")]
    public enum PositionH
    {
        Left,
        Right,
        Center
    }

    [ComVisible(true)]
    [Guid("5C96CFAA-FE54-49A9-8AB7-E85B66731228")]
    public enum ImageBackgroundType
    {
        Single = 0,
        Slideshow = 1,
        SingleEach = 2,
        TextImage = 3
    }

    [ComVisible(true)]
    [Guid("C89AFB79-39AF-4716-BB91-0F77323DD89B")]
    public enum ImageStretch
    {
        None = 0,
        Uniform = 1,
        UniformToFill = 2,
        Fill = 3
    }

    public static class ImageStretchConverter
    {
        public static System.Windows.Media.Stretch ConvertTo(this ImageStretch source)
        {
            switch(source)
            {
                case ImageStretch.Fill:
                    return System.Windows.Media.Stretch.Fill;
                case ImageStretch.None:
                    return System.Windows.Media.Stretch.None;
                case ImageStretch.Uniform:
                    return System.Windows.Media.Stretch.Uniform;
                case ImageStretch.UniformToFill:
                    return System.Windows.Media.Stretch.UniformToFill;
            }
            return System.Windows.Media.Stretch.None;
        }
    }

    public static class PositionConverter
    {
        public static System.Windows.Media.AlignmentY ConvertTo(this PositionV source)
        {
            switch (source)
            {
                case PositionV.Bottom:
                    return System.Windows.Media.AlignmentY.Bottom;
                case PositionV.Center:
                    return System.Windows.Media.AlignmentY.Center;
                case PositionV.Top:
                    return System.Windows.Media.AlignmentY.Top;
            }
            return System.Windows.Media.AlignmentY.Bottom;
        }

        public static System.Windows.VerticalAlignment ConvertToVerticalAlignment(this PositionV source)
        {
            switch (source)
            {
                case PositionV.Bottom:
                    return System.Windows.VerticalAlignment.Bottom;
                case PositionV.Center:
                    return System.Windows.VerticalAlignment.Center;
                case PositionV.Top:
                    return System.Windows.VerticalAlignment.Top;
            }
            return System.Windows.VerticalAlignment.Bottom;
        }

        public static System.Windows.Media.AlignmentX ConvertTo(this PositionH source)
        {
            switch (source)
            {
                case PositionH.Left:
                    return System.Windows.Media.AlignmentX.Left;
                case PositionH.Center:
                    return System.Windows.Media.AlignmentX.Center;
                case PositionH.Right:
                    return System.Windows.Media.AlignmentX.Right;
            }
            return System.Windows.Media.AlignmentX.Right;
        }

        public static System.Windows.HorizontalAlignment ConvertToHorizontalAlignment(this PositionH source)
        {
            switch (source)
            {
                case PositionH.Left:
                    return System.Windows.HorizontalAlignment.Left;
                case PositionH.Center:
                    return System.Windows.HorizontalAlignment.Center;
                case PositionH.Right:
                    return System.Windows.HorizontalAlignment.Right;
            }
            return System.Windows.HorizontalAlignment.Right;
        }
    }
}
