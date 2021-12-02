using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public abstract class BasicCommand
    {
        public abstract string Name { get; }

        public abstract void OnExecute(BasicCommandArguments arguments);

        internal void PreExecute(string[] arguments, object[] dataCollection, bool containsBlock)
        {
            if (containsBlock)
            {
                var newArguments = new List<string>();
                var sentence = new StringBuilder();

                for (var i = 0; i < arguments.Length; i++)
                {
                    var arg = arguments[i];

                    if (arg.StartsWith("\"") || sentence.Length > 0)
                    {
                        sentence.Append(arg);

                        if (arg.EndsWith("\""))
                        {
                            newArguments.Add(sentence.ToString().Replace("\"", string.Empty));
                            sentence = new StringBuilder();
                        }
                        else
                        {
                            sentence.Append(" ");
                        }
                    }
                    else
                    {
                        newArguments.Add(arg);
                    }
                }

                arguments = newArguments.ToArray();
            }

            OnExecute(new BasicCommandArguments(arguments, dataCollection));
        }
    }
}