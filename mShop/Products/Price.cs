using Newtonsoft.Json;

namespace mShop
{
    public partial class PresentmentPrice
    {
        [JsonProperty("price")]
        public Price Price { get; set; }

        [JsonProperty("compare_at_price")]
        public object CompareAtPrice { get; set; }
    }

    public partial class Price
    {
        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }
    }
}
