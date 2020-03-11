using backoffice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace mstore_backoffice_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MMTPriceList pricelist;
        string mmtdatafeed = "https://www.mmt.com.au/datafeed/index.php?lt=s&ft=xml&tk=94M0C1O223NF7AI59BS94903AC004E0B4A%20D09%2083A%2046B%20D80%20648%2031F%2075D%20665F9461C558F25AE&af[]=et&af[]=st";

        Shopify_Products shopify;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Download_MMT()
        {
            StackPanelLabel.Content = "Processing MMT Download";
            pricelist = MMTPriceList.loadFromURL(mmtdatafeed);

            if (pricelist != null)
            {
                StackPanelLabel.Content = "Successful download";
                TextBlock_Results.Text += Environment.NewLine;
                TextBlock_Results.Text += "Downloaded MMT " + ((MMTPriceListProducts)pricelist.Items[1]).Product.Count() + " items retreived.";
            }
            else
            {
                StackPanelLabel.Content = "Error in download as csv";
            }

        }

        private async void Download_Shopify()
        {
            StackPanelLabel.Content = "Processing Shopify Download";
            shopify = new Shopify_Products();

            await shopify.getallproducts();


            if (shopify.products.Count > 0)
            {
                StackPanelLabel.Content = "Shopify Downloaded Completed";
                TextBlock_Results.Text += Environment.NewLine;
                TextBlock_Results.Text = "Successful download - " + shopify.products.Count() + " items loaded";
            }
            else
            {
                StackPanelLabel.Content = "Shopify Download returned no results";
            }
        }

        private void Unmatched_Items()
        {

            StackPanelLabel.Content = "Finding unmatched products";

            int nomatchcount = 0;
            List<string> notoffered = new List<string>();

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
                }

                if (!match)
                {
                    nomatchcount++;
                    notoffered.Add(String.Format(@"""{0}"",""{1}"",""{2}""", product.handle.ToLower(), product.title, product.variants[0].sku));
                }
            }

            TextBlock_Results.Text += Environment.NewLine;
            foreach (string item in notoffered)
            {
                TextBlock_Results.Text += item + Environment.NewLine;
            }

            StackPanelLabel.Content = "Unmatched Product Search Completed";

        }

        private async void Update_Metafields()
        {
            string logstr;

            MMTPriceListProducts mmtproducts = (MMTPriceListProducts)pricelist.Items[1];

            StackPanelLabel.Content = "Starting ETA Metafield Update";

            StatusBarLabel.Content = "Running";
            StatusBarProgress.Value = 0;
            StatusBarProgress.Maximum = mmtproducts.Product.Count();
            
            foreach (MMTPriceListProductsProduct prod in mmtproducts.Product)
            {
                string result = await shopify.update_availability(prod.Manufacturer[0].ManufacturerCode, prod.Availability, prod.ETA);
                logstr = DateTime.Now + "," + prod.Manufacturer[0].ManufacturerCode + "," + result;

                StatusBarProgress.Value++;

                TextBlock_Results.Text += logstr + Environment.NewLine;
            }

            StackPanelLabel.Content = "Finished ETA Metafield Update";
            StatusBarLabel.Content = "Completed";
        }

        private void Update_Pricing()
        { 
            /* foreach item in mmtpricelist
             * get mmt pricing
             * compare to shopify current price
             *
             * if its changed
             * then update pricing fields RRP and Cost Price
             * update variant price as function of current cost price vs variant price to work out margin
             * then replicate that with new pricing
            */
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Download_MMT();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Download_Shopify();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Unmatched_Items();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Update_Metafields();
        }
    }
}
