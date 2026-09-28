using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using System.ComponentModel.Composition;
using NINA.Core.Utility.Notification;

namespace TemplateNamespace.Features.instruction {
    [Export(typeof(ISequenceItem))]
    [ExportMetadata("Name", "Example notification")]
    [ExportMetadata("Description", "Reports progress and displays a message.")]
    [ExportMetadata("Category", "TemplateAssembly")]
    [ExportMetadata("Icon", "PuzzlePieceSVG")]
    [JsonObject(MemberSerialization.OptIn)]
    public class NotificationInstruction : SequenceItem {
        private string message = "Hello from your plugin";
        [ImportingConstructor]
        public NotificationInstruction() { }
        private NotificationInstruction(NotificationInstruction source) : base(source) {
            Message = source.Message;
        }
        [JsonProperty]
        public string Message {
            get => message;
            set { message = value; RaisePropertyChanged(); }
        }
        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            progress?.Report(new ApplicationStatus { Status = Message });
            Notification.ShowSuccess(Message);
            return Task.CompletedTask;
        }
        public override object Clone() => new NotificationInstruction(this);
    }
}
