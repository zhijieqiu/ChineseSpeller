using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChineseSpeller.Interface;

namespace ChineseSpeller
{
    public class ChineseNormalier: INormalizer
    {
        private readonly ISegmenter _segmenter;
        private readonly HashSet<string> _stopwords;
        private readonly SpellerModel _spellerModel;

        public override string Normalize(string text, bool lowerCase = false, bool removeStopWords = false, bool spellerCheck = false,
            bool useStem = false)
        {
            if (lowerCase)
            {
                text = text.ToLower();
            }

            text = spellerCheck ? _spellerModel.DoCorrect(text) : text;
            var wordsList = _segmenter.Cut(text);
            if (removeStopWords)
            {
                wordsList = wordsList.Where(x => !_stopwords.Contains(x)).ToList();
            }

            return string.Join("", wordsList);
        }

        public ChineseNormalier(string stopwordsFilePath, string spellerModelPath)
        {
            _segmenter = new JBSegmenter();
            _spellerModel = new SpellerModel(spellerModelPath, _segmenter);
            var stopWordsSet = new HashSet<string>();
            using (var streamReader = new StreamReader(stopwordsFilePath))
            {
                string line = null;
                while ((line = streamReader.ReadLine()) != null)
                {
                    stopWordsSet.Add(line.Trim());
                }
            }

            _stopwords = stopWordsSet;
        }
    }
}
