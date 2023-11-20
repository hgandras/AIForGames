using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Prong
{
    internal class PlayerAIReactive : Player
    {
        public PlayerAction GetAction(StaticState config, DynamicState state)
        {
            if (state.ballY > state.plr2PaddleY)
            {
                return PlayerAction.UP;
            }
            else
                return PlayerAction.DOWN;
        }
    }
}
