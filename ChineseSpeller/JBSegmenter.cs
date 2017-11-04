using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChineseSpeller.Interface;
using JiebaNet.Segmenter;

namespace ChineseSpeller
{
    public class JBSegmenter:ISegmenter
    {
        //TODO using jieba must promise JiebaSegmenter ResourcePath
        private JiebaSegmenter JiebaSegmenter { get; set; }

        public JBSegmenter()
        {
            JiebaSegmenter = new JiebaSegmenter();
        }

        public IList<string> Cut(string sentence)
        {
            return JiebaSegmenter.Cut(sentence, false, false).ToList();
        }
    }
}
