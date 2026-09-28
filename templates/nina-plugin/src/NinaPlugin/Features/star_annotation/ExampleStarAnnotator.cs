using NINA.Core.Interfaces;
using NINA.Image.ImageAnalysis;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TemplateNamespace.Features.star_annotation {
    [Export(typeof(IPluggableBehavior))]
    public class ExampleStarAnnotator : IStarAnnotator {
        public string Name => "TemplateAssembly star markers";
        public string ContentId => GetType().FullName;
        public Task<BitmapSource> GetAnnotatedImage(StarDetectionParams parameters, StarDetectionResult result,
            BitmapSource imageToAnnotate, int maxStars = 200, CancellationToken token = default) {
            token.ThrowIfCancellationRequested();
            var visual = new DrawingVisual();
            using (var drawing = visual.RenderOpen()) {
                drawing.DrawImage(imageToAnnotate, new Rect(0, 0, imageToAnnotate.PixelWidth, imageToAnnotate.PixelHeight));
                foreach (var star in (result.StarList ?? []).Take(Math.Max(0, maxStars))) {
                    token.ThrowIfCancellationRequested();
                    drawing.DrawEllipse(null, new Pen(Brushes.Lime, 1), new Point(star.Position.X, star.Position.Y), 4, 4);
                }
            }
            var annotated = new RenderTargetBitmap(imageToAnnotate.PixelWidth, imageToAnnotate.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            annotated.Render(visual);
            annotated.Freeze();
            return Task.FromResult<BitmapSource>(annotated);
        }
    }
}
