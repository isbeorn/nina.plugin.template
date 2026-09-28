using NUnit.Framework;
using Newtonsoft.Json;
using TemplateNamespace.Features.editor_integration;
using NINA.Sequencer.Container;
using NINA.Sequencer.Editing;
using NINA.Sequencer.Behaviors;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
namespace TemplateNamespace.Tests.Features.editor_integration {
    [NonParallelizable, Apartment(ApartmentState.STA)]
    public class FeatureTests {
        [TestCase(UpdateSourceTrigger.PropertyChanged)]
        [TestCase(UpdateSourceTrigger.LostFocus)]
        public void RealBindingUndoRedoRestoresBothDimensions(UpdateSourceTrigger trigger) {
            var root = new SequenceRootContainer();
            var item = new CoupledInstruction { Width = 2, Height = 7 };
            root.Add(item);
            // The journal is internal to NINA. This pinned-host integration test uses reflection only
            // to host the real binding behavior; production plugin code uses public extension contracts.
            var historyType = typeof(SequenceEditContext).Assembly.GetType("NINA.Sequencer.Editing.SequenceEditHistory", true);
            var history = (ISequenceEditHistory)Activator.CreateInstance(historyType, root);
            using var lifetime = (IDisposable)history;
            var box = new TextBox { DataContext = item };
            box.SetBinding(TextBox.TextProperty, new Binding(nameof(item.Width)) { UpdateSourceTrigger = trigger });
            var behavior = new SequenceEditBehavior { History = history };
            behavior.Attach(box);
            try {
                box.RaiseEvent(new TextCompositionEventArgs(Keyboard.PrimaryDevice, new TextComposition(InputManager.Current, box, "5")) {
                    RoutedEvent = TextCompositionManager.PreviewTextInputEvent
                });
                box.SetCurrentValue(TextBox.TextProperty, "5");
                typeof(SequenceEditBehavior).GetMethod("Commit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(behavior, null);
                Assert.That(item.Width, Is.EqualTo(5));
                Assert.That(item.Height, Is.EqualTo(5));
                Assert.That(historyType.GetMethod("Undo").Invoke(history, null), Is.True);
                Assert.That(item.Width, Is.EqualTo(2));
                Assert.That(item.Height, Is.EqualTo(7));
                Assert.That(historyType.GetMethod("Redo").Invoke(history, null), Is.True);
                Assert.That(item.Width, Is.EqualTo(5));
                Assert.That(item.Height, Is.EqualTo(5));
            } finally { behavior.Detach(); }
        }
    }
}
