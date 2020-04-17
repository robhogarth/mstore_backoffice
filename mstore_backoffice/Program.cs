using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backoffice;
using System.IO;

namespace mstore_backoffice
{

    enum MMTDownloadType : int
    {
        Standard = 0,
        Clearance = 1
    }
    class Program
    {

        static MMTPriceList pricelist;

        static string[] mmtdatafeed = {"https://www.mmt.com.au/datafeed/index.php?lt=s&ft=xml&tk=94M0C1O223NF7AI59BS94903AC004E0B4A%20D09%2083A%2046B%20D80%20648%2031F%2075D%20665F9461C558F25AE&af[]=et&af[]=st", "https://www.mmt.com.au/datafeed/index.php?lt=c&ft=xml&tk=94M0C1O223NF7AI59BS94903AC004E0B4A%20D09%2083A%2046B%20D80%20648%2031F%2075D%20665F9461C558F25AE&af[]=et&af[]=st"};

        static Shopify_Products shopify;

        static string[] tasklist = { "updateeta", "updatepricing", "findunmatched", "updateitemeta", "updateclearance", "addsku", "updatefreeshipping" };

        const string taskarg_prefix = "/task:";
        const string logarg_prefix = "/log:";
        const string logdate_prefix = "/filedatesuffix";
        const string verbose_prefix = "/verbose";
        const string item_prefix = "/item:";
        const string itemeta_prefix = "/eta:";
        const string itemavailable_prefix = "/available:";
        const string itemstatus_prefix = "/itemstatus:";

        static string logfilename = "";
        static StreamWriter logfile;
        static bool addlogdate = false;
        static bool verbose = false;

        static string itemnumber = "";
        static string itemeta = "";
        static string itemavailable = "";
        static string itemstatus = "";

        static string ClearanceTag = "Clearance";

        /* Commandline args:
         *    /task:[updateETA|updatepricing|findunmatched|updateitemeta|updateclearance]
         *    /log:<logfilename>   - optional
         *    /filedatsuffic       - add date and time to log file name
         *    /verbose             - more logging, adds readkey to end of main()
         *    /item:<itemnumber>   - item for updateitemeta
         *    /eta:<eta>           - eta for item string format displayed exactly like this on website
         *    /available:<count>   - number of items available
         *    /itemstatus:<status> - status of item availability
         */

        static void Main(string[] args)
        {
            string taskoption = "";

            foreach (string arg in args)
            {
                if (arg.ToLower().StartsWith(taskarg_prefix))
                {
                    taskoption = arg.ToLower().Substring(taskarg_prefix.Length);
                }

                if (arg.ToLower().StartsWith(logarg_prefix))
                {
                    logfilename = arg.ToLower().Substring(logarg_prefix.Length);
                }

                if (arg.ToLower().StartsWith(item_prefix))
                {
                    itemnumber = arg.ToLower().Substring(item_prefix.Length);
                }

                if (arg.ToLower().StartsWith(itemeta_prefix))
                {
                    itemeta = arg.ToLower().Substring(itemeta_prefix.Length);
                }

                if (arg.ToLower().StartsWith(itemavailable_prefix))
                {
                    itemavailable = arg.ToLower().Substring(itemavailable_prefix.Length);
                }

                if (arg.ToLower().StartsWith(itemstatus_prefix))
                {
                    itemstatus = arg.ToLower().Substring(itemstatus_prefix.Length);
                }


                if (arg.ToLower() == logdate_prefix)
                {
                    addlogdate = true;
                }

                if (arg.ToLower() == verbose_prefix)
                {
                    verbose = true;
                }


            }

            if (logfilename != "")
            {
                if (addlogdate)
                {
                    logfilename = logfilename.Substring(0, logfilename.LastIndexOf(".")) + "_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute + logfilename.Substring(logfilename.LastIndexOf("."));
                }

                logfile = File.AppendText(logfilename);
                logfile.AutoFlush = true;

            }


            if (taskoption != "")
            {
                if (verbose)
                {
                    LogStr("Task Option is: " + taskoption);
                }

                switch (taskoption)
                {
                    case "updateeta":
                        UpdateETA().Wait();
                        break;
                    case "updatepricing":
                        UpdatePricing().Wait();
                        break;
                    case "findunmatched":
                        FindUnmatched().Wait();
                        break;
                    case "updateitemeta":
                        UpdateItemETA(itemnumber, itemeta, itemavailable, itemstatus);
                        break;
                    case "updateclearance":
                        UpdateClearance().Wait();
                        break;
                    case "addsku":
                        AddSKU().Wait();
                        break;
                    case "updatefreeshipping":
                        UpdateFreeShipping().Wait();
                        break;                        
                    default:
                        LogStr("Cannot Match task: " + taskoption);
                        LogStr("Ending");
                        break;
                }

            }
            else
            {
                LogStr("No Option detected.  Ending");
            }

            LogStr("Completed Processing.  Ending");

            if (verbose) { Console.ReadKey(); }

            if (logfilename != "")
            {
                logfile.Close();
            }
        }
       
        
        private static bool IsProductMatch(Shopify_Product s_product, MMTPriceListProductsProduct mmt_prod)
        {
            bool match = false;

            if (s_product.handle.ToLower() == mmt_prod.Manufacturer[0].ManufacturerCode.ToLower())
            {
                match = true;
            }
            else
            {
                if (s_product.variants.FirstOrDefault().sku.ToLower() == mmt_prod.Manufacturer[0].ManufacturerCode.ToLower())
                {
                    match = true;
                }
            }

            return match;
        }
                     
        private static void UpdateItemETA(string itemnumber, string eta, string available, string status)
        {

            if (shopify == null)
            {
                shopify = new Shopify_Products();
            }

            string result = shopify.update_availability(itemnumber, available, eta, true, "", status).GetAwaiter().GetResult();
        }

        public static async Task<bool> UpdateClearance()
        {
            shopify = new Shopify_Products();
           
            try
            {
                LogStr("Downloading Product Lists - Async", true);
                var mmt_download = Download_MMTAsync(MMTDownloadType.Clearance);
                var shopify_download = Download_Shopify(new string[] { "collection_id=182307225737" });

                await Task.WhenAll(mmt_download, shopify_download);
                LogStr("Updating Clearance...", true);

                MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];

                bool match = false;
                MMTPriceListProductsProduct mmt_prod_match = new MMTPriceListProductsProduct();

                //check all shopify items against clearance
                //if item exists - update ETA or something for good measure
                //else remove from list
                foreach (Shopify_Product sprod in shopify.products)
                {
                    match = false;

                    foreach (MMTPriceListProductsProduct mmt_prod in mmtproducts.Product)
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
                        UpdateItemETA(sprod.id.ToString(), mmt_prod_match.ETA, mmt_prod_match.Availability, mmt_prod_match.Status[0].StatusName);
                        LogStr(String.Format(@"""{0}"", Item in list updated eta", sprod.id.ToString()));
                    }
                    else
                    {
                        sprod.tags = RemoveClearanceTag(sprod.tags);
                        await shopify.updatetags(sprod.id, sprod.tags);
                        LogStr(String.Format(@"""{0}"", Item removed from list", sprod.id.ToString()));
                        
                    }
                }

                Shopify_Product sprod_match = new Shopify_Product();

                List<MMTPriceListProductsProduct> AddList = new List<MMTPriceListProductsProduct>();

                foreach (MMTPriceListProductsProduct mmt_prod in mmtproducts.Product)
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
                        LogStr(String.Format(@"""{0}"", Item added to list", mmt_prod.Manufacturer[0].ManufacturerCode));
                    }
                }


                LogStr("Getting list of all shopify items");

                

                await shopify.getallproducts();

                LogStr("Matching MMT products to Shopify to add clearance tags");
                
                foreach(MMTPriceListProductsProduct mmt_prod in AddList)
                {
                    match = false;
                    foreach (Shopify_Product sprod in shopify.products)
                    {
                        if(IsProductMatch(sprod,mmt_prod))
                        {
                            match = true;
                            sprod_match = sprod;
                            break;
                        }
                    }

                    if (match)
                    {
                        sprod_match.tags = AddClearanceTag(sprod_match.tags);
                        await shopify.updatetags(sprod_match.id, sprod_match.tags);
                        LogStr(String.Format(@"""{0}"", Item added to clearnce collection", sprod_match.id.ToString()));
                    }
                    else
                    {
                        LogStr(String.Format(@"""{0}"", Item didn't find a shopify match", sprod_match.id.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                LogStr(ex.Message, true);
            }

            return true;
        }

        private static string AddClearanceTag(string tags)
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

        private static string RemoveClearanceTag(string tags)
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

        public static async Task<bool> FindUnmatched()
        {
            shopify = new Shopify_Products();

            try
            {
                var mmt_download = Download_MMTAsync();
                var shopify_download = Download_Shopify();

                await Task.WhenAll(mmt_download, shopify_download);
                LogStr("Updating Unmatched Items...", true);
            }
            catch (Exception ex)
            {
                LogStr(ex.Message, true);
            }

            try
            {

                int nomatchcount = 0;
                MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];
                foreach (Shopify_Product product in shopify.products)
                {

                    if (product.vendor != "Sangoma")
                    {
                        bool match = false;

                        if (shopify.MatchProductByShopify(product.handle, product.variants.FirstOrDefault().sku, mmtproducts) != null)
                        { 
                            match = true;
                        }
                                                                
                        if (!match)
                        {
                            nomatchcount++;
                            if (product.variants == null)
                            {
                                LogStr(String.Format(@"""{0}"",""{1}"",""{2}""", product.handle.ToLower(), product.title, "null"));
                            }
                            else
                            {
                                if (await shopify.unpublishitem(product.id))
                                {
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"" - Successfully unpublished", product.handle.ToLower(), product.title, product.variants[0].sku));
                                }
                                else
                                {
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"" - Error unpublishing", product.handle.ToLower(), product.title, product.variants[0].sku));
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

        public static async Task<bool> AddSKU()
        {
            shopify = new Shopify_Products();

            try
            {
                var mmt_download = Download_MMTAsync();
                var shopify_download = Download_Shopify();

                await Task.WhenAll(mmt_download, shopify_download);
                LogStr("Updating Pricing...", true);
            }
            catch (Exception ex)
            {
                LogStr(ex.Message, true);
            }

            MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];

            foreach (MMTPriceListProductsProduct mmt_prod in mmtproducts.Product)
            {
                try
                {
                    Shopify_Product sprod = shopify.MatchProductByMMT(mmt_prod.Manufacturer[0].ManufacturerCode, mmt_prod.Manufacturer[0].ManufacturerCode);
                    if (sprod.variants[0].sku == "")
                    {
                        if (await shopify.updatesku(sprod.variants[0].id, mmt_prod.Manufacturer[0].ManufacturerCode))
                        {
                            LogStr(sprod.id + " Successfully updates SKU to - " + mmt_prod.Manufacturer[0].ManufacturerCode);
                        }
                        else
                        {
                            LogStr(sprod.id + " Error updating SKU to - " + mmt_prod.Manufacturer[0].ManufacturerCode);
                        }
                    }
                    else
                        LogStr(sprod.id + " already set to - " + sprod.variants[0].sku);
                }
                catch (Exception ex)
                {
                    LogStr("Error processing " + mmt_prod.Manufacturer[0].ManufacturerCode + ": " + ex.Message);
                }
            }

            return true;
        }

        public static async Task<bool> UpdatePricing()
        {

            shopify = new Shopify_Products();

            try
            {
                var mmt_download = Download_MMTAsync();
                //var shopify_download = Download_Shopify(new string[] { "ids=4563433914505" } );
                var shopify_download = Download_Shopify();

                await Task.WhenAll(mmt_download, shopify_download);
                LogStr("Updating Pricing...", true);
            }
            catch (Exception ex)
            {
                LogStr(ex.Message, true);
            }

            MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];
            
            bool match = false;
            string new_price = "";
                    
            Shopify_Product matcheditem;


            Dictionary<string, string> inv_ids = new Dictionary<string, string>();
            Dictionary<string, InventoryItemElement> invList = new Dictionary<string, InventoryItemElement>();
            InventoryItem inv_items;
            string inv_uri = "";

            foreach (Shopify_Product s_prod in shopify.products)
            {
                inv_ids.Add(s_prod.handle, s_prod.variants.FirstOrDefault().inventory_item_id.ToString());
                if (inv_ids.Count > 100)
                {
                    LogStr("Retreiving 100 inventory items", true);
                    foreach (string inv_id in inv_ids.Values)
                    {
                        inv_uri += "," + inv_id;
                    }

                    inv_uri = inv_uri.Substring(1);

                    inv_items = await shopify.Get_InventoryItemList(inv_uri);
                    foreach (InventoryItemElement inv_item in inv_items.InventoryItems)
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
                foreach (InventoryItemElement inv_item in inv_items.InventoryItems)
                {
                    invList.Add(inv_item.Id.ToString(), inv_item);
                }

                inv_ids.Clear();
                inv_uri = "";

                LogStr("Retreived " + invList.Count() + " total inventory items", true);
            }


            LogStr("Matching items", true);
            bool update_price = false;

            foreach (MMTPriceListProductsProduct mmt_prod in mmtproducts.Product)
            {
                matcheditem = shopify.MatchProductByMMT(mmt_prod.Manufacturer[0].ManufacturerCode, mmt_prod.Manufacturer[0].ManufacturerCode);

                if (matcheditem != null)
                {
                    //InventoryItemElement inv = await shopify.Get_InventoryItem(matcheditem.variants.FirstOrDefault().inventory_item_id.ToString());
                    InventoryItemElement inv = null;

                    if (invList.ContainsKey(matcheditem.variants.FirstOrDefault().inventory_item_id.ToString()))
                    {
                        inv = invList[matcheditem.variants.FirstOrDefault().inventory_item_id.ToString()];
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

                        update_price = false;
                        if (matcheditem.variants.FirstOrDefault().taxable)
                        {
                            if ((matcheditem.variants.FirstOrDefault().compare_at_price != mmt_prod.Pricing[0].RRPInc) || (inv.Cost != mmt_prod.Pricing[0].YourPrice))
                                update_price = true;
                        }
                        else
                        {
                            if ((matcheditem.variants.FirstOrDefault().compare_at_price != mmt_prod.Pricing[0].RRPInc) || (inv.Cost != shopify.addgst(mmt_prod.Pricing[0].YourPrice)))
                                update_price = true;
                        }

                        if (update_price)
                        {
                            //LogStr(String.Format(@"""{0}"",""{1}"",""{2}"", Match Found, Price Not Equal", product.handle, product.title, mmt_prod.Manufacturer[0].ManufacturerCode));

                            //generate new price
                            try
                            {
                                if (matcheditem.variants[0].taxable)
                                    new_price = shopify.createnewprice(inv.Cost, matcheditem.variants.FirstOrDefault().compare_at_price, matcheditem.variants.FirstOrDefault().price, mmt_prod.Pricing[0].YourPrice, mmt_prod.Pricing[0].RRPInc, false, true, true);
                                else
                                    new_price = shopify.createnewprice(inv.Cost, matcheditem.variants.FirstOrDefault().compare_at_price, matcheditem.variants.FirstOrDefault().price, mmt_prod.Pricing[0].YourPrice, mmt_prod.Pricing[0].RRPInc, false, true, false);
                            }
                            catch (Exception ex)
                            {
                                string[] formatlistex = { matcheditem.handle, matcheditem.title, mmt_prod.Pricing[0].YourPrice, mmt_prod.Pricing[0].RRPInc, ex.Message};
                                LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"", ""Match Found. Price Not Equal. Error creating New Price"", ""{4}""", formatlistex));
                                new_price = "0";
                            }


                            if (new_price != "0")
                            {

                                //Create formatlist for logging purposes
                                string[] formatlist = { matcheditem.handle, matcheditem.title, new_price, mmt_prod.Pricing[0].YourPrice, mmt_prod.Pricing[0].RRPInc };
                                //LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"",""{4}"", Match Found Price Not Equal Creating New Price", formatlist), true);

                                //now you have a new price update all pricing in shopify
                                try
                                {
                                    if (matcheditem.variants[0].taxable)
                                        await shopify.updateprice(inv.Id, matcheditem.variants.FirstOrDefault().id, mmt_prod.Pricing[0].YourPrice, new_price, mmt_prod.Pricing[0].RRPInc);
                                    else
                                        await shopify.updateprice(inv.Id, matcheditem.variants.FirstOrDefault().id, shopify.addgst(mmt_prod.Pricing[0].YourPrice), new_price, mmt_prod.Pricing[0].RRPInc);
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"",""{4}"", Match Found. Price Not Equal. Created New Price", formatlist));

                                }
                                catch (Exception ex)
                                {
                                    string[] formatlistex = { matcheditem.handle, matcheditem.title, mmt_prod.Pricing[0].YourPrice, mmt_prod.Pricing[0].RRPInc, ex.Message };
                                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"", ""Match Found. Price Not Equal. Error creating uploading new pricing"", ""{4}""", formatlistex));
                                }
                            }
                            else
                            {
                                string[] formatlistex = { matcheditem.handle, matcheditem.title, mmt_prod.Pricing[0].YourPrice, mmt_prod.Pricing[0].RRPInc};
                                LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"", ""Match Found. Price Not Equal. New price is 0", formatlistex));
                            }
                        }
                        else
                        {
                            LogStr(String.Format(@"""{0}"",""{1}"",""{2}"", Match Found, Price Equal", matcheditem.handle, matcheditem.title, mmt_prod.Manufacturer[0].ManufacturerCode));
                        }
                    }

                    shopify.products.Remove(matcheditem);
                }
                else
                {
                    string[] formatlistex = { mmt_prod.Manufacturer[0].ManufacturerCode, mmt_prod.Description[0].ShortDescription, mmt_prod.Pricing[0].YourPrice};
                    LogStr(String.Format(@"""{0}"",""{1}"",""{2}"", ""No Match Found.", formatlistex));
                }
            }

            return true;
        }

        public static async Task<bool> UpdateETA()
        {
            if (Download_MMT())
            {
                if(await Download_Shopify())
                {
                    MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];
                    LogStr("Starting ETA Metafield Update", true);
                    string result = "";

                    foreach (MMTPriceListProductsProduct prod in mmtproducts.Product)
                    {
                        try
                        {
                            result = await shopify.update_availability(prod.Manufacturer[0].ManufacturerCode, prod.Availability, prod.ETA, false, prod.Manufacturer[0].ManufacturerCode, prod.Status[0].StatusName);
                            LogStr(DateTime.Now + "," + prod.Manufacturer[0].ManufacturerCode + "," + result);
                        }
                        catch (Exception ex)
                        {
                            LogStr(DateTime.Now + "," + prod.Manufacturer[0].ManufacturerCode + "," + ex.Message);
                        }
                    }
                    LogStr("Finished ETA Metafield Update", true);
                }
            }

            return true;
        }

        public static async Task<bool> UpdateFreeShipping()
        {
            if (await Download_Shopify())
            {
                LogStr("Starting Free Shipping Update", true);
                //string result = "";

                foreach (Shopify_Product sprod in shopify.products)
                {
                    double sprod_price = Convert.ToDouble(sprod.variants.FirstOrDefault().price);
                    if (sprod_price > 490)
                    {
                        
                        //update tags
                        try
                        {
                            if (!sprod.tags.Contains("Free_Shipping"))
                            {
                                await shopify.updatetags(sprod.id, sprod.tags + ", Free_Shipping");
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


        public static bool Download_MMT(MMTDownloadType DownloadType = MMTDownloadType.Standard)
        {
            bool retval = false;

            LogStr("Processing MMT Download", true);

            pricelist = MMTPriceList.loadFromURL(mmtdatafeed[(int)DownloadType]);

            if (pricelist != null)
            {
                LogStr("Successful download",true);
                LogStr("Downloaded MMT " + ((MMTPriceListProducts)pricelist.Items[1]).Product.Count() + " items retreived.", true);
                retval = true;
            }
            else
            {
                LogStr("Error in download as csv");
            }

            return retval;
        }

        public async static Task<bool> Download_MMTAsync(MMTDownloadType DownloadType = MMTDownloadType.Standard)
        {
            bool retval = false;

            LogStr("Processing MMT Download", true);

            pricelist = await MMTPriceList.loadFromURLAsync(mmtdatafeed[(int)DownloadType]);

            if (pricelist != null)
            {
                LogStr("Successful download", true);
                LogStr("Downloaded MMT " + ((MMTPriceListProducts)pricelist.Items[1]).Product.Count() + " items retreived.", true);
                retval = true;
            }
            else
            {
                LogStr("Error in download as csv");
            }

            return retval;
        }

        public static async Task<bool> Download_Shopify(string[] querystrings = null)
        {
            bool retval = false;

            LogStr("Processing Shopify Download", true);

            shopify = new Shopify_Products();

            bool shopify_download = await shopify.getallproducts(querystrings);
            
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

        private static void Update_Metafields()
        {

            MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];

            LogStr("Starting ETA Metafield Update", true);

            foreach (MMTPriceListProductsProduct prod in mmtproducts.Product)
            {
                string result = shopify.update_availability(prod.Manufacturer[0].ManufacturerCode, prod.Availability, prod.ETA).GetAwaiter().GetResult();
                LogStr(DateTime.Now + "," + prod.Manufacturer[0].ManufacturerCode + "," + result);
            }

            LogStr("Finished ETA Metafield Update",true);
       
        }

        public static void LogStr(string message, bool consoleonly = false)
        {
            Console.WriteLine(message);

            if ((logfilename != "") & (!consoleonly))
            {
                logfile.WriteLine(message);
            }
        }

    }
}
