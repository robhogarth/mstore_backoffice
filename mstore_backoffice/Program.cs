using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backoffice;

namespace mstore_backoffice
{
    class Program
    {

        static MMTPriceList pricelist;
        static string mmtdatafeed = "https://www.mmt.com.au/datafeed/index.php?lt=s&ft=xml&tk=94M0C1O223NF7AI59BS94903AC004E0B4A%20D09%2083A%2046B%20D80%20648%2031F%2075D%20665F9461C558F25AE&af[]=et&af[]=st";

        static Shopify_Products shopify;

        static string[] tasklist = { "updateeta", "updatepricing", "findunmatched" };

        const string taskarg_prefix = "/task:";

        /* Commandline args:
         *    /task:[updateETA|updatepricing|findunmatched]
         * 
         * 
         * 
         */



        static void Main(string[] args)
        {
            string taskoption = "";

            foreach(string arg in args)
            {
                if (arg.ToLower().StartsWith(taskarg_prefix))
                {
                    taskoption = arg.ToLower().Substring(taskarg_prefix.Length);
                }
            }

            if (taskoption != "")
            {
                LogStr("Task Option is: " + taskoption);
                                
                switch (taskoption)
                {
                    case "updateeta":
                        UpdateETA();
                        break;
                    case "updatepricing":
                        UpdatePricing();
                        break;
                    case "findunmatched":
                        FindUnmatched();
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
            Console.ReadKey();

        }

        public static async void FindUnmatched()
        {
            if (Download_MMT())
            {
                if (await Download_Shopify())
                {
                    LogStr("Finding unmatched products");

                    int nomatchcount = 0;
                    
                    MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];
                    foreach (Shopify_Product product in shopify.products)
                    {
                        bool match = false;

                        foreach (MMTPriceListProductsProduct mmt_prod in mmtproducts.Product)
                        {

                            if (product.handle.ToLower() == mmt_prod.Manufacturer[0].ManufacturerCode.ToLower())
                            {
                                match = true;
                                break;
                            }
                            else
                            { 
                                if (product.variants.FirstOrDefault().sku.ToLower() == mmt_prod.Manufacturer[0].ManufacturerCode.ToLower())
                                {
                                    match = true;
                                    break;
                                }        
                                        
                            }
                            
                        }

                        if (!match)
                        {
                            nomatchcount++;
                            LogStr(String.Format(@"""{0}"",""{1}"",""{2}""", product.handle.ToLower(), product.title, product.variants[0].sku));

                        }
                    }
                                        
                    LogStr("Unmatched Product Search Completed");
                }
            }
        }

        public static async void UpdatePricing()
        {
            shopify = new Shopify_Products();
            await shopify.getallproducts();
           
            Shopify_Product product = null;
            if (Download_MMT())
            {
                if (await Download_Shopify())
                {
                    bool match = false;
                    string new_price = "";

                    Shopify_Product matcheditem;

                    MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];

                    foreach (MMTPriceListProductsProduct mmt_prod in mmtproducts.Product)
                    {
                        matcheditem = null;

                        foreach (Shopify_Product s_product in shopify.products)
                        {
                            match = false;

                            if (s_product.handle.ToLower() == mmt_prod.Manufacturer[0].ManufacturerCode.ToLower())
                            {
                                
                                match = true;
                                product = s_product;
                                break;
                            }
                            else
                            {
                                if (s_product.variants.FirstOrDefault().sku.ToLower() == mmt_prod.Manufacturer[0].ManufacturerCode.ToLower())
                                {
                                    match = true;
                                    product = s_product;
                                    break;
                                }

                            }
                        }

                        if (match)
                        {

                            InventoryItemElement inv = await shopify.Get_InventoryItem(product.variants.FirstOrDefault().inventory_item_id.ToString());

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

                                if((product.variants.FirstOrDefault().compare_at_price != mmt_prod.Pricing[0].RRPInc) || (inv.Cost != mmt_prod.Pricing[0].YourPrice))
                                {
                                    //LogStr(String.Format(@"""{0}"",""{1}"",""{2}"", Match Found, Price Not Equal", product.handle, product.title, mmt_prod.Manufacturer[0].ManufacturerCode));

                                    try
                                    {
                                        new_price = shopify.createnewprice(inv.Cost, product.variants.FirstOrDefault().compare_at_price, product.variants.FirstOrDefault().price, mmt_prod.Pricing[0].YourPrice, mmt_prod.Pricing[0].RRPInc);

                                        string[] formatlist = { product.handle, product.title, new_price, mmt_prod.Pricing[0].YourPrice, mmt_prod.Pricing[0].RRPInc };

                                        LogStr(String.Format(@"""{0}"",""{1}"",""{2}"",""{3}"",""{4}"", Match Found Price Not Equal Creating New Price", formatlist));
                                           
                                        //shopify.updateprice(product.handle, new_price, mmt_prod.Pricing[0].YourPrice, mmt_prod.Pricing[0].RRPInc);

                                    }
                                    catch (Exception ex)
                                    {
                                        LogStr(String.Format(@"""{0}"",""{1}""", product.handle, ex.Message));
                                    }
                                }
                                else
                                {
                                    //LogStr(String.Format(@"""{0}"",""{1}"",""{2}"", Match Found, Price Equal", product.handle, product.title, mmt_prod.Manufacturer[0].ManufacturerCode));
                                }
                            }

                            //if you've match an item in then remove it from the shopify list to reduce search time
                            matcheditem = product;
                        }

                        if (matcheditem != null)
                        {
                            shopify.products.Remove(product);
                        }                     
                    }
                }
            }
        }

        public static async void UpdateETA()
        {
            if (Download_MMT())
            {
                if(await Download_Shopify())
                {
                    Update_Metafields();
                }
            }
        }

        public static bool Download_MMT()
        {
            bool retval = false;
            
            LogStr("Processing MMT Download");
            pricelist = MMTPriceList.loadFromURL(mmtdatafeed);

            if (pricelist != null)
            {
                LogStr("Successful download");
                LogStr("Downloaded MMT " + ((MMTPriceListProducts)pricelist.Items[1]).Product.Count() + " items retreived.");
                retval = true;
            }
            else
            {
                LogStr("Error in download as csv");
            }

            return retval;
        }

        public static async Task<bool> Download_Shopify()
        {
            bool retval = false;
            LogStr("Processing Shopify Download");
            shopify = new Shopify_Products();

            await shopify.getallproducts();
            
            if (shopify.products.Count > 0)
            {
                LogStr("Shopify Downloaded Completed");
                LogStr("Successful download - " + shopify.products.Count() + " items loaded");

                retval = true;
            }
            else
            {
                LogStr("Shopify Download returned no results");
            }

            return retval;
        }

        private static async void Update_Metafields()
        {

            MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];

            LogStr("Starting ETA Metafield Update");

            foreach (MMTPriceListProductsProduct prod in mmtproducts.Product)
            {
                string result = await shopify.update_availability(prod.Manufacturer[0].ManufacturerCode, prod.Availability, prod.ETA);
                LogStr(DateTime.Now + "," + prod.Manufacturer[0].ManufacturerCode + "," + result);
            }

            LogStr("Finished ETA Metafield Update");
        
        }

        public static void LogStr(string message)
        {
            Console.WriteLine(message);
        }

    }
}
