using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.symbols;
using Moq;
using NINA.Sequencer.Logic;
namespace TemplateNamespace.Tests.Features.symbols {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public void RegistersPureFunctionAndRemovesSymbol() {
            var broker = new Mock<ISymbolBroker>();
            var provider = new Mock<ISymbolProvider>();
            broker.Setup(b => b.RegisterSymbolProvider(It.IsAny<string>())).Returns(provider.Object);
            SymbolFunction function = null;
            provider.Setup(p => p.RegisterFunction(It.IsAny<SymbolFunction>())).Callback<SymbolFunction>(f => function = f);
            using var symbols = new PluginSymbols(broker.Object);
            symbols.Initialize(); symbols.Initialize();
            var arguments = new Mock<ISymbolFunctionArguments>();
            arguments.Setup(a => a.Evaluate(0)).Returns(21.0);
            Assert.That(function.Implementation(arguments.Object), Is.EqualTo(42.0));
            symbols.Dispose(); symbols.Dispose();
            provider.Verify(p => p.RemoveSymbol("ExampleValue"), Times.Once);
            symbols.Initialize();
            broker.Verify(b => b.RegisterSymbolProvider(It.IsAny<string>()), Times.Once);
        }
    }
}
