using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace import_fest_to_neo4j
{
    public static class Extensions
    {
        public static string TableTrim(this string input, int width)
        {
            if (input.Length > width)
            {
                var substring = input.Substring(0, width - 3) + "...";
                return substring;
            }

            var paddedInput = input.PadRight(width, ' ');
            return paddedInput;
        }

        public static string GetAttributeValueByAttributeLocalName(this XElement elem, string attributename)
        {
            return elem.Attributes().Where(a => a.Name.LocalName == attributename).Select(a => a.Value).FirstOrDefault();
        }
    }
}
