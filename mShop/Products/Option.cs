using System.Collections.Generic;
using Newtonsoft.Json;

namespace mShop
{
    public partial class Option
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("product_id")]
        public long ProductId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("values")]
        public List<string> Values { get; set; }
    }
}
