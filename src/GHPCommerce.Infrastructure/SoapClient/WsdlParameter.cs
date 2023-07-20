using System.Xml.Linq;

namespace SoapClient
{
    /// <summary>
    /// Used to serialize parameters
    /// </summary>
    public class WsdlParameter
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type of the parameter
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameter">Xelement parsed from wsdl</param>
        public WsdlParameter(XElement parameter)
        {
            this.Name = parameter.Attribute("name").Value;
            this.Type = parameter.Attribute("type").Value;

            this.Type = Utils.parseNs(this.Type);
        }

        public WsdlParameter() { }
    }
}
