using NINA.Sequencer;
using System.ComponentModel.Composition;

namespace TemplateNamespace.Features.sequence_upgrader {
    [Export(typeof(ISequenceEntityUpgrader))]
    public class ExampleUpgrader : ISequenceEntityUpgrader {
        public string Name { get; set; } = "TemplateAssembly configuration upgrader";
        public SequenceUpgradeStage Stages => SequenceUpgradeStage.BeforeCreate;
        public object Upgrade(SequenceUpgradeContext context, SequenceUpgradeStage stage, object current) {
            if (stage != SequenceUpgradeStage.BeforeCreate || context.RequestedType != typeof(VersionedInstruction)) return current;
            if (context.Json["Message"] == null && context.Json["Text"] != null) {
                context.Json["Message"] = context.Json["Text"].DeepClone();
            }
            context.Json.Remove("Text");
            return current;
        }
    }
}
