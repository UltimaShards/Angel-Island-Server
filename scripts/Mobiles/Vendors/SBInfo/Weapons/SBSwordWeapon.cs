/***************************************************************************
 *
 *   RunUO                   : May 1, 2002
 *   portions copyright      : (C) The RunUO Software Team
 *   email                   : info@runuo.com
 *   
 *   Angel Island UO Shard   : March 25, 2004
 *   portions copyright      : (C) 2004-2024 Tomasello Software LLC.
 *   email                   : luke@tomasello.com
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

/* ChangeLog
 *  11/8/22, Adam: (Vendor System)
 *  Complete overhaul of the vendor system:
 * - DisplayCache:
 *   Display cache objects are now strongly typed and there is a separate list for each.
 *   I still dislike the fact it is a �Container�, but we can live with that.
 *   Display cache objects are serialized and are deleted on each server restart as the vendors rebuild the list.
 *   Display cache objects are marked as �int map storage� so Cron doesn�t delete them.
 * - ResourcePool:
 *   Properly exclude all ResourcePool interactions when resource pool is not being used. (Buy/Sell now works correctly without ResourcePool.)
 *   Normalize/automate all ResourcePool resources for purchase/sale within a vendor. If a vendor Sells X, he will Buy X. 
 *   At the top of each SB<merchant> there is a list of ResourcePool types. This list is uniformly looped over for creating all ResourcePool buy/sell entries.
 * - Standard Pricing Database
 *   No longer do we hardcode in what we believe the buy/sell price of X is. We now use a Standard Pricing Database for assigning these prices.
 *   I.e., BaseVendor.PlayerPays(typeof(Drums)), or BaseVendor.VendorPays(typeof(Drums))
 *   This database was derived from RunUO 2.6 first and added items not covered from Angel Island 5.
 *   The database was then filtered, checked, sorted and committed. 
 * - Make use of Rule Sets as opposed to hardcoding shard configurations everywhere.
 *   Exampes:
 *   if (Core.UOAI_SVR) => if (Core.RuleSets.AngelIslandRules())
 *   if (Server.Engines.ResourcePool.ResourcePool.Enabled) => if (Core.RuleSets.ResourcePoolRules())
 *   etc. In this way we centrally adjust who sell/buys what when. And using the SPD above, for what price.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using Server.Items;
using System.Collections;

namespace Server.Mobiles
{
    public class SBSwordWeapon : SBInfo
    {
        private ArrayList m_BuyInfo = new InternalBuyInfo();
        private IShopSellInfo m_SellInfo = new InternalSellInfo();

        public SBSwordWeapon()
        {
        }

        public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
        public override ArrayList BuyInfo { get { return m_BuyInfo; } }

        public class InternalBuyInfo : ArrayList
        {
            public InternalBuyInfo()
            {
                Add(new GenericBuyInfo(typeof(Broadsword), BaseVendor.PlayerPays(typeof(Broadsword)), 20, 0xF5E, 0));
                Add(new GenericBuyInfo(typeof(Cutlass), BaseVendor.PlayerPays(typeof(Cutlass)), 20, 0x1441, 0));
                Add(new GenericBuyInfo(typeof(Katana), BaseVendor.PlayerPays(typeof(Katana)), 20, 0x13FF, 0));
                Add(new GenericBuyInfo(typeof(Kryss), BaseVendor.PlayerPays(typeof(Kryss)), 20, 0x1401, 0));
                Add(new GenericBuyInfo(typeof(Longsword), BaseVendor.PlayerPays(typeof(Longsword)), 20, 0xF61, 0));
                Add(new GenericBuyInfo(typeof(Scimitar), BaseVendor.PlayerPays(typeof(Scimitar)), 20, 0x13B6, 0));
                Add(new GenericBuyInfo(typeof(ThinLongsword), BaseVendor.PlayerPays(typeof(ThinLongsword)), 20, 0x13B8, 0));
                Add(new GenericBuyInfo(typeof(VikingSword), BaseVendor.PlayerPays(typeof(VikingSword)), 20, 0x13B9, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                if (Core.RuleSets.ShopkeepersBuyItemsRules())
                {
                    Add(typeof(Broadsword), BaseVendor.VendorPays(typeof(Broadsword)));
                    Add(typeof(Cutlass), BaseVendor.VendorPays(typeof(Cutlass)));
                    Add(typeof(Katana), BaseVendor.VendorPays(typeof(Katana)));
                    Add(typeof(Kryss), BaseVendor.VendorPays(typeof(Kryss)));
                    Add(typeof(Longsword), BaseVendor.VendorPays(typeof(Longsword)));
                    Add(typeof(Scimitar), BaseVendor.VendorPays(typeof(Scimitar)));
                    Add(typeof(ThinLongsword), BaseVendor.VendorPays(typeof(ThinLongsword)));
                    Add(typeof(VikingSword), BaseVendor.VendorPays(typeof(VikingSword)));

                    if (Core.AOS)
                    {
                        Add(typeof(Scythe), 23);
                        Add(typeof(BoneHarvester), 18);
                        Add(typeof(Scepter), 18);
                        Add(typeof(BladedStaff), 22);
                        Add(typeof(Pike), 21);
                        Add(typeof(DoubleBladedStaff), 20);
                        Add(typeof(Lance), 20);
                        Add(typeof(CrescentBlade), 22);
                    }
                }
            }
        }
    }
}