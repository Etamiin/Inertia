using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    public class LoopContext
    {
        private event LoopContextHandler ContextHandler = () => { };
        private Thread _looper;

        public LoopContext()
        {
            _looper = new Thread(ExecuteLoopContext);
        }

        private void ExecuteLoopContext()
        {
            while (_looper.IsAlive)
            {
                try
                {
                    ContextHandler();
                }
                catch (Exception e)
                {
                    Logger.Error("Loop context error : " + e.ToString());
                }
            }
        }

        public void AddHandler(ActionHandler handler)
        {
            ContextHandler += () => handler();
        }

        public void Clear()
        {
            ContextHandler = () => { };
        }

        public void Start()
        {
            _looper.Start();
        }
        public void Stop()
        {
            _looper.Abort();
        }

    }
}
