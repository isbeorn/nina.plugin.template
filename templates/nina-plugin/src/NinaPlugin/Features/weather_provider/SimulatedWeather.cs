using NINA.Core.Utility;
using NINA.Equipment.Interfaces;

namespace TemplateNamespace.Features.weather_provider {
    public sealed class SimulatedWeather : BaseINPC, IWeatherData {
        private bool connected;
        public string Id => new Plugin().Identifier + ":weather";
        public string Name => "TemplateAssembly simulated weather";
        public string DisplayName => Name;
        public string Category => "Simulation";
        public string Description => "Fixed demonstration readings. Never use for observatory safety.";
        public string DriverInfo => "Template example";
        public string DriverVersion => typeof(SimulatedWeather).Assembly.GetName().Version.ToString();
        public bool Connected { get => connected; private set { connected = value; RaisePropertyChanged(); } }
        public IList<string> SupportedActions => Array.Empty<string>();
        public bool HasSetupDialog => false;
        public double Temperature => Connected ? 10 : double.NaN;
        public double Humidity => Connected ? 50 : double.NaN;
        public double DewPoint => Connected ? 0 : double.NaN;
        public double AveragePeriod => 0;
        public double Pressure => double.NaN;
        public double RainRate => double.NaN;
        public double SkyBrightness => double.NaN;
        public double SkyQuality => double.NaN;
        public double SkyTemperature => double.NaN;
        public double StarFWHM => double.NaN;
        public double WindDirection => double.NaN;
        public double WindGust => double.NaN;
        public double WindSpeed => double.NaN;
        public double CloudCover => double.NaN;
        public Task<bool> Connect(CancellationToken token) {
            token.ThrowIfCancellationRequested();
            Connected = true;
            return Task.FromResult(true);
        }
        public void Disconnect() { Connected = false; }
        public void SetupDialog() => throw new NotSupportedException();
        public string Action(string actionName, string actionParameters) => throw new NotSupportedException();
        public void SendCommandBlind(string command, bool raw = true) => throw new NotSupportedException();
        public bool SendCommandBool(string command, bool raw = true) => throw new NotSupportedException();
        public string SendCommandString(string command, bool raw = true) => throw new NotSupportedException();
    }
}
