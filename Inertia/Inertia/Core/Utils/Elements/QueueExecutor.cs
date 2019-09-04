using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    public static class QueueExecutor
    {
        public class Manual
        {
            private event ActionHandler Handlers = () => { };

            public void Enqueue(Action action)
            {
                void handler()
                {
                    action();
                    Handlers -= handler;
                }

                Handlers += handler;
            }

            public void Execute()
            {
                Handlers();
            }
        }
        public class Manual<T>
        {
            private readonly Queue<T> _queue;
            private readonly Action<T> _executeAction;

            public Manual(Action<T> OnExecute)
            {
                _queue = new Queue<T>();
                _executeAction = OnExecute;
            }
            public void Enqueue(T obj)
            {
                _queue.Enqueue(obj);
            }

            public void Execute()
            {
                if (_queue.Count == 0)
                    return;

                var obj = _queue.Dequeue();
                try {
                    _executeAction(obj);
                }
                catch (Exception e)
                {
                    Logger.Log("QueueExecutor handle error >> " + e.ToString());
                }
            }
        }
        public class Auto
        {
            private Queue<Action> _queue;

            public Auto()
            {
                _queue = new Queue<Action>();
                RuntimeModule.ExecuteAsync(Execute);
            }

            public void Enqueue(Action action)
            {
                _queue.Enqueue(action);
            }

            private void Execute()
            {
                while (true)
                {
                    if (_queue.Count == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    var obj = _queue.Dequeue();
                    try
                    {
                        obj();
                    }
                    catch (Exception e)
                    {
                        Logger.Log("QueueExecutor handle error >> " + e.ToString());
                    }
                }
            }
        }
        public class Auto<T>
        {
            private readonly Queue<T> _queue;
            private readonly Action<T> _executeAction;

            public Auto(Action<T> OnExecute)
            {
                _queue = new Queue<T>();
                _executeAction = OnExecute;

                RuntimeModule.ExecuteAsync(Execute);
            }

            public void Enqueue(T obj)
            {
                _queue.Enqueue(obj);
            }
            private void Execute()
            {
                while (true)
                {
                    if (_queue.Count == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    var obj = _queue.Dequeue();
                    try
                    {
                        _executeAction(obj);
                    }
                    catch (Exception e)
                    {
                        Logger.Log("QueueExecutor handle error >> " + e.ToString());
                    }
                }
            }
        }
    }
}
