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

namespace backoffice
{
    public class NotifyEventArgs : EventArgs
    {
        public int Threshold { get; set; }
        public string Message { get; set; }
        public bool ConsoleOnly { get; set; }
    }




    public class MBot
    {
        const string ClearanceTag = "Clearance";

        private List<string> mstore_stock;
        static string mstore_stock_file;

        public Shopify shopify;
        public bool verbose = false;

        public event EventHandler<NotifyEventArgs> Notify;

        public MBot(string stock_file)
        {
            mstore_stock_file = stock_file;

        }

        public async Task<int> Test()
        {
            shopify = new Shopify();

            while (shopify.Location_Status != Location_Status_Enum.Loaded)
            {
                Thread.Sleep(500);
                LogStr("Waiting for Location Load - " + shopify.Location_Status.ToString());
            }

            foreach(Location loc in shopify.Locations)
            {
                LogStr(loc.Id + " - " + loc.Name + " - " + loc.City);

            }

            Shopify_Product prod = await shopify.GetProduct("4563526713481");

            Prod_Availability retval = await shopify.GetAvailability(prod.Id);
            //GetMetafields retval = await shopify.GetAvailability(prod.Id);
            LogStr(retval.ToString());              
        

            return 0;
        }


        public async Task<bool> ProcessDataFile(string filename)
        {
            if (shopify == null)
            {
                shopify = new Shopify();
                await Download_Shopify();
            }

            string prod_eta = "";
            string prod_status = "";
            int counter = 0;
            double available = 0;

            TechDataSupplier FileSupplier = new TechDataSupplier(filename);
            FileSupplier.Filename = filename;

            int count = await FileSupplier.LoadProducts();

            //TODO: THIS IS UNFINISHED!!!!!

            return true;
        }


        private bool IsProductMatch(Shopify_Product s_product, Product product)
        {
            bool match = false;

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

            return match;
        }

        /// <summary>
        /// Updates Item ETA by Shopify Item number
        /// </summary>
        /// <param name="itemnumber"></param>
        /// <param name="eta"></param>
        /// <param name="available"></param>
        /// <param name="status"></param>
        public void UpdateItemETA(string itemnumber, string eta, string available, string status)
        {

            if (shopify == null)
            {
                shopify = new Shopify();
            }

            string result = shopify.update_availability(itemnumber, available, eta, true, "", status).GetAwaiter().GetResult();

            LogStr(result);
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
            shopify = new Shopify();

            try
            {
                LogStr("Downloading Product Lists - Async", true);

                //var mmt_download = Download_MMTAsync(MMTDownloadType.Clearance);
                Supplier mmt = new MMTSupplier();
                ((MMTSupplier)mmt).DownloadType = backoffice.MMTDownloadType.Clearance;

                var mmt_download = mmt.LoadProducts();
                var shopify_download = Download_Shopify(new string[] { "collection_id=182307225737" });

                await Task.WhenAll(mmt_download, shopify_download);
                LogStr("Updating Clearance...", true);


                //MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];

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

                    if (match)
                    {
                        //updateETA and stuff
                        if (mmt_prod_match.Status == null)
                            UpdateItemETA(sprod.Id.ToString(), mmt_prod_match.ETA.ToString(), mmt_prod_match.Available.ToString(), "Clearance");
                        else
                            UpdateItemETA(sprod.Id.ToString(), mmt_prod_match.ETA.ToString(), mmt_prod_match.Available.ToString(), mmt_prod_match.Status);
                        LogStr(String.Format(@"""{0}"", Item in list updated eta", sprod.Id.ToString()));
                    }
                    else
                    {
                        sprod.Tags = RemoveClearanceTag(sprod.Tags);
                        await shopify.updatetags(sprod.Id, sprod.Tags);
                        LogStr(String.Format(@"""{0}"", Item removed from list", sprod.Id.ToString()));

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
                        await shopify.updatetags(sprod_match.Id, sprod_match.Tags);
                        LogStr(String.Format(@"""{0}"", Item added to clearance collection", sprod_match.Id.ToString()));
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
        public async Task<bool> FindUnmatched(SupplierType sType = SupplierType.MMT)
        {
            shopify = new Shopify();
            Supplier supplier = SupplierProducer.CreateSupplier(sType);

            try
            {

                var supplier_download = supplier.LoadProducts();

                var shopify_download = Download_Shopify(new string[] { });

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

                        if (!match)
                        {
                            nomatchcount++;
                            if (product.Variants == null)
                            {
                                LogStr(String.Format(@"""{0}"",""{1}"",""{2}""", product.Handle.ToLower(), product.Title, "null"));
                            }
                            else
                            {
                                if (await shopify.unpublishitem(product.Id))
                                {
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"" - Successfully unpublished", product.Handle.ToLower(), product.Title, product.Variants[0].Sku));
                                }
                                else
                                {
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"" - Error unpublishing", product.Handle.ToLower(), product.Title, product.Variants[0].Sku));
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


        /// <summary>
        /// Updates pricing for supplier based on a lot of complex assumptions
        /// </summary>
        /// <param name="DownloadType"></param>
        /// <returns></returns>
        public  async Task<bool> UpdatePricing(SupplierType sType = SupplierType.MMT, string filename = "")
        {

            shopify = new Shopify();
            Supplier supplier = SupplierProducer.CreateSupplier(sType, filename);

            try
            {

                var supplier_download = supplier.LoadProducts();
                var shopify_download = Download_Shopify();

                await Task.WhenAll(supplier_download, shopify_download);
                LogStr("Updating Pricing...", true);
            }
            catch (Exception ex)
            {
                LogStr(ex.Message, true);
            }
           
            bool match = false;
            string new_price = "";

            Shopify_Product matcheditem;

            // Download associated inventory items so you can get cost price etc
            // shopify makes this a two part process due to I expect variants or something

            Dictionary<string, string> inv_ids = new Dictionary<string, string>();
            Dictionary<string, InventoryItem> invList = new Dictionary<string, InventoryItem>();
            InventoryItems inv_items;
            string inv_uri = "";

            foreach (Shopify_Product s_prod in shopify.products)
            {
                inv_ids.Add(s_prod.Handle, s_prod.Variants.FirstOrDefault().InventoryItemId.ToString());
                if (inv_ids.Count > 100)
                {
                    LogStr("Retreiving 100 inventory items", true);
                    foreach (string inv_id in inv_ids.Values)
                    {
                        inv_uri += "," + inv_id;
                    }

                    inv_uri = inv_uri.Substring(1);

                    inv_items = await shopify.Get_InventoryItemList(inv_uri);
                    foreach (InventoryItem inv_item in inv_items.Items)
                    {
                        invList.Add(inv_item.Id.ToString(), inv_item);
                    }

                    inv_ids.Clear();
                    inv_uri = "";

                    LogStr("Retreived " + invList.Count() + " total inventory items", true);
                }
            }

            if (inv_ids.Count > 0)
            {
                LogStr("Retreiving remaining inventory items", true);
                foreach (string inv_id in inv_ids.Values)
                {
                    inv_uri += "," + inv_id;
                }

                inv_uri = inv_uri.Substring(1);

                inv_items = await shopify.Get_InventoryItemList(inv_uri);
                foreach (InventoryItem inv_item in inv_items.Items)
                {
                    invList.Add(inv_item.Id.ToString(), inv_item);
                }

                inv_ids.Clear();
                inv_uri = "";

                LogStr("Retreived " + invList.Count() + " total inventory items", true);
            }


            LogStr("Matching items", true);
            bool update_price = false;
            bool update_cost = false;
            bool force_cost_eval = false;
            bool force_eta_update = false;

            double comp_cost_price = 0;

            foreach (Product supplier_prod in supplier.Products)
            {
                try
                {
                    matcheditem = shopify.MatchProductByMMT(supplier_prod.SKU, supplier_prod.SKU);

                    if (matcheditem != null)
                    {

                        //InventoryItemElement inv = await shopify.Get_InventoryItem(matcheditem.variants.FirstOrDefault().inventory_item_id.ToString());
                        InventoryItem inv = null;

                        if (invList.ContainsKey(matcheditem.Variants.FirstOrDefault().InventoryItemId.ToString()))
                        {
                            inv = invList[matcheditem.Variants.FirstOrDefault().InventoryItemId.ToString()];
                        }

                        if (inv != null)
                        {
                            /*  You have MMT Product
                                *  You have Shopify product and cost price
                                *  
                                *  So now test if RRPBuy and Cost Price are the same
                                *  
                                *  If they are, then nothing needs to be done
                                *  If they aren't then we need to adjust pricing
                            */



                            /*  New Section for products with multiple distributors
                             *  
                             *  Check if Tags show product is matched to supplier
                             *  if it is then proceed as usual - no code included here
                             *  
                             *  if it's notthen we need to evaulate if it should be switched.
                             *  
                             *  throw it to new method to check processing for that and decide if we should switch
                             *  supplier and contine price processing
                             * 
                             * 
                             */
                            if (!Match_Supplier_Product_Tag(matcheditem.Tags, supplier.Supplier_Tag))
                            {
                                if (await Evaluate_Product_Supplier_Change(matcheditem, inv, supplier_prod))
                                {
                                    force_cost_eval = true;
                                    force_eta_update = true;
                                    bool change_result = await shopify.Change_InventoryLocation(inv.Id, supplier.Supplier_Location_Id);

                                }
                            }


                            update_price = false;
                            update_cost = false;


                            if (matcheditem.Variants.FirstOrDefault().CompareAtPrice.ToString() != supplier_prod.RRPPrice.ToShopify())
                            {
                                update_price = true;
                            }

                            if (matcheditem.Variants.FirstOrDefault().Taxable)
                                comp_cost_price = supplier_prod.CostPrice;
                            else
                                comp_cost_price = shopify.addgst(supplier_prod.CostPrice);


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

                            if (update_price || update_cost || force_cost_eval)
                            {
                                //LogStr(String.Format(@"""{0}"",""{1}"",""{2}"", Match Found, Price Not Equal", product.handle, product.title, supplier_prod.Manufacturer[0].ManufacturerCode));

                                //generate new price
                                try
                                {
                                    if (matcheditem.Variants[0].Taxable)
                                        new_price = shopify.createnewprice(inv.Cost, matcheditem.Variants.FirstOrDefault().CompareAtPrice.ToString(), matcheditem.Variants.FirstOrDefault().Price, supplier_prod.CostPrice.ToShopify(), supplier_prod.RRPPrice.ToShopify(), false, true, true);
                                    else
                                        new_price = shopify.createnewprice(inv.Cost, matcheditem.Variants.FirstOrDefault().CompareAtPrice.ToString(), matcheditem.Variants.FirstOrDefault().Price, supplier_prod.CostPrice.ToShopify(), supplier_prod.RRPPrice.ToShopify(), false, true, false);
                                }
                                catch (Exception ex)
                                {
                                    string[] formatlistex = { matcheditem.Handle, matcheditem.Title, comp_cost_price.ToString(), supplier_prod.RRPPrice.ToShopify(), ex.Message };
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"", ""Match Found. Price Not Equal. Error creating New Price"", ""{4}""", formatlistex));
                                    new_price = "0";
                                }


                                if (new_price != "0")
                                {

                                    //Create formatlist for logging purposes
                                    string[] formatlist = { matcheditem.Handle, matcheditem.Title, new_price, comp_cost_price.ToShopify(), supplier_prod.RRPPrice.ToShopify() };
                                    //LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"",""{4}"", Match Found Price Not Equal Creating New Price", formatlist), true);

                                    //now you have a new price update all pricing in shopify
                                    try
                                    {

                                        if (matcheditem.Variants[0].Taxable)
                                            await shopify.updateprice(inv.Id, matcheditem.Variants.FirstOrDefault().Id, supplier_prod.CostPrice.ToShopify(), new_price, supplier_prod.RRPPrice.ToShopify());
                                        else
                                            await shopify.updateprice(inv.Id, matcheditem.Variants.FirstOrDefault().Id, shopify.addgst(supplier_prod.CostPrice.ToShopify()), new_price, supplier_prod.RRPPrice.ToShopify());

                                        LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"",""{4}"", Match Found. Price Not Equal. Created New Price", formatlist));

                                    }
                                    catch (Exception ex)
                                    {
                                        string[] formatlistex = { matcheditem.Handle, matcheditem.Title, comp_cost_price.ToString(), supplier_prod.RRPPrice.ToShopify(), ex.Message };
                                        LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"", ""Match Found. Price Not Equal. Error creating uploading new pricing"", ""{4}""", formatlistex));
                                    }

                                }
                                else
                                {
                                    string[] formatlistex = { matcheditem.Handle, matcheditem.Title, comp_cost_price.ToString(), supplier_prod.RRPPrice.ToShopify() };
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"", ""Match Found. Price Not Equal. New price is 0", formatlistex));
                                }

                                if (force_eta_update)
                                {
                                    UpdateItemETA(matcheditem.Id.ToString(), supplier_prod.ETA.ToString(), supplier_prod.Available.ToString(), supplier_prod.Status);
                                }
                            }
                            else
                            {
                                //                                LogStr(String.Format(@"""{0}"",""{1}"",""{2}"", Match Found, Price Equal", matcheditem.handle, matcheditem.title, supplier_prod.Manufacturer[0].ManufacturerCode));
                            }
                        }

                        shopify.products.Remove(matcheditem);
                    }
                    else
                    {
                        string[] formatlistex = { supplier_prod.SKU, supplier_prod.Title, supplier_prod.CostPrice.ToShopify() };
                        LogStr(String.Format(@"""{0}"",""{1}"",""{2}"", ""No Match Found.", formatlistex));
                    }
                }
                catch (Exception ex)
                {
                    LogStr("Error matching product - " + supplier_prod.SKU + "  Moving to next item");
                }

            }

            return true;
        }

        private async Task<bool> Evaluate_Product_Supplier_Change(Shopify_Product matcheditem, InventoryItem inv, Product sprod)
        {
            /*  Item is matched in pricing method, but currently not supplied by this supplier
             *  
             *  So evaluate if it should be based on stock availability and price        * 
             *              * 
             */
            
            bool retval = false;

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

            return retval;
        }

        private bool Match_Supplier_Product_Tag(string tags, string supplier_Tag)
        {
            bool retval = false;
            tags = tags.ToLower();

            if (tags.Contains(supplier_Tag.ToLower()))
                retval = true;

            return retval;
        }

        public async Task<bool> UpdateETA(SupplierType sType = SupplierType.MMT)
        {


            shopify = new Shopify();
            Supplier supplier = SupplierProducer.CreateSupplier(sType);

            try
            {

                var supplier_download = supplier.LoadProducts();
                var shopify_download = Download_Shopify();

                await Task.WhenAll(supplier_download, shopify_download);
                LogStr("Downloading products...", true);
            }
            catch (Exception ex)
            {
                LogStr(ex.Message, true);
            }

            LogStr("Starting ETA Metafield Update", true);
            string result = "";
            string tags = "";

            foreach (Product prod in supplier.Products)
            {
                try
                {
                    if (prod.Vendor != null)
                    {
                        if (prod.Status == null)
                            result = await shopify.update_availability(prod.SKU, prod.Available.ToString(), prod.ETA.ToString(), false, prod.SKU, "");
                        else
                            result = await shopify.update_availability(prod.SKU, prod.Available.ToString(), prod.ETA.ToString(), false, prod.SKU, prod.Status);

                        LogStr(DateTime.Now + "," + prod.SKU + "," + result);
                    }
                }
                catch (Exception ex)
                {
                    LogStr(DateTime.Now + "," + prod.SKU + "," + ex.Message);
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


        public async Task<bool> Download_Shopify(string[] querystrings = null, bool images = false)
        {
            bool retval = false;

            LogStr("Processing Shopify Download", true);

            shopify = new Shopify();

            bool shopify_download = await shopify.getallproducts(querystrings, images);

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

        /*
        private void Update_Metafields()
        {
            MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];

            LogStr("Starting ETA Metafield Update", true);

            foreach (MMTPriceListProductsProduct prod in mmtproducts.Product)
            {
                string result = shopify.update_availability(prod.Manufacturer[0].ManufacturerCode, prod.Availability, prod.ETA).GetAwaiter().GetResult();
                LogStr(DateTime.Now + "," + prod.Manufacturer[0].ManufacturerCode + "," + result);
            }

            LogStr("Finished ETA Metafield Update", true);

        }
        */

        private void LogStr(string v1)
        {
            LogStr(v1, false);
        }

        private void LogStr(string v1, bool v2)
        {
            OnNotify(new NotifyEventArgs { ConsoleOnly = v2, Message = v1, Threshold = 0 });
        }

        protected virtual void OnNotify(NotifyEventArgs e)
        {
            Notify?.Invoke(this, e);
        }

    }
}
