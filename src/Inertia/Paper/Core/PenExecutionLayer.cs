using System;
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
            Change(tickPerSecond, type);

            if (!ReflectionProvider.IsPaperOwned)
            {
                _task = Task.Run(ExecuteCycle);
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
                if (_clock == null)
                {
                    _clock = new Clock();
                }
                else _clock.Reset();

                _type = type;
            }
        }
        internal void OnTicking(float deltaTime)
        {
            Ticking?.Invoke(this, new PenLayerTickingArgs(deltaTime));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private async Task ExecuteCycle()
        {
            while (!IsDisposed && _type != PenExecutionLayerType.None)
            {
                var currentMsUpdate = _clock.GetElapsedMilliseconds();

                if (_type == PenExecutionLayerType.TickBased)
                {
                    if (currentMsUpdate < _targetMsPerTick)
                    {
                        var msToSleep = _targetMsPerTick - currentMsUpdate;
                        var sleepTicks = TimeSpan.TicksPerMillisecond * msToSleep;

                        while (_clock.ElapsedTicks < sleepTicks) ;
                    }
                }
                else if (_type == PenExecutionLayerType.FixedSleep)
                {
                    await Task.Delay(1).ConfigureAwait(false);
                }

                currentMsUpdate = _clock.GetElapsedMilliseconds();

                _clock.Reset();
                Ticking?.Invoke(this, new PenLayerTickingArgs((float)(currentMsUpdate / 1000.0d)));
            }

            _task = null;
        }
        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                _type = PenExecutionLayerType.None;
            }

            IsDisposed = true;
        }
    }
}