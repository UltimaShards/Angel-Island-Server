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

/* Scripts/Mobiles/Vendors/SBInfo/SBBanker.cs
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
 *	04/19/05, Kit
 *		Added  VendorRentalContract
 *	05/02/05 TK
 *		Added Account Book
 * */


using Server.Items;
using System.Collections;

namespace Server.Mobiles
{
    public class SBBanker : SBInfo
    {
        private ArrayList m_BuyInfo = new InternalBuyInfo();
        private IShopSellInfo m_SellInfo = new InternalSellInfo();

        public SBBanker()
        {
        }

        public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
        public override ArrayList BuyInfo { get { return m_BuyInfo; } }

        public class InternalBuyInfo : ArrayList
        {
            public InternalBuyInfo()
            {
                if (Core.RuleSets.CommodityDeedRules())
                    Add(new GenericBuyInfo("1047016", typeof(CommodityDeed), BaseVendor.PlayerPays(typeof(CommodityDeed)), 20, 0x14F0, 0x47));

                // not sure about the publish date here - barkeep contracts appeared September 13, 2001, publish 13, so maybe?
                if (Core.RuleSets.AngelIslandRules() || (Core.RuleSets.StandardShardRules() && PublishInfo.Publish >= 13.5))
                {
                    Add(new GenericBuyInfo("1041243", typeof(ContractOfEmployment), BaseVendor.PlayerPays(typeof(ContractOfEmployment)), 20, 0x14F0, 0));
                    Add(new GenericBuyInfo("vendor rental contract", typeof(VendorRentalContract), BaseVendor.PlayerPays(typeof(VendorRentalContract)), 20, 0x14F0, 0));
                }

                if (Core.RuleSets.ResourcePoolRules())
                    Add(new GenericBuyInfo("account book", typeof(AccountBook), BaseVendor.PlayerPays(typeof(AccountBook)), 10, 0xFF1, 0));

                if (Core.RuleSets.AngelIslandRules())
                    Add(new GenericBuyInfo("certificate of identity", typeof(CertificateOfIdentity), BaseVendor.PlayerPays(typeof(CertificateOfIdentity)), 20, 0x14F0, 0));

            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
            }
        }
    }
}