using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.trigger;

namespace TemplateNamespace.Tests.Features.trigger {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public void FiresOnlyAfterThresholdAndResets() {
            var item = new PeriodicTrigger { Interval = 2 };
            var previous = new NINA.Sequencer.SequenceItem.Utility.WaitForTimeSpan();
            Assert.That(item.ShouldTrigger(previous, null), Is.False);
            Assert.That(item.ShouldTriggerAfter(null, null), Is.False);
            Assert.That(item.ShouldTriggerAfter(previous, null), Is.False);
            Assert.That(item.ShouldTriggerAfter(previous, null), Is.True);
            Assert.That(item.ShouldTriggerAfter(previous, null), Is.False);
            item.SequenceBlockInitialize();
            Assert.That(item.ShouldTriggerAfter(previous, null), Is.False);
            Assert.That(item.ShouldTriggerAfter(previous, null), Is.True);
            item.Interval = 1;
            Assert.That(item.ShouldTriggerAfter(previous, null), Is.True);
            item.Interval = int.MaxValue;
            Assert.That(item.ShouldTriggerAfter(previous, null), Is.False);
        }
        [Test]
        public async Task ExecutesActionsRepeatedlyWithIsolatedContextAndRestoresAttachment() {
            var parent = new NINA.Sequencer.Container.SequentialContainer();
            var trigger = new PeriodicTrigger();
            parent.Add(trigger);
            var original = trigger.TriggerRunner.Parent;
            var child = new Moq.Mock<NINA.Sequencer.SequenceItem.SequenceItem> { CallBase = true };
            child.Setup(c => c.Execute(Moq.It.IsAny<IProgress<NINA.Core.Model.ApplicationStatus>>(), Moq.It.IsAny<CancellationToken>()))
                .Callback(() => Assert.That(trigger.TriggerRunner.Parent, Is.Not.SameAs(parent)))
                .Returns(Task.CompletedTask);
            trigger.TriggerRunner.Add(child.Object);
            await trigger.Execute(parent, null, CancellationToken.None);
            await trigger.Execute(parent, null, CancellationToken.None);
            child.Verify(c => c.Execute(Moq.It.IsAny<IProgress<NINA.Core.Model.ApplicationStatus>>(), Moq.It.IsAny<CancellationToken>()), Moq.Times.Exactly(2));
            Assert.That(trigger.TriggerRunner.Parent, Is.SameAs(original));
            Assert.ThrowsAsync<OperationCanceledException>(() => trigger.Execute(parent, null, new CancellationToken(true)));
            Assert.That(trigger.TriggerRunner.Parent, Is.SameAs(original));
        }
        [TestCase(0)]
        [TestCase(-1)]
        public void RejectsNonpositiveInterval(int interval) => Assert.Throws<ArgumentOutOfRangeException>(() => new PeriodicTrigger { Interval = interval });
        [Test]
        public void CloneRetainsConfigurationAndDeepCopiesActions() {
            var item = new PeriodicTrigger { Interval = 4 };
            item.TriggerRunner.Add(new NINA.Sequencer.SequenceItem.Utility.WaitForTimeSpan());
            var clone = (PeriodicTrigger)item.Clone();
            Assert.That(clone.Interval, Is.EqualTo(4));
            var restored = JsonConvert.DeserializeObject<PeriodicTrigger>("{\"Interval\":7}");
            Assert.That(restored.Interval, Is.EqualTo(7));
            Assert.That(clone.TriggerRunner.Items.Single(), Is.Not.SameAs(item.TriggerRunner.Items.Single()));
            Assert.That(clone.TriggerRunner.Items.Single().Parent, Is.SameAs(clone.TriggerRunner));
        }
    }
}
