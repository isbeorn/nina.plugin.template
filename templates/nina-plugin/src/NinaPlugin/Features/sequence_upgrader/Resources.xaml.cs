using System.ComponentModel.Composition;
using System.Windows;

namespace TemplateNamespace.Features.sequence_upgrader {
    [Export(typeof(ResourceDictionary))]
    public partial class Resources : ResourceDictionary {
        public Resources() { InitializeComponent(); }
    }
}
