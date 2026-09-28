using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.container;

namespace TemplateNamespace.Tests.Features.container {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public void CloneDeepCopiesAndReparentsChildren() {
            var item = new ExampleContainer();
            item.Add(new NINA.Sequencer.SequenceItem.Utility.WaitForTimeSpan());
            item.Add(new NINA.Sequencer.Conditions.LoopCondition());
            item.Add(new NINA.Sequencer.Trigger.Utility.CustomTrigger(Moq.Mock.Of<NINA.Core.Utility.IApplicationResourceDictionary>()));
            var clone = (ExampleContainer)item.Clone();
            Assert.That(clone.Items.Single(), Is.Not.SameAs(item.Items.Single()));
            Assert.That(clone.Items.Single().Parent, Is.SameAs(clone));
            Assert.That(clone.Conditions.Single(), Is.Not.SameAs(item.Conditions.Single()));
            Assert.That(clone.Conditions.Single().Parent, Is.SameAs(clone));
            Assert.That(clone.Triggers.Single(), Is.Not.SameAs(item.Triggers.Single()));
            Assert.That(clone.Triggers.Single().Parent, Is.SameAs(clone));
        }
    }
}
