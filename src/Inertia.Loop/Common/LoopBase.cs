using System;
using System.Threading.Tasks;

namespace Inertia.Loop
{
    public abstract class LoopBase : IDisposable
    {
        private const int MaxTicksPerSecond = 60;

        protected LoopTime LoopTime { get; private set; }

        private float _alwaysSleepMs;
        private LoopMode _loopMode;
        private Func<bool>? _canRunLoopCondition;
        private bool _inLoop;

        public LoopBase() : this(LoopMode.AlwaysSleep)
        {
        }
        public LoopBase(LoopMode loopMode) : this(loopMode, new LoopTime())
        {
        }
        public LoopBase(LoopTime loopTime) : this(LoopMode.AlwaysSleep, new LoopTime())
        {
        }
        public LoopBase(LoopMode loopMode, LoopTime loopTime)
        {
            LoopTime = loopTime;
            _loopMode = loopMode;
            _alwaysSleepMs = (1.0f / MaxTicksPerSecond) * 1000;
        }

        public bool IsDisposed { get; private set; }

        public void Run()
        {
            if (_inLoop)
            {
                throw new InvalidOperationException($"The loop is already running.");
            }

            _inLoop = true;

            DoRun(LoopTime);

            _inLoop = false;
        }
        public Task RunLoopAsync()
        {
            return RunLoopAsync(null);
        }
        public Task RunLoopAsync(Func<bool>? canRunLoop)
        {
            if (_inLoop)
            {
                throw new InvalidOperationException($"The loop is already running.");
            }

            this.ThrowIfDisposable(IsDisposed);

            _canRunLoopCondition = canRunLoop;

            return Task.Factory.StartNew(DoRunLoop, TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void DoRun(LoopTime loopTime);
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                IsDisposed = true;
            }
        }
    
        private async void DoRunLoop()
        {
            var clock = new Clock();
            var time = 0f;

            _inLoop = true;

            while (!IsDisposed)
            {
                clock.Reset();

                DoRun(LoopTime);

                if (_loopMode == LoopMode.AlwaysSleep)
                {
                    var elapsedTime = (float)clock.GetElapsedMilliseconds();
                    if (elapsedTime < _alwaysSleepMs)
                    {
                        await Task.Delay((int)(_alwaysSleepMs - elapsedTime));
                    }
                }
                else if (_loopMode == LoopMode.MinimalSleep)
                {
                    await Task.Delay(1);
                }

                LoopTime.DeltaTime = (float)clock.GetElapsedSeconds();
                LoopTime.Time += LoopTime.DeltaTime;
                LoopTime.Ticks++;

                if (LoopTime.Time - time >= 1)
                {
                    time = LoopTime.Time;
                }

                if (_canRunLoopCondition != null && !_canRunLoopCondition.Invoke()) break;
            }
        }
    }
}