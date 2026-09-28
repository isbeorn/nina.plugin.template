using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.weather_provider;

namespace TemplateNamespace.Tests.Features.weather_provider {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public async Task ConnectDisconnectAndCancellation() {
            var device = new SimulatedWeather();
            Assert.That(double.IsNaN(device.Temperature), Is.True);
            Assert.Throws<OperationCanceledException>(() => device.Connect(new CancellationToken(true)));
            Assert.That(device.Connected, Is.False);
            Assert.That(await device.Connect(CancellationToken.None), Is.True);
            Assert.That(device.Temperature, Is.EqualTo(10));
            device.Disconnect(); device.Disconnect();
            Assert.That(device.Connected, Is.False);
            Assert.That(double.IsNaN(device.Temperature), Is.True);
            Assert.That(await device.Connect(CancellationToken.None), Is.True);
            device.Disconnect();
        }
    }
}
