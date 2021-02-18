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
using System.Data.SqlTypes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
using System.Security.Cryptography;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Threading;
using mShop;

namespace backoffice
{
    public class Availability : INotifyPropertyChanged
    {
        private HttpClient client;
        private string username = "0972a8a70db60724c7a5af71be2fba66";
        private string password = "982bf0cf17152977d1cfbeb71a0d60e6";

        private string _availability;
        public string availability
        {
            get { return _availability; }
            set
            {
                if (_availability != value)
                {
                    _availability = value;
                    NotifyPropertyChanged("availability");
                }
            }
        }

        private string _handle;
        public string handle
        {
            get { return _handle; }
            set
            {
                if (_handle != value)
                {
                    _handle = value;
                    NotifyPropertyChanged("handle");
                }
            }
        }

        private string _eta;
        public string eta
        {
            get { return _eta; }
            set
            {
                if (_eta != value)
                {
                    _eta = value;
                    NotifyPropertyChanged("eta");
                }
            }
        }


        private string _status;
        public string status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    NotifyPropertyChanged("status");
                }
            }
        }

        public bool AutoUpdate = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public Availability()
        {
            client = new HttpClient();
            //client.Timeout = TimeSpan.FromMinutes(30);

            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }


        public async void NotifyPropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            if (AutoUpdate) await SetAvailability(propName);
        }

        public async Task<string> GetAvailability(string prd_handle)
        {
            MetaRoot meta_collection = new MetaRoot();

            string uri = "https://monpearte-it-solutions.myshopify.com/admin/api/2020-07/products/" + prd_handle + "/metafields.json?namespace=mstore";

            HttpResponseMessage response = await client.GetAsync(uri);

            meta_collection = JsonConvert.DeserializeObject<MetaRoot>(await response.Content.ReadAsStringAsync());

            foreach (Metafield2 meta in meta_collection.metafields)
            {
                switch (meta.key)
                {
                    case "availability":
                        _availability = meta.value;
                        break;
                    case "eta":
                        _eta = meta.value;
                        break;
                    case "status":
                        _status = meta.value;
                        break;
                    default:
                        break;
                }
            }

            _handle = prd_handle;

            if (response.IsSuccessStatusCode)
                AutoUpdate = true;

            return response.StatusCode.ToString();
        }

        private async Task<string> SetAvailability(string propName)
        {

                StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("metafield");
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(_handle);

                writer.WritePropertyName("value");
                switch (propName)
                {
                    case "availability":
                        writer.WriteValue(_availability);
                        break;
                    case "eta":
                        writer.WriteValue("_eta");
                        break;
                    case "status":
                        writer.WriteValue(_status);
                        break;
                    default:
                        break;
                }

                writer.WritePropertyName("value_type");
                writer.WriteValue("string");
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            string utri = "https://monpearte-it-solutions.myshopify.com/admin/api/2020-07/metafields/" + _handle + ".json";

            HttpContent content = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");

            Shopify shop = new Shopify();

            HttpStatusCode retval = await shop.put_product_data(utri, content);

            if (retval == HttpStatusCode.OK)
                AutoUpdate = true;

            return retval.ToString();
        }


        public async Task<string> CreateAvailability()
        {

            string retval = "";

            if (_handle != null)
            {
                Shopify shop = new Shopify();
                retval = await shop.update_availability(_handle, _availability, _eta, true);
            }

            return retval;
           
        }

    }

    public class Metafield2
    {
        public object id { get; set; }

        [JsonProperty("namespace")]
        public string _namespace { get; set; }
    public string key { get; set; }
    public string value { get; set; }
    public string value_type { get; set; }
    public object description { get; set; }
    public object owner_id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public string owner_resource { get; set; }
    public string admin_graphql_api_id { get; set; }
}
    public class MetaRoot
{
    public ObservableCollection<Metafield2> metafields { get; set; }
}

    public enum Location_Status_Enum
    {
        UnLoaded,
        Loading,
        Loaded
    }

    public class Shopify
    {
        public string uri_basic = @"https://monpearte-it-solutions.myshopify.com/admin/api/2021-01/products.json?fields=id,handle,title,published_scope,variants,vendor,inventory,tags,status";
        public string uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2021-01/products.json?fields=id,handle,title,published_scope,variants,vendor,inventory,tags,status&limit=250&published_status=published";
        public string uri_all = @"https://monpearte-it-solutions.myshopify.com/admin/api/2021-01/products.json?fields=id,handle,title,published_scope,variants,vendor,inventory,tags,status,published_at&limit=250";
        public string uri_images = @"https://monpearte-it-solutions.myshopify.com/admin/api/2021-01/products.json?fields=id,handle,title,published_scope,variants,vendor,inventory,tags,images,status&limit=250&published_status=published";
        const char splitter = ',';

        private Location_Status_Enum _Location_Status;

        //public ObservableCollection<Shopify_Product> products { get; set; }
        public ObservableCollection<Shopify_Product> products;
        public List<Location> Locations;

        public Location_Status_Enum Location_Status { get { return _Location_Status; } }

        private string username = "0972a8a70db60724c7a5af71be2fba66";
        private string password = "982bf0cf17152977d1cfbeb71a0d60e6";
        private HttpClient client;

        public int ratelimit = 2000;
        private string[] specialChars = { "#", "/", ".", "+", " ", "=", "*", "&", "@", "!", "^", "%", ":", ";" };

        private Shop_API API;

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


        private string CreateTags(Product newprod, Supplier sup)
        {
            List<string> tags = new List<string>();

            // set:
            //      vendor
            tags.Add("Vendor_" + newprod.Vendor.ToTitleCase());

            //      Product Types
            //tags.Add(newprod.ProductCategory);

            //      Shipping
            tags.Add(sup.Supplier_Tag);

            //      mbot available
            tags.Add(newprod.Available.ToAvailableTag());
            tags.Add(newprod.ETA.ToETATag());
            tags.Add(newprod.Status.ToStatusTag());
            tags.Add(common.Updatembot(""));

            return String.Join(", ", tags.ToArray());
        }

        public Shopify_Product MatchProductByMMT(string handle, string sku)
        {
            bool checksku = true;
            if ((sku == null) || (sku == ""))
            {
                checksku = false;
            }
            
            Shopify_Product retval = null;

            sku = sku.ToLower();

            handle = replace_specialchars(handle);

            foreach (Shopify_Product prod in products)
            {
                if ((prod.Handle.ToLower() == handle))
                {
                    retval = prod;
                    break;
                }

                if ((sku != "") || (!checksku))
                {
                    
                    if ((prod.Variants.FirstOrDefault().Sku.ToLower() == sku))
                    {
                        retval = prod;
                        break;
                    }
                }
            }

            return retval;
        }

        public Shopify_Product MatchProductBySupplier(Product supplier_prod)
        {
            Shopify_Product retval = null;
            string sku = supplier_prod.SKU.ToLower();

            foreach (Shopify_Product prod in products)
            {
                if ((prod.Handle.ToLower() == sku))
                {
                    retval = prod;
                    break;
                }

                if ((prod.Variants.FirstOrDefault().Sku.ToLower() == sku))
                {
                    retval = prod;
                    break;
                }
            }

            return retval;
        }

        public Product MatchProductByShopify(string handle, string sku, List<Product> prods)
        {
            Product retval = null;

            try
            {
                sku = sku.ToLower();
                handle = replace_specialchars(handle);

                foreach (Product prod in prods)
                {
                    if (prod.Vendor != null)
                    {
                        if (prod.SKU != null)
                        {
                            if (prod.SKU.ToLower() == handle)
                            {
                                retval = prod;
                                break;
                            }

                            if (sku != "")
                            {
                                if ((prod.SKU.ToLower() == sku))
                                {
                                    retval = prod;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Error trying to match data");
            }

            return retval;
        }

        public async Task<bool> getallproducts(string[] extraquerystrings = null, bool images = false, bool basic = false, bool include_unpublished = false)
        {
            bool retval = false;
            bool repeat = true;
            bool retry = true;

            string geturi;

            //TODO: Choices can be overriden by others here, which is not great
            geturi = images ? uri_images : uri;
            geturi = basic ? uri_basic: geturi;
            geturi = include_unpublished ? uri_all : geturi;

            if (extraquerystrings != null)
            {
                foreach (string queryString in extraquerystrings)
                {
                    geturi += "&" + queryString;
                }
            }
             
            if (products == null) 
            { 
                products = new ObservableCollection<Shopify_Product>(); 
            }
            else
            {
                products.Clear();
            }

            while (repeat)
            {
                retval = true;
                retry = true;

                HttpResponseMessage response = new HttpResponseMessage();
                Shopify_Products result = new Shopify_Products();
                string response_string = "";

                while (retry)
                {
                    response = await client.GetAsync(geturi);
                    
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        retry = false;
                    }
                    else
                    {
                        _ = await check_ratelimit(response.Headers);
                    }
                 }

                response_string = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<Shopify_Products>(response_string);

                foreach (Shopify_Product prod in result.Products)
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

        /// <summary>
        /// Updates Shopify Product online with new tags as supplied.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public async Task<bool> UpdateTags(object id, string tags)
        {
            return await API.UpdateTags(id, tags);
        }

        public async Task<HttpStatusCode> post_product_data(string uri, HttpContent hcontent)
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
            bool postretry = true;
            HttpResponseMessage response;

            HttpStatusCode retval = HttpStatusCode.Unused;


            while (postretry)
            {
                response = await client.PutAsync(uri, hcontent);

                retval = response.StatusCode;
                if (mshop_common.IsStatusCodeSuccess(response.StatusCode))
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

        public async Task<ObservableCollection<Metafield2>> GetMetaFields(string id)
        {
            MetaRoot meta_collection = new MetaRoot();

            string uri = "https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/products/" + id + "/metafields.json";

            HttpResponseMessage response = await client.GetAsync(uri);

            meta_collection = JsonConvert.DeserializeObject<MetaRoot>(await response.Content.ReadAsStringAsync());

            return meta_collection.metafields;
        }

        public async Task<string> update_metafield(Metafield2 meta)
        {
            string retval;

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("metafield");
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(meta.id);
                writer.WritePropertyName("value");
                writer.WriteValue(meta.value);
                writer.WritePropertyName("value_type");
                writer.WriteValue(meta.value_type);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            HttpContent content = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");

            var hcontent = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");

            uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-07/metafields/" + meta.id + ".json";

            try
            {
               HttpStatusCode retcode = await put_product_data(uri, hcontent);
               retval = retcode.ToString();
            }
            catch (Exception ex)
            {
                retval = ex.Message;
            }

            return retval;

        }

        public async Task<bool> Update_Availability(Shopify_Product shop_prod, Product supplier_product, bool AlwaysUpdate = false)
        {
            bool retval;
            bool api_return;

            if ((!ETA_Tags_Match(shop_prod, supplier_product)) | AlwaysUpdate)
            {
                api_return = await API.UpdateProductETAMetafields(shop_prod.Id.ToString(), supplier_product.Available.ToString(), supplier_product.ETA.ToString(), supplier_product.Status);

                shop_prod.Tags = common.Updatembot(shop_prod.Tags);
                shop_prod.Tags = common.ReplaceTag(shop_prod.Tags, supplier_product.Available.ToAvailableTag(), ETAExtensions.AvailableTagPrefix);
                shop_prod.Tags = common.ReplaceTag(shop_prod.Tags, supplier_product.ETA.ToETATag(), ETAExtensions.ETATagPrefix);
                shop_prod.Tags = common.ReplaceTag(shop_prod.Tags, supplier_product.Status.ToStatusTag(), ETAExtensions.StatusTagPrefix);

                if ((api_return) | AlwaysUpdate)
                    _ = await API.UpdateTags(shop_prod.Id, shop_prod.Tags);

                retval = api_return;
            }
            else
            {
                _ = await API.UpdateTags(shop_prod.Id, common.Updatembot(shop_prod.Tags));
                retval = false;
            }

            // returns if product required update
            return retval;
        }
        
        public async Task<string> update_availability(string handle, string availability, string eta, bool skipprodcheck = false, string sku = "", string MMTStatus = "", string tags = "")
        {

            string retval = handle + ", ";
            bool doETAUpdate = false;


            Shopify_Product a_prod = null;

            if (!skipprodcheck)
            {
                a_prod = MatchProductByMMT(handle, sku);
            }

            if ((a_prod != null) | (skipprodcheck) | doETAUpdate)
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
                    uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/products/" + a_prod.Id + "/metafields.json";

                }
                
                //update mbot tags
                if (tags != "")
                    await API.UpdateTags(handle, common.Updatembot(tags));


                if (eta == null)
                    mwrapper.Add(new Metafield("mstore", "eta", "UNKNOWN", "string"));
                else
                    mwrapper.Add(new Metafield("mstore", "eta", eta, "string"));

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
                        //retval += "Debug Only";
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

        private bool ETA_Tags_Match(Shopify_Product shop_prod, Product supplier_product)
        {
            bool retval = true;

            retval = retval & ContainsTags(shop_prod, supplier_product.Available.ToAvailableTag());
            retval = retval & ContainsTags(shop_prod, supplier_product.ETA.ToETATag());
            retval = retval & ContainsTags(shop_prod, supplier_product.Status.ToStatusTag());

            return retval;
        }

        private bool ContainsTags(Shopify_Product shop_prod, string match_tag)
        {
            return ContainsTags(shop_prod.Tags, match_tag);
        }

        private bool ContainsTags(string shop_prod_tags, string match_tag)
        {
            return shop_prod_tags.ToLower().Contains(match_tag.ToLower());              
        }


        public async Task<Prod_Availability> GetAvailability(object id)
        {

            GetMetafields metas = await API.Get_Product_Metafield_Data(id.ToString(), "namespace=mstore");
            Prod_Availability retval = new Prod_Availability();
            retval.Id = id.ToString();

            foreach(GetMetafield meta in metas.Metafields)
            {
                switch (meta.Key.ToLower())
                {
                    case "availability":
                        retval.Available = Convert.ToInt16(meta.Value);
                        break;
                    case "eta":
                        retval.ETA = Convert.ToDateTime(meta.Value);
                        break;
                    case "status":
                        retval.Status = meta.Value;
                        break;
                }
            }    

            return retval;
        }

        public async Task<bool> unpublishitem(object id)
        {
            string uturi = @"https://monpearte-it-solutions.myshopify.com/admin/api/2021-01/products/" + id.ToString() + ".json";

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
                writer.WritePropertyName("published");
                writer.WriteValue("false");
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
                        
            return mshop_common.IsStatusCodeSuccess(await API.Put_Product_Data(uturi, sw.ToString()));
        }

        public async Task<bool> republishitem(object id)
        {
            string uturi = @"https://monpearte-it-solutions.myshopify.com/admin/api/2021-01/products/" + id.ToString() + ".json";

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
                writer.WritePropertyName("published");
                writer.WriteValue("true");
                writer.WritePropertyName("published_scope");
                writer.WriteValue("");
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            return mshop_common.IsStatusCodeSuccess(await API.Put_Product_Data(uturi, sw.ToString()));
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

            return mshop_common.IsStatusCodeSuccess(await put_product_data(uturi, content));
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

                retStatusCode = await API.put_product_data(inv_uri, inv_hcontent);

                if (mshop_common.IsStatusCodeSuccess(retStatusCode))
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

                if (mshop_common.IsStatusCodeSuccess(retStatusCode))
                    retprice = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating variant pricing.  Item: " + variant_id, ex);
            }

            retval = retprice & retcostprice;

            return retval;
        }

        public async Task<InventoryItems> Get_InventoryItem(string prod_id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Remove_InventoryItemLocation(object inv_id, long location_id)
        {
            return await API.Remove_InventoryItemLocation(inv_id, location_id);
        }

        public async Task<bool> Set_InventoryItemLocation(object inv_id, long location_id)
        {
            return await API.ConnectInventoryItemLocation(inv_id, location_id);
        }

        public async Task<bool> Change_InventoryLocation(object inv_id, long old_location, long new_location)
        {
            //connect first then disconnect
            
            bool result2 = await API.ConnectInventoryItemLocation(inv_id, new_location);
            bool result1 = await API.Remove_InventoryItemLocation(inv_id, old_location);

            return result1 && result2;
        }

        //when you don't know the old location, only the new one.
        //quicker to just go through API calls to get old location for one product and not a bunch
        public async Task<bool> Change_InventoryLocation(object inv_id, long new_location)
        {
            //connect first then disconnect
            InventoryLevels inv_levels = await API.GetInventoryLevels(inv_id.ToString());

            bool result2 = await API.ConnectInventoryItemLocation(inv_id, new_location);
            bool result1 = false;
            bool result3 = false;

            foreach (InventoryLevel iLevel in inv_levels.Levels)
            {
                if (iLevel.LocationId != new_location)
                {
                    result3 = await API.Remove_InventoryItemLocation(inv_id, iLevel.LocationId);
                    result1 = result1 & result3;
                }
            }    

            return result1 & result2;
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
            return await API.GetInventoryLevels(inv_id.ToString());
        }

        public async Task<InventoryItems> Get_InventoryItemList(string ids)
        { 
            string uri = @"https://monpearte-it-solutions.myshopify.com/admin/api/2020-01/inventory_items.json?limit=100&ids=" + ids;

            string resp_content = await API.Get_Data(uri);
            InventoryItems retvals = JsonConvert.DeserializeObject<InventoryItems>(resp_content);
            
            return retvals;
        }

        public async Task<string> GetProductLocation(string handle)
        {
            string get_invlevels = "/admin/api/2021-01/inventory_levels.json?invetory_item_ids=";

            HttpResponseMessage response = await client.GetAsync(get_invlevels);
            Shopify_Products result = JsonConvert.DeserializeObject<Shopify_Products>(await response.Content.ReadAsStringAsync());

            return "True";
        }

        public async Task<InventoryItems> GetInventoryItems(string Handle)
        {
            InventoryItems invs = await API.GetInventoryItems(Handle);

            return invs;
        }

        public string createnewprice(string current_cost, string current_rrp, string current_price, string new_cost, string new_rrp, bool new_cost_taxable = false, bool new_rrp_taxable = false, bool shopify_taxable = false)
        {
            const double sellvsrrp_threshold = 1000;
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
            //
            // make everything shopify target - 11/2/21 - this code looks like a good idea.  I will turn off taxable on all products soon anyways
            if ((!new_cost_taxable) & (!shopify_taxable))
            {             
                new_cost = common.AddGST(new_cost).ToString();
                new_rrp = common.AddGST(new_rrp).ToString();
            }

            if ((!new_cost_taxable) & (shopify_taxable))
            {
                new_cost = new_cost.ToString();
                new_rrp = new_rrp.ToString();
            }



            // margin code looks kinds of wrong, as it is not the same as shopify cals
            // but I suspect ratio of prices is all that is needed.  Seems to have been working.
            double current_margin = (Convert.ToDouble(current_price) / Convert.ToDouble(current_cost));

            // fix widely incorrect cost price
            // if current margin less than 1 then things are messed up
            if (current_margin < 1)
            {
                double dcurrent_cost = Convert.ToDouble(current_cost);
                dcurrent_cost = Math.Round(dcurrent_cost / 1000, 2);

                current_margin = Convert.ToDouble(current_price) / dcurrent_cost;
            }

            // fix under 10% margins for smaller items
            if ((current_margin < 1.1) & (Convert.ToDouble(current_cost) < sellvsrrp_threshold))
                current_margin = 1.12;

            // create sell price based on new cost price and current margin
            //
            double sell_price = Convert.ToDouble(new_cost) * current_margin;
                        
            if ((sell_price > 10) & (sell_price < 100))
            {
                sell_price = Math.Round(sell_price / 0.5) * 0.5;
            }

            if ((sell_price > 100))
            {
                sell_price = Math.Round(sell_price);
            }

            double dnew_cost = Convert.ToDouble(new_cost);
            double dnew_rrp = Convert.ToDouble(new_rrp);


            // test new pricing to make sure it makes sense
            // 
            //
            // 1 - cant sell for less than cost (as long as cost is not 0)
            if ((sell_price < dnew_cost) & (dnew_cost > 0))
            { throw new Exception("Sell price can't be less than cost price"); }


            // 2 - dont sell for over RRP
            //     reduce margin if over sellvsrrp_threshold ($1000)
            if ((sell_price > dnew_rrp) & (dnew_rrp != 0))
            {                

                // try reducing margin to 1.1 if over 10%               
                if (current_margin > 1.1 )
                {
                    sell_price = dnew_cost * 1.1;
                }

                // if price is still over RRP and Price is above threshold then lower it to rrp
                // unless it's below 5% then we wont sell things
                if ((sell_price > Convert.ToDouble(new_rrp)) & (sell_price > sellvsrrp_threshold))
                {
                    double rrpcost_margin = dnew_rrp / dnew_cost;

                    if (rrpcost_margin > 1.05)
                    {
                        sell_price = dnew_rrp;
                    }
                    else
                    {
                        sell_price = dnew_cost * 1.05;
                    }

                    //throw new Exception("Sell price is more than rrp price");
                }
            }

            // 3 - if item is low value ie below lowsell_threshold ($20)
            //     ensure we can still make money.  10% generally wont cut it
            //     the smaller the price the more agressive the margin
            if (sell_price < 20)
            {
                double new_margin = current_margin;

                if (current_margin < 1.5)
                    new_margin = 1.5;

                if (sell_price < 10)
                {
                    if (sell_price < 5)
                        if (current_margin < 2.5)
                            new_margin = 2.5;

                    if (current_margin < 2)
                        new_margin = 2;
                }

                // round to nearest 25c
                sell_price = Math.Round((dnew_cost * new_margin) / 0.25) * 0.25;
            }


            // round values to nearest 50c if under $100
            // or nearest $1 if over 100
            if ((sell_price > 20) & (sell_price < 100))
            {
                sell_price = Math.Round(sell_price / 0.5) * 0.5;
            }

            if ((sell_price > 100))
            {
                sell_price = Math.Round(sell_price);
            }

            return sell_price.ToString();
        }

        private Task<string> getTags(object id)
        {
            throw new NotImplementedException();
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

        public Shopify(int ratelimit_interval = 1000)
        {
            API = new Shop_API(ratelimit_interval);

            ratelimit = ratelimit_interval;
                                    
            client = new HttpClient();
            //client.Timeout = TimeSpan.FromMinutes(30);

            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            this.Locations = new List<Location>();

            _Location_Status = Location_Status_Enum.UnLoaded;
            LoadLocations();
        }

        public async void LoadLocations()
        {
            _Location_Status = Location_Status_Enum.Loading;

            Locations temp_locations;

            temp_locations = await API.GetLocations();

            if (temp_locations.LocationsLocations == null)
            { 
                _Location_Status = Location_Status_Enum.UnLoaded;
             }
            else
            {

                foreach (Location loc in temp_locations.LocationsLocations)
                {
                    this.Locations.Add(loc);
                }

                _Location_Status = Location_Status_Enum.Loaded;
            }
        }

        public async Task<Shopify_Product> GetProduct(string Handle)
        {
            return await API.GetProduct(Handle);
        }

        /*
        public async Task<Variants> GetVariants(string Handle)
        {
            return await API.GetVariants(Handle);
        }
        */
    }
}
