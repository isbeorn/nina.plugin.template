using System.ComponentModel.Composition;
using System.Windows;

namespace TemplateNamespace.Features.container {
    [Export(typeof(ResourceDictionary))]
    public partial class Resources : ResourceDictionary {
        public Resources() { InitializeComponent(); }
    }
}
