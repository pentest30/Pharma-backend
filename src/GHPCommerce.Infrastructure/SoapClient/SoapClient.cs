using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace SoapClient
{
    /// <summary>
    /// Soap client
    /// </summary>
    public class SoapClient
    {
        /// <summary>
        /// Wsdl url
        /// </summary>
        public string WSDLUrl { get; set; }
        /// <summary>
        /// True if save wsdl definition
        /// </summary>
        public bool UseLocal { get; set; }

        private XDocument xDocument;
        private readonly CustomWebClient client = new CustomWebClient();

        private WSDL wsdl;

        public SoapHeader Header { get; set; }
               

        public WSDL Wsdl
        {
            get { return wsdl; }
            set { wsdl = value; }
        }

        public SoapClient()
        {
            //client.Timeout = 2800;
        }

        /// <summary>
        /// Load Soapclient with wsdl url
        /// </summary>
        /// <param name="wsdlUrl"></param>
        public SoapClient(string wsdlUrl)
        {
            WSDLUrl = wsdlUrl;            
        }

        /// <summary>
        /// Load and parse WSDL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="saveLocally"></param>
        public void LoadWSDL(string url, bool saveLocally)
        {   
            WSDLUrl = url;            
            UseLocal = saveLocally;

            //try to load local definition 
            if (File.Exists("wsdl.xml"))
                wsdl = WSDL.Deserialize();
            else
               ParseWSDL();               

            if (saveLocally)
                wsdl.Serialize();                
        }

        private void ParseWSDL()
        {

            if (xDocument == null)
                xDocument = XDocument.Load(WSDLUrl);

            // declaring wsdl definition
            wsdl = new WSDL();
            wsdl.TargetNS = xDocument.Root.Attribute("targetNamespace").Value;

            // get the soap portType   
            // if ports types count > 1 get soap one. Else get first one
            XElement soapContainer = null, binding = null;
            string portType = string.Empty;
            if (xDocument.Root.Elements(XName.Get(string.Format("{{{0}}}portType", xDocument.Root.Name.NamespaceName))).Count() > 1)
            {
                soapContainer = xDocument.Root.Elements(XName.Get(string.Format("{{{0}}}portType", xDocument.Root.Name.NamespaceName))).FirstOrDefault(c => c.Attribute("name").Value.ToLower().Contains("soap"));
                portType = soapContainer.Attribute("name").Value;
                binding = xDocument.Root.Elements(XName.Get(string.Format("{{{0}}}binding", xDocument.Root.Name.NamespaceName))).FirstOrDefault(c => c.Attribute("name").Value == portType);
            }
            else
            {
                soapContainer = xDocument.Root.Element(XName.Get(string.Format("{{{0}}}portType", xDocument.Root.Name.NamespaceName)));
                portType = soapContainer?.Attribute("name")?.Value;
                binding = xDocument.Root.Element(XName.Get(string.Format("{{{0}}}binding", xDocument.Root.Name.NamespaceName)));
            }
            

            if (soapContainer == null)
                throw new ArgumentNullException("soap port type cannot be found");

            // creating operations
            wsdl.Operations = soapContainer.Elements(XName.Get(string.Format("{{{0}}}operation",soapContainer.Name.NamespaceName))).Select(c => new WsdlOperation(c)).ToList();

            // getting parameters 
            XElement soapTypes = xDocument.Root.Element(XName.Get(string.Format("{{{0}}}types",xDocument.Root.Name.NamespaceName)));
            IEnumerable<XElement> soapMessages = xDocument.Root.Elements(XName.Get(string.Format("{{{0}}}message", xDocument.Root.Name.NamespaceName)));
            if (soapTypes == null)
                throw new ArgumentNullException("soap types cannot be found");

            // associating operations and types
            XElement parameter = null;
            
            foreach (WsdlOperation operation in wsdl.Operations)
            {
                
                // parameter is defined in type 
                parameter = soapTypes.Descendants().FirstOrDefault(c => c.Name.LocalName == "element" && c.Attribute("name").Value == operation.Name);
                if (parameter != null)
                {
                    parameter = parameter.Descendants().FirstOrDefault(c => c.Name.LocalName == "sequence");
                    if (parameter != null)
                        operation.Parameters = parameter.Elements().Select(c => new WsdlParameter(c)).ToList();
                }
                else // parameter is in messages
                {
                    parameter = soapMessages.SingleOrDefault(c => c.Attribute("name").Value == operation.InputMessage);
                    if (parameter == null)
                        continue;
                    operation.Parameters = parameter.Descendants(XName.Get(string.Format("{{{0}}}part", xDocument.Root.Name.NamespaceName))).Select(c => new WsdlParameter(c)).ToList();
                }

                parameter = binding.Descendants().FirstOrDefault(c => c.Name.LocalName == "operation" && c.Attribute("name") != null && c.Attribute("name").Value == operation.Name);
                if (parameter != null)
                    operation.SoapAction = ((XElement)parameter.FirstNode).Attribute("soapAction").Value;   //this.wsdl.TargetNS + operation.Name;
            }

            // getting address
            XElement xAddress = xDocument.Root.Descendants(XName.Get(string.Format("{{{0}}}port", xDocument.Root.Name.NamespaceName))).FirstOrDefault(c => c.Attribute("name").Value == portType);
            if(xAddress != null)
                wsdl.Address = ((XElement)xAddress.FirstNode).Attribute("location").Value;
        }

        /// <summary>
        /// Invoke wsdl operation and return results
        /// </summary>
        /// <param name="operationName">Operation Name</param>
        /// <param name="args">ordered parameters</param>
        /// <returns></returns>
        public XElement Invoke(string operationName, params object[] args)
        {            
            if (wsdl== null)
                ParseWSDL();
            
            // creating input message
            SoapBuilder message = new SoapBuilder();

            // enveloppe 
            message.WriteStartElement("Envelope", "soap", wsdl.SoapNS);

            // header 
            if (Header != null && !string.IsNullOrEmpty(Header.Name))
            {
                message.WriteStartElement("Header", "soap",null);
                message.WriteStartElement(Header.Name, null, wsdl.TargetNS);
                foreach (KeyValue kvp in Header.Values)
                {
                    message.WriteElement(kvp.Key, kvp.Value);
                    
                }
                message.WriteEndElement();//header
                message.WriteEndElement();//header name
            }

            message.WriteStartElement("Body", "soap", null);

            message.WriteStartElement(operationName, null, wsdl.TargetNS);
            
            WsdlOperation operation = wsdl.Operations.SingleOrDefault(c => c.Name == operationName);
            for (int i = 0; i < args.Length; i++)
            {
                message.WriteElement(operation.Parameters[i].Name, Convert.ToString(args[i]));
            }
            message.WriteEndElement();//operation
            message.WriteEndElement();//body

            message.WriteEndElement();//envelope
                        
            
            // send command               
            // setting client header            
            client.Headers = new WebHeaderCollection();
            client.Headers.Add("Content-Type", "text/xml; charset=utf-8");

            // get soapAction            
            client.Headers.Add("SOAPAction", operation.SoapAction);
            
            string result = client.UploadString(wsdl.Address,"POST", message.ToString());

            //parsing result

            xDocument = XDocument.Parse(result);
            XElement xResponse = xDocument.Descendants().Where(c => c.Name.LocalName.ToLower() == "body").FirstOrDefault();

            return xResponse;
        }


    }
}

