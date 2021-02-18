using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace backoffice.ShopifyAPI
{
    public partial class InventoryItems
    {
        [JsonProperty("inventory_items")]
        public List<InventoryItem> Items { get; set; }
    }

    public partial class InventoryItem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("requires_shipping")]
        public bool RequiresShipping { get; set; }

        [JsonProperty("cost")]
        public string Cost { get; set; }

        [JsonProperty("country_code_of_origin")]
        public object CountryCodeOfOrigin { get; set; }

        [JsonProperty("province_code_of_origin")]
        public object ProvinceCodeOfOrigin { get; set; }

        [JsonProperty("harmonized_system_code")]
        public object HarmonizedSystemCode { get; set; }

        [JsonProperty("tracked")]
        public bool Tracked { get; set; }

        [JsonProperty("country_harmonized_system_codes")]
        public List<object> CountryHarmonizedSystemCodes { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }
    }

    public partial class InventoryItemWrapper
    {
        [JsonProperty("inventory_item")]
        public InventoryItem InventoryItem { get; set; }
    }

}

