using System.ComponentModel.Composition;
using System.Windows;

namespace TemplateNamespace.Features.settings {
    [Export(typeof(ResourceDictionary))]
    public partial class Resources : ResourceDictionary {
        public Resources() { InitializeComponent(); this[new Plugin().Name + "_Options"] = this["OptionsTemplate"]; Remove("OptionsTemplate"); }
    }
}
