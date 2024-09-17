using MSU;

namespace EntityStates.GameplayEvents
{
    /// <summary>
    /// Base state to use for GameplayEvents.
    /// </summary>
    public class GameplayEventState : EntityState
    {
        /// <summary>
        /// The GameplayEvent that instantiated this state.
        /// </summary>
        public GameplayEvent gameplayEvent { get; private set; }
        public override void OnEnter()
        {
            base.OnEnter();
            gameplayEvent = GetComponent<GameplayEvent>();
        }
    }
}
