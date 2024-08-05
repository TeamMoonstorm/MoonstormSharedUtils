using MSU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public GameplayEvent GameplayEvent { get; private set; }
        public override void OnEnter()
        {
            base.OnEnter();
            GameplayEvent = GetComponent<GameplayEvent>();
        }
    }
}
