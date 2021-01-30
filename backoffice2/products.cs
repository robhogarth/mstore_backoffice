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

    public class MMTProduct: Product
    {
        private MMTPriceListProductsProduct mmtproduct;

        public override string Title
        {
            get
            {
                return this.mmtproduct.Description[0].ShortDescription;
            }
        }
        public string Description
        {
            get
            {
                return this.mmtproduct.Description[0].LongDescription;
            }
        }
        public override double CostPrice
        {
            get
            {
                return Math.Round(Convert.ToDouble(this.mmtproduct.Pricing[0].YourPrice)*1.1,2);
            }
        }
        public override double RRPPrice
        {
            get
            {
                return Convert.ToDouble(this.mmtproduct.Pricing[0].RRPInc);
            }
        }
        public override string Vendor
        {
            get
            {
                return this.mmtproduct.Manufacturer[0].ManufacturerName;
            }
        }
        public override string SKU
        {
            get
            {
                return this.mmtproduct.Manufacturer[0].ManufacturerCode;
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
                return this.mmtproduct.Status[0].StatusName;
            }
        }

        public void LoadMMTProduct(MMTPriceListProductsProduct mmtprod)
        {
            this.mmtproduct = mmtprod;
        }

        public MMTProduct()
        {

        }

        public MMTProduct(MMTPriceListProductsProduct mmtprod)
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


}
