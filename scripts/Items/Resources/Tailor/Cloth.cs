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

/* Scripts\Items\Resources\Tailor\Cloth.cs
 * ChangeLog
 *	10/11/21 Adam
 *	    OnSingleClick change text from:
 *	    m_map[1049123] = "piles of folded cloth (~1_PILES~ yards)";
 *	    to:
 *      m_map[1049125] = "pieces of cloth (~1_PIECES~ yards)";
 *      Folded cloth is reserved for our 'folded' cloth.
 */


using Server.Network;
using System;

namespace Server.Items
{
    [FlipableAttribute(0x1766, 0x1768)]
    public class Cloth : Item, IScissorable, IDyable, ICommodity
    {
        string ICommodity.Description
        {
            get
            {
                return String.Format(Amount == 1 ? "{0} piece of cloth" : "{0} pieces of cloth", Amount);
            }
        }

        [Constructable]
        public Cloth()
            : this(1)
        {
        }

        [Constructable]
        public Cloth(int amount)
            : base(0x1766)
        {
            Stackable = true;
            Weight = 0.1;
            Amount = amount;
        }

        public Cloth(Serial serial)
            : base(serial)
        {
        }

        public bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            Hue = sender.DyedHue;

            return true;
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

        public override void OnSingleClick(Mobile from)
        {
            int number = (Amount == 1) ? 1049126 : 1049125;

            from.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, 0x3B2, 3, number, "", Amount.ToString()));
        }

        public override Item Dupe(int amount)
        {
            return base.Dupe(new Cloth(), amount);
        }

        public bool Scissor(Mobile from, Scissors scissors)
        {
            if (Deleted || !from.CanSee(this)) return false;

            base.ScissorHelper(scissors, from, new Bandage(), 1);

            return true;
        }
    }
}