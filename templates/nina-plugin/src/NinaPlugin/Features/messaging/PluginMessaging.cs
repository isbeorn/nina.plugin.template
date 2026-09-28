using NINA.Core.Utility;
using NINA.Plugin.Interfaces;
using System.ComponentModel.Composition;

namespace TemplateNamespace.Features.messaging {
    [Export(typeof(PluginMessaging))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class PluginMessaging : ISubscriber, IDisposable {
        private readonly IMessageBroker broker;
        private readonly string topic = new Plugin().Identifier + "/example/v1";
        private bool initialized;
        [ImportingConstructor]
        public PluginMessaging(IMessageBroker broker) { this.broker = broker; }
        public void Initialize() {
            if (initialized) return;
            broker.Subscribe(topic, this);
            initialized = true;
        }
        public Task Publish(string text) => broker.Publish(new ExampleMessage(topic, text));
        public Task OnMessageReceived(IMessage message) {
            if (initialized && message.Topic == topic && message.Version == 1 &&
                (message.Expiration == null || message.Expiration > DateTimeOffset.UtcNow)) {
                Logger.Info($"Example message received: {message.Content}");
            }
            return Task.CompletedTask;
        }
        public void Dispose() {
            if (!initialized) return;
            broker.Unsubscribe(topic, this);
            initialized = false;
        }
        private sealed class ExampleMessage(string topic, string text) : IMessage {
            public Guid SenderId { get; } = Guid.Parse(new Plugin().Identifier);
            public string Sender { get; } = new Plugin().Name;
            public DateTimeOffset SentAt { get; } = DateTimeOffset.UtcNow;
            public Guid MessageId { get; } = Guid.NewGuid();
            public DateTimeOffset? Expiration => SentAt.AddMinutes(1);
            public Guid? CorrelationId => null;
            public int Version => 1;
            public IDictionary<string, object> CustomHeaders { get; } = new Dictionary<string, object>();
            public string Topic => topic;
            public object Content => text;
        }
    }
}
