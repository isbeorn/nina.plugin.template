using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using System.ComponentModel.Composition;
using NINA.Sequencer.Editing;
using NINA.Core.Utility;

namespace TemplateNamespace.Features.editor_integration {
    [Export(typeof(ISequenceItem))]
    [ExportMetadata("Name", "Example coupled edit")]
    [ExportMetadata("Description", "Editing the edge updates both dimensions and can be undone together.")]
    [ExportMetadata("Category", "TemplateAssembly")]
    [ExportMetadata("Icon", "PuzzlePieceSVG")]
    [JsonObject(MemberSerialization.OptIn)]
    public class CoupledInstruction : SequenceItem, ISequenceCustomPropertyEditProvider {
        private int width = 10;
        private int height = 10;
        [ImportingConstructor]
        public CoupledInstruction() { }
        private CoupledInstruction(CoupledInstruction source) : base(source) {
            Width = source.Width;
            Height = source.Height;
        }
        [JsonProperty]
        public int Width { get => width; set { width = value; RaisePropertyChanged(); Height = value; } }
        [JsonProperty]
        public int Height { get => height; set { height = value; RaisePropertyChanged(); } }
        public bool TryCapturePropertyState(object source, string propertyName, out ISequenceEditSnapshot snapshot) {
            snapshot = null;
            if (!ReferenceEquals(source, this) || propertyName != nameof(Width)) return false;
            snapshot = new DimensionsSnapshot(this, Width, Height);
            return true;
        }
        private sealed record DimensionsSnapshot(CoupledInstruction Owner, int Width, int Height) : ISequenceEditSnapshot {
            public string Description => $"{Width} x {Height}";
            public bool IsCurrent => Owner.Width == Width && Owner.Height == Height;
            public void Restore() { Owner.Width = Width; Owner.Height = Height; }
        }
        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            Logger.Info($"Configured dimensions: {Width} x {Height}");
            return Task.CompletedTask;
        }
        public override object Clone() => new CoupledInstruction(this);
    }
}
