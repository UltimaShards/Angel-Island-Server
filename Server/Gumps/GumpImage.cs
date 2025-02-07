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

using Server.Network;
using System;

namespace Server.Gumps
{
    public class GumpImage : GumpEntry
    {
        private int m_X, m_Y;
        private int m_GumpID;
        private int m_Hue;

        public GumpImage(int x, int y, int gumpID)
            : this(x, y, gumpID, 0)
        {
        }

        public GumpImage(int x, int y, int gumpID, int hue)
        {
            m_X = x;
            m_Y = y;
            m_GumpID = gumpID;
            m_Hue = hue;
        }

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int GumpID
        {
            get
            {
                return m_GumpID;
            }
            set
            {
                Delta(ref m_GumpID, value);
            }
        }

        public int Hue
        {
            get
            {
                return m_Hue;
            }
            set
            {
                Delta(ref m_Hue, value);
            }
        }

        public override string Compile()
        {
            if (m_Hue == 0)
                return String.Format("{{ gumppic {0} {1} {2} }}", m_X, m_Y, m_GumpID);
            else
                return String.Format("{{ gumppic {0} {1} {2} hue={3} }}", m_X, m_Y, m_GumpID, m_Hue);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("gumppic");
        private static byte[] m_HueEquals = Gump.StringToBuffer(" hue=");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_GumpID);

            if (m_Hue != 0)
            {
                disp.AppendLayout(m_HueEquals);
                disp.AppendLayoutNS(m_Hue);
            }
        }
    }
}