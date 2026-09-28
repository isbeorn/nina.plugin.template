using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using System.ComponentModel.Composition;

namespace TemplateNamespace.Features.messaging {
    [Export(typeof(ISequenceItem))]
    [ExportMetadata("Name", "Example publish message")]
    [ExportMetadata("Description", "Publish to the plugin's example topic.")]
    [ExportMetadata("Icon", "PuzzlePieceSVG")]
    [ExportMetadata("Category", "TemplateAssembly")]
    [JsonObject(MemberSerialization.OptIn)]
    public class PublishMessageInstruction : SequenceItem {
        private readonly PluginMessaging messaging;
        [ImportingConstructor]
        public PublishMessageInstruction(PluginMessaging messaging) { this.messaging = messaging; }
        private PublishMessageInstruction(PublishMessageInstruction source) : base(source) { messaging = source.messaging; }
        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            return messaging.Publish("Hello from the sequence");
        }
        public override object Clone() => new PublishMessageInstruction(this);
    }
}
