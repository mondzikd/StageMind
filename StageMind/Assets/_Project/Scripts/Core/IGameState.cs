namespace StageMind
{
    public interface IGameState
    {
        void Enter();
        void Exit();
        void Update();
        bool HandleInput(InputActionType actionType);
    }
}
