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
            string sPbfFilePath = "";
            string sOutputTextFile = "";

            //check args
            if (args.Count() == 2)
            {
                sPbfFilePath = args[0];
                sOutputTextFile = args[1];
            }
            else
            {
                Console.WriteLine($"Wrong input parameters!");
                Console.WriteLine($"Input example --> OsmParser.exe pbf_filename output_textfile_name.txt");
                Console.ReadKey();
                return;
            }
                
            //check input file exist
            if (!File.Exists(sPbfFilePath))
            {
                Console.WriteLine($"File do not exist:  {sPbfFilePath}");
                Console.ReadKey();
                return;
            }

            //check input and output files extension
            var pbfFIleExtension = Path.GetExtension(sPbfFilePath).ToLower();
            if (pbfFIleExtension != ".pbf")
            {
                Console.WriteLine($"Wrong File Extension: {pbfFIleExtension}");
                Console.ReadKey();
                return;
            }
            var txtFileExtension = Path.GetExtension(sOutputTextFile).ToLower();
            if (txtFileExtension != ".txt")
            {
                Console.WriteLine($"Wrong File Extension: {txtFileExtension}");
                Console.ReadKey();
                return;
            }

            //Reading .pbf
            Dictionary<string,string> dcPostalCodes;
            try
            {
                Console.WriteLine($"Reading .pbf file...");
                dcPostalCodes = GetPostalCodes(sPbfFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while reaing file: {ex.InnerException}");
                Console.ReadKey();
                return;
            }

            //writing output txt file
            try
            {
                Console.WriteLine($"Creating postal codes file");
                var lines = GetPostalCodeLinesFromDictionary(dcPostalCodes);
                File.WriteAllLines(sOutputTextFile, lines);
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

        private static List<string> GetPostalCodeLinesFromDictionary(Dictionary<string, string> dc)
        {
            var list = new List<string>();
            foreach (var entry in dc.OrderBy(i=>i.Key))
                list.Add(entry.Key + " " +entry.Value);
            return list;
        }

        private static Dictionary<string,string> GetPostalCodes(string sOsmFilePath)
        {
            var dcAllPostalCodes  = new Dictionary<string, string>();

            using (var fileStream = new FileInfo(sOsmFilePath).OpenRead())
            {
                var source = new PBFOsmStreamSource(fileStream);

                foreach (var element in source)
                {
                    var lElementTags = element.Tags;

                    if (lElementTags.Any(i => i.Key == "addr:suburb"))
                    {
                        var sPostalCodeRaw = lElementTags.FirstOrDefault(i => i.Key == "addr:postcode").Value;
                        if (string.IsNullOrWhiteSpace(sPostalCodeRaw))
                            continue;
                        var sPostalCodeOnlyDigits = Regex.Replace(sPostalCodeRaw, "[^0-9.]", " ");//substitute non digit with whitespace
                        var sPostalCodeClean = Regex.Replace(sPostalCodeOnlyDigits, @"\s+", ";");//substitute whitespaces with separator
                        var lPostalCodes = sPostalCodeClean.Split(';');
                        foreach (var sPostalCode in lPostalCodes)
                        {
                            if (dcAllPostalCodes.ContainsKey(sPostalCode))
                                continue;

                            var sName = lElementTags.FirstOrDefault(i => i.Key == "addr:suburb").Value?.ToString();
                            dcAllPostalCodes.Add(sPostalCode, sName);
                        }

                    }
                }
            }

            return dcAllPostalCodes;
        }
    }
}
