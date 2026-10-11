using NUnit.Framework;

namespace IPTech.Utils.Tests {
    public class SignalTests {
        [Test]
        public void Signal_defaults_to_non_sticky() {
            var signal = new TestSignal();

            Assert.That(signal.Sticky, Is.False);
        }

        [Test]
        public void Signal_can_be_created_as_sticky() {
            var signal = new TestSignal(true);

            Assert.That(signal.Sticky, Is.True);
        }

        [Test]
        public void Signal_can_be_created_as_non_sticky() {
            var signal = new TestSignal(false);

            Assert.That(signal.Sticky, Is.False);
        }

        private sealed class TestSignal : Signal {
            public TestSignal() { }

            public TestSignal(bool sticky) : base(sticky) { }
        }
    }
}