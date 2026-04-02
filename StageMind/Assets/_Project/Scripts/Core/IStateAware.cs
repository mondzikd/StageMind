namespace StageMind
{
    public interface IStateAware
    {
        void OnStateEnter(GameStateType state);
        void OnStateExit(GameStateType state);
    }
}
