using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.instruction;

namespace TemplateNamespace.Tests.Features.instruction {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public void CloneAndJsonRetainEditedMessage() {
            var item = new NotificationInstruction { Message = "Changed message" };
            Assert.That(((NotificationInstruction)item.Clone()).Message, Is.EqualTo(item.Message));
            Assert.That(JsonConvert.DeserializeObject<NotificationInstruction>(JsonConvert.SerializeObject(item)).Message, Is.EqualTo(item.Message));
        }
        [Test]
        public void CancellationPreventsNotification() {
            Assert.Throws<OperationCanceledException>(() => new NotificationInstruction().Execute(null, new CancellationToken(true)));
        }
    }
}
