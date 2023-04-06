using System;
using System.Threading.Tasks;

namespace Inertia
{
    public sealed class AsyncActionQueue : ActionQueue
    {
        private DateTime? _emptySince;

        public AsyncActionQueue(ActionQueueParameters parameters) : base(parameters)
        {
            Task.Factory.StartNew(Execute, TaskCreationOptions.LongRunning);
        }

        private async void Execute()
        {
            while (true)
            {
                if (DisposeRequested)
                {
                    Clean();
                    break;
                }

                if (Count == 0)
                {
                    if (_parameters.TimeBeforeDisposeOnEmptyQueue.HasValue)
                    {
                        if (_emptySince.HasValue)
                        {
                            var span = DateTime.Now - _emptySince;
                            if (span >= _parameters.TimeBeforeDisposeOnEmptyQueue.Value)
                            {
                                RequestDispose();
                                continue;
                            }
                        }
                        else
                        {
                            _emptySince = DateTime.Now;
                        }
                    }

                    await Task.Delay(_parameters.SleepTimeOnEmptyQueue);
                }
                else
                {
                    _emptySince = null;
                    ProcessQueue();
                }
            }
        }
    }
}