using $pluginnamespace$.Properties;
using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Settings = $pluginnamespace$.Properties.Settings;

namespace $pluginnamespace$ {
    /// <summary>
    /// This class exports the IPluginManifest interface and will be used for the general plugin information and options.
    /// The base class "PluginBase" populates manifest metadata from the AssemblyInfo attributes.
    /// </summary>
    [Export(typeof(IPluginManifest))]
    public class $pluginclassname$ : PluginBase {
        [ImportingConstructor]
        public $pluginclassname$() {
            if (Settings.Default.UpdateSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Settings.Default);
            }
        }

        public override Task Teardown() {
            return base.Teardown();
        }
    }
}
