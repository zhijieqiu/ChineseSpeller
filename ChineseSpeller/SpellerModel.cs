using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChineseSpeller.Interface;
using ChineseSpeller.tool;

namespace ChineseSpeller
{
    public class SpellerModel
    {
        private readonly Dictionary<string, double> _unigramProb = new Dictionary<string, double>();

        private readonly Dictionary<string, Dictionary<string, double>> _bigramProb =
            new Dictionary<string, Dictionary<string, double>>();

        private readonly ISegmenter _segmenter;

        public double ThresholdGap { get; set; } = 0.05;

        public SpellerModel(string modelPath, ISegmenter segmenter)
        {
            this._segmenter = segmenter;
            var unigramFilePath = Path.Combine(modelPath, Constants.UnigramFileName);
            var bigramFilePath = Path.Combine(modelPath, Constants.BigramFileName);
            LoadModel(unigramFilePath, bigramFilePath);
        }

        public string DoCorrect(string sentence)
        {
            var finalSentence = sentence;
            var words = _segmenter.Cut(sentence);
            var correctPoints = GetCandidateCorrectPoint(words);
            var end = 0;
            foreach (var cp in ReOrderCorrectPoint(correctPoints))
            {
                if (cp.WordIndex <= end)
                {
                    continue;
                }

                var isCorrected = CheckCorrectPoint(words, cp);
                if (!isCorrected)
                {
                    continue;
                }

                finalSentence = finalSentence.Substring(0, cp.CharIndex) + cp.Word +
                                finalSentence.Substring(cp.CharIndex + cp.Length);
                end = cp.WordIndex + cp.Length - 1;
            }
            return finalSentence;
        }

        private IList<CorrectPoint> GetCandidateCorrectPoint(IList<string> words)
        {
            var correctPoints = new List<CorrectPoint>();
            var startIndex = 0;
            for (var i = 0; i < words.Count; i++)
            {
                if (i + 1 < words.Count)
                {
                    if (words[i].Length == 1 && words[i + 1].Length == 1)
                    {
                        correctPoints.Add(new CorrectPoint(startIndex, i, 2));
                    }
                }

                if (i + 2 < words.Count)
                {
                    if (words[i].Length == 1 && words[i + 1].Length == 1 && words[i + 2].Length == 1)
                    {
                        correctPoints.Add(new CorrectPoint(startIndex, i, 3));
                    }
                }

                startIndex += words[i].Length;
            }

            return correctPoints;
        }

        private void LoadModel(string unigramFilePath, string bigramFilePath)
        {
            using (var unigramReader = new StreamReader(unigramFilePath))
            {
                string line = null;
                while ((line = unigramReader.ReadLine()) != null)
                {
                    var tokens = line.Split('\t');
                    _unigramProb[tokens[0]] = double.Parse(tokens[1]);
                }
            }

            using (var bigramReader = new StreamReader(bigramFilePath))
            {
                string line = null;
                while ((line = bigramReader.ReadLine()) != null)
                {
                    var tokens = line.Split('\t');
                    _bigramProb[tokens[0]] = new Dictionary<string, double>();
                    var index = 1;
                    while (index < tokens.Length)
                    {
                        _bigramProb[tokens[0]][tokens[index]] = double.Parse(tokens[index + 1]);
                        index += 2;
                    }
                }
            }
        }

        private IList<CorrectPoint> ReOrderCorrectPoint(IList<CorrectPoint> correctPoints)
        {
            return correctPoints.OrderBy(x => x, new CorrectPointComparer()).ToList();
        }

        private bool CheckCorrectPoint(IList<string> originWords, CorrectPoint correctPoint)
        {
            var wordsList = new List<string>();
            var candidatesWords = new List<string>();
            if (correctPoint.WordIndex != 0)
            {
                wordsList.Add(originWords[correctPoint.WordIndex - 1]);
                candidatesWords.Add(originWords[correctPoint.WordIndex - 1]);
            }

            wordsList.AddRange(originWords.Skip(correctPoint.WordIndex).Take(correctPoint.Length));
            candidatesWords.Add("[PlaceHolder]");
            if (correctPoint.WordIndex + correctPoint.Length < originWords.Count)
            {
                wordsList.Add(originWords[correctPoint.WordIndex + correctPoint.Length]);
                candidatesWords.Add(originWords[correctPoint.WordIndex + correctPoint.Length]);
            }

            var originScore = CalculateScore(wordsList);
            var pinyinSeqCandidates =
                PinyinTool.ChineseWord2PinyinSeqCandidates(
                    string.Join("", originWords.Skip(correctPoint.WordIndex).Take(correctPoint.Length)));

            double minCandidateScore = double.MaxValue;
            string candidateWord = null;
            foreach (var pinyinSeq in pinyinSeqCandidates)
            {
                var chineseWordCandidates = PinyinTool.PinyinSequence2ChineseWordsCandidates(pinyinSeq.Split(' '));
                foreach (var candidate in chineseWordCandidates)
                {
                    var placeHodlerIndex = correctPoint.WordIndex == 0 ? 0 : 1;
                    candidatesWords[placeHodlerIndex] = candidate;
                    var currentScore = CalculateScore(candidatesWords);
                    if (currentScore < minCandidateScore)
                    {
                        candidateWord = candidate;
                        minCandidateScore = currentScore;
                    }
                }
            }

            if (candidateWord == null)
            {
                return false;
            }

            correctPoint.Score = minCandidateScore;
            correctPoint.Word = candidateWord;
            return originScore - correctPoint.Score > ThresholdGap;
        }
        
        private double CalculateScore(IList<string> wordsList)
        {
            double originScore = 0.0;
            for (int i = 0; i < wordsList.Count - 1; i++)
            {
                if (_bigramProb.ContainsKey(wordsList[i]) && _bigramProb[wordsList[i]].ContainsKey(wordsList[i + 1]))
                {
                    originScore -= Math.Log(_bigramProb[wordsList[i]][wordsList[i + 1]]);
                }
                else
                {
                    originScore += 20;
                }
            }

            originScore /= (wordsList.Count - 1);
            return originScore;
        }
    }
}
