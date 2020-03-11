using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace backoffice
{
    public class MMTProduct
    {
        public string MMTCode { get; set; }
        public string ManufacturerCode { get; set; }
        public string Manufacturer { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public float RRPInc { get; set; }
        public float YourBuy { get; set; }
        public int Available { get; set; }
        public string ParentCategoryName { get; set; }
        public string CategoryName { get; set; }
        public string ExtDescription { get; set; }

        public int csvStrToObj(string csvobject)
        {

            const char splitter = ',';
            string[] fields = csvobject.Split(splitter);

            MMTCode = fields[0];
            ManufacturerCode = fields[1];
            Manufacturer = fields[2];
            Category = fields[3];
            Description = fields[4];
            try
            {
                RRPInc = float.Parse(fields[5]);
            }
            catch (Exception ex)
            {
                RRPInc = -1;
            }

            try
            {
                YourBuy = float.Parse(fields[6]);
            }
            catch (Exception ex)
            {
                YourBuy = -1;
            }

            try
            {
                Available = int.Parse(fields[7]);
            }
            catch (Exception ex)
            {
                Available = -1;
            }

            ParentCategoryName = fields[8];
            CategoryName = fields[9];
            ExtDescription = fields[10];

            return fields.Count();

        }

        public MMTProduct(string csvline)
        {
            csvStrToObj(csvline);
        }

    }

    public class MMTProducts
    {
        public List<MMTProduct> Products = new List<MMTProduct>();

        public bool DownloadAsCSV(string datafeedurl)
        {

            bool retval = false;

            try
            {
                WebClient client = new WebClient();
                client.Headers.Add(HttpRequestHeader.ContentType, "apoplication/csv");
                string csvdownload = client.DownloadString(datafeedurl);

                char[] newline = { '\n' };
                string[] rawlines = csvdownload.Split(newline, StringSplitOptions.RemoveEmptyEntries);
                string[] lines = rawlines.Skip(1).ToArray();

                foreach (string line in lines)
                {
                    Products.Add(new MMTProduct(line));
                }

                retval = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return retval;
        }


    }
}
