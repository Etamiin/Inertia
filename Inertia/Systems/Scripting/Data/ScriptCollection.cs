using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Scripting
{
    public class ScriptCollection
    {
        #region Public variables

        public int Count
        {
            get
            {
                return Scripts.Count;
            }
        }

        #endregion

        #region Private variables

        private List<Scriptable> Scripts;

        #endregion

        #region Constructors

        public ScriptCollection()
        {
            Scripts = new List<Scriptable>();
        }

        #endregion

        public T Add<T>() where T : Scriptable
        {
            var type = typeof(T);
            var obj = type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
            var script = (Scriptable)obj;

            script.SetParameters(this);
            Scripts.Add(script);

            return (T)obj;
        }

        public T Get<T>() where T : Scriptable
        {
            lock (Scripts)
            {
                foreach (var v in Scripts)
                {
                    if (v is T)
                        return (T)v;
                }
            }

            return null;
        }
        public T[] GetAll<T>() where T : Scriptable
        {
            var scripts = new List<T>();
            lock (Scripts)
            {
                foreach (var v in Scripts)
                {
                    if (v is T)
                        scripts.Add((T)v);
                }
            }

            return scripts.ToArray();
        }

        public void Remove<T>() where T : Scriptable
        {
            var script = Get<T>();
            script.Destroy();
        }
        public void RemoveAll<T>() where T : Scriptable
        {
            var scripts = GetAll<T>();
            foreach (var script in scripts)
                script.Destroy();
        }
        internal void Remove(Scriptable script)
        {
            Scripts.Remove(script);
        }

        public void Dispose()
        {
            Scriptable[] allScripts = null;
            lock (Scripts)
                allScripts = Scripts.ToArray();

            foreach (var script in allScripts)
                script.Dispose();

            Scripts.Clear();
            Scripts = null;
        }
    }
}
