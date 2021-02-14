using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backoffice
{
    public interface IProduct
    {

    }

    public interface IHasDescription
    {
        string Description { get; }
    }

    public interface IHasCategories
    {
        string Category { get; }
    }

    public interface IHasImages
    {
        List<string> Images { get; }
    }

    public enum Suppliers
    {
        MMT,
        TechData,
        DickerData,
        Wavelink
    }

    public static class PricingExtensions
    {
        public static string ToShopify(this double price)
        {
            return price.ToString("F");
        }
    }

    public static class ETAExtensions
    {
        public const string AvailableTagPrefix = "mbot_Avail__";
        public const string ETATagPrefix = "mbot_ETA__";
        public const string StatusTagPrefix = "mbot_Status__";

        public static string ToAvailableTag(this int available)
        {
            return AvailableTagPrefix + available.ToString();
        }

        public static string ToETATag(this DateTime ETA)
        {
            return ETATagPrefix + ETA.ToString().Replace(" ", "_");
        }
        public static string ToStatusTag(this string status)
        {
            return StatusTagPrefix + status;
        }

        public static string ToTitleCase(this string working)
        {
            return working.Substring(0, 1).ToUpper() + working.Substring(1).ToLower();
        }
    }
       


    abstract public class Product: IProduct
    {

        abstract public string Title { get; }
        abstract public double CostPrice { get; }
        abstract public double RRPPrice { get; }
        abstract public string Vendor { get; }
        abstract public string SKU { get; }
        abstract public int Available { get; }
        abstract public DateTime ETA { get; }
        abstract public string Status { get; }

        public override string ToString()
        {
            return this.SKU + ", " + this.Title + ", " + this.Vendor + ", $" + this.CostPrice + ", $" + this.RRPPrice + ", " + this.Available.ToString() + ", " + this.ETA.ToString() + ", " + this.Status;
        }
    }

    public class MMTProduct: Product, IHasDescription, IHasImages, IHasCategories
    {
        private MMTXMLProduct mmtproduct;
        private List<string> _images;

        public override string Title
        {
            get
            {
                return this.mmtproduct.Description.ShortDescription;
                //return this.mmtproduct.Description[0].ShortDescription;
            }
        }
        public string Description
        {
            get
            {
                if (this.mmtproduct.Description.DotPoints == null)
                    return this.mmtproduct.Description.LongDescription;
                else
                    return this.mmtproduct.Description.LongDescription + "<ul><li>" + string.Join("</li><li>", this.mmtproduct.Description.DotPoints.Point.ToArray()) + "</ul>";

                //return this.mmtproduct.Description[0].LongDescription;
            }
        }
        public override double CostPrice
        {
            get
            {
                return Math.Round(Convert.ToDouble(this.mmtproduct.Pricing.YourPrice) * 1.1, 2);
                //return Math.Round(Convert.ToDouble(this.mmtproduct.Pricing[0].YourPrice)*1.1,2);
            }
        }
        public override double RRPPrice
        {
            get
            {
                return Convert.ToDouble(this.mmtproduct.Pricing.RRPInc);
                //return Convert.ToDouble(this.mmtproduct.Pricing[0].RRPInc);
            }
        }
        public override string Vendor
        {
            get
            {
                return this.mmtproduct.Manufacturer.ManufacturerName;
            }
        }
        public override string SKU
        {
            get
            {
                return this.mmtproduct.Manufacturer.ManufacturerCode;
            }
        }
        public override int Available
        {
            get
            {
                return Convert.ToInt16(this.mmtproduct.Availability);
            }
        }
        public override DateTime ETA
        {
            get
            {
                return DateTime.Parse(this.mmtproduct.ETA);
            }
        }
        public override string Status
        {
            get
            {
                return this.mmtproduct.Status.StatusName;
            }
        }

        public string Category
        {
            get
            {
                return this.mmtproduct.Category.CategoryName;
            }
        }

        public List<string> Images
        {
            get
            {
                return _images;
            }
        }

        public string Barcode
        {
            get
            {
                return this.mmtproduct.Barcode;
            }
        }

        public long Weight
        {
            get
            {
                return Convert.ToInt64(Convert.ToDouble(this.mmtproduct.Weight) * 1000);      //MMT returns wieght in KGs.  Shopify likes weight in grams
            }
        }

        public void LoadMMTProduct(MMTXMLProduct mmtprod)
        {
            this.mmtproduct = mmtprod;

            if (mmtproduct.Files != null)
            {
                _images = new List<string>();
                foreach (string image in mmtproduct.Files.LargeImageURL)
                {
                    _images.Add(image);
                }
            }


        }

        public MMTProduct()
        {

        }

        public MMTProduct(MMTXMLProduct mmtprod)
        {
            LoadMMTProduct(mmtprod);
        }
       
    }

    public class TechDataProduct: Product
    {
        /*
         * TechData rField Headers
         * 0 -  Customer number
         * 1 -  Customer name
         * 2 -  Vendor name
         * 3 -  Prod. Hier. Lvl. 1
         * 4 -  Mfg part Number
         * 5 -  Tech Data part number
         * 6 -  Product Description
         * 7 -  RRP (Excluding GST)
         * 8 -  Your Price (Excluding GST)
         * 9 -  Currency
         * 10 - Available
         * 11 - UOM
         * 12 - Prod. Hier. Lvl. 2
         * 13 - Prod. Hier. Lvl. 3
         * 14 - Prod. Hier. Lvl. 4
         * 15 - RRP (Including GST)
         * 16 - Material group description
         * 17 - Serialized?
         * 18 - Maintenance
         */

        private string[] rFields;

        public override string Title
        {
            get
            {
                return this.rFields[6];
            }
        }
        public override double CostPrice
        {
            get
            {
                return Math.Round(Convert.ToDouble(this.rFields[8]) * 1.1,2);
            }
        }
        public override double RRPPrice
        {
            get
            {
                return Convert.ToDouble(this.rFields[15]);
            }
        }
        public override string Vendor
        {
            get
            {
                return this.rFields[3];
            }
        }
        public override string SKU
        {
            get
            {
                return this.rFields[4];
            }
        }
        public override int Available
        {
            get
            {
                string avail_string = this.rFields[10]; 
                return Convert.ToInt32(Math.Round(Convert.ToDouble(avail_string),0));
            }
        }
        public override DateTime ETA
        {
            get
            {
                return DateTime.MinValue;
            }
        }
        public override string Status
        {
            get
            {
                if (this.Available > 0)
                {
                    return "In Stock";
                }
                else
                {
                    return "Order to Order";
                }
            }
        }

        public string[] RawFields { get { return this.rFields; } }

        public TechDataProduct()
        {

        }

        public TechDataProduct(string[] fields)
        {
            this.rFields = fields;
        }
    }

    public class DickerDataProduct : Product
    {
        /*
         * DickerData rField Headers
         * 0 -  StockCode
         * 1 -  Vendor
         * 2 -  VendorStockCode
         * 3 -  StockDescription
         * 4 -  PrimaryCategory
         * 5 -  SecondaryCategory
         * 6 -  TertiaryCategory
         * 7 -  RRPEx
         * 8 -  DealerEx
         * 9 -  StockAvailable
         * 10 - ETA
         * 11 - Status
         * 12 - Type
         * 13 - BundledItem1
         * .....
         * 17 - BundledItem5
         */

        private string[] rFields;

        public override string Title
        {
            get
            {
                return this.rFields[3];
            }
        }
        public override double CostPrice
        {
            get
            {
                return Math.Round(Convert.ToDouble(this.rFields[8]) * 1.1, 2);
            }
        }
        public override double RRPPrice
        {
            get
            {
                return Math.Round(Convert.ToDouble(this.rFields[7]) * 1.1, 2);
            }
        }
        public override string Vendor
        {
            get
            {
                return this.rFields[1];
            }
        }
        public override string SKU
        {
            get
            {
                return this.rFields[2];
            }
        }
        public override int Available
        {
            get
            {
                return Convert.ToInt32(this.rFields[9]);
            }
        }
        public override DateTime ETA
        {
            get
            {
                DateTime dFormat;
                if (DateTime.TryParse(rFields[10], out dFormat))
                {
                    return dFormat.Date;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }
        public override string Status
        {
            //TODO: Field from csv contains a code for status.  Figure it out
            get
            {
                if (this.Available > 0)
                {
                    return "In Stock";
                }
                else
                {
                    if (this.Available == 0)
                    {
                        return "Out of Stock";
                    }

                    return "Order to Order";
                }

            }
        }

        public string[] RawFields { get { return this.rFields; } }

        public DickerDataProduct()
        {

        }

        public DickerDataProduct(string[] fields)
        {
            this.rFields = fields;
        }
    }

    public class WavelinkProduct : Product
    {
        /*
         * Wavelink rField Headers
         * 0 -  Status
         * 1 -  Available
         * 2 -  Status
         * 3 -  SKU
         * 4 -  BuyPrice Ex
         * 5 -  RRP Ex
         * 6 -  Title
         * 7 -  ImageURL
         */

        private string[] rFields;
        private const string CostStringPrefix = "Reseller Buy ex GST:  $";
        private const string RRPStringPrefix = "MSRP ex GST: $";

        public override string Title
        {
            get
            {
                return this.rFields[6];
            }
        }
        public override double CostPrice
        {
            get
            {
                if (this.rFields[4].Contains(CostStringPrefix))
                    this.rFields[4] = this.rFields[4].Substring(CostStringPrefix.Length);
                return Math.Round(Convert.ToDouble(this.rFields[4]) * 1.1, 2);
            }
        }
        public override double RRPPrice
        {
            get
            {
                if (this.rFields[5].Contains(RRPStringPrefix))
                    this.rFields[5] = this.rFields[5].Substring(RRPStringPrefix.Length);
                return Math.Round(Convert.ToDouble(this.rFields[5]) * 1.1, 2);
            }
        }
        public override string Vendor
        {
            get
            {
                return "Fortinet";
            }
        }
        public override string SKU
        {
            get
            {
                return this.rFields[3];
            }
        }
        public override int Available
        {
            get
            {
                return Convert.ToInt32(this.rFields[1]);
            }
        }
        public override DateTime ETA
        {
            get
            {
                    return DateTime.MinValue;
            }
        }
        public override string Status
        {

            get
            {
                return this.rFields[0];
            }
        }

        public string[] RawFields { get { return this.rFields; } }

        public WavelinkProduct()
        {

        }

        public WavelinkProduct(string[] fields)
        {
            this.rFields = fields;
        }
    }
}
