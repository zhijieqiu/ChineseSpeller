using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChineseSpeller.Interface;
using ChineseSpeller.tool;

namespace ChineseSpeller
{
    public class Trainer
    {
        private ISegmenter _segmenter;
        public string OutputDataFolder { get; set; }
        public string InputDataFolder { get; set; }

        
        public Trainer(ISegmenter segmenter, string inputFolder, string outputFolder)
        {
            _segmenter = segmenter;
            this.OutputDataFolder = outputFolder;
            this.InputDataFolder = inputFolder;
        }

        public void Execution()
        {
            var unigram = new Dictionary<string, int>();
            var bigram = new Dictionary<string, Dictionary<string,int>>();
            var delimeters = ",.!;，。；！".ToCharArray();
            DirectoryInfo inputDirectoryInfo = new DirectoryInfo(InputDataFolder);
            long handledFileNumber = 0, handledLineNumber = 0;
            foreach (var file in inputDirectoryInfo.EnumerateFiles())
            {
                handledFileNumber++;
                using (var inputFileReader = new StreamReader(file.FullName))
                {
                    string line = null;
                    
                    while ((line = inputFileReader.ReadLine()) != null)
                    {
                        var tokens = line.Split('\t');
                        if (tokens.Length < 9)
                        {
                            continue;
                        }

                        var sentences = tokens[8].Split(delimeters,StringSplitOptions.RemoveEmptyEntries);
                        foreach (var sentence in sentences)
                        {
                            var words = _segmenter.Cut(sentence);
                            UnigramUpdate(words,unigram);
                            BigramUpdate(words, bigram);    
                        }

                        handledLineNumber++;
                        if (handledLineNumber % 10000 == 0)
                        {
                            Console.WriteLine($"[{file.Name}]: We have handled {handledLineNumber} {(handledLineNumber == 1 ? "Line" : "Lines")}");
                        }
                    }
                }

                Console.WriteLine($"We have handled {handledFileNumber} {(handledFileNumber==1?"File":"Files")}");

            }

            var unigramProb = new Dictionary<string,double>();
            var bigramProb = new Dictionary<string, Dictionary<string, double>>();
            UnigramProbability(unigram, unigramProb);
            BigramProbability(bigram, bigramProb);
            SaveModel(unigramProb, bigramProb);
        }

        private void UnigramUpdate(IList<string> words, Dictionary<string, int> unigram)
        {
            foreach (var word in words)
            {
                if (unigram.ContainsKey(word))
                {
                    unigram[word]++;
                }
                else
                {
                    unigram[word] = 1;
                }
            }
        }

        private void BigramUpdate(IList<string> words, Dictionary<string, Dictionary<string, int>> bigram)
        {
            for (var i = 0; i < words.Count - 1; i++)
            {
                if (!bigram.ContainsKey(words[i]))
                {
                    bigram[words[i]] = new Dictionary<string, int>();
                }

                if (bigram[words[i]].ContainsKey(words[i + 1]))
                {
                    bigram[words[i]][words[i + 1]]++;
                }
                else
                {
                    bigram[words[i]][words[i + 1]] = 1;
                }
            }
        }

        private void UnigramProbability(Dictionary<string, int> unigram, Dictionary<string, double> unigramProb)
        {
            var greaterThan5Unigrams = unigram.ToList().Where(x => x.Value > 5).ToList();
            var totalCount = greaterThan5Unigrams.Sum(x => x.Value);
            var unigramNumber = greaterThan5Unigrams.Count;
            foreach (var kv in greaterThan5Unigrams)
            {
                unigramProb[kv.Key] = (kv.Value + 1.0) / (totalCount + unigramNumber);
            }
        }

        private void BigramProbability(Dictionary<string, Dictionary<string, int>> bigram,
            Dictionary<string, Dictionary<string, double>> bigramPorb)
        {
            foreach (var kv in bigram)
            {
                var prefixWord = kv.Key;
                var suffixMap = kv.Value;
                var totalSuffixCount = suffixMap.Sum(x => x.Value);
                if (!bigramPorb.ContainsKey(prefixWord))
                {
                    bigramPorb[prefixWord] = new Dictionary<string, double>();
                }

                foreach (var suffixWord in suffixMap)
                {
                    bigramPorb[prefixWord][suffixWord.Key] = (suffixWord.Value + 1.0) / (totalSuffixCount + suffixMap.Count);
                }
            }   
        }

        private void SaveModel(Dictionary<string, double> unigramProb, Dictionary<string, Dictionary<string,double>> bigramProb)
        {
            var unigramFilePath = Path.Combine(OutputDataFolder, Constants.UnigramFileName);
            var bigramFilePath = Path.Combine(OutputDataFolder, Constants.BigramFileName);
            using (var unigramWriter = new StreamWriter(unigramFilePath))
            {
                foreach (var ug in unigramProb.OrderByDescending(x => x.Value))
                {
                    unigramWriter.WriteLine(ug.Key + "\t" + ug.Value);
                }
            }

            using (var bigramWriter = new StreamWriter(bigramFilePath))
            {
                foreach (var bg in bigramProb)
                {
                    var prefix = bg.Key;
                    var suffixMap = bg.Value;
                    bigramWriter.Write(prefix);
                    foreach (var suffix in suffixMap.OrderByDescending(x => x.Value))
                    {
                        bigramWriter.Write("\t" + suffix.Key + "\t" + suffix.Value);
                    }
                    bigramWriter.WriteLine();
                }
            }
        }
    }
}
