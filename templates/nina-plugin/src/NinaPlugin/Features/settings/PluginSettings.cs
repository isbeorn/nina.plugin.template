using NINA.Core.Utility;
using NINA.Profile;
using NINA.Profile.Interfaces;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.Json;

namespace TemplateNamespace.Features.settings {
    [Export(typeof(PluginSettings))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class PluginSettings : BaseINPC, IDisposable {
        private readonly IProfileService profiles;
        private readonly PluginOptionsAccessor profileOptions;
        private readonly string globalPath;
        private string globalMessage = "Hello from your plugin";
        private bool initialized;

        [ImportingConstructor]
        public PluginSettings(IProfileService profiles) {
            this.profiles = profiles;
            var id = new Plugin().Identifier;
            profileOptions = new PluginOptionsAccessor(profiles, Guid.Parse(id));
            globalPath = Path.Combine(CoreUtil.APPLICATIONTEMPPATH, "PluginSettings", id, "settings.json");
        }
        public void Initialize() {
            if (initialized) return;
            if (File.Exists(globalPath)) {
                globalMessage = JsonSerializer.Deserialize<string>(File.ReadAllText(globalPath)) ?? globalMessage;
            }
            profiles.ProfileChanged += OnProfileChanged;
            initialized = true;
            RaisePropertyChanged(nameof(GlobalMessage));
        }
        public string GlobalMessage {
            get => globalMessage;
            set {
                if (globalMessage == value) return;
                Directory.CreateDirectory(Path.GetDirectoryName(globalPath));
                var temporaryPath = globalPath + ".tmp";
                File.WriteAllText(temporaryPath, JsonSerializer.Serialize(value));
                File.Move(temporaryPath, globalPath, true);
                globalMessage = value;
                RaisePropertyChanged();
            }
        }
        public string ProfileMessage {
            get => profileOptions.GetValueString(nameof(ProfileMessage), "Profile message");
            set { profileOptions.SetValueString(nameof(ProfileMessage), value); RaisePropertyChanged(); }
        }
        private void OnProfileChanged(object sender, EventArgs args) => RaisePropertyChanged(nameof(ProfileMessage));
        public void Dispose() {
            if (!initialized) return;
            profiles.ProfileChanged -= OnProfileChanged;
            initialized = false;
        }
    }
}
