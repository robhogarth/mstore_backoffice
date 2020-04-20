using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net.Http.Headers;

namespace backoffice
{
    public class Variant
    {
        public long id { get; set; }
        public object product_id { get; set; }
        public string title { get; set; }
        public string price { get; set; }
        public string sku { get; set; }
        public int position { get; set; }
        public string inventory_policy { get; set; }
        public string compare_at_price { get; set; }
        public string fulfillment_service { get; set; }
        public object inventory_management { get; set; }
        public string option1 { get; set; }
        public object option2 { get; set; }
        public object option3 { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool taxable { get; set; }
        public string barcode { get; set; }
        public int grams { get; set; }
        public object image_id { get; set; }
        public double weight { get; set; }
        public string weight_unit { get; set; }
        public object inventory_item_id { get; set; }
        public int inventory_quantity { get; set; }
        public int old_inventory_quantity { get; set; }
        public bool requires_shipping { get; set; }
        public string admin_graphql_api_id { get; set; }
    }
    public class Shopify_Product
    {
        public object id { get; set; }
        public string handle { get; set; }
        public string title { get; set; }
        public string vendor { get; set; }
        public string published_scope { get; set; }
        public string tags { get; set; }
        public IList<Variant> variants { get; set; }
    }
    public partial class InventoryItem
    {
        [JsonProperty("inventory_items")]
        public InventoryItemElement[] InventoryItems { get; set; }
    }

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


    public partial class InventoryItemElement
    {
        public long Id { get; set; }
        public string Sku { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool RequiresShipping { get; set; }
        public string Cost { get; set; }
        public object CountryCodeOfOrigin { get; set; }
        public object ProvinceCodeOfOrigin { get; set; }
        public object HarmonizedSystemCode { get; set; }
        public bool Tracked { get; set; }
        public object[] CountryHarmonizedSystemCodes { get; set; }
        public string AdminGraphqlApiId { get; set; }
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

        public Metafield(string mnamespace, string mkey, string mvalue, string mvalue_type)
        {
            nspace = mnamespace;
            key = mkey;
            value = mvalue;
            value_type = mvalue_type;
        }

    }
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

    public class Shopify_Products
    {
        public string uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/products.json?fields=id,handle,title,published_scope,variants,vendor,inventory,tags&limit=250&published_status=published";
        const char splitter = ',';
        public IList<Shopify_Product> products { get; set; }

        private string username = "0972a8a70db60724c7a5af71be2fba66";
        private string password = "982bf0cf17152977d1cfbeb71a0d60e6";
        private HttpClient client;

        public int ratelimit = 2000;
        private string[] specialChars = { "#", "/", ".", "+", " ", "=", "*", "&", "@", "!", "^", "%", ":", ";" };

        private bool IsStatusCodeSuccess(HttpStatusCode code)
        {
            bool retval = false;

            if (((int)code > 199) & ((int)code < 300))
                retval = true;

            return retval;
        }

        private string replace_specialchars(string handle)
        {
            try
            {
                handle = handle.ToLower();

                foreach (string schar in specialChars)
                {
                    if (handle.EndsWith(schar))
                    {
                        handle = handle.Substring(0, handle.Length - 1);
                    }

                    handle = handle.Replace(schar, "-");
                }

                handle = handle.Replace(" ", "-");
            }
            catch (Exception ex)
            {
                throw new Exception("Error replacing special chars in handle", ex);
            }

            return handle;
        }


        public Shopify_Product MatchProductByMMT(string handle, string sku)
        {
            Shopify_Product retval = null;

            if (handle == "MU-PB500B/WW")
            {
                int i = 0;
                i = 1;
            }

            sku = sku.ToLower();

            handle = replace_specialchars(handle);

            foreach (Shopify_Product prod in products)
            {
                if ((prod.handle.ToLower() == handle))
                {
                    retval = prod;
                    break;
                }

                if (sku != "")
                {
                    if ((prod.variants.FirstOrDefault().sku.ToLower() == sku))
                    {
                        retval = prod;
                        break;
                    }
                }
            }

            return retval;
        }

        public MMTPriceListProductsProduct MatchProductByShopify(string handle, string sku, MMTPriceListProducts mmtprods)
        {
            MMTPriceListProductsProduct retval = null;

            sku = sku.ToLower();

            handle = replace_specialchars(handle);

            foreach (MMTPriceListProductsProduct prod in mmtprods.Product)
            {
                if (prod.Manufacturer[0].ManufacturerCode.ToLower() == handle)                  
                {
                    retval = prod;
                    break;
                }

                if (sku != "")
                {
                    if ((prod.Manufacturer[0].ManufacturerCode.ToLower() == sku))
                    {
                        retval = prod;
                        break;
                    }
                }
            }

            return retval;
        }

        public async Task<bool> getallproducts(string[] extraquerystrings = null)
        {
            bool retval = false;
            bool repeat = true;
            string geturi = uri;
            
            if (extraquerystrings != null)
            {
                foreach (string queryString in extraquerystrings)
                {
                    geturi += "&" + queryString;
                }
            }
             
            if (products == null) 
            { 
                products = new List<Shopify_Product>(); 
            }
            else
            {
                products.Clear();
            }

            while (repeat)
            {
                retval = true;

                HttpResponseMessage response = await client.GetAsync(geturi);

                Shopify_Products result = JsonConvert.DeserializeObject<Shopify_Products>(await response.Content.ReadAsStringAsync());

                foreach (Shopify_Product prod in result.products)
                {
                    products.Add(prod);
                }
                
                repeat = false;

                if (response.Headers.Contains("Link"))
                {
                    string[] links = response.Headers.GetValues("Link").ToArray();
                    foreach (string link in links[0].Split(splitter))
                    {
                        if (link.Contains("next"))
                        {
                            geturi = link;
                            geturi = geturi.Substring(geturi.IndexOf("<") + 1);
                            geturi = geturi.Substring(0, geturi.IndexOf(">"));
                            repeat = true;
                        }
                    }
                }
            }

            return retval;
        }

        public async Task<bool> updatetags(object id, string tags)
        {

            string uturi = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/products/" + id.ToString() + ".json";
            
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("product");
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(id);
                writer.WritePropertyName("tags");
                writer.WriteValue(tags);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            HttpContent content = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");

            return IsStatusCodeSuccess(await put_product_data(uturi, content));
        }

        private async Task<HttpStatusCode> post_product_data(string uri, HttpContent hcontent)
        {
            bool postretry = true;
            HttpResponseMessage response;

            HttpStatusCode retval = HttpStatusCode.Unused;
                      

            while (postretry)
            {
                response = await client.PostAsync(uri, hcontent);

                retval = response.StatusCode;
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    postretry = false;

                    await check_ratelimit(response.Headers);
                }
                else
                {

                    if (response.ReasonPhrase == "Too Many Requests")
                    {
                        await Task.Delay(ratelimit);
                    }
                    else
                    {
                        postretry = false;
                        throw new Exception("Unable to post_product_data. " + response.StatusCode.ToString() + " - " + await response.Content.ReadAsStringAsync());
                    }
                }
             
            }

            return retval;
        }

        private async Task<HttpStatusCode> put_product_data(string uri, HttpContent hcontent)
        {
            bool postretry = true;
            HttpResponseMessage response;

            HttpStatusCode retval = HttpStatusCode.Unused;


            while (postretry)
            {
                response = await client.PutAsync(uri, hcontent);

                retval = response.StatusCode;
                if (IsStatusCodeSuccess(response.StatusCode))
                {
                    postretry = false;

                    await check_ratelimit(response.Headers);
                }
                else
                {

                    if (response.ReasonPhrase == "Too Many Requests")
                    {
                        await Task.Delay(ratelimit);
                    }
                    else
                    {
                        postretry = false;
                        throw new Exception("Unable to post_product_data. " + response.StatusCode.ToString() + " - " + await response.Content.ReadAsStringAsync());
                    }
                }

            }

            return retval;
        }

        public async Task<string> update_availability(string handle, string availability, string eta, bool skipprodcheck = false, string sku = "", string MMTStatus = "")
        {

            string retval = handle + ", ";

            Shopify_Product a_prod = null;

            if (!skipprodcheck)
            {
                a_prod = MatchProductByMMT(handle, sku);
            }

            if (handle == "25BINTSPRO4K")
            {
                retval += " BlackMagic ";

            }


            if ((a_prod != null) | (skipprodcheck))
            {
                List<Metafield> mwrapper = new List<Metafield>();

                mwrapper.Add(new Metafield("mstore", "availability", availability, "string"));

                string uri = "";

                if (skipprodcheck)
                {
                    uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/products/" + handle + "/metafields.json";
                }
                else
                {
                    uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/products/" + a_prod.id + "/metafields.json";
                }


                if (eta == null)
                    mwrapper.Add(new Metafield("mstore", "eta", "UNKNOWN", "string"));
                else
                    mwrapper.Add(new Metafield("mstore", "eta", eta, "string"));

                if ((availability == "0") | (availability == ""))
                {
                    mwrapper.Add(new Metafield("mm-google-shoppping", "custom_label_0", "out of stock", "string"));
                }
                else
                    mwrapper.Add(new Metafield("mm-google-shoppping", "custom_label_0", "in stock", "string"));

                if (MMTStatus == "Order to Order")
                {
                    mwrapper.Add(new Metafield("mm-google-shoppping", "custom_label_0", "preorder", "string"));
                }

                mwrapper.Add(new Metafield("mstore", "status", MMTStatus, "string"));

                foreach (Metafield meta in mwrapper)
                {
                    metawrapper mwrap = new metawrapper();
                    mwrap.metafield = meta;
                    string content = JsonConvert.SerializeObject(mwrap, Formatting.Indented);
                    var hcontent = new StringContent(content, Encoding.UTF8, "application/json");

                    try
                    {
                        retval += await post_product_data(uri, hcontent);
                    }
                    catch (Exception ex)
                    {
                        retval += ex.Message;
                    }
                }               
            }
            else
            {
                retval += ", No match found";
            }
            return retval;
        }

        public async Task<bool> unpublishitem(object id)
        {
            string uturi = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/products/" + id.ToString() + ".json";

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("product");
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(id);
                writer.WritePropertyName("published_at");
                writer.WriteNull();
                writer.WritePropertyName("published_scope");
                writer.WriteValue("");
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            HttpContent content = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");

            return IsStatusCodeSuccess(await put_product_data(uturi, content));
        }

        public async Task<bool> updatesku(object id, string sku)
        {
            string uturi = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/variants/" + id.ToString() + ".json";

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("variant");
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(id);
                writer.WritePropertyName("sku");
                writer.WriteValue(sku);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            HttpContent content = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");

            return IsStatusCodeSuccess(await put_product_data(uturi, content));
        }

        public async Task<bool> updateprice(long inv_id, long variant_id, string cost, string price, string rRPInc)
        {
            bool retval = false;
            bool retcostprice = false;
            bool retprice = false;

            HttpStatusCode retStatusCode;
            
            try
            {
                if (Convert.ToDouble(price) == 0)
                {
                    throw new Exception("Price is 0");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Price not a valid double.  Item: " + variant_id, ex);
            }
          
            try
            {
                if (Convert.ToDouble(cost) > Convert.ToDouble(price))
                {
                    throw new Exception("Cost more than Price");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cost or Price not a valid double", ex);
            }

            try
            {
                //update cost price

                string inv_uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/inventory_items/" + inv_id.ToString() + ".json";

                inventory_cost_update inv_update = new inventory_cost_update();
                inv_update.id = inv_id;
                inv_update.cost = cost;

                inventory_cost_wrapper inv_wrapper = new inventory_cost_wrapper();
                inv_wrapper.inv = inv_update;

                string inv_content = JsonConvert.SerializeObject(inv_wrapper, Formatting.Indented);
                var inv_hcontent = new StringContent(inv_content, Encoding.UTF8, "application/json");

                retStatusCode = await put_product_data(inv_uri, inv_hcontent);

                if (IsStatusCodeSuccess(retStatusCode))
                    retcostprice = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating cost price.  Item: " + inv_id, ex);
            }

            try
            {
                //update variant pricing

                string var_uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/variants/" + variant_id.ToString() + ".json";

                variant_cost_update var_update = new variant_cost_update();
                var_update.id = variant_id;
                var_update.compare_at_price = rRPInc;
                var_update.price = price;

                variant_price_wrapper var_wrapper = new variant_price_wrapper();
                var_wrapper.variant = var_update;

                string var_content = JsonConvert.SerializeObject(var_wrapper, Formatting.Indented);
                var var_hcontent = new StringContent(var_content, Encoding.UTF8, "application/json");

                retStatusCode = await put_product_data(var_uri, var_hcontent);

                if (IsStatusCodeSuccess(retStatusCode))
                    retprice = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating variant pricing.  Item: " + variant_id, ex);
            }

            retval = retprice & retcostprice;

            return retval;
        }

        public async Task<InventoryItemElement> Get_InventoryItem(string prod_id)
        {
            InventoryItemElement retval = null;

            string uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/inventory_items.json?ids=" + prod_id;

            HttpResponseMessage response = await client.GetAsync(uri);
            string resp_content = await response.Content.ReadAsStringAsync();

            InventoryItem retvals = JsonConvert.DeserializeObject<InventoryItem>(resp_content);

            retval = retvals.InventoryItems.FirstOrDefault();
                                    
            IEnumerable<string> ratelimit_header = response.Headers.GetValues("X-Shopify-Shop-Api-Call-Limit");
            string ratelimit_text = ratelimit_header.FirstOrDefault();

            int remainingrequests = Convert.ToInt32(ratelimit_text.Substring(0, ratelimit_text.IndexOf("/")));

            if (remainingrequests > 35)
            {
                await Task.Delay(ratelimit);
            }

            return retval;
        }

        public async Task<bool> Delete_InventoryItemLocation(object inv_id, long location_id)
        {
            string uturi = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-04/inventory_levels.json?inventory_item_id=" + inv_id.ToString() + "&location_id=" + location_id.ToString();

            HttpResponseMessage response = await client.DeleteAsync(uturi);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Set_InventoryItemLocation(object inv_id, long location_id)
        {
            string uturi = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-04/inventory_levels/connect.json";

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("location_id");
                writer.WriteValue(location_id);
                writer.WritePropertyName("inventory_item_id");
                writer.WriteValue(inv_id);
                writer.WriteEndObject();              
            }

            HttpContent content = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");

            return IsStatusCodeSuccess(await post_product_data(uturi, content));

        }



        public string addgst(string price)
        {
            double tax_price = Convert.ToDouble(price);
            return addgst(tax_price).ToString("0.00");
// return String.Format("{0:.##}",addgst(tax_price));
        }

        public double addgst(double price)
        {
            return Math.Round(price * 1.1,2);
        }

        public async Task<InventoryLevels> Get_InventoryLevelsList(object inv_id)
        {
            string uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-04/inventory_levels.json?inventory_item_ids=" + inv_id.ToString() + "&location_ids=44811321481,41088974985,39927775369";

            HttpResponseMessage response = await client.GetAsync(uri);
            string resp_content = await response.Content.ReadAsStringAsync();

            InventoryLevels retvals = JsonConvert.DeserializeObject<InventoryLevels>(resp_content);

            return retvals;

        }

        public async Task<InventoryItem> Get_InventoryItemList(string ids)
        { 
            string uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/inventory_items.json?limit=100&ids=" + ids;

            HttpResponseMessage response = await client.GetAsync(uri);
            string resp_content = await response.Content.ReadAsStringAsync();

            InventoryItem retvals = JsonConvert.DeserializeObject<InventoryItem>(resp_content);
            
            return retvals;
        }

        public string createnewprice(string current_cost, string current_rrp, string current_price, string new_cost, string new_rrp, bool new_cost_taxable = false, bool new_rrp_taxable = false, bool shopify_taxable = false)
        {
            /* get current margin = (price/cost) - 1
             * 
             * apply margin to new cost price to determine new sell price
             * test to ensure this is not more than the RRP
            */

            /* if rrpprice is empty, change it to at least a number for this process
             * to allow compares
            */ 
            if (new_rrp == "") { new_rrp = "0"; }
            
            // do stuff with GST amounts.  currently this is easy.  With more data sources logic will need to change                       
            if ((!new_cost_taxable) & (!shopify_taxable))
            {             
                new_cost = addgst(new_cost);
            }

            // no need to do anything with shopif_taxable right now but cant hurt to include it for things later on down the track
            // if (shopify_taxable)

            
            // margin code looks kinds of wrong, as it is not the same as shopify cals
            // but I suspect ratio of prices is all that is needed.  Seems to have been working.
            double current_margin = (Convert.ToDouble(current_price) / Convert.ToDouble(current_cost));

            double sell_price = Convert.ToDouble(new_cost) * current_margin;
                        
            if ((sell_price > 10) & (sell_price < 100))
            {
                sell_price = Math.Round(sell_price / 0.5) * 0.5;
            }

            if ((sell_price > 100))
            {
                sell_price = Math.Round(sell_price);
            }

            if (sell_price < Convert.ToDouble(new_cost))
            { throw new Exception("Sell price can't be less than cost price"); }

            if ((sell_price > Convert.ToDouble(new_rrp)) & (Convert.ToDouble(new_rrp) != 0))
            { 
                
                if (current_margin > 1.1 )
                {
                    sell_price = Convert.ToDouble(new_cost) * 1.1;
                }

                if (sell_price > Convert.ToDouble(new_rrp)) 
                {
                    throw new Exception("Sell price is more than rrp price");
                }
            }

            if ((sell_price > 10) & (sell_price < 100))
            {
                sell_price = Math.Round(sell_price / 0.5) * 0.5;
            }

            if ((sell_price > 100))
            {
                sell_price = Math.Round(sell_price);
            }

            return sell_price.ToString();
        }

        private async Task<bool> check_ratelimit(HttpResponseHeaders headers)
        {

            bool retval = false;
            IEnumerable<string> ratelimit_header = headers.GetValues("X-Shopify-Shop-Api-Call-Limit");
            string ratelimit_text = ratelimit_header.FirstOrDefault();

            int remainingrequests = Convert.ToInt32(ratelimit_text.Substring(0, ratelimit_text.IndexOf("/")));

            if (remainingrequests > 35)
            {
                retval = true;
                await Task.Delay(ratelimit);
            }

            return retval;
        }

        public Shopify_Products(int ratelimit_interval = 1000)
        {
            ratelimit = ratelimit_interval;
                                    
            client = new HttpClient();
            //client.Timeout = TimeSpan.FromMinutes(30);

            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
    }
}
