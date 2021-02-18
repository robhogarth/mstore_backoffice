using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace backoffice
{


    public static class common
    {
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
        public static bool DoTagsHaveETA(string tags)
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
                for (int i = 0; i < tagarray.Length - 1; i++)
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


        public static bool DoesSupplierTagMatchesLocationID(long locationId, string tag)
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


        private static string Generate_mbot_string()
        {
            const string mbot_prefix = "Mbot_";

            //generate new mbot string
            string mbot_newtag = mbot_prefix + DateTime.Now.ToString();
            return mbot_newtag.Replace(' ', '_');
        }

        private static bool Findmbotstring(string s)
        {
            if (s.Contains("Mbot"))
            {
                return true;
            }
            else
            {
                return false;
            }
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
