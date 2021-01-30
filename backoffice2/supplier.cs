using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace backoffice
{
    public enum MMTDownloadType : int
    {
        Standard = 0,
        Clearance = 1
    }

    public enum SupplierType : int
    {
        MMT = 0,
        TechData = 1,
        DickerData = 2,
        Wavelink = 3
    }

    
    public interface ISupplier
    {
        Task<int> LoadProducts();      
    }

    public abstract class Supplier : ISupplier
    {
        //Base Properties of supplier
        public List<Product> Products { get; set; }
        public abstract Task<int> LoadProducts();
        public abstract long Supplier_Location_Id { get; }
        public abstract string Supplier_Tag { get; }
        public abstract string CollectionID { get; }
    }

    public abstract class FileSupplier: Supplier
    {
        public string Filename;
                
    }

    public static class SupplierProducer
    {

        public static Supplier CreateSupplier(SupplierType sType, string filename = "")
        {
            Supplier retval;

            switch (sType)
            {
                case SupplierType.MMT:
                    retval = new MMTSupplier();
                    break;
                case SupplierType.TechData:
                    retval = new TechDataSupplier(filename);
                    break;
                default:
                    retval = new DickerDataSupplier(filename);
                    break;
            }

            return retval;
        }       
    }


    public class MMTSupplier: Supplier
    {
        private MMTPriceList pricelist;

        public string[] mmtdatafeed = { "https://www.mmt.com.au/datafeed/index.php?lt=s&ft=xml&tk=94M0C1O223NF7AI59BS94903AC004E0B4A%20D09%2083A%2046B%20D80%20648%2031F%2075D%20665F9461C558F25AE&af[]=et&af[]=st", "https://www.mmt.com.au/datafeed/index.php?lt=c&ft=xml&tk=94M0C1O223NF7AI59BS94903AC004E0B4A%20D09%2083A%2046B%20D80%20648%2031F%2075D%20665F9461C558F25AE&af[]=et&af[]=st" };
        public MMTDownloadType DownloadType = MMTDownloadType.Standard;
        public override long Supplier_Location_Id { get { return 41088974985; } }
        public override string Supplier_Tag { get {return "MMTShipping"; } }
        public override string CollectionID { get { return "235730206870"; } }

        public override async Task<int> LoadProducts()
        {
            await Task.Run(() => { Thread.Sleep(100); });

            if (this.Products == null)
                this.Products = new List<Product>();
            else
                this.Products.Clear();
            

            //LogStr("Processing MMT Download", true);

            pricelist = await MMTPriceList.loadFromURLAsync(mmtdatafeed[(int)DownloadType]);

            if (pricelist != null)
            {
                //LogStr("Successful download", true);
                //LogStr("Downloaded MMT " + ((MMTPriceListProducts)pricelist.Items[1]).Product.Count() + " items retreived.", true);
            }
            else
            {
                //LogStr("Error in download as csv");
            }

            MMTPriceListProductsProduct[] mmtproducts = ((MMTPriceListProducts)pricelist.Items[1]).Product;

            foreach (MMTPriceListProductsProduct mmtprod in mmtproducts)
            {
                Products.Add(new MMTProduct(mmtprod));
            }

            return this.Products.Count();
        }
    }

    public class TechDataSupplier: FileSupplier
    {

        public override long Supplier_Location_Id { get { return 45740195977; } }
        public override string Supplier_Tag { get { return "TechDataShipping"; } }
        public override string CollectionID { get { return "235731419286"; } }

        public TechDataSupplier(string _filename)
        {
            Filename = _filename;

        }

        public override async Task<int> LoadProducts()
        {
            await Task.Yield();

            if (this.Products == null)
                this.Products = new List<Product>();
            else
                this.Products.Clear();

            using (TextFieldParser parser = new TextFieldParser(Filename))
            {

                int counter = 0;
                Product prod;

                parser.TextFieldType = FieldType.Delimited;


                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Process row
                    string[] fields = parser.ReadFields();

                    if (counter > 0)
                    {
                        prod = new TechDataProduct(fields);
                        this.Products.Add(prod);
                    }

                    counter++;
                }
            }

            return this.Products.Count - 1;
        }
    }

    public class DickerDataSupplier : FileSupplier
    {
        public override long Supplier_Location_Id { get { return 50476220566; } }
        public override string Supplier_Tag { get { return "DickerDataShipping"; } }
        public override string CollectionID { get { return "235732009110"; } }

        public DickerDataSupplier(string _filename)
        {
            Filename = _filename;
        }

        public async override Task<int> LoadProducts()
        {
            await Task.Yield();

            if (this.Products == null)
                this.Products = new List<Product>();
            else
                this.Products.Clear();


            using (TextFieldParser parser = new TextFieldParser(Filename))
            {

                int counter = 0;
                Product prod;

                parser.TextFieldType = FieldType.Delimited;


                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //TODO: Exclude Dell Items so they are not screwed up by loading two different suppliers with similar parts.


                    //Process row
                    string[] fields = parser.ReadFields();

                    if (counter > 0)
                    {
                        prod = new DickerDataProduct(fields);
                        this.Products.Add(prod);
                    }

                    counter++;
                }
            }

            return this.Products.Count - 1;
        }
    }


}
