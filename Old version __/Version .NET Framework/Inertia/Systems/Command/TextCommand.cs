using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// Represent the parent heritage of all textual commands
    /// </summary>
    public abstract class TextCommand
    {
        #region Public variables

        /// <summary>
        /// Return the command's name
        /// </summary>
        public abstract string Name { get; }

        #endregion

        /// <summary>
        /// The method executed when this command is called
        /// </summary>
        /// <param name="args">The attached command's arguments</param>
        public abstract void Execute(TextCommandArgs args);
    }
}
