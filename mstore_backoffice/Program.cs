using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backoffice;
using System.IO;
using Microsoft.VisualBasic.FileIO;
//using backoffice;

namespace mstore_backoffice
{

    enum MMTDownloadType : int
    {
        Standard = 0,
        Clearance = 1
    }
    class Program
    {

        //static MMTPriceList pricelist;

        static string[] mmtdatafeed = { "https://www.mmt.com.au/datafeed/index.php?lt=s&ft=xml&tk=94M0C1O223NF7AI59BS94903AC004E0B4A%20D09%2083A%2046B%20D80%20648%2031F%2075D%20665F9461C558F25AE&af[]=et&af[]=st", "https://www.mmt.com.au/datafeed/index.php?lt=c&ft=xml&tk=94M0C1O223NF7AI59BS94903AC004E0B4A%20D09%2083A%2046B%20D80%20648%2031F%2075D%20665F9461C558F25AE&af[]=et&af[]=st" };

        //static Shopify_Products shopify;

        //static List<string> mstore_stock;
        static string mstore_stock_file;

        static string[] tasklist = { "updateeta", "updatepricing", "updatepricing_clearance", "findunmatched", "updateitemeta", "updateclearance", "addsku", "updatefreeshipping, updateitemetabysku, updatenoimage" };

        const string taskarg_prefix = "/task:";
        const string logarg_prefix = "/log:";
        const string logdate_prefix = "/filedatesuffix";
        const string verbose_prefix = "/verbose";
        const string item_prefix = "/item:";
        const string itemeta_prefix = "/eta:";
        const string itemavailable_prefix = "/available:";
        const string itemstatus_prefix = "/itemstatus:";
        const string sku_prefix = "/sku:";
        const string statusfile_prefix = "/statusfile:";
        const string supplier_prefix = "/supplier:";

        static string logfilename = "";
        static StreamWriter logfile;
        static bool addlogdate = false;
        static bool verbose = false;

        static string itemnumber = "";
        static string itemeta = "";
        static string itemavailable = "";
        static string itemstatus = "";
        static string sku = "";
        static string statusfile = "";
        static SupplierType supplier = SupplierType.MMT;

        static string ClearanceTag = "Clearance";

        static string args_help = @"Commandline args:
             /task:[updateETA|updatepricing|findunmatched|updateitemeta|updateclearance]
             /supplier:MMT|Techdata|Dickerdata|Wavelink
             /log:<logfilename>   - optional
             /filedatesuffix       - add date and time to log file name
             /verbose             - more logging, adds readkey to end of main()
             /item:<itemnumber>   - item for updateitemeta
             /eta:<eta>           - eta for item string format displayed exactly like this on website
             /available:<count>   - number of items available
             /itemstatus:<status> - status of item availability
             /statusfile:<path_to_file> - path to file for sku updates - techdata mostly";
         

        static void Main(string[] args)
        {
            string taskoption = "";

            mstore_stock_file = mstore_backoffice.Properties.Settings.Default.mstore_stock_file;

            MBot mbot = new MBot(mstore_stock_file);
            mbot.Notify += c_Notify;


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
                    itemstatus = arg.Substring(itemstatus_prefix.Length);
                }

                if (arg.ToLower().StartsWith(sku_prefix))
                {
                    sku = arg.ToLower().Substring(itemstatus_prefix.Length);
                }

                if (arg.ToLower() == logdate_prefix)
                {
                    addlogdate = true;
                }

                if (arg.ToLower() == verbose_prefix)
                {
                    verbose = true;
                }

                if (arg.ToLower().StartsWith(statusfile_prefix))
                {
                    statusfile = arg.ToLower().Substring(statusfile_prefix.Length);
                }

                if (arg.ToLower().StartsWith(supplier_prefix))
                {
                    string raw_supplier = arg.ToLower().Substring(supplier_prefix.Length);

                    switch (raw_supplier.ToLower())
                    {
                        case "mmt":
                            supplier = SupplierType.MMT;
                            break;
                        case "techdata":
                            supplier = SupplierType.TechData;
                            break;
                        case "dickerdata":
                            supplier = SupplierType.DickerData;
                            break;
                        case "wavelink":
                            supplier = SupplierType.Wavelink;
                            break;
                    }
                }
            }

            if (logfilename != "")
            {
                if (addlogdate)
                {
                    logfilename = logfilename.Substring(0, logfilename.LastIndexOf(".")) + "_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute + logfilename.Substring(logfilename.LastIndexOf("."));
                }

                if (!Directory.Exists(logfilename.Substring(0, logfilename.LastIndexOf(@"\"))))
                {
                    Console.WriteLine("Creating directory - " + logfilename.Substring(0,logfilename.LastIndexOf(@"\")));
                    Directory.CreateDirectory(logfilename.Substring(0,logfilename.LastIndexOf(@"\")));
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
                        mbot.UpdateETA(supplier).Wait();
                        break;
                    case "updatepricing":
                        mbot.UpdatePricing(supplier).Wait();
                        break;
                    case "findunmatched":
                        mbot.FindUnmatched(supplier).Wait();
                        break;
                    case "updateitemeta":
                        mbot.UpdateItemETA(itemnumber, itemeta, itemavailable, itemstatus);
                        break;
                    case "updateitemetabysku":
                        mbot.UpdateItemETAbySKU(statusfile).Wait();
                        break;
                    case "updateclearance":
                        mbot.UpdateClearance().Wait();
                        break;
                    /*
                    case "addsku":
                        mbot.AddSKU().Wait();
                        break;
                        
                    case "updatefreeshipping":
                        mbot.UpdateFreeShipping().Wait();
                        break;
                        */
                    case "updatenoimage":
                        mbot.UpdateNoImage().Wait();
                        break;
                    case "test":
                        mbot.Test().Wait();
                        break;
                    default:
                        LogStr("Cannot Match task: " + taskoption, true);
                        LogStr(args_help, true);
                        LogStr("Ending", true);
                        break;
                }

            }
            else
            {
                LogStr("No Option detected.", true);
                LogStr(args_help, true);
            }

            LogStr("Completed Processing.  Ending");

            if (verbose) { Console.ReadKey(); }

            if (logfilename != "")
            {
                logfile.Close();
            }
        }

        private static void c_Notify(object sender, NotifyEventArgs e)
        {
            LogStr(e.Message, e.ConsoleOnly);
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
