using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GHPCommerce.Domain.Domain.Queries
{
    public class SyncDataGridQuery
    {
        [JsonPropertyName("requiresCounts")]
        public bool RequiresCounts { get; set; }
        [JsonPropertyName("skip")]
        public int Skip { get; set; }
        [JsonPropertyName("take")]
        public int Take { get; set; }
        [JsonPropertyName("where")]
        public List<Where> Where { get; set; }
        [JsonPropertyName("sorted")]
        public List<Sorted> Sorted  { get; set; }
    }
    public class Predicate
    {
        [JsonPropertyName("isComplex")]
        public bool IsComplex { get; set; }
        [JsonPropertyName("field")]
        public string Field { get; set; }
        [JsonPropertyName("operator")]
        public string Operator { get; set; }
        [JsonPropertyName("value")]
        public object Value { get; set; }
        [JsonPropertyName("ignoreCase")]
        public bool IgnoreCase { get; set; }
        [JsonPropertyName("ignoreAccent")]
        public bool IgnoreAccent { get; set; }
        public List<Predicate> predicates { get; set; }
    }

    public class Sorted
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("direction")]
        public string Direction { get; set; }
    }

    public class Where
    {
        [JsonPropertyName("isComplex")]
        public bool IsComplex { get; set; }
        [JsonPropertyName("ignoreAccent")]
        public bool IgnoreAccent { get; set; }
        [JsonPropertyName("condition")]
        public string Condition { get; set; }
        [JsonPropertyName("predicates")]
        public List<Predicate> Predicates { get; set; }
    }
}