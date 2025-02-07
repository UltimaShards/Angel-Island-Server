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

/* Scripts/Items/Addons/BrazierAddon.cs
 * ChangeLog
 *	7/27/23, Yoar
 *		Initial version
 */

using Server.Township;

namespace Server.Items
{
    [TownshipAddon]
    public class BrazierAddon : BaseAddon
    {
        public override bool Redeedable { get { return true; } }
        public override BaseAddonDeed Deed { get { return new BrazierAddonDeed(); } }

        [Constructable]
        public BrazierAddon()
        {
            AddComponent(new AddonComponent(0x0E31), 0, 0, 0);
        }

        public BrazierAddon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class BrazierAddonDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new BrazierAddon(); } }
        public override string DefaultName { get { return "brazier"; } }

        [Constructable]
        public BrazierAddonDeed()
        {
        }

        public BrazierAddonDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}