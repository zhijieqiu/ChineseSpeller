using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiebaNet.Segmenter;

namespace ChineseSpeller.tool
{
    public static class PinyinTool
    {
        private static readonly Dictionary<string,List<string>> ChineseChar2PinyinList = new Dictionary<string, List<string>>();
        private static readonly Dictionary<string,List<string>> Pinyin2ChineseCharList = new Dictionary<string, List<string>>();
        private static readonly HashSet<string> ChineseWordSet = new HashSet<string>();
        
        static PinyinTool()
        {
        }

        public static IList<string> ChineseCharToPinyinList(string chineseChar)
        {
            return ChineseChar2PinyinList.ContainsKey(chineseChar)
                ? ChineseChar2PinyinList[chineseChar]
                : new List<string>();
        }

        public static IList<string> PinyinToChineseCharList(string pinyin)
        {
            return Pinyin2ChineseCharList.ContainsKey(pinyin)
                ? Pinyin2ChineseCharList[pinyin]
                : new List<string>();
        }

        public static IEnumerable<string> ChineseWord2PinyinSeqCandidates(string chinesePhrase)
        {
            List<string> pinyinSeqs = new List<string>();
            List<string> currentPinyinSeq = new List<string>();
            ChineseCharSeq2PinyinSeqList(chinesePhrase, 0, pinyinSeqs, currentPinyinSeq);
            return pinyinSeqs;
        }

        public static IEnumerable<string> PinyinSequence2ChineseWordsCandidates(IList<string> pinyinSeq)
        {
            List<string> wordCandidates = new List<string>();
            List<string> currentWord = new List<string>();
            PinyinSeq2ChineseWords(pinyinSeq, 0, wordCandidates, currentWord);
            return wordCandidates;
        }

        private static void PinyinSeq2ChineseWords(IList<string> pinyinSeq, int current, IList<string> wordCandidates,
            List<string> currentWord)
        {
            if (current == pinyinSeq.Count)
            {
                var word = string.Join("", currentWord);
                if (ChineseWordSet.Contains(word) && !wordCandidates.Contains(word))
                {
                    wordCandidates.Add(word);
                }

                return;
            }

            var chineseChars = PinyinToChineseCharList(pinyinSeq[current]);
            foreach (var c in chineseChars)
            {
                currentWord.Add(c);
                PinyinSeq2ChineseWords(pinyinSeq, current + 1, wordCandidates, currentWord);
                currentWord.RemoveAt(currentWord.Count - 1);
            }
        }

        private static void ChineseCharSeq2PinyinSeqList(string chineseCharSeq, int current,
            IList<string> pinyinSeqs, List<string> currentPinyinSeq)
        {
            if (current == chineseCharSeq.Length)
            {
                var pinyinSeqStr = string.Join(" ", currentPinyinSeq);
                if (!pinyinSeqs.Contains(pinyinSeqStr))
                {
                    pinyinSeqs.Add(pinyinSeqStr);
                }

                return;
            }

            var pinyinList = ChineseCharToPinyinList(chineseCharSeq[current].ToString());
            foreach (var c in pinyinList)
            {
                currentPinyinSeq.Add(c);
                ChineseCharSeq2PinyinSeqList(chineseCharSeq, current+1,pinyinSeqs, currentPinyinSeq);
                currentPinyinSeq.RemoveAt(currentPinyinSeq.Count-1);
            }
        }

        public static void Init(string chineseToPinyinPath)
        {
            using (var reader = new StreamReader(chineseToPinyinPath))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    var tokens = line.Split(' ');
                    if (!ChineseChar2PinyinList.ContainsKey(tokens[0]))
                    {
                        ChineseChar2PinyinList[tokens[0]] = new List<string>();
                    }

                    ChineseChar2PinyinList[tokens[0]].Add(tokens[1]);

                    if (!Pinyin2ChineseCharList.ContainsKey(tokens[1]))
                    {
                        Pinyin2ChineseCharList[tokens[1]] = new List<string>();
                    }

                    Pinyin2ChineseCharList[tokens[1]].Add(tokens[0]);
                }
            }
        }

        public static void InitChineseWordTable(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!ChineseWordSet.Contains(line))
                    {
                        ChineseWordSet.Add(line);
                    }
                }
            }
        }
    }
}
