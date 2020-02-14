using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Commands
{
    public class ArgumentsCollection
    {
        #region Public variables

        public int Count
        {
            get
            {
                return Collection.Length;
            }
        }
        public int Position { get; set; }

        #endregion

        #region Private variables

        private string[] Collection;

        #endregion

        #region Constructors

        internal ArgumentsCollection(string[] _args)
        {
            var collection = new List<string>();

            var sentence = string.Empty;
            var createSentence = false;

            for (var i = 0; i < _args.Length; i++)
            {
                if (_args[i].Contains('"'))
                {
                    sentence += _args[i];
                    if (createSentence)
                    {
                        collection.Add(sentence.Replace('"'.ToString(), string.Empty));
                        sentence = string.Empty;
                        createSentence = false;
                    }
                    else
                        createSentence = true;
                }
                else
                {
                    if (createSentence)
                        sentence += _args[i];
                    else
                        collection.Add(_args[i]);
                }

                if (createSentence && i < _args.Length - 1)
                    sentence += " ";

                if (i == _args.Length - 1 && createSentence)
                    collection.Add(sentence);
            }

            Collection = collection.ToArray();
        }

        #endregion

        public string this[int index]
        {
            get
            {
                return Collection[index];
            }
        }

        public bool NextArgument(out string argument)
        {
            if (Position < 0 || Position >= Count) {
                argument = string.Empty;
                return false;
            }

            argument = this[Position++];
            return true;
        }
        public string[] GetCollection()
        {
            return Collection;
        }

        public void Dispose()
        {
            Collection = null;
        }
    }
}
