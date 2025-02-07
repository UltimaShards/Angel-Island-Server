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

using System;

namespace Server.Mobiles
{
    public class AnimalBuyInfo : GenericBuyInfo
    {
        private int m_ControlSlots;

        public AnimalBuyInfo(int controlSlots, Type type, int price, int amount, int itemID, int hue)
            : this(controlSlots, null, type, price, amount, itemID, hue)
        {
        }

        public AnimalBuyInfo(int controlSlots, string name, Type type, int price, int amount, int itemID, int hue)
            : base(name, type, price, amount, itemID, hue)
        {
            m_ControlSlots = controlSlots;
        }

        public override int ControlSlots
        {
            get
            {
                return m_ControlSlots;
            }
        }
    }
}