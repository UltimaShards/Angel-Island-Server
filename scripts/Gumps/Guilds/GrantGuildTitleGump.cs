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

/* Scripts/Gumps/Guilds/GrantGuildTitleGump.cs
 * CHANGELOG:
 * 01/23/22 Pix
 *      Changed base class to GuildMobileListServerPagedGump
 */

using Server.Guilds;
using Server.Network;

namespace Server.Gumps
{
    public class GrantGuildTitleGump : GuildMobileListServerPagedGump
    {
        public GrantGuildTitleGump(Mobile from, Guild guild)
            : this(from, guild, 0)
        {
        }

        public GrantGuildTitleGump(Mobile from, Guild guild, int page)
            : base(from, guild, true, guild.Members, page)
        {
        }

        protected override void Design()
        {
            AddHtmlLocalized(20, 10, 400, 35, 1011118, false, false); // Grant a title to another member.

            AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(55, 400, 245, 30, 1011127, false, false); // I dub thee...

            AddButton(300, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddHtmlLocalized(335, 400, 100, 35, 1011012, false, false); // CANCEL
        }

        protected override void SendPage(Mobile from, Guild guild, int newpage)
        {
            m_Mobile.SendGump(new GrantGuildTitleGump(from, guild, newpage));
        }

        protected override void HandleDesignedReponse(NetState state, RelayInfo info)
        {
            if (GuildGump.BadLeader(m_Mobile, m_Guild))
                return;

            if (info.ButtonID == 1)
            {
                int[] switches = info.Switches;

                if (switches.Length > 0)
                {
                    int index = switches[0];

                    if (index >= 0 && index < m_List.Count)
                    {
                        Mobile m = (Mobile)m_List[index];

                        if (m != null && !m.Deleted)
                        {
                            m_Mobile.SendLocalizedMessage(1013074); // New title (20 characters max):
                            m_Mobile.Prompt = new GuildTitlePrompt(m_Mobile, m, m_Guild);
                        }
                    }
                }
            }
            else if (info.ButtonID == 2)
            {
                GuildGump.EnsureClosed(m_Mobile);
                m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
            }
        }
    }
}