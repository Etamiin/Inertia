namespace Inertia
{
    /// <summary>
    /// Represent the parent heritage of all textual commands
    /// </summary>
    public abstract class TextCommand
    {
        /// <summary>
        /// Returns the name of the command.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// This method is executed when the command is called
        /// </summary>
        /// <param name="args"></param>
        public abstract void Execute(TextCommandArgs args);
    }
}
