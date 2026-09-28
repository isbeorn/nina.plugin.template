using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using System.ComponentModel.Composition;
using NINA.Sequencer.Container;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.Trigger;
using NINA.Sequencer.Container.ExecutionStrategy;

namespace TemplateNamespace.Features.container {
    [Export(typeof(ISequenceContainer))]
    [ExportMetadata("Name", "Example group")]
    [ExportMetadata("Description", "A group that executes its children sequentially.")]
    [ExportMetadata("Category", "TemplateAssembly")]
    [ExportMetadata("Icon", "PuzzlePieceSVG")]
    [JsonObject(MemberSerialization.OptIn)]
    [Export(typeof(ISequenceItem))]
    public class ExampleContainer : SequenceContainer {
        [ImportingConstructor]
        public ExampleContainer() : base(new SequentialStrategy()) { }
        public override object Clone() {
            var clone = new ExampleContainer();
            clone.CopyMetaData(this);
            foreach (var item in Items) {
                var child = (ISequenceItem)item.Clone();
                clone.Items.Add(child);
                child.AttachNewParent(clone);
            }
            foreach (var condition in Conditions) {
                var child = (ISequenceCondition)condition.Clone();
                clone.Conditions.Add(child);
                child.AttachNewParent(clone);
            }
            foreach (var trigger in Triggers) {
                var child = (ISequenceTrigger)trigger.Clone();
                clone.Triggers.Add(child);
                child.AttachNewParent(clone);
            }
            return clone;
        }
    }
}
