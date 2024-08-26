using System;
using System.Collections.Generic;

namespace Inertia.Paper
{
    public class EnumeratorPaper : PaperObject
    {
        public IEnumerator<EnumeratorPaperElement> Enumerator { get; private set; }

        public EnumeratorPaper(IEnumerator<EnumeratorPaperElement> enumerator)
        {
            if (enumerator == null)
            {
                throw new ArgumentNullException(nameof(enumerator));
            }

            Enumerator = enumerator;
        }
    }
}
