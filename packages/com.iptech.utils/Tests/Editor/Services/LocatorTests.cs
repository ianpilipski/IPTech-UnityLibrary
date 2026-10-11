using NUnit.Framework;

namespace IPTech.Utils.Tests {
    public class LocatorTests {
        [TearDown]
        public void TearDown() {
            Locator.Spy.ClearAll();
        }

        [Test]
        public void Inst_lazily_returns_the_default_context() {
            var locator = Locator.Inst;

            Assert.That(locator, Is.SameAs(Locator.GetContext()));
            Assert.That(Locator.Inst, Is.SameAs(locator));
        }

        [Test]
        public void Inst_can_be_overridden() {
            var locator = new ServiceContainer();

            Locator.Spy.SetInst(locator);

            Assert.That(Locator.Inst, Is.SameAs(locator));
        }

        [Test]
        public void GetContext_returns_the_same_container_for_a_name() {
            var name = GetUniqueContextName();
            var first = Locator.GetContext(name);
            var second = Locator.GetContext(name);

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void GetContext_returns_separate_containers_for_different_names() {
            var first = Locator.GetContext(GetUniqueContextName());
            var second = Locator.GetContext(GetUniqueContextName());

            Assert.That(second, Is.Not.SameAs(first));
        }

        private static string GetUniqueContextName() {
            return $"LocatorTests-{System.Guid.NewGuid():N}";
        }
    }
}