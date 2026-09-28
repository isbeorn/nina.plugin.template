using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.image_hooks;
using Moq;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
namespace TemplateNamespace.Tests.Features.image_hooks {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public void InitializeAndDisposeAreBalancedAndIdempotent() {
            var images = new Mock<IImageSaveMediator>();
            var options = new Mock<IOptionsVM>();
            var hooks = new ImageHooks(images.Object, options.Object);
            hooks.Initialize(); hooks.Initialize(); hooks.Dispose(); hooks.Dispose();
            options.Verify(o => o.AddImagePattern(It.IsAny<NINA.Core.Model.ImagePattern>()), Times.Once);
            options.Verify(o => o.RemoveImagePattern(It.IsAny<string>()), Times.Once);
            Assert.That(images.Invocations.Count(i => i.Method.Name == "add_BeforeImageSaved"), Is.EqualTo(1));
            Assert.That(images.Invocations.Count(i => i.Method.Name == "remove_BeforeImageSaved"), Is.EqualTo(1));
            Assert.That(images.Invocations.Count(i => i.Method.Name == "add_BeforeFinalizeImageSaved"), Is.EqualTo(1));
            Assert.That(images.Invocations.Count(i => i.Method.Name == "remove_BeforeFinalizeImageSaved"), Is.EqualTo(1));
        }
    }
}
