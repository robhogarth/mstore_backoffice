using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace backoffice.ShopifyAPI
{
    public class Shop_API
    {
        const char splitter = ',';
        private string url_prefix = @"https://monpearte-it-solutions.myshopify.com/admin/api/";

        private HttpClient client;
        
        private string uri_basic = @"https://monpearte-it-solutions.myshopify.com/admin/api/2021-01/products.json?fields=id,handle,title,published_scope,variants,vendor,inventory,tags";
        private string uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2021-01/products.json?fields=id,handle,title,published_scope,variants,vendor,inventory,tags&limit=250&published_status=published";
        private string uri_images = @"https://monpearte-it-solutions.myshopify.com/admin/api/2021-01/products.json?fields=id,handle,title,published_scope,variants,vendor,inventory,tags,images&limit=250&published_status=published";

        private string username = "0972a8a70db60724c7a5af71be2fba66";
        private string password = "982bf0cf17152977d1cfbeb71a0d60e6";

        public readonly int ratelimit;
        public readonly string API_Version;


        private async Task<bool> check_ratelimit(HttpResponseHeaders headers)
        {

            bool retval = false;
            try
            {
                IEnumerable<string> ratelimit_header = headers.GetValues("X-Shopify-Shop-Api-Call-Limit");
                string ratelimit_text = ratelimit_header.FirstOrDefault();

                int remainingrequests = Convert.ToInt32(ratelimit_text.Substring(0, ratelimit_text.IndexOf("/")));

                if (remainingrequests > 35)
                {
                    retval = true;
                    await Task.Delay(ratelimit);
                }
            }
            catch (Exception ex)
            {
                await Task.Delay(ratelimit);
            }
            return retval;
        }

        public Shop_API(int ratelimit_interval = 1000, string _API_Version = "2021-01")
        {
            ratelimit = ratelimit_interval;
            API_Version = _API_Version;
            url_prefix += _API_Version;

            client = new HttpClient();
            //client.Timeout = TimeSpan.FromMinutes(30);

            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<bool> UpdateTags(object id, string tags)
        {
            string uturi = url_prefix + "/products/" + id.ToString() + ".json";

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

            return common.IsStatusCodeSuccess(await Put_Product_Data(uturi, sw.ToString()));
        }

        public async Task<Locations> GetLocations()
        {
            string gl_uri = url_prefix + "/locations.json";
            Locations retval;

            string response = await Get_Data(gl_uri);
            retval = JsonConvert.DeserializeObject<Locations>(response);

            return retval;
        }

        public async Task<InventoryLevels> GetInventoryLevels(string InventoryItemID)
        {
            string gl_uri = url_prefix + "/inventory_levels.json?inventory_item_ids=" + InventoryItemID;

            InventoryLevels retval;

            string content = await Get_Data(gl_uri);

            retval = JsonConvert.DeserializeObject<InventoryLevels>(content);

            return retval;
        }

        public async Task<InventoryItems> GetInventoryItems(string InventoryItem)
        {
            string gl_uri = url_prefix + "/inventory_items.json?ids=" + InventoryItem;

            InventoryItems retval;

            string content = await Get_Data(gl_uri);

            retval = JsonConvert.DeserializeObject<InventoryItems>(content);

            return retval;
        }

        public async Task<InventoryItem> GetInventoryItem(string InventoryItem)
        {
            string gl_uri = url_prefix + "/inventory_items/" + InventoryItem + ".json";

            InventoryItemWrapper retval;

            string content = await Get_Data(gl_uri);

            retval = JsonConvert.DeserializeObject<InventoryItemWrapper>(content);

            return retval.InventoryItem;
        }

        internal Task<bool> UpdateProductTitle(Shopify_Product shopify_Product)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ConnectInventoryItemLocation(object inv_id, long location_id)
        {
            string uturi = url_prefix + "/inventory_levels/connect.json";

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

            return common.IsStatusCodeSuccess(await Post_Product_Data(uturi, sw.ToString()));
        }

        internal Task<bool> UpdateInventory(InventoryItem invItem)
        {
            throw new NotImplementedException();
        }

        internal Task<bool> UpdateVariant(Variant variant)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Remove_InventoryItemLocation(object inv_id, long location_id)
        {
            string uturi = url_prefix + "/inventory_levels.json?inventory_item_id=" + inv_id.ToString() + "&location_id=" + location_id.ToString();

            HttpResponseMessage response = await client.DeleteAsync(uturi);

            return response.IsSuccessStatusCode;
        }

        public async Task<Shopify_Product> GetProduct(string Handle)
        {
            string gl_uri = url_prefix + "/products/" + Handle + ".json";

            Shopify_Product_Wrapper retval;

            HttpResponseMessage response = await client.GetAsync(gl_uri);

            string content = await response.Content.ReadAsStringAsync();

            retval = JsonConvert.DeserializeObject<Shopify_Product_Wrapper>(content);

            return retval.Product;
        }

        public async Task<GetMetafields> Get_Product_Metafield_Data(string productID, string querystrings = "")
        {
            string uri;

            if (querystrings != "")
                uri = url_prefix + "/products/" + productID + "/metafields.json?" + querystrings;
            else
                uri = url_prefix + "/products/" + productID + "/metafields.json";

            string content = await Get_Data(uri);
                        
            GetMetafields retval = JsonConvert.DeserializeObject<GetMetafields>(content);

            return retval;
        }

        public async Task<string> Get_Data(string uri)
        {
            bool retry = true;
            string content = "";
            HttpResponseMessage response = new HttpResponseMessage();

            while (retry)
            {
                response = await client.GetAsync(uri);
                retry = await TooManyRequests(response.Headers, response.ReasonPhrase);              
            }

            content = await response.Content.ReadAsStringAsync();

            return content;
        }

        private async Task<bool> TooManyRequests(HttpResponseHeaders headers, string ReasonPhrase)
        {
            bool retval = false;

            if (ReasonPhrase == "Too Many Requests")
            {
                await check_ratelimit(headers);
                retval = true;
            }

            return retval;
        }

        public async Task<bool> UpdateProductETAMetafields(string handle, string availability, string ETA, string status)
        {
            List<Metafield> mwrapper = new List<Metafield>();
            var tasks = new List<Task<string>>();
            bool retval = true;

            mwrapper.Add(new Metafield("mstore", "availability", availability, "string"));

            string uri = "";

            uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/products/" + handle + "/metafields.json";

            if (ETA == null)
                mwrapper.Add(new Metafield("mstore", "eta", "UNKNOWN", "string"));
            else
                mwrapper.Add(new Metafield("mstore", "eta", ETA, "string"));

            mwrapper.Add(new Metafield("mstore", "status", status, "string"));

            foreach (Metafield meta in mwrapper)
            {
                metawrapper mwrap = new metawrapper();
                mwrap.metafield = meta;
                string content = JsonConvert.SerializeObject(mwrap, Formatting.Indented);

                try
                {
                    retval = retval & common.IsStatusCodeSuccess(await Post_Product_Data(uri, content));
                }
                catch (Exception e)
                {
                    throw;
                }
                
            }

            return retval;
        }

        public async Task<HttpStatusCode> Put_Product_Data(string uri, string content)
        {
            bool postretry = true;
            HttpResponseMessage response;
            HttpStatusCode retval = HttpStatusCode.Unused;

            while (postretry)
            {
                try
                {
                    HttpContent hcontent = new StringContent(content, Encoding.UTF8, "application/json");
                    response = await Put_Data(uri, hcontent);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error in put_product_data " + ex.Message);
                }

                retval = response.StatusCode;
                if (response.StatusCode == HttpStatusCode.OK)
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
                        throw new Exception("Unable to put_product_data. " + response.StatusCode.ToString() + " - " + await response.Content.ReadAsStringAsync());
                    }
                }
            }

            return retval;
        }

        public async Task<HttpStatusCode> Post_Product_Data(string uri, string content)
        {
            bool postretry = true;
            HttpResponseMessage response;
            HttpStatusCode retval = HttpStatusCode.Unused;
            
            while (postretry)
            {
                try
                {
                    HttpContent hcontent = new StringContent(content, Encoding.UTF8, "application/json");
                    response = await Post_Data(uri, hcontent);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error in post_product_data " + ex.Message);
                }

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


        private async Task<HttpResponseMessage> Put_Data(string uri, HttpContent hcontent)
        {
            HttpResponseMessage response;
            HttpClient mclient = new HttpClient();

            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);

            mclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            try
            {
                response = await mclient.PutAsync(uri, hcontent);
            }
            catch (Exception ex)
            {
                throw new Exception("Error Posting Data", ex);
            }

            return response;

        }

        private async Task<HttpResponseMessage> Post_Data(string uri, HttpContent hcontent)
        {
            HttpResponseMessage response;
            HttpClient mclient = new HttpClient();

            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);

            mclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            try
            {
                response = await mclient.PostAsync(uri, hcontent);
            }
            catch (Exception ex)
            {
                throw new Exception("Error Posting Data", ex);
            }

            return response;

        }


        public async Task<HttpResponseMessage> post_product_data_response(string uri, HttpContent hcontent)
        {
            bool postretry = true;
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unused);


            while (postretry)
            {
                response = await client.PostAsync(uri, hcontent);

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

            return response;
        }

        public async Task<HttpStatusCode> put_product_data(string uri, HttpContent hcontent)
        {
            HttpClient mclient = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
            mclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            bool postretry = true;
            HttpResponseMessage response;

            HttpStatusCode retval = HttpStatusCode.Unused;

            while (postretry)
            {
                response = await mclient.PutAsync(uri, hcontent);

                retval = response.StatusCode;
                if (common.IsStatusCodeSuccess(response.StatusCode))
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

        private Task<string> getTags(object id)
        {
            //TODO: getTags no implemented.  Not sure what this is actually supposed to do
            throw new NotImplementedException();
        }

    }
}
