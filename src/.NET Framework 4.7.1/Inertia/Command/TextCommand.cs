namespace Inertia
{
    public abstract class TextCommand
    {
        /// <summary>
        /// Returns the name of the command.
        /// </summary>
        public abstract string Name { get; }

        public abstract void OnExecute(TextCommandArgs args);
    }
}
