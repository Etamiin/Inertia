using System;
using System.Linq;
using System.Reflection;

namespace Inertia.Scriptable
{
    public abstract class ScriptableObject : IDisposable
    {
        internal enum ScriptableDataState : byte
        {
            NoState = 0,
            Initialized = 1,
            Disposing = 2,
            Disposed = 3
        }

        public static T CreateAndActive<T>(params object[] args) where T : ScriptableObject
        {
            var type = typeof(T);
            var types = args.Select((obj) => obj.GetType()).ToArray();
            var cnstr = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);

            if (cnstr == null)
            {
                throw new NotFoundConstructorException(type, types);
            }

            var instance = (T)cnstr.Invoke(args);
            instance.SetActive();

            return instance;
        }

        public bool IsDisposed
        {
            get
            {
                return State == ScriptableDataState.Disposing || State == ScriptableDataState.Disposed;
            }
        }
        
        internal ScriptableDataState State { get; set; }

        public void SetActive()
        {
            if (IsDisposed || State == ScriptableDataState.Initialized) return;

            var component = RuntimeManager.GetScriptableSystem(GetType());
            if (component != null)
            {
                State = ScriptableDataState.Initialized;
                component.RegisterComponentData(this);
            }
            else Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        { 
            if (disposing)
            {
                State = ScriptableDataState.Disposing;
            }
        }
    }
}