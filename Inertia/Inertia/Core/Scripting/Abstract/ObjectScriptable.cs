using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class ObjectScriptable
    {
        internal Dictionary<uint, Scriptable> Scripts = new Dictionary<uint, Scriptable>();

        public T Add<T>() where T : Scriptable
        {
            var type = typeof(T);
            var obj = type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });

            var script = (Scriptable)obj;

            script.SetParameters(this, ScriptingModule.currentScriptId++);
            Scripts.Add(script.Id, script);

            return (T)obj;
        }

        public T Get<T>() where T : Scriptable
        {
            lock (Scripts)
                foreach (var v in Scripts)
                    if (v.Value is T) {
                        return (T)v.Value;
                    }
            return null;
        }
        public T[] GetAll<T>() where T : Scriptable
        {
            var scripts = new List<T>();
            lock (this.Scripts)
                foreach (var v in this.Scripts)
                    if (v.Value is T)
                        scripts.Add((T)v.Value);

            return scripts.ToArray();
        }

        public void Dispose()
        {
            Scriptable[] allScripts = null;
            lock (Scripts)
                allScripts = Scripts.Values.ToArray();

            foreach (var script in allScripts)
                script.Dispose();

            Scripts.Clear();
        }
    }
}
