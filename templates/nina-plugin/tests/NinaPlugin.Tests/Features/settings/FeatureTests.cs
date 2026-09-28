using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.settings;
using Moq;
using NINA.Profile.Interfaces;
namespace TemplateNamespace.Tests.Features.settings {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public void OptionsLookupAndBindingsUseTheManifestSettings() {
            var profiles = new Mock<IProfileService>();
            var profile = new Mock<IProfile>();
            profile.SetupGet(p => p.PluginSettings).Returns(new NINA.Profile.PluginSettings());
            profiles.SetupGet(p => p.ActiveProfile).Returns(profile.Object);
            using var settings = new PluginSettings(profiles.Object);
            settings.Initialize();
            settings.ProfileMessage = "Initial profile value";
            var manifest = new Plugin { Settings = settings };
            var resources = new Resources();
            Assert.That(resources.Contains("OptionsTemplate"), Is.False);
            var template = (System.Windows.DataTemplate)resources[manifest.Name + "_Options"];
            var view = (System.Windows.Controls.StackPanel)template.LoadContent();
            view.DataContext = manifest;
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.DataBind);
            var boxes = view.Children.OfType<System.Windows.Controls.TextBox>().ToArray();
            Assert.That(boxes[0].Text, Is.EqualTo(settings.GlobalMessage));
            Assert.That(boxes[1].Text, Is.EqualTo("Initial profile value"));
            boxes[1].SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, "Edited through options");
            boxes[1].GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            Assert.That(settings.ProfileMessage, Is.EqualTo("Edited through options"));
            settings.ProfileMessage = "Changed by code";
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.DataBind);
            Assert.That(boxes[1].Text, Is.EqualTo("Changed by code"));
        }
        [Test]
        public void ProfileSwitchAndGlobalPersistenceAreIndependent() {
            var profiles = new Mock<IProfileService>();
            var first = new Mock<IProfile>();
            first.SetupGet(p => p.PluginSettings).Returns(new NINA.Profile.PluginSettings());
            var second = new Mock<IProfile>();
            second.SetupGet(p => p.PluginSettings).Returns(new NINA.Profile.PluginSettings());
            profiles.SetupGet(p => p.ActiveProfile).Returns(first.Object);
            using var settings = new PluginSettings(profiles.Object);
            settings.Initialize();
            settings.Initialize();
            settings.GlobalMessage = Guid.NewGuid().ToString();
            settings.ProfileMessage = "First profile";
            var notifications = new List<string>();
            settings.PropertyChanged += (_, args) => notifications.Add(args.PropertyName);
            profiles.SetupGet(p => p.ActiveProfile).Returns(second.Object);
            profiles.Raise(p => p.ProfileChanged += null, EventArgs.Empty);
            Assert.That(settings.ProfileMessage, Is.EqualTo("Profile message"));
            Assert.That(notifications, Does.Contain(nameof(PluginSettings.ProfileMessage)));
            settings.ProfileMessage = "Second profile";
            profiles.SetupGet(p => p.ActiveProfile).Returns(first.Object);
            profiles.Raise(p => p.ProfileChanged += null, EventArgs.Empty);
            Assert.That(settings.ProfileMessage, Is.EqualTo("First profile"));
            using var restored = new PluginSettings(profiles.Object);
            restored.Initialize();
            Assert.That(restored.GlobalMessage, Is.EqualTo(settings.GlobalMessage));
            settings.Dispose();
            notifications.Clear();
            profiles.Raise(p => p.ProfileChanged += null, EventArgs.Empty);
            Assert.That(notifications, Is.Empty);
        }
    }
}
