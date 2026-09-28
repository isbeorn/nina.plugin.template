using NINA.Sequencer.Logic;
using System.ComponentModel.Composition;
using System.Globalization;

namespace TemplateNamespace.Features.symbols {
    [Export(typeof(PluginSymbols))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class PluginSymbols : IDisposable {
        private readonly ISymbolBroker broker;
        private ISymbolProvider provider;
        private bool initialized;
        [ImportingConstructor]
        public PluginSymbols(ISymbolBroker broker) { this.broker = broker; }
        public void Initialize() {
            if (initialized) return;
            if (provider == null) {
                provider = broker.RegisterSymbolProvider("TemplateAssembly");
                provider.RegisterFunction(new SymbolFunction("TemplateAssemblyDouble", "TemplateAssembly",
                    "Double a number", "TemplateAssemblyDouble(21)",
                    args => 2 * Convert.ToDouble(args.Evaluate(0), CultureInfo.InvariantCulture), 1, 1));
            }
            provider.AddOrUpdateSymbol("ExampleValue", 42.0);
            initialized = true;
        }
        public void Dispose() {
            if (!initialized) return;
            provider.RemoveSymbol("ExampleValue");
            // NINA exposes no function/provider unregister API. This pure function captures no plugin state.
            initialized = false;
        }
    }
}
