using System.Collections.Generic;
using System.Linq;

namespace SoapClient
{
    /// <summary>
    /// Used to materialize SoapHeader
    /// </summary>
    public class SoapHeader
    {
        /// <summary>
        /// Header custom name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Values
        /// </summary>
        
        public List<KeyValue> Values { get; set; }

        public string this[string Key]
        {
            get { return this.Values.FirstOrDefault(c => c.Key == Key).Value; }
        }

        public SoapHeader()
        {
            this.Values = new List<KeyValue>() ;
        }
        public void Add(string key, string value)
        {
            this.Values.Add(new KeyValue(key, value));
        }

        public void Clear()
        {
            this.Values.Clear();
            this.Name = string.Empty;
        }
        
    }

    public class KeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public KeyValue() { }
        public KeyValue(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
