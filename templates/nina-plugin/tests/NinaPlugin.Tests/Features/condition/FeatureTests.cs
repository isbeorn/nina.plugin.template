using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.condition;

namespace TemplateNamespace.Tests.Features.condition {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [TestCase(true)]
        [TestCase(false)]
        public void EditedValueSurvivesCloneAndJson(bool enabled) {
            var item = new BooleanCondition { Enabled = enabled };
            Assert.That(item.Check(null, null), Is.EqualTo(enabled));
            Assert.That(((BooleanCondition)item.Clone()).Enabled, Is.EqualTo(enabled));
            Assert.That(JsonConvert.DeserializeObject<BooleanCondition>(JsonConvert.SerializeObject(item)).Enabled, Is.EqualTo(enabled));
        }
    }
}
