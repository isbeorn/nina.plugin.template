using NUnit.Framework;
using NINA.Core.Utility;
using System.IO;
using System.Windows;

namespace TemplateNamespace.Tests {
    [SetUpFixture]
    public class TestEnvironment {
        private string previousRoot;
        public static string Root { get; private set; }
        [OneTimeSetUp]
        public void Start() {
            previousRoot = CoreUtil.APPLICATIONTEMPPATH;
            Root = Path.Combine(TestContext.CurrentContext.WorkDirectory, "runtime", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
            CoreUtil.APPLICATIONTEMPPATH = Root;
        }
        [OneTimeTearDown]
        public void Stop() {
            CoreUtil.APPLICATIONTEMPPATH = previousRoot;
            // NINA may retain open log handles until the test process exits.
        }
    }
}
