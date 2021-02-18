using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace mShop
{
    public class inventory_cost_wrapper
    {
        [JsonProperty("inventory_item")]
        public inventory_cost_update inv { get; set; }

    }
    public class inventory_cost_update
    {
        public long id { get; set; }
        public string cost { get; set; }
    }
    public class variant_price_wrapper
    {
        [JsonProperty("variant")]
        public variant_cost_update variant { get; set; }

    }
    public class variant_cost_update
    {
        public long id { get; set; }
        public string price { get; set; }

        public string compare_at_price { get; set; }
    }
    public class metawrapper
    {
        [JsonProperty("metafield")]
        public Metafield metafield { get; set; }

    }
    public class Metafield
    {
        [JsonProperty("namespace")]
        public string nspace { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public string value_type { get; set; }

        public Metafield()
        {

        }

        public Metafield(string mnamespace, string mkey, string mvalue, string mvalue_type)
        {
            nspace = mnamespace;
            key = mkey;
            value = mvalue;
            value_type = mvalue_type;
        }
    }
    public class Prod_Availability
    {
        public string Id;
        public int Available;
        public string Status;
        public DateTime ETA;

        public override string ToString()
        {
            return Available.ToString() + " - " + ETA.ToString() + " - " + Status;
        }
    }
    public partial class GetMetafields
    {
        [JsonProperty("metafields")]
        public List<GetMetafield> Metafields { get; set; }
    }
    public partial class GetMetafield
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }
    }


}
