using System;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Paper
{
    internal sealed class PenExecutionLayer : IDisposable
    {
        private const int DefaultTickPerSecond = 60;

        internal event EventHandler<PenLayerTickingArgs>? Ticking;
        internal bool IsDisposed { get; private set; }

        private PenExecutionLayerType _type;
        private double _targetMsPerTick;
        private Task? _task;
        private Clock? _clock;

        internal PenExecutionLayer(int tickPerSecond, PenExecutionLayerType type)
        {
            _clock = new Clock();
            Change(tickPerSecond, type);

            if (!ReflectionProvider.IsPaperOwned)
            {
                _task = Task.Run(TickLayer);
            }
            else
            {
                PaperFactory.LayersTick += DoTick;
            }
        }

        internal void Change(int tickPerSecond, PenExecutionLayerType type)
        {
            if (IsDisposed) return;

            if (tickPerSecond <= 0)
            {
                tickPerSecond = DefaultTickPerSecond;
            }

            _targetMsPerTick = 1000.0d / tickPerSecond;
            if (!ReflectionProvider.IsPaperOwned)
            {
                _clock.Reset();
                _type = type;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private async Task TickLayer()
        {
            while (!IsDisposed)
            {
                var currentTickMs = _clock.GetElapsedMilliseconds();
                if (currentTickMs < _targetMsPerTick)
                {
                    var msToSleep = _targetMsPerTick - currentTickMs;

                    if (_type == PenExecutionLayerType.TickBased)
                    {
                        var waitToTicks = _clock.ElapsedTicks + (TimeSpan.TicksPerMillisecond * msToSleep);

                        while (_clock.ElapsedTicks < waitToTicks) ;
                    }
                    else if (_type == PenExecutionLayerType.ProcessorClockBased)
                    {
                        await Task.Delay((int)msToSleep).ConfigureAwait(false);
                    }
                }

                var deltaTime = (float)(_clock.GetElapsedMilliseconds() / 1000.0d);

                _clock.Reset();
                Ticking?.Invoke(this, new PenLayerTickingArgs(deltaTime));
            }

            _task = null;
        }
        private void DoTick(float deltaTime)
        {
            Ticking?.Invoke(this, new PenLayerTickingArgs(deltaTime));
        }
        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                _task?.Dispose();
                _task = null;

                if (ReflectionProvider.IsPaperOwned)
                {
                    PaperFactory.LayersTick -= DoTick;
                }
            }

            IsDisposed = true;
        }
    }
}