using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backoffice.ShopifyAPI
{
    public partial class Variant
    {
        Shop_API _api;

        private void CreateAPI()
        {
            if (_api == null)
                _api = new Shop_API();
        }


        public async Task<Cost_Fault_Codes> VerifyPrice()
        {
            CreateAPI();

            Cost_Fault_Codes retval = 0;

            Double wCost = 0;
            Double wPrice = 0;
            Double wRRP = 0;
            InventoryItem invItem;

            invItem = await _api.GetInventoryItem(this.InventoryItemId.ToString());
            bool noRRP = false;


            // Try parsing cost to double
            // if it's invalide flag No_Cost
            if ((invItem.Cost == "") || (invItem.Cost == null))
                retval = retval | Cost_Fault_Codes.Cost_Not_Set;
            else
                wCost = Convert.ToDouble(invItem.Cost);

            // Try parsing price
            // On rare event it's invalid then raise the flag
            wPrice = -1; //needs to be set to something
            if ((this.Price == "") || (this.Price == null))
                retval = retval | Cost_Fault_Codes.Price_Not_Set;
            else
                wPrice = Convert.ToDouble(this.Price);


            // Try parsing RRP - it will not be uncommon to have no RRP
            // set wRRP to 0 as starting point
            try
            {
                if (this.CompareAtPrice != null)
                {
                    if (this.CompareAtPrice.ToString() != "")
                        wRRP = Convert.ToDouble(this.CompareAtPrice);
                    else
                        noRRP = true;
                }
                else
                    noRRP = true;
            }
            catch
            {
                //TODO: Log the excpetion somewhere if we get here, but don't stop anything
                noRRP = true;
            }

            //No_RRP
            if (noRRP)
                retval = retval | Cost_Fault_Codes.RRP_Not_Set;


            //Price_More_Than_RRP
            if (!retval.HasFlag(Cost_Fault_Codes.Price_Not_Set))
            {
                if ((wPrice > wRRP) & (!noRRP))
                {
                    retval = retval | Cost_Fault_Codes.Price_More_Than_RRP;
                }

                //Price_Less_Than_Cost
                if (!retval.HasFlag(Cost_Fault_Codes.Cost_Not_Set))
                {
                    if (wPrice < wCost)
                        retval = retval | Cost_Fault_Codes.Price_Less_Than_Cost;
                }
            }

            //TODO: Margin Analysis...possibly

            return retval;
        }

        public async Task<bool> Verify_SupplierTags(string tags)
        {
            CreateAPI();

            InventoryLevels iLevels = await _api.GetInventoryLevels(this.InventoryItemId.ToString());

            if (iLevels.Levels == null)
                return false;

            if (iLevels.Levels.Count > 1)
                return false;

            if (!common.DoesSupplierTagMatchesLocationID(iLevels.Levels[0].LocationId, tags))
                return false;

            return true;
        }

        public async Task<bool> FixSupplierLocation (string sup_tag, bool whatif = false)
        {
            // Get inventorylevels.  There should only be one
            // if there are multiple, find the shipping tag and match to that
            // remove the non matching ones

            // if there's one make sure shipping tag matches invlocationid.

            // if there are none then add one matched to shipping tag

            bool retval = false;
            InventoryLevels iLevels = await _api.GetInventoryLevels(this.InventoryItemId.ToString());

            Dictionary<string, long> locationList = SupplierProducer.GetSupplierLocationIDs();

            if ((iLevels.Levels == null) | (iLevels.Levels.Count == 0))
            {
                if (locationList.ContainsKey(sup_tag))
                {
                    if (!whatif)
                        await _api.ConnectInventoryItemLocation(this.InventoryItemId, locationList[sup_tag]);

                     retval = true;
                }
                else
                    retval = false;
            }

            if (iLevels.Levels.Count == 1)
            {
                if (locationList[sup_tag] != iLevels.Levels[0].LocationId)
                {
                    await _api.ConnectInventoryItemLocation(this.InventoryItemId, locationList[sup_tag]);
                    await _api.Remove_InventoryItemLocation(this.InventoryItemId, iLevels.Levels[0].LocationId);
                }
            }

            if (iLevels.Levels.Count > 1)
            { 
                foreach (InventoryLevel level in iLevels.Levels)
                {
                    if (level.LocationId != locationList[sup_tag])
                    {
                        if (!whatif)
                            await _api.Remove_InventoryItemLocation(this.InventoryItemId, level.LocationId);

                        retval = true;                        
                    }
                }
            }    

            return retval;
        }

        public async Task<bool> MakeProductUntaxable(bool whatif)
        {
            //if taxable then modify all prices to include GST
            bool retval = false;

            try
            {
                this.CompareAtPrice = (Convert.ToDouble(this.CompareAtPrice) * 1.1).ToString();
                this.Price = (Convert.ToDouble(this.Price) * 1.1).ToString();

                this.Taxable = false;

                InventoryItem invItem = await _api.GetInventoryItem(this.InventoryItemId.ToString());

                invItem.Cost = (Convert.ToDouble(invItem.Cost) * 1.1).ToString();

                if (!whatif)
                {
                    retval = retval | (await _api.UpdateVariant(this));
                    retval = retval | (await _api.UpdateInventory(invItem));
                }
            }
            catch 
            {
                retval = false;
            }

            return retval;
        }
    }
}