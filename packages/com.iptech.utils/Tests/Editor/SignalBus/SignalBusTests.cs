using System;
using NUnit.Framework;

namespace IPTech.Utils.Tests {
    public class SignalBusTests {
        private SignalBus signalBus;

        [SetUp]
        public void SetUp() {
            signalBus = new SignalBus();
        }

        [Test]
        public void Send_notifies_listeners_of_the_signal_type() {
            TestSignal received = null;
            signalBus.Listen<TestSignal>(signal => received = signal);
            var sent = new TestSignal();

            signalBus.Send(sent);

            Assert.That(received, Is.SameAs(sent));
        }

        [Test]
        public void Send_notifies_listeners_registered_for_a_base_signal_type() {
            BaseTestSignal received = null;
            signalBus.Listen<BaseTestSignal>(signal => received = signal);
            var sent = new DerivedTestSignal();

            signalBus.Send(sent);

            Assert.That(received, Is.SameAs(sent));
        }

        [Test]
        public void Unlisten_stops_notifying_the_listener() {
            var notificationCount = 0;
            Action<TestSignal> listener = _ => notificationCount++;
            signalBus.Listen(listener);

            signalBus.Unlisten(listener);
            signalBus.Send(new TestSignal());

            Assert.That(notificationCount, Is.Zero);
        }

        [Test]
        public void Listen_raises_listener_registered_with_the_type_and_delegate() {
            Type registeredType = null;
            Delegate registeredDelegate = null;
            Action<TestSignal> listener = _ => { };
            signalBus.ListenerRegistered += (type, callback) => {
                registeredType = type;
                registeredDelegate = callback;
            };

            signalBus.Listen(listener);

            Assert.That(registeredType, Is.EqualTo(typeof(TestSignal)));
            Assert.That(registeredDelegate, Is.SameAs(listener));
        }

        [Test]
        public void Listen_replays_the_latest_sticky_signal() {
            signalBus.Send(new StickyTestSignal(1));
            signalBus.Send(new StickyTestSignal(2));
            var receivedValue = 0;

            signalBus.Listen<StickyTestSignal>(signal => receivedValue = signal.Value);

            Assert.That(receivedValue, Is.EqualTo(2));
        }

        [Test]
        public void Listen_does_not_replay_non_sticky_signals() {
            signalBus.Send(new TestSignal());
            var notificationCount = 0;

            signalBus.Listen<TestSignal>(_ => notificationCount++);

            Assert.That(notificationCount, Is.Zero);
        }

        private class BaseTestSignal : Signal { }

        private sealed class DerivedTestSignal : BaseTestSignal { }

        private sealed class TestSignal : Signal { }

        private sealed class StickyTestSignal : Signal {
            public int Value { get; }

            public StickyTestSignal(int value) : base(true) {
                Value = value;
            }
        }
    }
}