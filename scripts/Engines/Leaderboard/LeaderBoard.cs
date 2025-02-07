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

/* Engines/Leaderboard/LeaderBoard.cs
 * ChangeLog:
 *	8/12/21, Craig
 *		Initial version
 */

using Scripts.Engines.Leaderboard;
using Server.Network;

namespace Server.Items
{
    [Flipable(0x1E5E, 0x1E5F)]
    public class LeaderBoard : Item
    {
        [Constructable]
        public LeaderBoard()
            : base(0x1E5E)
        {
            Movable = false;
            Hue = 0x3FF;
        }

        public override void OnSingleClick(Mobile from)
        {
            this.LabelTo(from, "Leaderboard");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (CheckRange(from))
            {
                from.SendGump(new LeaderboardGump(from));
            }
            else
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
        }

        public virtual bool CheckRange(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                return true;

            return (from.Map == this.Map && from.InRange(GetWorldLocation(), 2));
        }


        public LeaderBoard(Serial serial)
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