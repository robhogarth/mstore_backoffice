using Microsoft.VisualStudio.TestTools.UnitTesting;
using backoffice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backoffice.Tests
{
    [TestClass()]
    public class Shopify_ProductsTests
    {
        [TestMethod()]
        public async void UpdateMbotTest_NoMBotTag()
        {
            Shopify_Products shopify = new Shopify_Products();

            string currenttags = "Networking,Vendor_D-link";
            bool newtags;
            string expectedtags = "Networking,Vendor_D-link,Mbot_" + DateTime.Now.ToString();

            newtags = await shopify.UpdateMbot(1234, currenttags, true);

            Assert.AreEqual(true, newtags);
        }

        [TestMethod()]
        public async void UpdateMbotTest_ExistingMbotTag()
        {
            Shopify_Products shopify = new Shopify_Products();
            string currenttags = "Networking,Vendor_D-link,MBot_13/12/2020 4:06:57 PM";
            bool newtags;
            string expectedtags = "Networking,Vendor_D-link,Mbot_" + DateTime.Now.ToString();

            newtags = await shopify.UpdateMbot(1234, currenttags, true);

            Assert.AreEqual(true, newtags);
        }
    }
}