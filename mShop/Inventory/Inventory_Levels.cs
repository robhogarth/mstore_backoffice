using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace mShop
{
    public partial class InventoryLevels
    {
        [JsonProperty("inventory_levels")]
        public List<InventoryLevel> Levels { get; set; }
    }

    public partial class InventoryLevel
    {
        [JsonProperty("inventory_item_id")]
        public long InventoryItemId { get; set; }

        [JsonProperty("location_id")]
        public long LocationId { get; set; }

        [JsonProperty("available")]
        public object Available { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }
    }
}
