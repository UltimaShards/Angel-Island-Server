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

/* Scripts/Mobiles/Vendors/SBInfo/SBBard.cs
 * ChangeLog
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
 *  2/6/22, (MusicBox)
 *      Add MusicBox to inventory
 *  1/13/22, Adam
 *      Add ManuscriptPaper
 *      Add MusicCompositionBook
 *	4/24/04, mith
 *		Commented all items from SellList so that NPCs don't buy from players.
 */

using Server.Items;
using Server.Misc;
using System.Collections;

namespace Server.Mobiles
{
    public class SBBard : SBInfo
    {
        private ArrayList m_BuyInfo = new InternalBuyInfo();
        private IShopSellInfo m_SellInfo = new InternalSellInfo();

        public SBBard()
        {
        }

        public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
        public override ArrayList BuyInfo { get { return m_BuyInfo; } }

        public class InternalBuyInfo : ArrayList
        {
            public InternalBuyInfo()
            {
                Add(new GenericBuyInfo(typeof(LapHarp), BaseVendor.PlayerPays(typeof(LapHarp)), (10), 0x0EB2, 0));
                Add(new GenericBuyInfo(typeof(Lute), BaseVendor.PlayerPays(typeof(Lute)), (10), 0x0EB3, 0));
                Add(new GenericBuyInfo(typeof(Drums), BaseVendor.PlayerPays(typeof(Drums)), (10), 0x0E9C, 0));
                Add(new GenericBuyInfo(typeof(Harp), BaseVendor.PlayerPays(typeof(Harp)), (10), 0x0EB1, 0));
                Add(new GenericBuyInfo(typeof(Tambourine), BaseVendor.PlayerPays(typeof(Tambourine)), (10), 0x0E9E, 0));
                if (Core.RuleSets.AnyAIShardRules())
                {
                    Add(new GenericBuyInfo(typeof(ManuscriptPaper), BaseVendor.PlayerPays(typeof(ManuscriptPaper)), 10, 0x0E34, 0));
                    Add(new GenericBuyInfo(typeof(MusicCompositionBook), BaseVendor.PlayerPays(typeof(MusicCompositionBook)), 10, 0xFF0, 0));
                    Add(new GenericBuyInfo(typeof(MusicBox), BaseVendor.PlayerPays(typeof(MusicBox)), 10, 0x2AF9, 0));
                }

                if (Core.RuleSets.SiegeStyleRules())
                {
                    Add(new GenericBuyInfo(typeof(Ticket), 30, 100, 0x14F0, 0x8A5));
                }
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                if (Core.RuleSets.ShopkeepersBuyItemsRules())
                {   // cash buyback
                    Add(typeof(LapHarp), BaseVendor.VendorPays(typeof(LapHarp)));
                    Add(typeof(Lute), BaseVendor.VendorPays(typeof(Lute)));
                    Add(typeof(Drums), BaseVendor.VendorPays(typeof(Drums)));
                    Add(typeof(Harp), BaseVendor.VendorPays(typeof(Harp)));
                    Add(typeof(Tambourine), BaseVendor.VendorPays(typeof(Tambourine)));
                }
            }
        }
    }
}