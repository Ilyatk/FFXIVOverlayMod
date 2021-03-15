using ff14bot;
using ff14bot.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamLikeConfig;

namespace FFXIVOverlay
{
    class Program
    {

        
        static void Main(string[] args)
        {

            string pattern = @"(%.+?%)";
            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(pattern);
            string sentence = "Target: %ObjType% %NpcId% %Pos% %Distance2d% %1231%13%";

            Console.WriteLine("Orig: {0}", sentence);
            string replaceResult = rgx.Replace(sentence, new System.Text.RegularExpressions.MatchEvaluator((System.Text.RegularExpressions.Match m) =>
            {
                string token = m.Value.ToLower();
                switch(token)
                {
                    case "%objtype%":
                        return "4";

                    case "%npcid%":
                        return "2009507";
                    case "%distance2d%":
                    return (2037.15689f).ToString("F2", CultureInfo.InvariantCulture);
                }

                return m.Value;
            }));

            Console.WriteLine("Res:  {0}", replaceResult);

            //foreach (Match match in rgx.Matches(sentence))
            //    Console.WriteLine("Found '{0}' at position {1}",
            //                      match.Value, match.Index);

            //ConfigParser parser = new ConfigParser();
            //if (!parser.parse(@"E:\Games\Rebornbuddy64 1.0.259.0\Settings\Cheisy Lane\OverlayMod.yaml"))
            //{
            //    Console.WriteLine("Parse error");
            //}

            //var docs = parser.Result;

            //foreach (var doc in docs)
            //{
            //    Console.WriteLine(YamLikeConfig.Helper.Dump(doc));
            //}
            string name = "Cheisy Lane";
            string configPath = JsonSettings.GetSettingsFilePath(/*Core.Me.Name*/name, "OverlayMod.yaml");
            FFXIVOverlay.Plugin p = new Plugin();
            p.loadConfig(@"E:\Games\Rebornbuddy64 1.0.259.0\Settings\Cheisy Lane\OverlayMod.yaml");
        }
    }
}
