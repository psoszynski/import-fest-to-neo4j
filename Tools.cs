using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace import_fest_to_neo4j
{
    public class Tools
    {
        public static void WriteCsv<T>(List<T> rows, string filename, string separator = ",", List<string> excludeColumns = null)
        {
            var properties = typeof(T).GetProperties()
                .Where(x => !x.PropertyType.ToString().StartsWith("System.Collections"))
                .Where(x => !x.PropertyType.ToString().StartsWith("AtheneContextLib"));

            if (excludeColumns != null)
            {
                properties = properties.Where(p => !excludeColumns.Select(c => c.ToLower()).Contains(p.Name.ToLower()));
            }
            var colNames = properties.Select(p => p.Name).ToArray();

            File.WriteAllText(filename, string.Empty);
            using (StreamWriter w = File.AppendText(filename))
            {
                foreach (var r in rows)
                {
                    var rowSb = new StringBuilder();
                    for (var i = 0; i < colNames.Length; i++)
                    {
                        var rowValue = typeof(T).GetProperty(colNames[i]).GetValue(r, null);
                        rowSb.Append($"{rowValue?.ToString()}");
                        if (i < colNames.Length - 1) rowSb.Append(separator);
                    }
                    w.WriteLine(rowSb.ToString());
                }
            }
        }

        public static void PrintArrayAsTable<T>(List<T> rows, List<string> excludeColumns = null, int minColumnWidth = 10, int maxColumnWidth = 30)
        {
            var properties = typeof(T).GetProperties()
                .Where(x => !x.PropertyType.ToString().StartsWith("System.Collections"))
                .Where(x => !x.PropertyType.ToString().StartsWith("AtheneContextLib"));

            if (excludeColumns != null)
            {
                properties = properties.Where(p => !excludeColumns.Select(c => c.ToLower()).Contains(p.Name.ToLower()));
            }

            var colNames = properties.Select(p => p.Name).ToArray();

            var maxColWidths = new List<int>();
            for (var i = 0; i < colNames.Length; i++)
            {
                var maxColWidth = Math.Max(
                    Math.Min(
                        Math.Max(
                            rows.Max(
                                r => typeof(T).GetProperty(colNames[i]).GetValue(r, null)?.ToString().Length ?? 0
                            ), colNames[i].Length
                        ), maxColumnWidth
                    ), minColumnWidth
                );
                maxColWidths.Add(maxColWidth);
            }
            var colWidths = maxColWidths.ToArray();

            var headerSb = new StringBuilder();
            headerSb.Append("| No   |");
            for (var i = 0; i < colNames.Length; i++)
            {
                headerSb.Append($" {colNames[i].TableTrim(colWidths[i])} |");
            }
            var header = headerSb.ToString();
            var separator = "".PadRight(header.Length, '-');
            Console.WriteLine(separator);
            Console.WriteLine(header);
            Console.WriteLine(separator);

            var index = 1;
            foreach (var r in rows)
            {
                var rowSb = new StringBuilder();
                rowSb.Append($"| {index++,4} |");
                for (var i = 0; i < colNames.Length; i++)
                {
                    var rowValue = typeof(T).GetProperty(colNames[i]).GetValue(r, null);
                    rowSb.Append($" {rowValue?.ToString().TableTrim(colWidths[i]) ?? "".TableTrim(colWidths[i])} |");
                }
                Console.WriteLine(rowSb.ToString());
            }

            Console.WriteLine(separator);
        }

        public static string GetStyrke(XNamespace ns2, XElement vms, bool isAlternative = false)
        {
            StringBuilder sb = new StringBuilder();
            var styrkeString = isAlternative ? "AlternativStyrke" : "Styrke";
            var styrkeV = vms.Element(ns2 + styrkeString)?.GetAttributeValueByAttributeLocalName("V");
            var styrkeU = vms.Element(ns2 + styrkeString)?.GetAttributeValueByAttributeLocalName("U");
            var styrkeNevnerV = vms.Element(ns2 + $"{styrkeString}Nevner")?.GetAttributeValueByAttributeLocalName("V");
            var styrkeNevnerU = vms.Element(ns2 + $"{styrkeString}Nevner")?.GetAttributeValueByAttributeLocalName("U");
            sb.Append($"{styrkeV} {styrkeU}");
            if (!string.IsNullOrEmpty(styrkeNevnerV))
            {
                sb.Append("/");
                if (styrkeNevnerV != "1")
                {
                    sb.Append($"{styrkeNevnerV} ");
                }
                sb.Append($"{styrkeNevnerU}");
            }
            return sb.ToString();
        }
    }
}
