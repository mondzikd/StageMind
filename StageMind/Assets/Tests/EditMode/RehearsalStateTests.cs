using NUnit.Framework;
using StageMind.Tests.EditMode.Mocks;
using UnityEngine;

namespace StageMind.Tests.EditMode
{
    [TestFixture]
    public class RehearsalStateTests
    {
        private RehearsalState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new RehearsalState(null);
        }

        [Test]
        public void Enter_ExecutesWithoutException()
        {
            Assert.DoesNotThrow(() => _state.Enter());
        }

        [Test]
        public void Exit_ExecutesWithoutException()
        {
            Assert.DoesNotThrow(() => _state.Exit());
        }

        [Test]
        public void Update_ExecutesWithoutException()
        {
            Assert.DoesNotThrow(() => _state.Update());
        }

        [Test]
        public void HandleInput_ExecutesWithoutException()
        {
            Assert.DoesNotThrow(() => _state.HandleInput(InputActionType.AdvanceSlide));
        }

        [Test]
        public void HandleInput_AdvanceSlide_UsesWebViewAndReturnsResult()
        {
            var webView = new MockWebViewController { SendKeyEventResult = false };
            var state = new RehearsalState(null, webView);

            bool handled = state.HandleInput(InputActionType.AdvanceSlide);

            Assert.IsFalse(handled);
            Assert.AreEqual(1, webView.SentKeyEvents.Count);
            Assert.AreEqual(KeyCode.RightArrow, webView.SentKeyEvents[0]);
        }

        [Test]
        public void HandleInput_PreviousSlide_UsesWebViewAndReturnsResult()
        {
            var webView = new MockWebViewController { SendKeyEventResult = true };
            var state = new RehearsalState(null, webView);

            bool handled = state.HandleInput(InputActionType.PreviousSlide);

            Assert.IsTrue(handled);
            Assert.AreEqual(1, webView.SentKeyEvents.Count);
            Assert.AreEqual(KeyCode.LeftArrow, webView.SentKeyEvents[0]);
        }

        [Test]
        public void Constructor_ImplementsIGameState()
        {
            Assert.IsInstanceOf<IGameState>(_state);
        }
    }
}
