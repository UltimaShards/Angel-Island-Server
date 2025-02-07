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

/* Scripts/Gumps/Properties/SetListOptionGump.cs
 * Changelog:
 *  11/17,06, Adam
 *      Add: #pragma warning disable 429
 *      The Unreachable code complaints in this file are acceptable
 *      C:\Program Files\RunUO\Scripts\Gumps\Properties\SetListOptionGump.cs(122,69): warning CS0429: Unreachable expression code detected
 *      C:\Program Files\RunUO\Scripts\Gumps\Properties\SetListOptionGump.cs(129,90): warning CS0429: Unreachable expression code detected
 *      C:\Program Files\RunUO\Scripts\Gumps\Properties\SetListOptionGump.cs(144,37): warning CS0429: Unreachable expression code detected
 *      C:\Program Files\RunUO\Scripts\Gumps\Properties\SetListOptionGump.cs(144,82): warning CS0429: Unreachable expression code detected
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

#pragma warning disable 429

using Server.Commands;
using Server.Network;
using System.Collections;
using System.Reflection;

namespace Server.Gumps
{
    public class SetListOptionGump : Gump
    {
        protected PropertyInfo m_Property;
        protected Mobile m_Mobile;
        protected object m_Object;
        protected Stack m_Stack;
        protected int m_Page;
        protected ArrayList m_List;

        public const bool OldStyle = PropsConfig.OldStyle;

        public const int GumpOffsetX = PropsConfig.GumpOffsetX;
        public const int GumpOffsetY = PropsConfig.GumpOffsetY;

        public const int TextHue = PropsConfig.TextHue;
        public const int TextOffsetX = PropsConfig.TextOffsetX;

        public const int OffsetGumpID = PropsConfig.OffsetGumpID;
        public const int HeaderGumpID = PropsConfig.HeaderGumpID;
        public const int EntryGumpID = PropsConfig.EntryGumpID;
        public const int BackGumpID = PropsConfig.BackGumpID;
        public const int SetGumpID = PropsConfig.SetGumpID;

        public const int SetWidth = PropsConfig.SetWidth;
        public const int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
        public const int SetButtonID1 = PropsConfig.SetButtonID1;
        public const int SetButtonID2 = PropsConfig.SetButtonID2;

        public const int PrevWidth = PropsConfig.PrevWidth;
        public const int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
        public const int PrevButtonID1 = PropsConfig.PrevButtonID1;
        public const int PrevButtonID2 = PropsConfig.PrevButtonID2;

        public const int NextWidth = PropsConfig.NextWidth;
        public const int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
        public const int NextButtonID1 = PropsConfig.NextButtonID1;
        public const int NextButtonID2 = PropsConfig.NextButtonID2;

        public const int OffsetSize = PropsConfig.OffsetSize;

        public const int EntryHeight = PropsConfig.EntryHeight;
        public const int BorderSize = PropsConfig.BorderSize;

        private const int EntryWidth = 212;
        private const int EntryCount = 13;

        private const int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;

        private const int BackWidth = BorderSize + TotalWidth + BorderSize;

        private static bool PrevLabel = OldStyle, NextLabel = OldStyle;

        private const int PrevLabelOffsetX = PrevWidth + 1;
        private const int PrevLabelOffsetY = 0;

        private const int NextLabelOffsetX = -29;
        private const int NextLabelOffsetY = 0;

        protected object[] m_Values;

        public SetListOptionGump(PropertyInfo prop, Mobile mobile, object o, Stack stack, int propspage, ArrayList list, string[] names, object[] values)
            : base(GumpOffsetX, GumpOffsetY)
        {
            m_Property = prop;
            m_Mobile = mobile;
            m_Object = o;
            m_Stack = stack;
            m_Page = propspage;
            m_List = list;

            m_Values = values;

            int pages = (names.Length + EntryCount - 1) / EntryCount;
            int index = 0;

            for (int page = 1; page <= pages; ++page)
            {
                AddPage(page);

                int start = (page - 1) * EntryCount;
                int count = names.Length - start;

                if (count > EntryCount)
                    count = EntryCount;

                int totalHeight = OffsetSize + ((count + 2) * (EntryHeight + OffsetSize));
                int backHeight = BorderSize + totalHeight + BorderSize;

                AddBackground(0, 0, BackWidth, backHeight, BackGumpID);
                AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), totalHeight, OffsetGumpID);

                int x = BorderSize + OffsetSize;
                int y = BorderSize + OffsetSize;

                int emptyWidth = TotalWidth - PrevWidth - NextWidth - (OffsetSize * 4) - (OldStyle ? SetWidth + OffsetSize : 0);

                AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);

                if (page > 1)
                {
                    AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 0, GumpButtonType.Page, page - 1);

                    if (PrevLabel)
                        AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
                }

                x += PrevWidth + OffsetSize;

                if (!OldStyle)
                    AddImageTiled(x - (OldStyle ? OffsetSize : 0), y, emptyWidth + (OldStyle ? OffsetSize * 2 : 0), EntryHeight, HeaderGumpID);

                x += emptyWidth + OffsetSize;

                if (!OldStyle)
                    AddImageTiled(x, y, NextWidth, EntryHeight, HeaderGumpID);

                if (page < pages)
                {
                    AddButton(x + NextOffsetX, y + NextOffsetY, NextButtonID1, NextButtonID2, 0, GumpButtonType.Page, page + 1);

                    if (NextLabel)
                        AddLabel(x + NextLabelOffsetX, y + NextLabelOffsetY, TextHue, "Next");
                }

                AddRect(0, prop.Name, 0);

                for (int i = 0; i < count; ++i)
                    AddRect(i + 1, names[index], ++index);
            }
        }

        private void AddRect(int index, string str, int button)
        {
            int x = BorderSize + OffsetSize;
            int y = BorderSize + OffsetSize + ((index + 1) * (EntryHeight + OffsetSize));

            AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
            AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, str);

            x += EntryWidth + OffsetSize;

            if (SetGumpID != 0)
                AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);

            if (button != 0)
                AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, button, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int index = info.ButtonID - 1;

            if (index >= 0 && index < m_Values.Length)
            {
                try
                {
                    object toSet = m_Values[index];
                    Server.Commands.CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, (toSet == null ? Properties.PropNull : toSet.ToString()));
                    m_Property.SetValue(m_Object, toSet, null);
                    PropertiesGump.OnValueChanged(m_Object, m_Property, m_Stack);
                }
                catch
                {
                    m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
                }
            }

            m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
        }
    }
}