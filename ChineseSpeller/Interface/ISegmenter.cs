using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseSpeller.Interface
{
    public interface ISegmenter
    {
        IList<string> Cut(string sentence);
    }
}
