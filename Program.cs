using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OsmParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string sOsmFilePath = "";

            if (Debugger.IsAttached)
                sOsmFilePath = @"C:\Temp\Berlin\berlin-latest.osm.pbf";
            else if (args.Count() > 0)
                sOsmFilePath = args[0];
            else
                return;

            var lPostalCodes =  GetPostalCodes(sOsmFilePath);

        }

        private static List<int> GetPostalCodes(string sOsmFilePath)
        {
            List<int> lAllPostalCodes  = new List<int>();
            using (var fileStream = new FileInfo(sOsmFilePath).OpenRead())
            {
                var source = new PBFOsmStreamSource(fileStream);



                foreach (var element in source)
                {
                    var lElementTags = element.Tags;
                    if (lElementTags.Any(i=>i.Key == "postal_code"))
                    {
                        var sPostalCodeRaw = lElementTags.FirstOrDefault(i => i.Key == "postal_code").Value;
                        
                        var sPostalCodeOnlyDigits = Regex.Replace(sPostalCodeRaw, "[^0-9.]", " ");
                        var sPostalCodeClean = Regex.Replace(sPostalCodeOnlyDigits, @"\s+", ";");

                        var lPostalCodes = sPostalCodeClean.Split(';');

                        foreach (var sPostalCode in lPostalCodes)
                        {
                            var iPostalCode = Convert.ToInt32(sPostalCode);
                            if (!lAllPostalCodes.Contains(iPostalCode))
                                lAllPostalCodes.Add(iPostalCode);
                        }

                    }
                }
            }

            return lAllPostalCodes.Distinct().OrderBy(i=>i).ToList();
        }
    }
}
