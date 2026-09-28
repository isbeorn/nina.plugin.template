using System.ComponentModel.Composition;
using System.Windows;

namespace TemplateNamespace.Features.condition {
    [Export(typeof(ResourceDictionary))]
    public partial class Resources : ResourceDictionary {
        public Resources() { InitializeComponent(); }
    }
}
