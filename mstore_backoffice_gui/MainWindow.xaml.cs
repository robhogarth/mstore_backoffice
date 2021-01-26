using backoffice;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Windows;
using System.Net;


namespace mstore_backoffice_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
        
    public partial class MainWindow : Window
    {
        MMTPriceList pricelist;
        string mmtdatafeed = "https://www.mmt.com.au/datafeed/index.php?lt=s&ft=xml&tk=94M0C1O223NF7AI59BS94903AC004E0B4A%20D09%2083A%2046B%20D80%20648%2031F%2075D%20665F9461C558F25AE&af[]=et&af[]=st";

        Shopify_Products shopify;
        public Availability prod_avail;
                
        public MainWindow()
        {
            InitializeComponent();
            shopify = new Shopify_Products();
            prod_avail = new Availability();
            //prod_metafields = new ObservableCollection<Metafield>();
        }

        private async void Search_Shopify(string searchstring)
        {
            StatusBarLabel.Content = "Searching Shopify";

            await shopify.getallproducts(new string[] { "title=" + WebUtility.UrlEncode(searchstring) },false,true);

            if (shopify.products.Count > 0)
            {
                Shopify_Grid.ItemsSource = shopify.products;

                StatusBarLabel.Content = "Shopify Downloaded Completed";
            }
            else
            {
                StatusBarLabel.Content = "Shopify Download returned no results";
            }
        }


        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Search_Shopify(SearchBox.Text);
        }

        private async void Shopify_Grid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                await prod_avail.GetAvailability((e.AddedItems[0] as Shopify_Product).id.ToString());
                border1.DataContext = prod_avail;
            }

        }


        private async void updateMeta(Metafield2 meta)
        {
            StatusBarLabel.Content = await shopify.update_metafield(meta);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            prod_avail.CreateAvailability();
        }
    }
}
