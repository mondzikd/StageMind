using NUnit.Framework;

namespace StageMind.Tests.EditMode
{
    [TestFixture]
    public class PausedStateTests
    {
        private PausedState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new PausedState(null, GameStateType.Rehearsal);
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
            Assert.DoesNotThrow(() => _state.HandleInput());
        }

        [Test]
        public void Constructor_ImplementsIGameState()
        {
            Assert.IsInstanceOf<IGameState>(_state);
        }

        [Test]
        public void PreviousStateType_StoresConstructorValue()
        {
            Assert.AreEqual(GameStateType.Rehearsal, _state.PreviousStateType);
        }

        [Test]
        public void PreviousStateType_DifferentValue_StoresCorrectly()
        {
            var state = new PausedState(null, GameStateType.LobbyLanding);

            Assert.AreEqual(GameStateType.LobbyLanding, state.PreviousStateType);
        }
    }
}
