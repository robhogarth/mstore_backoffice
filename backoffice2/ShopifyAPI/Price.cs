using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace backoffice.ShopifyAPI
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
