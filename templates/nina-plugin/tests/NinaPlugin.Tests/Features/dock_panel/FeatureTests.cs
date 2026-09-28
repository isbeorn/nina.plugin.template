using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.dock_panel;
using Moq;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Equipment.Equipment.MyTelescope;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
namespace TemplateNamespace.Tests.Features.dock_panel {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public async Task ActualExportedPanelParticipatesInManifestLifecycleAcrossCatalogs() {
            var telescope = new Mock<ITelescopeMediator>();
            using var manifestCatalog = new CompositionContainer(new TypeCatalog(typeof(Plugin), typeof(TelescopePanel)));
            using var partsCatalog = new CompositionContainer(new TypeCatalog(typeof(TelescopePanel)));
            manifestCatalog.ComposeExportedValue(telescope.Object);
            partsCatalog.ComposeExportedValue(telescope.Object);
            // The complete composition test supplies dependencies for other selected features.
            var panel = (TelescopePanel)partsCatalog.GetExportedValue<IDockableVM>();
            panel.Initialize();
            panel.Initialize();
            panel.UpdateDeviceInfo(new TelescopeInfo { Connected = true });
            Assert.That(panel.Connected, Is.True);
            panel.UpdateDeviceInfo(new TelescopeInfo { Connected = false });
            Assert.That(panel.Connected, Is.False);
            panel.Hide(null);
            Assert.That(panel.IsVisible, Is.False);
            panel.Hide(null);
            Assert.That(panel.IsVisible, Is.True);
            panel.Dispose();
            telescope.Verify(t => t.RegisterConsumer(panel), Times.Once);
            telescope.Verify(t => t.RemoveConsumer(panel), Times.Once);
            await Task.CompletedTask;
        }
    }
}
