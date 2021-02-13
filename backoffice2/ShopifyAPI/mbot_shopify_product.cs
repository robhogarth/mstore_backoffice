using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Net;

namespace backoffice.ShopifyAPI
{


    public partial class Shopify_Product
    {
        private Shop_API _api;

        //TODO: this is not a great place to put these constants.
        public readonly int Price_Error_Threshold = 4;
        public readonly int Max_Title_Length = 255;

        public Product_Fault_Code_Details Last_Fault_Codes;

        private void CreateAPI()
        {
            if (_api == null)
                _api = new Shop_API();
        }

        public bool Product_Taxable()
        {
            foreach (Variant var in this.Variants)
            {
                if (var.Taxable)
                    return true;
            }

            return false;
        }

        public async Task<List<Cost_Fault_Codes>> Verify_Price()
        {
            CreateAPI();

            List<Cost_Fault_Codes> retval = new List<Cost_Fault_Codes>();

            foreach (Variant var in this.Variants)
            {
                retval.Add(await var.VerifyPrice());
            }

            this.Last_Fault_Codes.Cost_Fault_Code = retval;

            return retval;
        }

        public Title_Fault_Codes Verify_Title()
        {
            Title_Fault_Codes retval = 0;

            //Too Long
            if (this.Title.Length > this.Max_Title_Length)
                retval = retval | Title_Fault_Codes.Too_Long;

            //Invalid Chars
            //TODO: Contants should not be defined in methods
            string[] invalidChars = new string[] { "&amp", "&quot" };
            foreach(string invalidChar in invalidChars)
            {
                if (this.Title.Contains(invalidChar))
                    retval = retval | Title_Fault_Codes.Invalid_Chars;
            }

            this.Last_Fault_Codes.Title_Fault_Code = retval;

            return retval;
        }

        public async Task<bool> Verify_SupplierTags()
        {
            CreateAPI();

            bool retval = false;
            string sup_tag = common.ExtractSupplierTag(this.Tags);

            foreach(Variant var in this.Variants)
            {
                retval = retval | (await var.Verify_SupplierTags(sup_tag));
            }

            return retval;
        }

        public bool Verify_mbotTags()
        {
            return common.DoTagsHavembot(this.Tags);
        }

        public bool Verify_ETATags()
        {
            return common.DoTagsHaveETA(this.Tags);
        }


        public async Task<Product_Fault_Codes> Verify_Product(Supplier supplier_feed = null)
        {
            Last_Fault_Codes = new Product_Fault_Code_Details();
            Product_Fault_Codes retval = 0;

            //Is price invalid
            List<Cost_Fault_Codes> price_faults = await this.Verify_Price();
            foreach (Cost_Fault_Codes codes in price_faults)
            {
                if ((int)codes > Price_Error_Threshold)
                {
                    retval = retval | Product_Fault_Codes.Invalid_Price;
                }
            }

            //Is title reasonable
            if ((int)this.Verify_Title() > 0)
                retval = retval | Product_Fault_Codes.Poor_Title;

            //Are supplier tags correct - do they match warehouse
            if (!await this.Verify_SupplierTags())
                retval = retval | Product_Fault_Codes.Mismatched_Supplier;

            //Does product have an mbot tag
            if (!Verify_mbotTags())
                retval = retval | Product_Fault_Codes.No_mbot_Tags;

                //Does product have ETA tags
            if (!Verify_ETATags())
                retval = retval | Product_Fault_Codes.No_ETA_Tags;

            //Does Vendor Match Vendor Tag
            if (!Verify_VendorTagMatchesField())
                retval = retval | Product_Fault_Codes.Mismatched_Vendor;

            //Is product description longer than supplier feed one?
            //TODO: Create product description from supplier.  this seems excessive to implement at this time. probably something to implement in MMT Supplier code not here
           
            //Does product have taxable set
            if (this.Product_Taxable())
                retval = retval | Product_Fault_Codes.Product_Taxable;

            return retval;
        }

        private bool Verify_VendorTagMatchesField()
        {
            return (this.Tags.Contains(this.Vendor)) ? true : false;
        }

        public async Task<bool> FixTitle(bool whatif = false)
        {
            bool retval = false;
            
            try
            {
                this.Title = WebUtility.UrlDecode(this.Title);
                if (!whatif)
                    retval = await _api.UpdateProductTitle(this);
                else
                    retval = true;
            }
            catch (Exception ex)
            {
                retval = false;
            }

            return retval;
        }

        public bool FixVendorTags(bool whatif = false)
        {
            //Make Vendor_tag equal the vendor field
            //except if vendor field = mstore
            bool retval = false;

            if (this.Vendor.ToLower() == "mstore")
            {
                string sup_tag = common.ExtractVendorTag(this.Tags);
                if (sup_tag != "")
                {
                    sup_tag = sup_tag.Replace("Vendor_", "");
                    if (!whatif)
                        this.Vendor = sup_tag;

                    return true;
                }
            }
            else
            {
                // check title and description to see if vendor exists
                // if they do we can be certain vendor is correct
                bool vendor_acurate = false;

                if (this.Title.ToLower().Contains(this.Vendor.ToLower()) | (this.BodyHtml.ToLower().Contains(this.Vendor.ToLower())))
                {
                    vendor_acurate = true;
                    if (!whatif)
                    {
                        common.ReplaceTag(Tags, "Vendor_" + this.Vendor, "Vendor_");
                        retval = true;
                    }

                    if (!vendor_acurate)
                    {
                        string vendor_tag = common.ExtractVendorTag(this.Tags).Replace("Vendor_", "");
                        if (vendor_tag != "")
                        {
                            if (this.Title.ToLower().Contains(vendor_tag) | (this.BodyHtml.ToLower().Contains(this.Vendor.ToLower())))
                            {
                                if (!whatif)
                                {
                                    this.Vendor = vendor_tag;
                                    retval = true;
                                }
                            }
                        }
                    }
                }
            }

            return retval;
        }

        public async Task<bool> FixSupplierLocation(bool whatif = false)
        {
            bool retval = true;
            string sup_tag = common.ExtractSupplierTag(this.Tags);

            foreach(Variant var in this.Variants)
            {
                retval = retval | (await var.FixSupplierLocation(sup_tag, whatif));
            }    

            return retval;
        }

        public async Task<bool> MakeProductUntaxable(bool whatif = false)
        {
            bool retval = true;
            foreach (Variant var in this.Variants)
            {
                retval = retval | (await var.MakeProductUntaxable(whatif));
            }

            return retval;
        }

        public async Task<bool> UpdateETA(string avail, string status, string ETA)
        {
            bool retval = false;

            

            return retval;
        }

        public async Task<bool> UpdateETA()
        {
            bool retval = false;



            return retval;
        }

    }
}
