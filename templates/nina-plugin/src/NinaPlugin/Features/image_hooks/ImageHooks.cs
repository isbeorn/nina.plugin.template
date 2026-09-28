using NINA.Core.Model;
using NINA.Image.ImageData;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System.ComponentModel.Composition;

namespace TemplateNamespace.Features.image_hooks {
    [Export(typeof(ImageHooks))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class ImageHooks : IDisposable {
        private readonly IImageSaveMediator images;
        private readonly IOptionsVM options;
        private readonly ImagePattern pattern;
        private bool initialized;
        [ImportingConstructor]
        public ImageHooks(IImageSaveMediator images, IOptionsVM options) {
            this.images = images;
            this.options = options;
            pattern = new ImagePattern("$$TemplateAssembly_EXAMPLE$$", "Example plugin file pattern", new Plugin().Name);
        }
        public void Initialize() {
            if (initialized) return;
            images.BeforeImageSaved += BeforeSave;
            images.BeforeFinalizeImageSaved += BeforeFinalize;
            options.AddImagePattern(pattern);
            initialized = true;
        }
        private Task BeforeSave(object sender, BeforeImageSavedEventArgs args) {
            args.Image.MetaData.GenericHeaders.Add(new StringMetaDataHeader("PLGEXAMP", "TemplateAssembly", "Example plugin header"));
            return Task.CompletedTask;
        }
        private Task BeforeFinalize(object sender, BeforeFinalizeImageSavedEventArgs args) {
            args.AddImagePattern(new ImagePattern(pattern.Key, pattern.Description, pattern.Category) { Value = "example" });
            return Task.CompletedTask;
        }
        public void Dispose() {
            if (!initialized) return;
            images.BeforeImageSaved -= BeforeSave;
            images.BeforeFinalizeImageSaved -= BeforeFinalize;
            options.RemoveImagePattern(pattern.Key);
            initialized = false;
        }
    }
}
