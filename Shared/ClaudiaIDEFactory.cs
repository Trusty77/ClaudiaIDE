using System.ComponentModel.Composition;
using ClaudiaIDE.ImageProvider;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Classification;
using System.Linq;

namespace ClaudiaIDE
{
    #region Adornment Factory
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [ContentType("BuildOutput")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class ClaudiaIDEAdornmentFactory : IWpfTextViewCreationListener
    {
        private List<IImageProvider> ImageProviders;

        [Import(typeof(SVsServiceProvider))]
        internal System.IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Defines the adornment layer for the scarlet adornment. This layer is ordered 
        /// after the selection layer in the Z-order
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("ClaudiaIDE")]
        [Order(Before = PredefinedAdornmentLayers.DifferenceChanges)]
        public AdornmentLayerDefinition EditorAdornmentLayer { get; set; }

        /// <summary>
        /// Instantiates a ClaudiaIDE manager when a textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            #if Dev32
            var settings = Setting.Initialize(this.ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2);
            #else
            var settings = Setting.Initialize((EnvDTE.DTE)ServiceProvider.GetService(typeof(EnvDTE.DTE)));
            #endif
            var solution = VisualStudioUtility.GetSolutionSettingsFileFullPath();

            if (this.ImageProviders == null)
            {
                if (ProvidersHolder.Instance.Providers == null)
                {
                    if (string.IsNullOrEmpty(solution))
                    {
                        ProvidersHolder.Initialize(settings, new List<IImageProvider>
                        {
                            new SingleImageEachProvider(settings),
                            new SlideShowImageProvider(settings),
                            new SingleImageProvider(settings),
                            new TextImageProvider(settings)
                        });
                    }
                    else
                    {
                        ProvidersHolder.Initialize(settings, new List<IImageProvider>());
                        switch (settings.ImageBackgroundType)
                        {
                            case ImageBackgroundType.Single:
                                ProvidersHolder.Instance.Providers.Add(new SingleImageProvider(settings, solution));
                                break;
                            case ImageBackgroundType.SingleEach:
                                ProvidersHolder.Instance.Providers.Add(new SingleImageEachProvider(settings, solution));
                                break;
                            case ImageBackgroundType.Slideshow:
                                ProvidersHolder.Instance.Providers.Add(new SlideShowImageProvider(settings, solution));
                                break;
                            case ImageBackgroundType.TextImage:
                                ProvidersHolder.Instance.Providers.Add(new TextImageProvider(settings, solution));
                                break;
                            default:
                                ProvidersHolder.Instance.Providers.Add(new SingleImageEachProvider(settings, solution));
                                break;
                        }
                    }
                }
                this.ImageProviders = ProvidersHolder.Instance.Providers;
            }

            if (!string.IsNullOrEmpty(solution))
            {
                if (!this.ImageProviders.Any(x => x.SolutionConfigFile == solution))
                {
                    switch (settings.ImageBackgroundType)
                    {
                        case ImageBackgroundType.Single:
                            this.ImageProviders.Add(new SingleImageProvider(settings, solution));
                            break;
                        case ImageBackgroundType.SingleEach:
                            this.ImageProviders.Add(new SingleImageEachProvider(settings, solution));
                            break;
                        case ImageBackgroundType.Slideshow:
                            this.ImageProviders.Add(new SlideShowImageProvider(settings, solution));
                            break;
                        case ImageBackgroundType.TextImage:
                            this.ImageProviders.Add(new TextImageProvider(settings, solution));
                            break;
                        default:
                            this.ImageProviders.Add(new SingleImageEachProvider(settings, solution));
                            break;
                    }
                }
            }

            new ClaudiaIDE(textView, this.ImageProviders);
        }
    }

#endregion //Adornment Factory
}
