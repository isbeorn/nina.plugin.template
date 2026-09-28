using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.star_annotation;
using NINA.Image.ImageAnalysis;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace TemplateNamespace.Tests.Features.star_annotation {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [Test]
        public async Task AnnotatesWithoutChangingSourceAndHandlesCancellation() {
            var input = BitmapSource.Create(20, 20, 96, 96, PixelFormats.Bgra32, null, new byte[20 * 20 * 4], 80);
            input.Freeze();
            var result = new StarDetectionResult { StarList = [new DetectedStar { Position = new Accord.Point(10, 10) }] };
            var annotator = new ExampleStarAnnotator();
            var output = await annotator.GetAnnotatedImage(new StarDetectionParams(), result, input);
            Assert.That(output, Is.Not.SameAs(input));
            Assert.That(output.IsFrozen, Is.True);
            var pixels = new byte[20 * 20 * 4];
            output.CopyPixels(pixels, 80, 0);
            Assert.That(pixels.Any(p => p != 0), Is.True);
            var empty = await annotator.GetAnnotatedImage(new StarDetectionParams(), result, input, 0);
            empty.CopyPixels(pixels, 80, 0);
            Assert.That(pixels.All(p => p == 0), Is.True);
            Assert.Throws<OperationCanceledException>(() => annotator.GetAnnotatedImage(new StarDetectionParams(), result, input, token: new CancellationToken(true)));
        }
    }
}
