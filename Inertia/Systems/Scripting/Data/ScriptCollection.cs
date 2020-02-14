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

        private Dictionary<int, Scriptable> Scripts;

        #endregion

        #region Constructors

        public ScriptCollection()
        {
            Scripts = new Dictionary<int, Scriptable>();
        }

        #endregion

        public T Add<T>() where T : Scriptable
        {
            var type = typeof(T);
            var obj = type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
            var script = (Scriptable)obj;

            script.SetParameters(this);
            Scripts.Add(script.Id, script);

            return (T)obj;
        }

        public T Get<T>() where T : Scriptable
        {
            lock (Scripts)
            {
                foreach (var v in Scripts)
                {
                    if (v.Value is T)
                        return (T)v.Value;
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
                    if (v.Value is T)
                        scripts.Add((T)v.Value);
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
        internal void Remove(int scriptId)
        {
            Scripts.Remove(scriptId);
        }

        public void Dispose()
        {
            Scriptable[] allScripts = null;
            lock (Scripts)
                allScripts = Scripts.Values.ToArray();

            foreach (var script in allScripts)
                script.Dispose();

            Scripts.Clear();
            Scripts = null;
        }
    }
}
