using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChineseSpeller.tool;
using JiebaNet.Segmenter;
using System.Diagnostics;
using System.Security.Policy;

namespace ChineseSpeller
{
    class Program
    {
        static void TestCorrectPointComparer()
        {
            var c1 = new CorrectPoint()
            {
                CharIndex = 0,
                Length = 2
            };
            var c2 = new CorrectPoint()
            {
                CharIndex = 0,
                Length = 3
            };
            var c3 = new CorrectPoint()
            {
                CharIndex = 3,
                Length = 3
            };

            var correctPoints = new List<CorrectPoint>(){c1,c2,c3};
            correctPoints = correctPoints.OrderBy(x => x, new CorrectPointComparer()).ToList();
            foreach (var cp in correctPoints)
            {
                Console.WriteLine($"startIndex = {cp.CharIndex}, Length={cp.Length}");
            }

        }

        static void Main(string[] args)
        {
            /*
             * Following is the demo of ChineseNormalier
             */
            ChineseNormalier cn = new ChineseNormalier(@"D:\zhijie\ChineseSpeller\ChineseSpeller\packages\jieba.NET.0.38.3\Resources\stopwords.txt",
                @"D:\cmcc_task\CMCC\Data\chat\outputfolder");
            var normalizerResult = cn.Normalize("这个问提不好解答",false,true,true);
            Console.WriteLine(normalizerResult);
            
            JiebaSegmenter segmenter = new JiebaSegmenter();
            var tokens = segmenter.Cut("这个问提不好解答", false, false);
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }

            PinyinTool.InitChineseWordTable(@"D:\zhijie\ChineseSpeller\ChineseSpeller\data\ChineseWordDict\dict.txt");
            PinyinTool.Init(@"D:\zhijie\ChineseSpeller\ChineseSpeller\data\py\ChinesePinyinTable.txt");
            var pinyinList = PinyinTool.ChineseCharToPinyinList("里");
            var chineseCharList = PinyinTool.PinyinToChineseCharList("tian");
            var pinyinSeqCandidates = PinyinTool.ChineseWord2PinyinSeqCandidates("使用");
            var mylist = PinyinTool.PinyinSequence2ChineseWordsCandidates(new List<string>(){"xiang","yong"});

            //var trainer = new Trainer(new JBSegmenter(),
            //    @"D:\cmcc_task\CMCC\Data\chat\inputfolder",
            //    @"D:\cmcc_task\CMCC\Data\chat\outputfolder");

            //trainer.Execution();
            SpellerModel spellerModele = new SpellerModel(@"D:\cmcc_task\CMCC\Data\chat\outputfolder", new JBSegmenter());

            var testPairs = new Dictionary<string, string>
            {
                { string.Empty, string.Empty },
                { "我要够买流量包", "我要购买流量包" },
                { "如何订狗流亮包", "如何订购流量包" },
                { "本机有承诺连续12个月使用88元或以上4G主体套餐使用流亮年包的优惠未到其", "本机有承诺连续12个月使用88元或以上4G主体套餐使用流量年包的优惠未到期" },
               
            };

            int rightCnt = 0;
            foreach (var p in testPairs)
            {
                var ret = spellerModele.DoCorrect(p.Key);
                if (ret == p.Value)
                {
                    rightCnt++;
                }
                else
                {
                    Console.WriteLine($"result should be {p.Value} but is {ret}");
                }
            }
            
            Console.ReadLine();
        }
    }
}
