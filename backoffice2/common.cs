using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace backoffice
{
    /// <summary>
    /// Bitwise return codes that Verify_Product raises to determine which verification a product fails
    /// return code can be used to determine which fix(es) need to be applied.
    /// 
    /// Need this so that we can flag problems and then allow human intervention before fixing
    /// Although some codes we may feel safe just doing automatically
    /// </summary>
    [Flags]
    public enum Product_Fault_Codes
    {
        None                = 0b_0000_0000,
        Invalid_Price       = 0b_0000_0001,
        Poor_Description    = 0b_0000_0010,
        Mismatched_Supplier = 0b_0000_0100,
        No_ETA_Tags         = 0b_0000_1000,
        Poor_Title          = 0b_0001_0000,
        Product_Taxable     = 0b_0010_0000,
        Mismatched_Vendor   = 0b_0100_0000,
        No_mbot_Tags        = 0b_1000_0000
    }

    /// <summary>
    /// Bitware return codes for Cost issues
    /// First 3 options are more warnings and ordered so that
    /// you can search for anthing higher than No_Cost to find faults
    /// as you probably want to ignore other flags
    /// </summary>
    [Flags]
    public enum Cost_Fault_Codes
    {
        None                    = 0b_0000_0000,
        Price_More_Than_RRP     = 0b_0000_0001,
        RRP_Not_Set             = 0b_0000_0010,
        Cost_Not_Set            = 0b_0000_0100,
        Price_Less_Than_Cost    = 0b_0000_1000,
        Price_Is_Zero           = 0b_0001_0000,
        Price_Not_Set           = 0b_0010_0000
    }

    /// <summary>
    /// Bitware return codes for Description problems
    /// </summary>
    [Flags]
    public enum Title_Fault_Codes
    {
        None            = 0b_0000_0000,
        Too_Long        = 0b_0000_0001,
        Invalid_Chars   = 0b_0000_0010,
    }

    public class Product_Fault_Code_Details
    {
        public Title_Fault_Codes Title_Fault_Code { get; set; }
        public List<Cost_Fault_Codes> Cost_Fault_Code { get; set; }


    }

    public static class common
    {
        public static bool IsStatusCodeSuccess(HttpStatusCode code)
        {
            bool retval = false;

            if (((int)code > 199) & ((int)code < 300))
                retval = true;

            return retval;
        }

        private static string Generate_mbot_string()
        {
            const string mbot_prefix = "Mbot_";

            //generate new mbot string
            string mbot_newtag = mbot_prefix + DateTime.Now.ToString();
            return mbot_newtag.Replace(' ', '_');
        }

        public static string ExtractVendorTag(string tags)
        {
            if (tags != "")
            {
                string[] tagarray = tags.Split(',');

                int shipping_index = Array.FindIndex(tagarray, FindVendorstring);

                if (shipping_index > -1)
                    return tagarray[shipping_index].Trim();
                else
                    return "";
            }
            else
                return "";
        }

        public static string ExtractSupplierTag(string tags)
        {
            if (tags != "")
            {
                string[] tagarray = tags.Split(',');

                int shipping_index = Array.FindIndex(tagarray, FindShippingstring);

                if (shipping_index > -1)
                    return tagarray[shipping_index].Trim();
                else
                    return "";
            }
            else
                return "";
        }

        private static bool Findmbotstring(string s)
        {
            if (s.ToLower().Contains("mbot"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool FindShippingstring(string s)
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

        private static bool FindVendorstring(string s)
        {
            if (s.ToLower().Contains("vendor_"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Test to see if mbot ETA tags exist
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>

        internal static bool DoTagsHaveETA(string tags)
        {
            //start true and if you find any tags aren't there then go false

            //tags should contain mbot_Status, mbot_ETA, mbot_Avail
            //TODO: eta_tags should not be defined here.  A string array is fine but needs centralised storage
            string[] eta_tags = new string[] { "mbot_Status", "mbot_ETA", "mbot_Avail" };

            bool retval = true;

            foreach (string eta in eta_tags)
            {
                retval = retval & tags.Contains(eta);
            }

            return retval;
        }

        public static string ReplaceTag(string tags, string newtag, string tag_ident)
        {
            if (tags != "")
            {
                string[] tagarray = tags.Split(',');

                bool replaced = false;
                for(int i = 0; i < tagarray.Length - 1; i++ )
                {
                    if (tagarray[i].ToLower().Contains(tag_ident.ToLower()))
                    {
                        replaced = true;
                        tagarray[i] = newtag;
                    }
                }

                //int mbot_index = Array.FindIndex(tagarray, FindTagString(newtag));

                if (!replaced)
                {
                    Array.Resize(ref tagarray, tagarray.Length + 1);
                    tagarray[tagarray.Length - 1] = newtag;
                }

                return String.Join(",", tagarray);
            }
            else
                return newtag;
        }

        internal static bool DoesSupplierTagMatchesLocationID(long locationId, string tag)
        {
            Dictionary<string, long> locationList = SupplierProducer.GetSupplierLocationIDs();

            long locId = 0;
            if (locationList.TryGetValue(tag, out locId))
            {
                if (locId == locationId)
                    return true;
            }          

            return false;
        }
        public static bool DoTagsHavembot(string tags = "")
        {
            string[] tagarray = tags.Split(',');

            int mbot_index = Array.FindIndex(tagarray, Findmbotstring);

            return (mbot_index > -1) ? true : false;
        }

        public static string Updatembot(string tags = "")
        {
            if (tags != "")
            {
                string[] tagarray = tags.Split(',');

                int mbot_index = Array.FindIndex(tagarray, Findmbotstring);

                if (mbot_index > -1)
                    tagarray[mbot_index] = Generate_mbot_string();
                else
                {
                    Array.Resize(ref tagarray, tagarray.Length + 1);
                    tagarray[tagarray.Length - 1] = Generate_mbot_string();
                }

                return String.Join(",", tagarray);
            }
            else
                return Generate_mbot_string();
        }

        public static double AddGST(double price)
        {
            return Math.Round(price * 1.1, 2);
        }

        public static double AddGST(string price)
        {
            return Math.Round(Convert.ToDouble(price) * 1.1, 2);
        }

        public static double RemoveGST(double price)
        {
            return Math.Round(price / 1.1, 2);
        }

        public static double RemoveGST(string price)
        {
            return Math.Round(Convert.ToDouble(price) / 1.1, 2);
        }
    }
}
