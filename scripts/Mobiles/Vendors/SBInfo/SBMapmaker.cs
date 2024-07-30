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

/* Scripts/Mobiles/Vendors/SBInfo/SBMapmaker.cs
 * ChangeLog
 *  6/12/23, Yoar
 *      Enabled sale of preset maps for all shards. It's era accurate.
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
 *  10/14/04, Froste
 *      Changed the amount argument to GenericBuyInfo from 999 to 20 for MapmakersPen, so the argument means something in GenericBuy.cs
 *  9/24/04, Jade
 *      Changed BlankScroll price from 12gp to 5gp to be uniform with other scroll-selling npcs.
 *	4/24/04, mith
 *		Commented all items from SellList so that NPCs don't buy from players.
 */

using Server.Items;
using System.Collections;

namespace Server.Mobiles
{
    public class SBMapmaker : SBInfo
    {
        private ArrayList m_BuyInfo = new InternalBuyInfo();
        private IShopSellInfo m_SellInfo = new InternalSellInfo();

        public SBMapmaker()
        {
        }

        public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
        public override ArrayList BuyInfo { get { return m_BuyInfo; } }

        public class InternalBuyInfo : ArrayList
        {
            public InternalBuyInfo()
            {
                Add(new GenericBuyInfo(typeof(BlankScroll), BaseVendor.PlayerPays(typeof(BlankScroll)), SBScribe.ScrollStockMin, SBScribe.ScrollStockMin, SBScribe.ScrollStockMax, 0x0E34, 0));
                Add(new GenericBuyInfo(typeof(MapmakersPen), BaseVendor.PlayerPays(typeof(MapmakersPen)), 20, 0x0FBF, 0));
                Add(new GenericBuyInfo(typeof(BlankMap), BaseVendor.PlayerPays(typeof(BlankMap)), 40, 0x14EC, 0));

                for (int i = 0; i < PresetMapEntry.Table.Length; ++i)
                    Add(new PresetMapBuyInfo(PresetMapEntry.Table[i], Utility.RandomMinMax(7, 10), 20));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                if (Core.RuleSets.ShopkeepersBuyItemsRules())
                {   // cash buyback
                    Add(typeof(BlankScroll), BaseVendor.VendorPays(typeof(BlankScroll)));
                    Add(typeof(MapmakersPen), BaseVendor.VendorPays(typeof(MapmakersPen)));
                    Add(typeof(BlankMap), BaseVendor.VendorPays(typeof(BlankMap)));
                }
            }
        }
    }
}