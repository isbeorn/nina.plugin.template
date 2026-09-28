using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.expressions;

namespace TemplateNamespace.Tests.Features.expressions {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [TestCase("0", true)]
        [TestCase("3600", true)]
        [TestCase("-1", false)]
        [TestCase("3601", false)]
        [TestCase("broken(", false)]
        public void ValidatesBoundaries(string definition, bool valid) {
            var item = new ExpressionDelay();
            item.SecondsExpression.Definition = definition;
            Assert.That(item.Validate(), Is.EqualTo(valid));
        }
        [Test]
        public void CloneAndJsonKeepDefinitionAndOwner() {
            var item = new ExpressionDelay();
            item.SecondsExpression.Definition = "2 + 3";
            var clone = (ExpressionDelay)item.Clone();
            Assert.That(clone.Seconds, Is.EqualTo(5));
            Assert.That(clone.SecondsExpression.Context, Is.SameAs(clone));
            Assert.That(clone.SecondsExpression, Is.Not.SameAs(item.SecondsExpression));
            var restored = JsonConvert.DeserializeObject<ExpressionDelay>(JsonConvert.SerializeObject(item));
            Assert.That(restored.Seconds, Is.EqualTo(5));
        }
        [Test]
        public async Task CancelsAnExecutingDelay() {
            using var cancellation = new CancellationTokenSource();
            var item = new ExpressionDelay();
            item.SecondsExpression.Definition = "3600";
            var execution = item.Execute(null, cancellation.Token);
            cancellation.Cancel();
            Assert.ThrowsAsync<TaskCanceledException>(() => execution);
            await Task.CompletedTask;
        }
        [Test]
        public void SupportsCancellation() => Assert.Throws<OperationCanceledException>(() => new ExpressionDelay().Execute(null, new CancellationToken(true)));
    }
}
