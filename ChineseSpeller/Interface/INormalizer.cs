using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseSpeller.Interface
{
    public abstract class INormalizer
    {
        public abstract string Normalize(string text, bool lowerCase = false, bool removeStopWords = false,
            bool spellerCheck = false, bool useStem = false);
    }
}
