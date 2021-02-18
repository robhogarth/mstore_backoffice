using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace backoffice.ShopifyAPI
{
    public partial class Shopify_Products
    {
        [JsonProperty("products")]
        public List<Shopify_Product> Products { get; set; }

        public Shopify_Products()
        {
            Products = new List<Shopify_Product>();
        }
    }

    public class Shopify_Product_Wrapper
    {
        [JsonProperty("product")]
        public Shopify_Product Product { get; set; }
    }

    public partial class Shopify_Product
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }

        [JsonProperty("vendor")]
        public string Vendor { get; set; }

        [JsonProperty("product_type")]
        public string ProductType { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("published_at")]
        public string PublishedAt { get; set; }

        [JsonProperty("template_suffix")]
        public object TemplateSuffix { get; set; }

        [JsonProperty("published_scope")]
        public string PublishedScope { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonProperty("variants")]
        public List<Variant> Variants { get; set; }

        [JsonProperty("options")]
        public List<Option> Options { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

    }
}
