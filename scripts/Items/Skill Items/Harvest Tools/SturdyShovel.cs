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

using Server.Engines.Harvest;

namespace Server.Items
{
    public class SturdyShovel : BaseHarvestTool
    {
        public override int LabelNumber { get { return 1045125; } } // sturdy shovel
        public override HarvestSystem HarvestSystem { get { return Mining.System; } }

        [Constructable]
        public SturdyShovel()
            : this(180)
        {
        }

        [Constructable]
        public SturdyShovel(int uses)
            : base(uses, 0xF39)
        {
            Weight = 5.0;
            Hue = 0x973;
        }

        public SturdyShovel(Serial serial)
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