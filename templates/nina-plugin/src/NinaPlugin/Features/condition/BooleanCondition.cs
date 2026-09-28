using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using System.ComponentModel.Composition;
using NINA.Sequencer.Conditions;

namespace TemplateNamespace.Features.condition {
    [Export(typeof(ISequenceCondition))]
    [ExportMetadata("Name", "Example condition")]
    [ExportMetadata("Description", "Continue while enabled.")]
    [ExportMetadata("Category", "TemplateAssembly")]
    [ExportMetadata("Icon", "PuzzlePieceSVG")]
    [JsonObject(MemberSerialization.OptIn)]
    public class BooleanCondition : SequenceCondition {
        private bool enabled = true;
        [ImportingConstructor]
        public BooleanCondition() { }
        private BooleanCondition(BooleanCondition source) : base(source) { Enabled = source.Enabled; }
        [JsonProperty]
        public bool Enabled {
            get => enabled;
            set { enabled = value; RaisePropertyChanged(); }
        }
        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) => Enabled;
        public override object Clone() => new BooleanCondition(this);
    }
}
