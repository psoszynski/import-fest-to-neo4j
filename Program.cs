using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace import_fest_to_neo4j
{
    class Program
    {
        //dotnet run "C:\Users\psoszyns\Downloads\fest25\fest25.xml"
        //dotnet run "C:\Users\psoszyns\Downloads\fest25\fest25.xml" "C:\Users\psoszyns\Desktop\Neo4jImport"
        //dotnet run "C:\Users\psoszyns\Downloads\fest25\fest25.xml" LMM
        //dotnet run "C:\Users\psoszyns\Downloads\fest25\fest25.xml" LMV
        //dotnet run "C:\Users\psoszyns\Downloads\fest25\fest25.xml" VMS

        static void Main(string[] args)
        {
            bool justPrintFlag = false;

            if(args.Length < 1)
            {
                Console.WriteLine("There must be at least one parameter with a path to Fest file");
                return;
            }

            string festFile;
            string outputDir = @".\";

            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"{args[0]} must be a valid path to a file");
                return;
            }
            else
            {
                festFile = args[0];
            }

            if (args.Length == 2 && args[1].ToUpper() != "LMM" &&
                args[1].ToUpper() != "LMV" &&
                args[1].ToUpper() != "VMS")
            {
                if (!Directory.Exists(args[1]))
                {
                    try
                    {
                        Directory.CreateDirectory(args[1]);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Could not create output directory at {args[1]}");
                        return;
                    }
                }
                else
                {
                    outputDir = args[1];
                }
            }
            else
            {
                justPrintFlag = true;
            }

            if(args.Length == 1)
            {
                justPrintFlag = false;
            }

            if(!justPrintFlag)
            {
                Console.WriteLine($"Creating CSV files in {outputDir}");
            }
            XNamespace ns1 = @"http://www.kith.no/xmlstds/eresept/m30/2013-10-08";
            XNamespace ns2 = @"http://www.kith.no/xmlstds/eresept/forskrivning/2013-10-08";
            XDocument fest = new XDocument();

            try
            {
                fest = XDocument.Load(festFile);
            }
            catch (Exception)
            {
                Console.WriteLine("Could not parse the fest file!");
                return;
            }

            var virkestoff = (from v in fest.Descendants(ns2 + "Virkestoff")
                              let vId = v.Element(ns2 + "Id").Value
                              let vNavn = v.Element(ns2 + "Navn").Value
                              select new { vId, vNavn }).ToList();

            var q_lm = from lm in fest.Descendants(ns2 + "LegemiddelMerkevare")
                       from Svms in lm.Descendants(ns2 + "SortertVirkestoffMedStyrke").Select(n => new { Sortering = n.Element(ns2 + "Sortering").Value, RefVirkestoffMedStyrke = n.Element(ns2 + "RefVirkestoffMedStyrke").Value.Substring(3, 36).ToLower() })
                       let Id = lm.Element(ns2 + "Id").Value.Substring(3, 36).ToLower()
                       let NavnFormStyrke = lm.Element(ns2 + "NavnFormStyrke").Value
                       select new { Id, NavnFormStyrke, Svms.Sortering, Svms.RefVirkestoffMedStyrke };

            
            Console.WriteLine($"Det er {q_lm.Count()} legemiddelmerkevarer med SortertVirkestoffMedStyrke");
            if (justPrintFlag && args[1].ToUpper() == "LMM")
            {
                Tools.PrintArrayAsTable(q_lm.ToList(), null, 10, 50);
                return;
            }
            if(!justPrintFlag)
            {
                var lmmPath = Path.Combine(outputDir, "lmm.csv");
                Tools.WriteCsv(q_lm.ToList(), lmmPath, ";");
            }

            var q_vm = from vms in fest.Descendants(ns2 + "VirkestoffMedStyrke")
                       let Id = vms.Element(ns2 + "Id").Value.Substring(3, 36).ToLower()
                       let refVirkestoff = vms.Element(ns2 + "RefVirkestoff").Value
                       let Stoff = virkestoff.FirstOrDefault(v => v.vId == refVirkestoff).vNavn
                       let Styrke = Tools.GetStyrke(ns2, vms)
                       let AlternativStyrke = Tools.GetStyrke(ns2, vms, true)
                       let AtcKombipreparat = vms.Element(ns2 + "AtcKombipreparat")?.GetAttributeValueByAttributeLocalName("V")
                       orderby Stoff, Styrke
                       select new { Id, Stoff, Styrke, AlternativStyrke, AtcKombipreparat };

            Console.WriteLine($"Det er {q_vm.Count()} virkestoffmedstyrker");
            if (justPrintFlag && args[1].ToUpper() == "VMS")
            {
                Tools.PrintArrayAsTable(q_vm.ToList(), null, 10, 50);
                return;
            }
            if (!justPrintFlag)
            {
                var vmsPath = Path.Combine(outputDir, "vms.csv");
                Tools.WriteCsv(q_vm.ToList(), vmsPath, ";");
            }
            
            var q_lv = from lm in fest.Descendants(ns2 + "LegemiddelVirkestoff")
                       from Svms in lm.Descendants(ns2 + "SortertVirkestoffMedStyrke").Select(n => new { Sortering = n.Element(ns2 + "Sortering").Value, RefVirkestoffMedStyrke = n.Element(ns2 + "RefVirkestoffMedStyrke").Value.Substring(3, 36).ToLower() })
                       let Id = lm.Element(ns2 + "Id").Value.Substring(3, 36).ToLower()
                       let NavnFormStyrke = lm.Element(ns2 + "NavnFormStyrke").Value
                       orderby NavnFormStyrke
                       select new { Id, NavnFormStyrke, Svms.Sortering, Svms.RefVirkestoffMedStyrke };

            Console.WriteLine($"Det er {q_lv.Count()} legemiddelmerkevarer med SortertVirkestoffMedStyrke");
            if (justPrintFlag && args[1].ToUpper() == "LMV")
            {
                Tools.PrintArrayAsTable(q_lv.ToList(), null, 10, 50); return;
            }
            if (!justPrintFlag)
            {
                var lmvPath = Path.Combine(outputDir, "lmv.csv");
                Tools.WriteCsv(q_lv.ToList(), lmvPath, ";");
            }
        }
    }
}