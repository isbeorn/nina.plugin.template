using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using System.ComponentModel.Composition;
using NINA.Sequencer.Trigger;
using NINA.Sequencer.Container;
using NINA.Sequencer.Utility;
using NINA.Sequencer.Validations;

namespace TemplateNamespace.Features.trigger {
    [Export(typeof(ISequenceTrigger))]
    [ExportMetadata("Name", "Example periodic trigger")]
    [ExportMetadata("Description", "Run the trigger instructions after every N completed instructions.")]
    [ExportMetadata("Category", "TemplateAssembly")]
    [ExportMetadata("Icon", "PuzzlePieceSVG")]
    [JsonObject(MemberSerialization.OptIn)]
    public class PeriodicTrigger : SequenceTrigger, IValidatable {
        private int interval = 2;
        private int completed;
        [ImportingConstructor]
        public PeriodicTrigger() { }
        private PeriodicTrigger(PeriodicTrigger source) : base(source) {
            Interval = source.Interval;
            TriggerRunner = (SequentialContainer)source.TriggerRunner.Clone();
        }
        [JsonProperty]
        public int Interval {
            get => interval;
            set {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(value), "Interval must be positive.");
                interval = value;
                completed = 0;
                RaisePropertyChanged();
            }
        }
        public override bool ShouldTrigger(ISequenceItem previousItem, ISequenceItem nextItem) => false;
        public override bool ShouldTriggerAfter(ISequenceItem previousItem, ISequenceItem nextItem) {
            if (previousItem == null) return false;
            // Reset at the threshold, so even int.MaxValue cannot overflow.
            if (++completed < Interval) return false;
            completed = 0;
            return true;
        }
        public override void Initialize() { completed = 0; }
        public override void SequenceBlockInitialize() { completed = 0; }
        public IList<string> Issues => TriggerRunner.Issues;
        public bool Validate() => TriggerRunner.Validate();
        public override void AfterParentChanged() {
            TriggerRunner.AttachNewParent(ItemUtility.CreateTriggerRunnerContext(Parent));
        }
        public override async Task Execute(ISequenceContainer context, IProgress<ApplicationStatus> progress, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            var originalParent = TriggerRunner.Parent;
            // Preserve target context without walking back into this trigger's owning trigger set.
            TriggerRunner.AttachNewParent(ItemUtility.CreateTriggerRunnerContext(context ?? Parent));
            try {
                TriggerRunner.ResetAll();
                await TriggerRunner.Run(progress, token);
            } finally {
                TriggerRunner.AttachNewParent(originalParent);
            }
        }
        public override object Clone() => new PeriodicTrigger(this);
    }
}
