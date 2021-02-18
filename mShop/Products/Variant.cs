using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace mShop
{
    public partial class Variant
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("product_id")]
        public long ProductId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("inventory_policy")]
        public string InventoryPolicy { get; set; }

        [JsonProperty("compare_at_price")]
        public object CompareAtPrice { get; set; }

        [JsonProperty("fulfillment_service")]
        public string FulfillmentService { get; set; }

        [JsonProperty("inventory_management")]
        public string InventoryManagement { get; set; }

        [JsonProperty("option1")]
        public string Option1 { get; set; }

        [JsonProperty("option2")]
        public object Option2 { get; set; }

        [JsonProperty("option3")]
        public object Option3 { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("taxable")]
        public bool Taxable { get; set; }

        [JsonProperty("barcode")]
        public string Barcode { get; set; }

        [JsonProperty("grams")]
        public long Grams { get; set; }

        [JsonProperty("image_id")]
        public long? ImageId { get; set; }

        [JsonProperty("weight")]
        public double Weight { get; set; }

        [JsonProperty("weight_unit")]
        public string WeightUnit { get; set; }

        [JsonProperty("inventory_item_id")]
        public long InventoryItemId { get; set; }

        [JsonProperty("inventory_quantity")]
        public long InventoryQuantity { get; set; }

        [JsonProperty("old_inventory_quantity")]
        public long OldInventoryQuantity { get; set; }

        [JsonProperty("requires_shipping")]
        public bool RequiresShipping { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonProperty("presentment_prices")]
        public List<PresentmentPrice> PresentmentPrices { get; set; }
    }
}
