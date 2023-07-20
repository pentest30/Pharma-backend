using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SoapClient
{
    /// <summary>
    ///  Used to serialize wsdl informations
    /// </summary>
    public class WSDL
    {
        /// <summary>
        /// Constant string from soap namespace
        /// </summary>
        public string SoapNS = "http://schemas.xmlsoap.org/soap/envelope/";
        /// <summary>
        /// Target NameSpace parsed from wsdl
        /// </summary>
        public string TargetNS { get; set; }
        /// <summary>
        /// Address of soap binding
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Available operation 
        /// </summary>
        public List<WsdlOperation> Operations { get; set; }

        /// <summary>
        /// Serialize wsdl
        /// </summary>
        public void Serialize()
        {
            try
            {

                System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
                settings.Indent = true;
                settings.CloseOutput = true;
                System.Xml.XmlWriter xmlwriter = System.Xml.XmlWriter.Create("wsdl.xml", settings);
                XmlSerializer serializer = new XmlSerializer(typeof(WSDL));
                serializer.Serialize(xmlwriter, this);
                xmlwriter.Flush();
                xmlwriter.Close();                
            }
            catch
            {
                throw new Exception("Serialization Exception");
            }
        }

        public static WSDL Deserialize()
        {
            if (!File.Exists("wsdl.xml"))
                throw new FileNotFoundException("No configuration file found");
            System.IO.StreamReader reader = new System.IO.StreamReader("wsdl.xml", true);
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(WSDL));
            WSDL current = (WSDL)serializer.Deserialize(reader);
            reader.Close();
            reader.Dispose();
            return current;
        }
    }
}
