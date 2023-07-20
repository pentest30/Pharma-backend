using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoapClient
{
    public class SoapBuilder
    {
        private StringBuilder builder = new StringBuilder();
        private List<string> lastElements = new List<string>();

        public void WriteStartElement(string name, string xmlns, string xmlnsuri)
        {
            
            this.builder.Append("<");
            if (!string.IsNullOrEmpty(xmlns))
                name = xmlns + ":" + name;

            this.builder.Append(name);
            this.lastElements.Add(name);

            if (!string.IsNullOrEmpty(xmlnsuri))
                this.builder.AppendFormat(" xmlns{0}=\"{1}\"", string.IsNullOrEmpty(xmlns) ? "" : ":" + xmlns, xmlnsuri);
            
            this.builder.Append(">");
        }

        public void WriteElement(string name, string value)
        {
            this.builder.AppendFormat("<{0}>{1}</{0}>", name, value);
        }

        public void WriteValue(string value)
        {
            builder.Append(value);
        }

        public void WriteEndElement()
        {
            string element = lastElements.Last();
            builder.AppendFormat("</{0}>", element);
            lastElements.RemoveAt(lastElements.Count - 1);
        }

        public override string ToString()
        {
            return this.builder.ToString();
        }
    }
}
