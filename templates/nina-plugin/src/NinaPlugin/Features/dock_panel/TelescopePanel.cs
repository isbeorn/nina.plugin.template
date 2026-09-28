using NINA.Core.Utility;
using NINA.Equipment.Equipment.MyTelescope;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TemplateNamespace.Features.dock_panel {
    // The host composes this panel separately from the manifest.
    [Export(typeof(IDockableVM))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class TelescopePanel : BaseINPC, IDockableVM, ITelescopeConsumer, IDisposable {
        private readonly ITelescopeMediator telescope;
        private bool initialized;
        private bool connected;
        private bool visible = true;
        [ImportingConstructor]
        public TelescopePanel(ITelescopeMediator telescope) {
            this.telescope = telescope;
            HideCommand = new CommunityToolkit.Mvvm.Input.RelayCommand<object>(Hide);
            ImageGeometry = new GeometryGroup();
            ImageGeometry.Children.Add(Geometry.Parse("M0,0 L10,0 10,10 0,10 Z"));
            ImageGeometry.Freeze();
            Initialize();
        }
        public void Initialize() {
            if (initialized) return;
            initialized = true;
            telescope.RegisterConsumer(this);
            Plugin.ShuttingDown += Dispose;
        }
        public bool Connected { get => connected; private set { connected = value; RaisePropertyChanged(); } }
        public void UpdateDeviceInfo(TelescopeInfo info) {
            var next = info.Connected;
            void Update() { if (initialized) Connected = next; }
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess()) dispatcher.BeginInvoke((Action)Update);
            else Update();
        }
        public void Dispose() {
            if (!initialized) return;
            initialized = false;
            telescope.RemoveConsumer(this);
            Plugin.ShuttingDown -= Dispose;
        }
        public string ContentId => typeof(TelescopePanel).FullName;
        public string Title { get; set; } = "Example telescope status";
        public bool CanClose { get; set; } = true;
        public bool IsClosed { get; set; }
        public bool HasSettings { get; set; }
        public bool IsTool => false;
        public bool IsVisible { get => visible; set { visible = value; RaisePropertyChanged(); } }
        public GeometryGroup ImageGeometry { get; set; }
        public ICommand HideCommand { get; }
        public void Hide(object value) { IsVisible = !IsVisible; }
    }
}
