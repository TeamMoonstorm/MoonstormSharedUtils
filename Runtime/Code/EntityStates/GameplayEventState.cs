using MSU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.GameplayEvents
{
    public class GameplayEventState : EntityState
    {
        public GameplayEvent GameplayEvent { get; private set; }
        public override void OnEnter()
        {
            base.OnEnter();
            GameplayEvent = GetComponent<GameplayEvent>();
        }
    }
}
