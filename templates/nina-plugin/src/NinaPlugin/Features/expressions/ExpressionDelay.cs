using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using System.ComponentModel.Composition;
using NINA.Sequencer.Generators;
using NINA.Sequencer.Validations;

namespace TemplateNamespace.Features.expressions {
    [Export(typeof(ISequenceItem))]
    [ExportMetadata("Name", "Example expression delay")]
    [ExportMetadata("Description", "Wait for a duration computed from an expression.")]
    [ExportMetadata("Category", "TemplateAssembly")]
    [ExportMetadata("Icon", "PuzzlePieceSVG")]
    [JsonObject(MemberSerialization.OptIn)]
    [UsesExpressions(GenerateValidation = true)]
    public partial class ExpressionDelay : SequenceItem, IValidatable {
        [ImportingConstructor]
        public ExpressionDelay() { }
        private ExpressionDelay(ExpressionDelay source) : base(source) { }
        [IsExpression(Default = 1, Range = [0, 3600])]
        public partial double Seconds { get; set; }
        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            if (!Validate()) throw new InvalidOperationException(string.Join("; ", Issues));
            return Task.Delay(TimeSpan.FromSeconds(Seconds), token);
        }
        public override void AfterParentChanged() {
            base.AfterParentChanged();
            Validate();
        }
        // Clone(), expression storage and validation are emitted by NINA's generator.
    }
}
