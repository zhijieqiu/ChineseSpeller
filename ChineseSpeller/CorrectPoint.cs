using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseSpeller
{
    class CorrectPoint
    {
        public int WordIndex { get; set; }
        public int CharIndex { get; set; }
        public int Length { get; set; }

        public double Score { get; set; }

        public double OriginScore { get; set; }

        public string Word { get; set; }

        public CorrectPoint(int ci, int wi, int l)
        {
            CharIndex = ci;
            WordIndex = wi;
            Length = l;
        }

        public CorrectPoint()
        {
            
        }
    }
}
