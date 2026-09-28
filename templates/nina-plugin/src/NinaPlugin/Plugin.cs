using NINA.Plugin;
using NINA.Plugin.Interfaces;
using System.ComponentModel.Composition;

namespace TemplateNamespace {
    [Export(typeof(IPluginManifest))]
    public class Plugin : PluginBase {
#if (settings)
        [Import]
        public Features.settings.PluginSettings Settings { get; set; }
#endif
#if (dock_panel)
        // NINA composes manifests and UI parts in separate MEF containers.
        // An assembly-scoped shutdown event reaches the actual exported panel.
        internal static event Action ShuttingDown;
#endif
#if (image_hooks)
        [Import]
        public Features.image_hooks.ImageHooks ImageHooks { get; set; }
#endif
#if (messaging)
        [Import]
        public Features.messaging.PluginMessaging Messaging { get; set; }
#endif
#if (symbols)
        [Import]
        public Features.symbols.PluginSymbols Symbols { get; set; }
#endif

        public override Task Initialize() {
#if (settings)
            Settings.Initialize();
#endif
#if (image_hooks)
            ImageHooks.Initialize();
#endif
#if (messaging)
            Messaging.Initialize();
#endif
#if (symbols)
            Symbols.Initialize();
#endif
            return Task.CompletedTask;
        }

        public override Task Teardown() {
#if (symbols)
            Symbols.Dispose();
#endif
#if (messaging)
            Messaging.Dispose();
#endif
#if (image_hooks)
            ImageHooks.Dispose();
#endif
#if (dock_panel)
            ShuttingDown?.Invoke();
#endif
#if (settings)
            Settings.Dispose();
#endif
            return Task.CompletedTask;
        }
    }
}
