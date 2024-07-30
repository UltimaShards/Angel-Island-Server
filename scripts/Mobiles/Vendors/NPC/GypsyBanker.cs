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

using Server.Items;

namespace Server.Mobiles
{
    public class GypsyBanker : Banker
    {
        public override bool IsActiveVendor { get { return false; } }
        public override NpcGuild NpcGuild { get { return NpcGuild.None; } }
        public override bool ClickTitle { get { return false; } }

        [Constructable]
        public GypsyBanker()
        {
            Title = "the gypsy banker";
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            switch (Utility.Random(4))
            {
                case 0: AddItem(new JesterHat(RandomBrightHue())); break;
                case 1: AddItem(new Bandana(RandomBrightHue())); break;
                case 2: AddItem(new SkullCap(RandomBrightHue())); break;
            }

            Item item = FindItemOnLayer(Layer.Pants);

            if (item != null)
                item.Hue = RandomBrightHue();

            item = FindItemOnLayer(Layer.Shoes);

            if (item != null)
                item.Hue = RandomBrightHue();

            item = FindItemOnLayer(Layer.OuterLegs);

            if (item != null)
                item.Hue = RandomBrightHue();

            item = FindItemOnLayer(Layer.InnerLegs);

            if (item != null)
                item.Hue = RandomBrightHue();

            item = FindItemOnLayer(Layer.OuterTorso);

            if (item != null)
                item.Hue = RandomBrightHue();

            item = FindItemOnLayer(Layer.InnerTorso);

            if (item != null)
                item.Hue = RandomBrightHue();

            item = FindItemOnLayer(Layer.Shirt);

            if (item != null)
                item.Hue = RandomBrightHue();
        }

        public GypsyBanker(Serial serial)
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