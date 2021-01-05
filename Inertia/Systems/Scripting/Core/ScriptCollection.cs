using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Realtime
{
    /// <summary>
    /// Allows creation and management of scripts running in real time
    /// </summary>
    public class ScriptCollection : IDisposable
    {
        #region Public variables

        /// <summary>
        /// Return the number of <see cref="Script"/> running in the collection
        /// </summary>
        public int Count
        {
            get
            {
                return m_scripts.Count;
            }
        }

        #endregion

        #region Private variables

        private List<Script> m_scripts;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the class <see cref="ScriptCollection"/>
        /// </summary>
        public ScriptCollection()
        {
            if (!PluginManager.PluginsLoaded)
                PluginManager.ReloadPlugins();

            m_scripts = new List<Script>();
        }

        #endregion

        /// <summary>
        /// Create a new instance of <typeparamref name="T"/> with the specified arguments
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the target <see cref="Script"/></typeparam>
        /// <param name="args">The arguments to be passed on the initialization</param>
        /// <returns>Return the created instance of <typeparamref name="T"/></returns>
        public T Add<T>(params object[] args) where T : Script
        {
            var s = (Script)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(new object[] { });

            s.Collection = this;
            s.Awake(args);

            m_scripts.Add(s);

            return (T)s;
        }

        /// <summary>
        /// Remove the first occurence of type <typeparamref name="T"/> in the collection
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of <see cref="Script"/> to remove</typeparam>
        public void Remove<T>() where T : Script
        {
            lock (m_scripts)
            {
                try
                {
                    var script = m_scripts.Find((s) => s.GetType() == typeof(T) && !s.IsDisposed);
                    if (script != null)
                        script.Dispose();
                }
                catch (Exception ex) { this.GetLogger().Log("Failed to remove Script({0}): {1}", typeof(T).FullName, ex); }
            }
        }
        /// <summary>
        /// Remove all <see cref="Script"/> of type <typeparamref name="T"/> running in the collection
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of <see cref="Script"/> to remove</typeparam>
        public void RemoveAll<T>() where T : Script
        {
            lock (m_scripts)
            {
                try
                {
                    var scripts = m_scripts.FindAll((s) => s.GetType() == typeof(T) && !s.IsDisposed);
                    foreach (var script in scripts)
                    {
                        if (script != null)
                            script.Dispose();
                    }
                }
                catch (Exception ex) { this.GetLogger().Log("Failed to remove Scripts({0}): {1}", typeof(T).FullName, ex); }
            }
        }

        /// <summary>
        /// Get the first occurence of the specified <see cref="Script"/> of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The target <see cref="Script"/> to get</typeparam>
        /// <returns>The first occurence of <typeparamref name="T"/> running in the collection</returns>
        public T GetScript<T>() where T : Script
        {
            lock (m_scripts)
            {
                foreach (var script in m_scripts)
                    if (script.GetType() == typeof(T))
                        return (T)script;
            }

            return null;
        }
        /// <summary>
        /// Get all scripts of type <typeparamref name="T"/> in the collection
        /// </summary>
        /// <typeparam name="T">The target <see cref="Script"/> to get</typeparam>
        /// <returns>All <typeparamref name="T"/> script running in the collection</returns>
        public T[] GetScripts<T>() where T : Script
        {
            var result = new List<T>();
            lock (m_scripts)
            {
                foreach (var script in m_scripts)
                    if (script.GetType() == typeof(T))
                        result.Add((T)script);
            }

            return result.ToArray();
        }

        internal void FinalRemove(Script script)
        {
            m_scripts.Remove(script);
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            Script[] scripts;
            lock (m_scripts)
                scripts = m_scripts.ToArray();

            foreach (var script in m_scripts)
                script.Dispose();

            m_scripts.Clear();
            m_scripts = null;
        }
    }
}
