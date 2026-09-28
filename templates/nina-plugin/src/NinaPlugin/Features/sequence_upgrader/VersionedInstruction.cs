using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using System.ComponentModel.Composition;
using NINA.Core.Utility;

namespace TemplateNamespace.Features.sequence_upgrader {
    [Export(typeof(ISequenceItem))]
    [ExportMetadata("Name", "Example versioned instruction")]
    [ExportMetadata("Description", "Demonstrates migrating a legacy Text field to Message.")]
    [ExportMetadata("Category", "TemplateAssembly")]
    [ExportMetadata("Icon", "PuzzlePieceSVG")]
    [JsonObject(MemberSerialization.OptIn)]
    public class VersionedInstruction : SequenceItem {
        private string message = "Migrated configuration";
        [ImportingConstructor]
        public VersionedInstruction() { }
        private VersionedInstruction(VersionedInstruction source) : base(source) { Message = source.Message; }
        [JsonProperty]
        public string Message {
            get => message;
            set { message = value; RaisePropertyChanged(); }
        }
        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            Logger.Info(Message);
            return Task.CompletedTask;
        }
        public override object Clone() => new VersionedInstruction(this);
    }
}
