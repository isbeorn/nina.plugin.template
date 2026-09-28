using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.ViewModel;
using System.ComponentModel.Composition;

namespace TemplateNamespace.Features.weather_provider {
    [Export(typeof(IEquipmentProvider))]
    public class WeatherProvider : IEquipmentProvider<IWeatherData> {
        public string Name => "TemplateAssembly simulation";
        public IList<IWeatherData> GetEquipment() => new IWeatherData[] { new SimulatedWeather() };
    }
}
