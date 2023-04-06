using System;
using System.Threading;

namespace Inertia.Paper
{
    internal sealed class PenExecutionLayer : IDisposable
    {
        private const int DefaultTickPerSecond = 60;

        internal event BasicAction<float>? ComponentsUpdate;

        internal bool IsDisposed { get; private set; }

        private PenExecutionLayerType _type;
        private double _targetMsPerTick;
        private Thread? _thread;
        private Clock? _clock;

        internal PenExecutionLayer(int tickPerSecond, PenExecutionLayerType type)
        {
            Change(tickPerSecond, type);

            if (!ReflectionProvider.IsPaperOwned)
            {
                _thread = new Thread(ExecuteCycle);
                _thread.IsBackground = true;

                _thread.Start();
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
        internal void OnComponentsUpdate(float deltaTime)
        {
            ComponentsUpdate?.Invoke(deltaTime);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void ExecuteCycle()
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
                    Thread.Sleep(1);
                }


                currentMsUpdate = _clock.GetElapsedMilliseconds();

                _clock.Reset();
                ComponentsUpdate?.Invoke((float)(currentMsUpdate / 1000.0d));
            }
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