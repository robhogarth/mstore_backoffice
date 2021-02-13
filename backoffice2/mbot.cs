using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Runtime.CompilerServices;
using backoffice.ShopifyAPI;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Data;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;

namespace backoffice
{
    public class NotifyEventArgs : EventArgs
    {
        public int Threshold { get; set; }
        public string Message { get; set; }
        public bool ConsoleOnly { get; set; }
        public Dictionary<String, String> properties { get; set; }
        public Dictionary<String,Double> AIMetrics { get; set; }

    }

    public class NotifyExceptionEventArgs: EventArgs
    {
        public Exception NException { get; set; }
        public IDictionary<string,string> Properties { get; set; }
        public IDictionary<string, Double> Metrics { get; set; }
    }

    public class NotifyProductEventArgs: EventArgs
    {
        public string EventName { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public IDictionary<string, Double> Metrics { get; set; }
    }


    public class MBot
    {

        const string ClearanceTag = "Clearance";
        const string TechDataFile = "PRICING_FEED_0009043769.zip";
        const string DickerDataFile = "datafeed.zip";


        private List<string> mstore_stock;
        static string mstore_stock_file;

        public Shopify shopify;
        public bool verbose = false;

        public event EventHandler<NotifyEventArgs> Notify;
        public event EventHandler<NotifyExceptionEventArgs> Exception;
        public event EventHandler<NotifyProductEventArgs> ProductEvent;

        public MBot(string stock_file)
        {
            mstore_stock_file = stock_file;

        }

        public async Task<int> Test()
        {
            string _wfile = @"d:\temp\WaveLink Scrape.csv";
            string _tfile = @"d:\temp\0009043769.csv";
            string _dfile = @"c:\temp\datafeed.csv";


            UpdatePricing().Wait();

            
            /*
            LogStr("Loading Shopify Products", true);
            shopify = new Shopify();
            
            _ = await Download_Shopify();
                
             LogStr("Shopify load completed", true);
             LogStr("Doing Test Code...", true);
            
            

            foreach (Shopify_Product p in shopify.products) 
            {
                Product_Fault_Codes fcode = await p.Verify_Product();
                Console.WriteLine(p.Id + " - " + p.Title + " - " + fcode.ToString());
            };

            */

            




            //await FixHiddenProds(SupplierType.MMT);
            //await FixHiddenProds(SupplierType.TechData, _tfile);
            //await FixHiddenProds(SupplierType.DickerData, _dfile);

            //UpdatePricing(SupplierType.MMT).Wait();

            return 0;
        }


        public async Task<bool> Process_DataFile(SupplierType stype, string filename, bool extractFile = true)
        {
            try
            {
                string _file = "";

                if (extractFile)
                    _file = ExtractDataFile(stype, filename);
                else
                    _file = filename;

                await UpdatePricing(stype, _file);
                await UpdateETA(stype, _file);

                return true;
            }
            catch (Exception ex)
            {
                //LogStr("ProcessDatafileException - " + ex.Message);

                LogEx(ex);
                return false;
            }
        }

        private string ExtractDataFile(SupplierType sType, string _file)
        {
            FileSupplier sup = SupplierProducer.CreateFileSupplier(sType);

            string _path = sup.Temppath;
            
            try
            {
                if (Directory.Exists(_path))
                {
                    foreach (string file in Directory.EnumerateFiles(_path))
                    {
                        File.Delete(file);
                    }

                }

                ZipFile.ExtractToDirectory(_file, _path);
                string[] files = System.IO.Directory.GetFiles(_path);

                return files[0];
            }
            catch (Exception ex)
            {
                LogEx(ex);
            }

            return "";
        }

        private bool IsProductMatch(Shopify_Product s_product, Product product)
        {
            bool match = false;

            try
            {
                if (s_product.Handle.ToLower() == product.SKU.ToLower())
                {
                    match = true;
                }
                else
                {
                    //if (s_product.variants.FirstOrDefault().sku.ToLower() == mmt_prod.Manufacturer[0].ManufacturerCode.ToLower())
                    if (s_product.Variants.FirstOrDefault().Sku.ToLower() == product.SKU.ToLower())
                    {
                        match = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEx(ex);
            }

            return match;
        }

        /// <summary>
        /// Updates Item ETA by Shopify Item number
        /// </summary>
        /// <param name="itemnumber"></param>
        /// <param name="eta"></param>
        /// <param name="available"></param>
        /// <param name="status"></param>
        public async void UpdateItemETA(string itemnumber, string eta, string available, string status, string tags = "")
        {

            if (shopify == null)
            {
                shopify = new Shopify();
            }

            string result = await shopify.update_availability(itemnumber, available, eta, true, "", status, tags);

            LogStr(result);
        }

        public async void UpdateItemETA(Shopify_Product shop_prod, Product supplier_product)
        {
            if (shopify == null)
            {
                shopify = new Shopify();
            }

            bool result = await shopify.Update_Availability(shop_prod, supplier_product);

            LogStr("ETA Updated for " + shop_prod.Handle + " - " + result.ToString());
        }

        /// <summary>
        /// Updates list of item ETA by SKU.  Developed for TechData
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task<string> UpdateItemETAbySKU(string filename)
        {

            if (shopify == null)
            {
                shopify = new Shopify();
                await shopify.getallproducts();
            }

            string prod_eta = "";
            string prod_status = "";
            int counter = 0;
            double available = 0;

            using (TextFieldParser parser = new TextFieldParser(filename))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Process row
                    string[] fields = parser.ReadFields();

                    if (counter > 0)
                    {
                        //LogStr(shopify.update_availability(fields[0], fields[4], fields[2], false, fields[0], fields[3]).GetAwaiter().GetResult(), true);

                        available = Math.Round(Convert.ToDouble(fields[10]), 0);

                        if (available > 0)
                        {
                            prod_eta = "N/A";
                            prod_status = "In Stock";
                        }
                        else
                        {
                            prod_eta = "Approx 2 - 4 weeks";
                            prod_status = "Order to Order";
                        }

                        LogStr(shopify.update_availability(fields[4], available.ToString(), prod_eta, false, fields[4], prod_status).GetAwaiter().GetResult(), true);

                    }

                    counter++;

                }
            }

            return "finished processing";
        }

        
        /// <summary>
        /// Identifies all shopify items with no images.  Provides list for manipulation
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateNoImage()
        {
            shopify = new Shopify();

            try
            {
                LogStr("Downloading Product Lists - Async", true);
                var shopify_download = Download_Shopify(null, true);

                await Task.WhenAll(shopify_download);
                LogStr("Getting list of no image products...", true);

                //check if product has an image.  create list of 
                foreach (Shopify_Product sprod in shopify.products)
                {

                    if (sprod.Images.Count == 0)
                    {
                        LogStr("Found Match - " + sprod.Title, true);
                        LogStr(sprod.Handle + @",""" + sprod.Title + @"""");
                    }

                }
            }
            catch (Exception ex)
            {
                LogStr(ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates Clearance collection based on MMT clearance status code
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateClearance()
        {
            LogEvent("UpdateClearance");
            shopify = new Shopify();

            try
            {
                LogStr("Downloading Products", true);

                //var mmt_download = Download_MMTAsync(MMTDownloadType.Clearance);
                Supplier mmt = new MMTSupplier();
                ((MMTSupplier)mmt).DownloadType = backoffice.MMTDownloadType.Clearance;

                var mmt_download = mmt.LoadProducts();
                var shopify_download = Download_Shopify(new string[] { "collection_id=" + mmt.CollectionID });

                await Task.WhenAll(mmt_download, shopify_download);
                LogStr("Updating Clearance...", true);

                bool match = false;
                Product mmt_prod_match = null;

                //check all shopify items against clearance
                //if item exists - update ETA or something for good measure
                //else remove from list
                foreach (Shopify_Product sprod in shopify.products)
                {
                    match = false;

                    foreach (Product mmt_prod in mmt.Products)
                    {
                        if (IsProductMatch(sprod, mmt_prod))
                        {
                            match = true;
                            mmt_prod_match = mmt_prod;
                            break;
                        }
                    }

                    if (!match)
                    {
                        sprod.Tags = RemoveClearanceTag(sprod.Tags);
                        _ = await shopify.UpdateTags(sprod.Id, sprod.Tags);

                        //LogStr(String.Format(@"""{0}"", Item removed from list", sprod.Id.ToString()));

                        Dictionary<string, string> props = new Dictionary<string, string>();
                        props.Add("SKU", sprod.Variants.FirstOrDefault().Sku);
                        props.Add("Handle", sprod.Handle);
                        props.Add("Tags", sprod.Tags);
                        LogEvent("RemoveClearanceTag",props);
                    }
                }

                Shopify_Product sprod_match = new Shopify_Product();

                List<Product> AddList = new List<Product>();

                foreach (Product mmt_prod in mmt.Products)
                {
                    match = false;

                    foreach (Shopify_Product sprod in shopify.products)
                    {
                        if (IsProductMatch(sprod, mmt_prod))
                        {
                            match = true;
                            break;
                        }
                    }

                    if (!match)
                    {
                        AddList.Add(mmt_prod);
                        LogStr(String.Format(@"""{0}"", Item added to list", mmt_prod.Title));
                    }
                }

                LogStr("Getting list of all shopify items");

                await shopify.getallproducts();

                LogStr("Matching MMT products to Shopify to add clearance tags");

                foreach (Product mmt_prod in AddList)
                {
                    match = false;
                    foreach (Shopify_Product sprod in shopify.products)
                    {
                        if (IsProductMatch(sprod, mmt_prod))
                        {
                            match = true;
                            sprod_match = sprod;
                            break;
                        }
                    }

                    if (match)
                    {
                        sprod_match.Tags = AddClearanceTag(sprod_match.Tags);
                        _ = await shopify.UpdateTags(sprod_match.Id, sprod_match.Tags);

                        //LogStr(String.Format(@"""{0}"", Item added to clearance collection", sprod_match.Id.ToString()));

                        Dictionary<string, string> props = new Dictionary<string, string>();
                        props.Add("SKU", sprod_match.Variants.FirstOrDefault().Sku);
                        props.Add("Handle", sprod_match.Handle);
                        props.Add("Tags", sprod_match.Tags);
                        LogEvent("AddClearanceTag", props);
                    }
                    else
                    {
                        //await shopify.AddMMTProduct(mmt_prod);
                        LogStr("Item didn't find a shopify match, will be added into Shopify (when code works)");
                    }
                }
            }
            catch (Exception ex)
            {
                LogStr(ex.Message, true);
            }

            return true;
        }

        private string AddClearanceTag(string tags)
        {
            if (tags == null)
            {
                return ClearanceTag;
            }

            if (tags.Contains(ClearanceTag))
            {
                return tags;
            }

            if (tags.Length > 0)
            {
                tags += "," + ClearanceTag;
            }
            else
            {
                tags = ClearanceTag;
            }

            return tags;
        }

        private string RemoveClearanceTag(string tags)
        {
            string wrkstr = "";
            string[] wrkarray = wrkstr.Split(',');

            foreach (string wrks in wrkarray)
            {
                if (wrks != ClearanceTag)
                {
                    if (wrkstr == "")
                    {
                        wrkstr += wrks;
                    }
                    else
                    {
                        wrkstr += "," + wrks;
                    }
                }
            }

            return wrkstr;
        }


        /// <summary>
        /// FindUnmatched Items that are no longer published by vendor
        /// </summary>
        /// <returns></returns>
        public async Task<bool> FindUnmatched(SupplierType sType = SupplierType.MMT, string filename = "")
        {
            LogEvent("FindUnmatchedItems");

            shopify = new Shopify();
            Supplier supplier = SupplierProducer.CreateSupplier(sType, filename);

            try
            {
                var supplier_download = supplier.LoadProducts();
                var shopify_download = Download_Shopify(new string[] { "collection_id=" + supplier.CollectionID }, false, true);

                Get_mstore_stock();

                await Task.WhenAll(supplier_download, shopify_download);
                LogStr("Updating Unmatched Items...", true);
            }
            catch (Exception ex)
            {
                //LogStr(ex.Message, true);
                LogEx(ex);
            }

            try
            {

                int nomatchcount = 0;

                Product shopifymatchproduct;

                foreach (Shopify_Product product in shopify.products)
                {

                    if (product.Vendor != "Sangoma")
                    {
                        bool match = false;

                        try
                        {
                            shopifymatchproduct = shopify.MatchProductByShopify(product.Handle, product.Variants.FirstOrDefault().Sku, supplier.Products);
                        }
                        catch (Exception ex)
                        {
                            shopifymatchproduct = null;
                            LogStr(ex.Message);
                        }

                        if (shopifymatchproduct != null)
                        {
                            match = true;
                        }

                        if (mstore_stock.Contains(product.Handle))
                        {
                            match = true;
                        }

                        if (!match)
                        {
                            nomatchcount++;
                            if (product.Variants == null | (product.PublishedAt == null))
                            {
                                //LogStr(String.Format(@"""{0}"",""{1}"",""{2}""", product.Handle.ToLower(), product.Title, "Not published on Shopify"));
                            }
                            else
                            {
                                if (await shopify.unpublishitem(product.Id))
                                {
                                    _ = shopify.Update_Availability(product, shopifymatchproduct, true);

                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"" - Successfully unpublished", product.Handle.ToLower(), product.Title, product.Variants[0].Sku));

                                    Dictionary<string, string> props = new Dictionary<string, string>();
                                    props.Add("SKU", product.Variants.FirstOrDefault().Sku);
                                    props.Add("Handle", product.Handle);
                                    props.Add("Tags", product.Tags);
                                    LogEvent("UnpublishedProduct", props);
                                }
                                else
                                {
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"" - Error unpublishing", product.Handle.ToLower(), product.Title, product.Variants[0].Sku));
                                }
                            }
                        }
                        else
                        {
                            if (product.PublishedAt == null)
                            {

                                if (await shopify.republishitem(product.Id))
                                {
                                    _ = shopify.Update_Availability(product, shopifymatchproduct, true);
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"" - Successfully REpublished", product.Handle.ToLower(), product.Title, product.Variants[0].Sku));

                                    Dictionary<string, string> props = new Dictionary<string, string>();
                                    props.Add("SKU", product.Variants.FirstOrDefault().Sku);
                                    props.Add("Handle", product.Handle);
                                    props.Add("Tags", product.Tags);
                                    LogEvent("RepublishedProduct", props);
                                }
                                else
                                {
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"" - Error REpublishing", product.Handle.ToLower(), product.Title, product.Variants[0].Sku));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogEx(ex);
            }

            if (verbose)
            {
                LogStr("Unmatched Product Search Completed");
            }

            return true;
        }

        public async Task<bool> FixHiddenProds(SupplierType sType = SupplierType.MMT, string filename = "")
        {
            shopify = new Shopify();
            Supplier supplier = SupplierProducer.CreateSupplier(sType, filename);

            try
            {
                var supplier_download = supplier.LoadProducts();
                var shopify_download = Download_Shopify(new string[] { "collection_id=" + supplier.CollectionID }, false, true);

                Get_mstore_stock();

                await Task.WhenAll(supplier_download, shopify_download);
                LogStr("Updating Unmatched Items...", true);
            }
            catch (Exception ex)
            {
                LogStr(ex.Message, true);
            }

            try
            {

                int nomatchcount = 0;
                Product shopifymatchproduct;

                foreach (Shopify_Product product in shopify.products)
                {

                    if (product.Vendor != "Sangoma")
                    {
                        bool match = false;

                        try
                        {
                            shopifymatchproduct = shopify.MatchProductByShopify(product.Handle, product.Variants.FirstOrDefault().Sku, supplier.Products);
                        }
                        catch (Exception ex)
                        {
                            shopifymatchproduct = null;
                            LogStr(ex.Message);
                        }

                        if (shopifymatchproduct != null)
                        {
                            match = true;
                        }

                        if (mstore_stock.Contains(product.Handle))
                        {
                            match = true;
                        }

                        if (match)
                        {
                            if (product.PublishedAt == null)
                            {
                                if (await shopify.republishitem(product.Id))
                                {
                                    _ = shopify.Update_Availability(product, shopifymatchproduct, true);
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"" - Successfully REpublished", product.Handle.ToLower(), product.Title, product.Variants[0].Sku));
                                }
                                else
                                {
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"" - Error REpublishing", product.Handle.ToLower(), product.Title, product.Variants[0].Sku));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Processing item", ex);
            }

            if (verbose)
            {
                LogStr("Unmatched Product Search Completed");
            }


            return true;
        }

        public async Task<InventoryItems> GetSomeInventoryItems(List<string> inv_ids)
        {
            string inv_uri = "";

            //LogStr("Retreiving 100 inventory items", true);
            foreach (string inv_id in inv_ids)
            {
                inv_uri += "," + inv_id;
            }

            inv_uri = inv_uri.Substring(1);

            InventoryItems inv_items = await shopify.Get_InventoryItemList(inv_uri);
 
            return inv_items;
        }


        /// <summary>
        /// Updates pricing for supplier based on a lot of complex assumptions
        /// </summary>
        /// <param name="DownloadType"></param>
        /// <returns></returns>
        public  async Task<bool> UpdatePricing(SupplierType sType = SupplierType.MMT, string filename = "")
        {
            LogEvent("UpdatePricing");

            shopify = new Shopify();
            Supplier supplier = SupplierProducer.CreateSupplier(sType, filename);

            try
            {
                var supplier_download = supplier.LoadProducts();

                Task shopify_download;
                if (supplier.MultiSourceProducts)
                {
                    shopify_download = Download_Shopify();
                }
                else
                {
                    shopify_download = Download_Shopify(new string[] { "collection_id=" + supplier.CollectionID});
                }

                await Task.WhenAll(supplier_download, shopify_download);
                LogStr("Product Datafeed load completed", true);
                LogStr("Updating Pricing...", true);
            }
            catch (Exception ex)
            {
                LogEx(ex);
            }
           
            string new_price = "";

            Shopify_Product matcheditem;
            Variant MatchedVariant;

            // Download associated inventory items so you can get cost price etc
            // shopify separates costs into 3 different objects

            List<string> inv_ids = new List<string>();
            Dictionary<string, InventoryItem> invList = new Dictionary<string, InventoryItem>();
            InventoryItems inv_items;

            foreach (Shopify_Product s_prod in shopify.products)
            {
                foreach (Variant s_prod_var in s_prod.Variants)
                {
                    inv_ids.Add(s_prod_var.InventoryItemId.ToString());
                }

                // Shopify only allows us to retrieve 100 items at a time
                // once we have 100 added to the inv_ids list we can retrive them
                //
                // Shopify doesn't allow us to get ALL items at once like products,
                // so we can't utilize any paging functionality for this which would be logical

                if (inv_ids.Count > 99)
                {
                    inv_items = await GetSomeInventoryItems(inv_ids);
                    foreach (InventoryItem inv_item in inv_items.Items)
                    {
                        invList.Add(inv_item.Id.ToString(), inv_item);
                    }

                    // Once we processed 100 items, we clear the list and this resets us to then get the next 100
                    // This is where the magic happens
                    inv_ids.Clear();
                }
            }

            // Gets the remaining items if there are not an even 100
            if (inv_ids.Count > 0)
            {
                inv_items = await GetSomeInventoryItems(inv_ids);

                foreach (InventoryItem inv_item in inv_items.Items)
                {
                    invList.Add(inv_item.Id.ToString(), inv_item);
                }
            }


            LogStr("Matching items", true);
            bool update_price;
            bool update_cost;
            bool force_eta_update;
            bool change_supplier;
            bool supplier_match;

            double comp_cost_price;
            string[] formatlistex;
            
            foreach (Product supplier_prod in supplier.Products)
            {
                try
                {
                    update_price = false;
                    update_cost = false;
                    force_eta_update = false;
                    change_supplier = false;
                    supplier_match = false;

                    matcheditem = shopify.MatchProductByMMT(supplier_prod.SKU, supplier_prod.SKU);

                    if (matcheditem != null)
                    {

                        //Get InventoryItem from invList
                        InventoryItem inv = null;

                        if (invList.ContainsKey(matcheditem.Variants.FirstOrDefault().InventoryItemId.ToString()))
                        {
                            inv = invList[matcheditem.Variants.FirstOrDefault().InventoryItemId.ToString()];
                        }

                        if ((inv != null) & (matcheditem.Variants.Count() < 2))
                        {
                            supplier_match = Match_Product_Tag(matcheditem.Tags, supplier.Supplier_Tag);
                            if (!supplier_match)
                            {
                                change_supplier = await Evaluate_Product_Supplier_Change(matcheditem, inv, supplier_prod);
                                if (change_supplier)
                                {
                                    LogStr(supplier_prod.SKU + " - Changing to new supplier");

                                    Dictionary<string, string> props = new Dictionary<string, string>();
                                    props.Add("SKU", supplier_prod.SKU);
                                    props.Add("MatchedItem", matcheditem.Handle);
                                    props.Add("Title", matcheditem.Title);
                                    props.Add("Old Supplier", common.ExtractSupplierTag(matcheditem.Tags));
                                    props.Add("New Supplier", supplier.Supplier_Tag);
                                    LogEvent("ChangeSupplier", props);

                                    force_eta_update = true;
                                    bool change_result = await shopify.Change_InventoryLocation(inv.Id, supplier.Supplier_Location_Id);
                                    matcheditem.Tags = await UpdateShippingTag(matcheditem, supplier.Supplier_Tag, false); //update tags in matcheditem, but dont update it on store yet, as we will do eta update shortly anyways
                                }
                            }

                            update_price = false;
                            update_cost = false;

                            //standard checks if supplier is matched
                            if (supplier_match)
                            {
                                if (matcheditem.Variants.FirstOrDefault().CompareAtPrice != null)
                                {
                                    if (matcheditem.Variants.FirstOrDefault().CompareAtPrice.ToString() != supplier_prod.RRPPrice.ToShopify())
                                    {
                                        update_price = true;
                                    }
                                }

                                if (matcheditem.Variants.FirstOrDefault().Taxable)
                                {
                                    comp_cost_price = common.RemoveGST(supplier_prod.CostPrice);

                                    Dictionary<string, string> props = new Dictionary<string, string>();
                                    props.Add("SKU", matcheditem.Variants.FirstOrDefault().Sku);
                                    props.Add("Title", matcheditem.Title);

                                    LogEvent("ItemTaxable", props);
                                }
                                else
                                    comp_cost_price = supplier_prod.CostPrice;

                                if (inv.Cost != comp_cost_price.ToShopify())
                                {
                                    update_cost = true;
                                }

                                if (matcheditem.Tags.Contains("specialprice"))
                                {
                                    update_cost = false;
                                    update_price = false;
                                    LogStr(matcheditem.Handle + " contains special price.  Pricing not evaluated");
                                }
                            }

                            //once you get into this section price is evaulated and updated.  There is no return from here
                            if (update_price || update_cost || change_supplier)
                            {

                                //generate new price
                                try
                                {
                                    if (matcheditem.Variants.FirstOrDefault().CompareAtPrice == null)
                                        new_price = shopify.createnewprice(inv.Cost, "0", matcheditem.Variants.FirstOrDefault().Price, supplier_prod.CostPrice.ToShopify(), supplier_prod.RRPPrice.ToShopify(), false, false, matcheditem.Variants[0].Taxable);
                                    else
                                        new_price = shopify.createnewprice(inv.Cost, matcheditem.Variants.FirstOrDefault().CompareAtPrice.ToString(), matcheditem.Variants.FirstOrDefault().Price, supplier_prod.CostPrice.ToShopify(), supplier_prod.RRPPrice.ToShopify(), false, false, matcheditem.Variants[0].Taxable);
                                }
                                catch (Exception ex)
                                {
                                    LogEx(ex);
                                    formatlistex = new string[] { matcheditem.Handle, matcheditem.Title, supplier_prod.CostPrice.ToShopify(), supplier_prod.RRPPrice.ToShopify(), ex.Message };
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"", ""Match Found. Price Not Equal. Error creating New Price"", ""{4}""", formatlistex));
                                    new_price = "0";
                                }


                                if (new_price != "0")
                                {

                                    //Create formatlist for logging purposes
                                    string[] formatlist = { matcheditem.Handle, matcheditem.Title, new_price, supplier_prod.CostPrice.ToShopify(), supplier_prod.RRPPrice.ToShopify() };
                                    
                                    //now you have a new price update all pricing in shopify
                                    try
                                    {
                                        if (matcheditem.Variants[0].Taxable)
                                            await shopify.updateprice(inv.Id, matcheditem.Variants.FirstOrDefault().Id, common.RemoveGST(supplier_prod.CostPrice).ToShopify(), common.RemoveGST(new_price).ToShopify(), common.RemoveGST(supplier_prod.RRPPrice).ToShopify());
                                        else
                                            await shopify.updateprice(inv.Id, matcheditem.Variants.FirstOrDefault().Id, supplier_prod.CostPrice.ToShopify(), new_price, supplier_prod.RRPPrice.ToShopify());


                                        Dictionary<string, string> props = new Dictionary<string, string>();
                                        props.Add("SKU", matcheditem.Variants.FirstOrDefault().Sku);
                                        props.Add("Title", matcheditem.Title);
                                        props.Add("Old_Cost", inv.Cost);
                                        props.Add("New_Cost", supplier_prod.CostPrice.ToShopify());
                                        props.Add("Old_Price", matcheditem.Variants.FirstOrDefault().Price);
                                        props.Add("New_Price", new_price);
                                        props.Add("Old_RRP", matcheditem.Variants.FirstOrDefault().CompareAtPrice.ToString());
                                        props.Add("New_RRP", supplier_prod.RRPPrice.ToShopify());

                                        LogEvent("UpdatePrice", props);

                                        LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"",""{4}"", Updated Pricing", formatlist));

                                    }
                                    catch (Exception ex)
                                    {
                                        LogEx(ex);
                                        formatlistex = new string[] { matcheditem.Handle, matcheditem.Title, supplier_prod.CostPrice.ToShopify(), supplier_prod.RRPPrice.ToShopify(), ex.Message };
                                        LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"", ""Error uploading new pricing"", ""{4}""", formatlistex));
                                    }

                                }
                                else
                                {
                                    formatlistex = new string[] { matcheditem.Handle, matcheditem.Title, supplier_prod.CostPrice.ToShopify(), supplier_prod.RRPPrice.ToShopify() };
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"", ""Error price is 0", formatlistex));
                                }

                                if (force_eta_update)
                                {
                                    UpdateItemETA(matcheditem, supplier_prod);
                                    LogStr("Force ETA update for item - " + matcheditem.Id.ToString());
                                }
                            }
                            else
                            {
                                //                                LogStr(String.Format(@"""{0}"",""{1}"",""{2}"", Match Found, Price Equal", matcheditem.handle, matcheditem.title, supplier_prod.Manufacturer[0].ManufacturerCode));
                            }
                        }

                        if (!supplier.MatchVariants)
                            shopify.products.Remove(matcheditem);
                    }

                    if (supplier.MatchVariants)
                    {
                        MatchedVariant = MatchVariantbySKU(shopify.products, supplier_prod.SKU);

                        if (MatchedVariant != null)
                        {
                            InventoryItem inv = null;

                            if (invList.ContainsKey(MatchedVariant.InventoryItemId.ToString()))
                            {
                                inv = invList[MatchedVariant.InventoryItemId.ToString()];
                            }

                            //evaluate if pricing is changed
                            update_price = PriceChanged(MatchedVariant, inv, supplier_prod);
                            if (update_price)
                            {
                                new_price = shopify.createnewprice(inv.Cost, "0", MatchedVariant.Price, supplier_prod.CostPrice.ToShopify(), supplier_prod.RRPPrice.ToShopify(), false, false, MatchedVariant.Taxable);

                                //update variant if required
                                await shopify.updateprice(inv.Id, MatchedVariant.Id, supplier_prod.CostPrice.ToShopify(), new_price, supplier_prod.RRPPrice.ToShopify());

                                Dictionary<string, string> props = new Dictionary<string, string>();
                                props.Add("Title", MatchedVariant.Title);
                                props.Add("SKU", MatchedVariant.Sku);
                                props.Add("Old_Cost", inv.Cost);
                                props.Add("New_Cost", supplier_prod.CostPrice.ToShopify());
                                props.Add("Old_Price", MatchedVariant.Price);
                                props.Add("New_Price", new_price);
                                props.Add("Old_RRP", MatchedVariant.CompareAtPrice.ToString());
                                props.Add("New_RRP", supplier_prod.RRPPrice.ToShopify());
                                props.Add("UpdateVariant", MatchedVariant.Id.ToString());

                                LogEvent("UpdateVariantPrice", props);

                                string[] formatlist = { MatchedVariant.Id.ToString(), MatchedVariant.Title, new_price, supplier_prod.CostPrice.ToShopify(), supplier_prod.RRPPrice.ToShopify() };
                                LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"",""{4}"", Updated New Price", formatlist));
                            }
                            else
                                LogStr(supplier_prod.SKU + " - pricing update not required");
                        }
                        else
                        {
                            LogStr(supplier_prod.SKU + " - couldn't match sku");
                        }
                    }

                    formatlistex = new string[] { supplier_prod.SKU, supplier_prod.Title, supplier_prod.CostPrice.ToShopify() };
                    //LogStr(String.Format(@"""{0}"",""{1}"",""{2}"", ""No Match Found.", formatlistex));
                }
                catch (Exception ex)
                {
                    LogEx(ex);
                    LogStr("Error matching product - " + supplier_prod.SKU + "  Moving to next item: " + ex.Message);
                }
            }

            LogStr("Update Pricing Completed", true);
            return true;
        }

        private bool PriceChanged(Variant matchedVariant, InventoryItem inv, Product supplier_prod, string Tags = "")
        {
            double comp_cost_price;
            bool update_price = false;
            bool update_cost = false;

            if (matchedVariant.CompareAtPrice != null)
            {

                if (matchedVariant.CompareAtPrice.ToString() != supplier_prod.RRPPrice.ToShopify())
                {
                    update_price = true;
                }
            }

            if (matchedVariant.Taxable)
                comp_cost_price = common.RemoveGST(supplier_prod.CostPrice);
            else
                comp_cost_price = supplier_prod.CostPrice;

            if (inv.Cost != comp_cost_price.ToShopify())
            {
                update_cost = true;
            }

            if (Tags.Contains("specialprice"))
            {
                update_cost = false;
                update_price = false;
                LogStr(matchedVariant.Id + " contains special price.  Pricing not evaluated");
            }

            return (update_price & update_cost);
        }

        private Variant MatchVariantbySKU(ObservableCollection<Shopify_Product> products, string sKU)
        {
            Variant MatchedVariant = null;
            bool matched = false;

            foreach(Shopify_Product sprod in products)
            {
                foreach(Variant prod_var in sprod.Variants)
                {
                    if (prod_var.Sku.ToLower() == sKU.ToLower())
                    {
                        MatchedVariant = prod_var;
                        matched = true;
                    }
                    if (matched)
                        break;
                }
                if (matched)
                    break;
            }

            return MatchedVariant;
        }

        private async Task<bool> Evaluate_Product_Supplier_Change(Shopify_Product matcheditem, InventoryItem inv, Product sprod)
        {
            /*  Item is matched in pricing method, but currently not supplied by this supplier
             *  
             *  So evaluate if it should be based on stock availability and price 
             *  
             */
            
            bool retval = false;

            if (Match_Product_Tag(matcheditem.Tags, "Supplier_Change_Override"))
                retval = false;
            else
            {


                if (sprod.Available > 0)
                {
                    if (Convert.ToDouble(inv.Cost) > sprod.CostPrice)
                    {
                        //does new supplier have it and is cheaper?  then change

                        retval = true;
                    }
                    else
                    {
                        //does new supplier have stock and old supplier doesn't?  then change

                        //get current metafields for availability
                        Prod_Availability avail = await shopify.GetAvailability(matcheditem.Id);

                        if (avail.Available == 0)
                            retval = true;

                    }
                }
                else
                {

                    Prod_Availability avail = await shopify.GetAvailability(matcheditem.Id);

                    if (avail.Available == 0)
                    {
                        if (avail.ETA < sprod.ETA)
                        {
                            if (Convert.ToDouble(inv.Cost) > sprod.CostPrice)
                            {
                                retval = true;
                            }
                        }
                    }
                }
            }
            return retval;
        }

        private bool Match_Product_Tag(string tags, string match_Tag)
        {
            bool retval = false;
            tags = tags.ToLower();

            if (tags.Contains(match_Tag.ToLower()))
                retval = true;

            return retval;
        }
        
        public async Task<bool> UpdateETA(SupplierType sType = SupplierType.MMT, string filename = "")
        {
            LogEvent("UpdateETA");

            shopify = new Shopify();
            Supplier supplier = SupplierProducer.CreateSupplier(sType, filename);

            try
            {
                var tasks = new List<Task>();

                tasks.Add(Download_Shopify(new string[] { "collection_id=" + supplier.CollectionID }));
                tasks.Add(supplier.LoadProducts());

                LogStr("Downloading products...", true);
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                LogEx(ex);
            }

            LogStr("Starting ETA Metafield Update", true);
            
            bool result = false;
            Shopify_Product shop_prod;

            foreach (Product prod in supplier.Products)
            {
                try
                {
                    shop_prod = shopify.MatchProductBySupplier(prod);

                    if (shop_prod != null)
                    {
                        result = await shopify.Update_Availability(shop_prod, prod);

                        if (result)
                        {
                            // only log if updated
                            Dictionary<string, string> props = new Dictionary<string, string>();
                            props.Add("SKU", prod.SKU);
                            props.Add("Title", prod.Title);
                            LogEvent("UpdateETA", props);
                        }

                        LogStr(DateTime.Now + "," + prod.SKU + "," + prod.Available + " - " + prod.ETA.ToString() + " - " + prod.Status + ": Updated product = " + result);
                    }
                }
                catch (Exception ex)
                {
                    //LogStr(DateTime.Now + "," + prod.SKU + "," + ex.Message);

                    Dictionary<string, string> props = new Dictionary<string, string>();
                    props.Add("SKU", prod.SKU);
                    props.Add("Title", prod.Title);
                    LogEx(ex, props);
                }
            }

            LogStr("Finished ETA Metafield Update", true);

            return true;
        }

        // Free Shipping is done via tags and not updated via this method on a scheduled basis
        // This function doesn't include any update to latest supplier/product code

            /*
        public async Task<bool> UpdateFreeShipping()
        {
            if (await Download_Shopify())
            {
                LogStr("Starting Free Shipping Update", true);
                //string result = "";

                foreach (Shopify_Product sprod in shopify.products)
                {
                    double sprod_price = Convert.ToDouble(sprod.Variants.FirstOrDefault().price);
                    if (sprod_price > 490)
                    {

                        //update tags
                        try
                        {
                            if (!sprod.Tags.Contains("Free_Shipping"))
                            {
                                await shopify.updatetags(sprod.id, sprod.Tags + ", Free_Shipping");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogStr("Error updating tags for " + sprod.id.ToString() + " - " + ex.Message);
                        }

                        try
                        {

                            //update inventory item location
                            InventoryItemElement inv = await shopify.Get_InventoryItem(sprod.variants[0].inventory_item_id.ToString());

                            //get inv location
                            InventoryLevels inv_levels = await shopify.Get_InventoryLevelsList(inv.Id);
                            if (inv_levels.Levels.Count == 1)
                            {
                                //if item is based in MMT then change to Free Shipping
                                if (inv_levels.Levels[0].LocationId == 41088974985)
                                {
                                    bool update_resp = await shopify.Set_InventoryItemLocation(inv.Id, 44811321481);
                                    bool delete_resp = await shopify.Delete_InventoryItemLocation(inv.Id, inv_levels.Levels[0].LocationId);
                                    if (update_resp & delete_resp)
                                    {
                                        LogStr("Updated item " + sprod.id + " Successfully");
                                    }
                                    else
                                    {
                                        LogStr("Updated of item " + sprod.id + " failed");
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            LogStr("Error updating inventory location for " + sprod.id.ToString() + " - " + ex.Message);
                        }

                    }

                }
                LogStr("Finished Free Shipping Update", true);
            }

            return true;
        }
        */


        public async Task<bool> Download_Shopify(string[] querystrings = null, bool images = false, bool include_unpublished = false)
        {
            bool retval = false;

            try
            {
                LogStr("Processing Shopify Download", true);

                shopify = new Shopify();

                bool shopify_download = await shopify.getallproducts(querystrings, images, false, include_unpublished);

                if ((shopify.products.Count > 0) & (shopify_download))
                {
                    LogStr("Shopify Downloaded Completed", true);
                    LogStr("Successful download - " + shopify.products.Count() + " items loaded", true);

                    retval = true;
                }
                else
                {
                    LogStr("Shopify Download returned no results");
                }
            }
            catch (Exception ex)
            {
                LogEx(ex);
            }
            return retval;
        }

        private bool Get_mstore_stock()
        {
            mstore_stock = new List<string>();
            mstore_stock.AddRange(File.ReadLines(mstore_stock_file));

            if (mstore_stock.Count < 1)
                return false;
            else
            {
                for (int i = 0; i < mstore_stock.Count; i++)
                {
                    mstore_stock[i] = mstore_stock[i].ToLower();
                }

                return true;
            }
        }



        /// <summary>
        /// Updates Shipping Tag with new supplier tag and
        /// updates tags on shopify if requested.
        /// Returns string with updated tags
        /// </summary>
        /// <param name="matcheditem"></param>
        /// <param name="supplier_Tag"></param>
        /// <returns>Tag string with update Shipping tag</returns>
        public async Task<string> UpdateShippingTag(Shopify_Product matcheditem, string supplier_Tag, bool updateShopify)
        {
            string newtags;

            string[] tags = matcheditem.Tags.Split(',');

            int ship_index = Array.FindIndex(tags, FindShippingTag);

            if (ship_index > -1)
            {
                tags[ship_index] = supplier_Tag;
            }
            else
            {
                tags[0] = supplier_Tag;
            }

            newtags = String.Join(",", tags);

            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add("SKU", matcheditem.Variants.FirstOrDefault().Sku);
            props.Add("Title", matcheditem.Title);
            props.Add("OldTags", matcheditem.Tags);
            props.Add("NewTags", newtags);

            if (updateShopify)
            {
                bool UpdateTagsSuccess = await shopify.UpdateTags(matcheditem.Id, newtags);

                if (UpdateTagsSuccess)
                {
                    LogEvent("UpdateShippingTag", props, null);
                }
                else
                {
                    LogEvent("UpdateShippingTag_Error", props, null);
                }

            }
           
            return newtags;
        }

        private static bool FindShippingTag(string s)
        {
            if (s.ToLower().Contains("shipping"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void LogStr(string v1)
        {
            LogStr(v1, false);
        }

        private void LogStr(string v1, bool v2, int thres = 0)
        {
            OnNotify(new NotifyEventArgs { ConsoleOnly = v2, Message = v1, Threshold = thres });
        }

        protected virtual void OnNotify(NotifyEventArgs e)
        {
            Notify?.Invoke(this, e);
        }


        private void LogEx(Exception ex, Dictionary<string,string> properties = null, Dictionary<string,double> metrics = null)
        {
            OnException(new NotifyExceptionEventArgs { NException = ex, Properties = properties, Metrics = metrics });
        }

        protected virtual void OnException(NotifyExceptionEventArgs e)
        {
            Exception?.Invoke(this, e);
        }

        private void LogEvent(string eventname, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            OnProductEvent(new NotifyProductEventArgs { EventName = eventname, Properties = properties, Metrics = metrics } );
        }

        protected virtual void OnProductEvent(NotifyProductEventArgs e)
        {
            ProductEvent?.Invoke(this, e);
        }

    }
}
