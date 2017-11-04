using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseSpeller
{
    class CorrectPointComparer:IComparer<CorrectPoint>
    {
       public int Compare(CorrectPoint cp1, CorrectPoint cp2)
       {
            if (cp1.CharIndex != cp2.CharIndex)
            {
                return cp1.CharIndex - cp2.CharIndex;
            }
            else if (cp1.Length != cp2.Length)
            {
                return cp2.Length - cp1.Length;
            }
            else
            {
                return 0;
            }
        }
    }
}
