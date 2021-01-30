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

        public async Task<bool> updatetags(object id, string tags)
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

            HttpContent content = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");

            return common.IsStatusCodeSuccess(await put_product_data(uturi, content));
        }

        public async Task<Locations> GetLocations()
        {

            string gl_uri = url_prefix + "/locations.json";

            Locations retval;

            HttpResponseMessage response = await client.GetAsync(gl_uri);

            retval = JsonConvert.DeserializeObject<Locations>(await response.Content.ReadAsStringAsync());

            return retval;
        }

        public async Task<InventoryLevels> GetInventoryLevels(string InventoryItemID)
        {
            string gl_uri = url_prefix + "/inventory_levels.json?inventory_item_ids=" + InventoryItemID;

            InventoryLevels retval;

            HttpResponseMessage response = await client.GetAsync(gl_uri);

            string content = await response.Content.ReadAsStringAsync();

            retval = JsonConvert.DeserializeObject<InventoryLevels>(content);

            return retval;
        }

        public async Task<InventoryItems> GetInventoryItems(string InventoryItem)
        {
            string gl_uri = url_prefix + "/inventory_items.json?ids=" + InventoryItem;

            InventoryItems retval;
            
            HttpResponseMessage response = await client.GetAsync(gl_uri);

            retval = JsonConvert.DeserializeObject<InventoryItems>(await response.Content.ReadAsStringAsync());

            return retval;
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

            return common.IsStatusCodeSuccess(await post_product_data(uturi, sw.ToString()));
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

            HttpResponseMessage response = await client.GetAsync(uri);

            string content = await response.Content.ReadAsStringAsync();

            GetMetafields retval = JsonConvert.DeserializeObject<GetMetafields>(content);


            return retval;
        }

        public async Task<string> UpdateProductETAMetafields(string handle, string availability, string ETA, string status)
        {
            List<Metafield> mwrapper = new List<Metafield>();
            var tasks = new List<Task<string>>();

            int failed = 0;

            string retval = "";

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


                /*
                tasks.Add(Task.Run(async () => {
                                                    HttpStatusCode result = await post_product_data(uri, hcontent);
                                                    if (result != HttpStatusCode.Created)
                                                    {
                                                        Interlocked.Increment(ref failed);
                                                    }
                                                    return result.ToString();
                                            }));

            */
                try
                {


                    retval += await post_product_data(uri, content);
                    //retval += "Debug Only";
                }
                catch (Exception ex)
                {
                    retval += ex.Message;
                }
                
            }
            /*
            var continuation = Task.WhenAll(tasks);
            while (continuation.Status == TaskStatus.Running)
            {
                continuation.Wait();
            }

            if (continuation.Status == TaskStatus.RanToCompletion)
                foreach (var result in continuation.Result)
                {
                    retval += result;
                }
            else
            {
                retval = failed.ToString() + " failures";
            }*/

            return retval;
        }

        public async Task<HttpStatusCode> post_product_data(string uri, string content)
        {
            bool postretry = true;
            HttpResponseMessage response;
            HttpStatusCode retval = HttpStatusCode.Unused;
            //client.Timeout = TimeSpan.FromMinutes(30);
            int count = 0;
            bool m = false;
            
            while (postretry)
            {
                count++;

                if (count > 1)
                {
                    m = true;
                }


                try
                {
                    HttpContent hcontent = new StringContent(content, Encoding.UTF8, "application/json");
                    response = await post_data(uri, hcontent);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error in post_product_data " + ex.Message + count.ToString());
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

        private async Task<HttpResponseMessage> post_data(string uri, HttpContent hcontent)
        {
            HttpResponseMessage response;
            HttpStatusCode retval = HttpStatusCode.Unused;
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

        public async Task<bool> UpdateMbot(object id, string tags = "", bool noupdate = false)
        {
            const string mbot_prefix = "Mbot_";
            string newtags = tags;
            bool replaced_mbot = false;
            string temp_str;
            List<string> newtagslist = new List<string>();

            if (newtags == "")
            {
                newtags = await getTags(id);
            }

            //generate new mbot string
            string mbot_newtag = mbot_prefix + DateTime.Now.ToString();
            mbot_newtag = mbot_newtag.Replace(' ', '_');

            //split to array
            string[] array_tags = newtags.Split(',');

            //replace existing mbot tag
            for (int i = array_tags.Length - 1; i >= 0; i--)
            {
                temp_str = array_tags[i];
                temp_str = temp_str.Trim();

                if (temp_str.StartsWith(mbot_prefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (!replaced_mbot)
                    {
                        newtagslist.Add(mbot_newtag);
                        replaced_mbot = true;
                    }
                }
                else
                {
                    newtagslist.Add(temp_str);
                }
            }

            //or add tag
            if (!replaced_mbot)
            {
                newtagslist.Add(mbot_newtag);
            }

            bool update_retval = false;

            if (!noupdate)
            {
                newtags = "";

                foreach (string listtag in newtagslist)
                {
                    newtags += listtag + ", ";
                }

                newtags = newtags.Substring(0, newtags.Length - 2);
                update_retval = await updatetags(id, newtags);
            }

            return update_retval;
        }

        private Task<string> getTags(object id)
        {
            //TODO: getTags no implemented.  Not sure what this is actually supposed to do
            throw new NotImplementedException();
        }

    }
}
