namespace Inertia
{
<<<<<<< HEAD
    /// <summary>
    /// Represent the parent heritage of all textual commands
    /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
    public abstract class TextCommand
    {
        /// <summary>
        /// Returns the name of the command.
        /// </summary>
        public abstract string Name { get; }

<<<<<<< HEAD
        /// <summary>
        /// This method is executed when the command is called
        /// </summary>
        /// <param name="args"></param>
        public abstract void Execute(TextCommandArgs args);
=======
        public abstract void OnExecute(TextCommandArgs args);
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
    }
}
