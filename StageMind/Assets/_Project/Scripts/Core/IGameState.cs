namespace StageMind
{
    public interface IGameState
    {
        void Enter();
        void Exit();
        void Update();
        void HandleInput(InputActionType actionType);
    }
}
