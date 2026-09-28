using NUnit.Framework;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TemplateNamespace.Tests {
    public class PluginTests {
        [Test]
        public void ManifestHasValidIdentityAndHostBaseline() {
            var plugin = new Plugin();
            Assert.That(Guid.TryParse(plugin.Identifier, out _), Is.True);
            Assert.That(plugin.Name, Is.Not.Empty);
            Assert.That(plugin.MinimumApplicationVersion.ToString(), Is.EqualTo("3.3.0.1060"));
            Assert.That(typeof(Plugin).GetCustomAttributes<ExportAttribute>().Single().ContractType,
                Is.EqualTo(typeof(NINA.Plugin.Interfaces.IPluginManifest)));
        }
        [Test]
        public void AssemblyMetadataUsesDisplayNameAndDisablesComExposure() {
            var assembly = typeof(Plugin).Assembly;
            Assert.Multiple(() => {
                Assert.That(assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product, Is.EqualTo(new Plugin().Name));
                Assert.That(assembly.GetCustomAttribute<ComVisibleAttribute>()?.Value, Is.False);
            });
        }
    }
}
