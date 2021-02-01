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
