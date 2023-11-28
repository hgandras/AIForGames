using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prong
{
    class PongEnginePerf
    {
        private StaticState config;
        private DynamicState state;
        private PongEngine engine;
        private Player player2 = new PlayerAIReactive();
        private Player player1 = new PlayerAIAStar();

        public PongEnginePerf()
        {
            config = new StaticState();
            config.ClientSize_Width = 640;
            config.ClientSize_Height = 480;

            state = new DynamicState();

            engine = new PongEngine(config);
        }

        public void Run()
        {
            PlayerAction plr1 = player1.GetAction(config, state);
            PlayerAction plr2 = player2.GetAction(config, state);
            engine.Tick(state, plr1, plr2, 0.05f);
        }

        public void Perf(long iterations)
        {
            TimeIt.ExecuteAndReport("PongEngine", this.Run, iterations);
        }

        public void Perf(double maxTimeSecs)
        {
            TimeIt.ExecuteAndReport("PongEngine", this.Run, maxTimeSecs);
        }

        public static void RunPerf()
        {
            PongEnginePerf enginePerf = new PongEnginePerf();
            enginePerf.Perf(1000000);
            enginePerf.Perf(0.05);
        }
    }
}
