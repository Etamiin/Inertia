using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Commands
{
    public abstract class InertiaCommand
    {
        #region Public variables

        public abstract string Name { get; }

        #endregion

        public abstract void Execute(ArgumentsCollection collection);
        public virtual string GetHelp()
        {
            return $"No documentation for [{ Name }] command";
        }
    }
}
