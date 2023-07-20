using System.Collections.Generic;
using System.Xml.Linq;

namespace SoapClient
{
    /// <summary>
    /// Used to serialize operation
    /// </summary>
    public class WsdlOperation
    {
        /// <summary>
        /// Name of the operation
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Link of the soap action
        /// </summary>
        public string SoapAction { get; set; }
        /// <summary>
        /// List of parameters
        /// </summary>
        public List<WsdlParameter> Parameters { get; set; }

        /// <summary>
        /// Input message parsed from  wsdl
        /// </summary>
        public string InputMessage { get; set; }
        /// <summary>
        /// Output message parsed from wsdl
        /// </summary>
        public string OutputMessage { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="operation">Xelement parsed from wsdl</param>
        public WsdlOperation(XElement operation)
        {
            this.Name = operation.Attribute("name").Value;
            XElement current = operation.Element(XName.Get(string.Format("{{{0}}}input", operation.Name.NamespaceName)));
            if (current != null)
                this.InputMessage = current.Attribute("message").Value;
            current = operation.Element(XName.Get(string.Format("{{{0}}}output", operation.Name.NamespaceName)));
            if (current != null)
                this.OutputMessage = current.Attribute("message").Value;

            this.InputMessage = Utils.parseNs(this.InputMessage);
            this.OutputMessage = Utils.parseNs(this.OutputMessage);
        }

        public WsdlOperation() { }
    }
}

