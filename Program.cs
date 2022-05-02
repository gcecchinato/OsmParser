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
        

        //C:\Projects\DesktopApps\OsmParser\OsmParser\bin\Debug\net5.0
        //OsmParser.exe C:\Temp\Berlin\berlin-latest.osm.pbf C:\Temp\Berlin\postal_codes.txt

        static void Main(string[] args)
        {
            string sOsmFilePath = "";
            string sPostalCodesFilePath = "";

            if (Debugger.IsAttached)
            {
                sOsmFilePath = @"C:\Temp\Berlin\berlin-latest.osm.pbf";
                sPostalCodesFilePath = @"C:\Temp\Berlin\postal_codes.txt";
            }
            else if (args.Count() == 2)
            { 
                sOsmFilePath = args[0];
                sPostalCodesFilePath = args[1];
            }
            else
                return;

            if (!File.Exists(sOsmFilePath))
            {
                Console.WriteLine($"File do not exist:  {sOsmFilePath}");
                Console.ReadKey();
            }
            var extension = Path.GetExtension(sOsmFilePath).ToLower();
            if (extension != ".pbf")
            {
                Console.WriteLine($"Wrong File Extension: {extension}");
                Console.ReadKey();
            }

            //Reading .pbf
            List<string> lPostalCodes;
            try
            {
                Console.WriteLine($"Reading .pbf file...");
                lPostalCodes = GetPostalCodes(sOsmFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while reaing file: {ex.InnerException}");
                Console.ReadKey();
                return;
            }

            //writing txt result file
            try
            {
                Console.WriteLine($"Creating postal codes file");
                File.WriteAllLines(sPostalCodesFilePath, lPostalCodes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while writing file: {ex.InnerException}");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Execution Completed.");
            Console.ReadKey();
        }

        private static List<string> GetPostalCodes(string sOsmFilePath)
        {
            var dcAllPostalCodes  = new Dictionary<string, string>();

            using (var fileStream = new FileInfo(sOsmFilePath).OpenRead())
            {
                var source = new PBFOsmStreamSource(fileStream);

                foreach (var element in source)
                {
                    var lElementTags = element.Tags;
                    if (lElementTags.Any(i => i.Key == "postal_code"))
                    {
                        var sPostalCodeRaw = lElementTags.FirstOrDefault(i => i.Key == "postal_code").Value;
                        var sPostalCodeOnlyDigits = Regex.Replace(sPostalCodeRaw, "[^0-9.]", " ");//substitute non digit with whitespace
                        var sPostalCodeClean = Regex.Replace(sPostalCodeOnlyDigits, @"\s+", ";");//substitute whitespaces with separator
                        var lPostalCodes = sPostalCodeClean.Split(';');
                        foreach (var sPostalCode in lPostalCodes)
                        {
                            if (dcAllPostalCodes.ContainsKey(sPostalCode))
                                continue;
                            dcAllPostalCodes.Add(sPostalCode, sPostalCode);
                        }

                    }
                }
            }


            return dcAllPostalCodes.Select(i=>i.Key).OrderBy(i=>i).ToList();
        }
    }
}
