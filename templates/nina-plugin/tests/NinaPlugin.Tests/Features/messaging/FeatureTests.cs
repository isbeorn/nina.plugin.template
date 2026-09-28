using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.messaging;
using Moq;
using NINA.Plugin.Interfaces;
namespace TemplateNamespace.Tests.Features.messaging {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public async Task PublishesEnvelopeAndBalancesSubscriptions() {
            var broker = new Mock<IMessageBroker>();
            IMessage sent = null;
            broker.Setup(b => b.Publish(It.IsAny<IMessage>())).Callback<IMessage>(m => sent = m).Returns(Task.CompletedTask);
            using var messaging = new PluginMessaging(broker.Object);
            messaging.Initialize(); messaging.Initialize();
            await messaging.Publish("Hello");
            Assert.That(sent.Content, Is.EqualTo("Hello"));
            Assert.That(sent.SenderId.ToString(), Is.EqualTo(new Plugin().Identifier).IgnoreCase);
            Assert.That(sent.Expiration, Is.GreaterThan(sent.SentAt));
            messaging.Dispose(); messaging.Dispose();
            broker.Verify(b => b.Subscribe(sent.Topic, messaging), Times.Once);
            broker.Verify(b => b.Unsubscribe(sent.Topic, messaging), Times.Once);
        }
    }
}
