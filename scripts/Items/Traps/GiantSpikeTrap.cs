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

namespace Server.Items
{
    public class GiantSpikeTrap : BaseTrap
    {
        [Constructable]
        public GiantSpikeTrap()
            : base(1)
        {
        }

        public override bool PassivelyTriggered { get { return true; } }
        public override TimeSpan PassiveTriggerDelay { get { return TimeSpan.Zero; } }
        public override int PassiveTriggerRange { get { return 3; } }
        public override TimeSpan ResetDelay { get { return TimeSpan.FromSeconds(0.0); } }

        public override void OnTrigger(Mobile from)
        {
            Effects.SendLocationEffect(Location, Map, 0x1D99, 48, 2, GetEffectHue(), 0);

            if (from.Alive && CheckRange(from.Location, 0))
                Spells.SpellHelper.Damage(TimeSpan.FromTicks(1), from, from, Utility.Dice(10, 7, 0));

            base.OnTrigger(from);
        }

        public GiantSpikeTrap(Serial serial)
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