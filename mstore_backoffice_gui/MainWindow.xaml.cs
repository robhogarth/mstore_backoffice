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
using System.Windows.Input;
using System.Threading.Tasks;

namespace mstore_backoffice_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {

        MBot mbot;
        const string mbot_string = "";


        public MainWindow()
        {

            mbot = new MBot(mbot_string);
            InitializeComponent();

        }

        private async void LoadProducts()
        {
            Cursor currentCursor;

            currentCursor = this.Cursor;

            this.Cursor = Cursors.AppStarting;
            DickerDataSupplier ddsup = new DickerDataSupplier("");
            _ = await mbot.Download_Shopify(new string[] { ddsup.CollectionID });

            ProductGrid.ItemsSource = mbot.shopify.products;

            //ProductGrid.DataContext = mbot.shopify.products;
            this.Cursor = currentCursor;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {

        }

        private void Shopify_Grid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void MenuItemLoadProducts_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
