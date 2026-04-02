using NUnit.Framework;

namespace StageMind.Tests.EditMode
{
    [TestFixture]
    public class LobbySlidesLoadedStateTests
    {
        private LobbySlidesLoadedState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new LobbySlidesLoadedState(null);
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
    }
}
