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
using System.Net.Http.Headers;

namespace backoffice
{
    public class Variant
    {
        public object id { get; set; }
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
        public string published_scope { get; set; }
        public IList<Variant> variants { get; set; }
    }


    public partial class InventoryItem
    {
        public List<InventoryItemElement> InventoryItems { get; set; }
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
        public List<object> CountryHarmonizedSystemCodes { get; set; }
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
    }

    public class inventory_cost_wrapper
    {
        [JsonProperty("inventory_item")]
        public inventory_cost_update inv { get; set; }

    }
    public class inventory_cost_update
    {      
        public string id { get; set; }
        public string cost { get; set; }
    }
    public class variant_price_wrapper
    {
        [JsonProperty("variant")]
        public variant_cost_update variant { get; set; }

    }
    public class variant_cost_update
    {
        public string id { get; set; }
        public string price { get; set; }

        public string compare_at_price { get; set; }
    }

    

    public class Shopify_Products
    {

        const char splitter = ',';
        public IList<Shopify_Product> products { get; set; }

        private string username = "0972a8a70db60724c7a5af71be2fba66";
        private string password = "982bf0cf17152977d1cfbeb71a0d60e6";
        private HttpClient client;

        public int ratelimit = 1000;

        public async Task<Shopify_Products> getallproducts()
        {

            bool repeat = true;

            string uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/products.json?fields=id,handle,title,published_scope,variants&limit=250";
            if (products == null) { products = new List<Shopify_Product>(); }

            while (repeat)
            {
                HttpResponseMessage response = await client.GetAsync(uri);

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
                            uri = link;
                            uri = uri.Substring(uri.IndexOf("<") + 1);
                            uri = uri.Substring(0, uri.IndexOf(">"));
                            repeat = true;
                        }
                    }
                }
            }

            return this;

        }

        public async Task<string> update_availability(string handle, string availability, string eta)
        {

            string retval = "";

            Shopify_Product a_prod = null;
            foreach (Shopify_Product prod in products)
            {
                if ((prod.handle == handle.ToLower()) || (prod.handle == handle))
                {
                    a_prod = prod;
                    break;
                }
            }

            if (a_prod != null)
            {
                metawrapper mwrapper = new metawrapper();
                mwrapper.metafield = new Metafield();
                List<Metafield> metalist = new List<Metafield>();

                mwrapper.metafield.nspace = "mstore";
                mwrapper.metafield.key = "availability";
                mwrapper.metafield.value = availability;
                mwrapper.metafield.value_type = "string";

                string uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/products/" + a_prod.id + "/metafields.json";

                metalist.Add(mwrapper.metafield);

                string content = JsonConvert.SerializeObject(mwrapper, Formatting.Indented);
                var hcontent = new StringContent(content, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(uri, hcontent);

                //Console.WriteLine(await response.Content.ReadAsStringAsync());

                retval = response.StatusCode.ToString();


                retval = response.StatusCode.ToString();
                IEnumerable<string> ratelimit_header = response.Headers.GetValues("X-Shopify-Shop-Api-Call-Limit");
                string ratelimit_text = ratelimit_header.FirstOrDefault();

                int remainingrequests = Convert.ToInt32(ratelimit_text.Substring(0,ratelimit_text.IndexOf("/")));

                if (remainingrequests > 35)
                {
                    await Task.Delay(ratelimit);
                }




                metalist.Clear();
                metalist.Add(mwrapper.metafield);

                mwrapper.metafield.key = "eta";
                mwrapper.metafield.value = eta;
                               
                content = JsonConvert.SerializeObject(mwrapper, Formatting.Indented);
                hcontent = new StringContent(content, Encoding.UTF8, "application/json");
                response = await client.PostAsync(uri, hcontent);

                //Console.WriteLine(await response.Content.ReadAsStringAsync());

                retval += "," + response.StatusCode.ToString();
                //await Task.Delay(ratelimit);
            }

            return retval;

        }

        public async void updateprice(string variant_id, string cost, string price, string rRPInc)
        {

            try
            {
                //update cost price

                string inv_uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/inventory_items/" + variant_id + ".json";

                inventory_cost_update inv_update = new inventory_cost_update();
                inv_update.id = variant_id;
                inv_update.cost = cost;

                inventory_cost_wrapper inv_wrapper = new inventory_cost_wrapper();
                inv_wrapper.inv = inv_update;

                string inv_content = JsonConvert.SerializeObject(inv_wrapper, Formatting.Indented);
                var inv_hcontent = new StringContent(inv_content, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(inv_uri, inv_hcontent);
                check_ratelimit(response.Headers);

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating cost price", ex);
            }

            try
            {
                //update variant pricing

                string var_uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/variant/" + variant_id + ".json";

                variant_cost_update var_update = new variant_cost_update();
                var_update.id = variant_id;
                var_update.compare_at_price = rRPInc;
                var_update.price = price;

                variant_price_wrapper var_wrapper = new variant_price_wrapper();
                var_wrapper.variant = var_update;

                string var_content = JsonConvert.SerializeObject(var_wrapper, Formatting.Indented);
                var var_hcontent = new StringContent(var_content, Encoding.UTF8, "application/json");
                HttpResponseMessage var_response = await client.PostAsync(var_uri, var_hcontent);
                check_ratelimit(var_response.Headers);

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating variant pricing", ex);
            }


        }

        public async Task<InventoryItemElement> Get_InventoryItem(string prod_id)
        {
            InventoryItemElement retval = null;

            string uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/inventory_items.json?ids=" + prod_id;

            HttpResponseMessage response = await client.GetAsync(uri);

            InventoryItem retvals = JsonConvert.DeserializeObject<InventoryItem>(await response.Content.ReadAsStringAsync());

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

        public string createnewprice(string current_cost, string current_rrp, string current_price, string new_cost, string new_rrp)
        {
            /* get current margin = (price/cost) - 1
             * 
             * apply margin to new cost price to determine new sell price
             * test to ensure this is not more than the RRP
            */

            double current_margin = (Convert.ToDouble(current_price) / Convert.ToDouble(current_cost));

            double sell_price = Convert.ToDouble(new_cost) * current_margin;

            sell_price = Math.Round(sell_price, 2);

            if(sell_price < Convert.ToDouble(new_cost))
            { throw new Exception("Sell price can't be less than cost price"); }

            if (sell_price > Convert.ToDouble(new_rrp))
            { throw new Exception("Sell price is more than rrp price"); }

            return sell_price.ToString();
        }

        private async void check_ratelimit(HttpResponseHeaders headers)
        {
            IEnumerable<string> ratelimit_header = headers.GetValues("X-Shopify-Shop-Api-Call-Limit");
            string ratelimit_text = ratelimit_header.FirstOrDefault();

            int remainingrequests = Convert.ToInt32(ratelimit_text.Substring(0, ratelimit_text.IndexOf("/")));

            if (remainingrequests > 35)
            {
                await Task.Delay(ratelimit);
            }

        }

        public Shopify_Products(int ratelimit_interval = 1000)
        {
            ratelimit = ratelimit_interval;
                                    
            client = new HttpClient();
            
            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
    }
}
