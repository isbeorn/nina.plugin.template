using System.ComponentModel.Composition;
using System.Windows;

namespace TemplateNamespace.Features.editor_integration {
    [Export(typeof(ResourceDictionary))]
    public partial class Resources : ResourceDictionary {
        public Resources() { InitializeComponent(); }
    }
}
