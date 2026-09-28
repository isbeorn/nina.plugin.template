using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.sequence_upgrader;
using Newtonsoft.Json.Linq;
using NINA.Sequencer;
namespace TemplateNamespace.Tests.Features.sequence_upgrader {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public void MigratesOnlyOwnedLegacyDataAndPreservesNewValue() {
            var upgrader = new ExampleUpgrader();
            var json = JObject.Parse("{ 'Text': 'legacy' }");
            var context = new SequenceUpgradeContext { RequestedType = typeof(VersionedInstruction), Json = json, Serializer = new JsonSerializer() };
            upgrader.Upgrade(context, SequenceUpgradeStage.BeforeCreate, null);
            Assert.That(json["Message"].Value<string>(), Is.EqualTo("legacy"));
            Assert.That(json["Text"], Is.Null);
            json["Text"] = "older";
            upgrader.Upgrade(context, SequenceUpgradeStage.BeforeCreate, null);
            Assert.That(json["Message"].Value<string>(), Is.EqualTo("legacy"));
            var unrelated = new SequenceUpgradeContext { RequestedType = typeof(object), Json = JObject.Parse("{ 'Text': 'keep' }"), Serializer = new JsonSerializer() };
            upgrader.Upgrade(unrelated, SequenceUpgradeStage.BeforeCreate, null);
            Assert.That(unrelated.Json["Text"].Value<string>(), Is.EqualTo("keep"));
        }
    }
}
