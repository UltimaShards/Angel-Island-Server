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
using System.Collections;
using System.IO;

namespace Server.Engines.BulkOrders
{
    public class SmallBulkEntry
    {
        private Type m_Type;
        private int m_Number;
        private int m_Graphic;

        public Type Type { get { return m_Type; } }
        public int Number { get { return m_Number; } }
        public int Graphic { get { return m_Graphic; } }

        public SmallBulkEntry(Type type, int number, int graphic)
        {
            m_Type = type;
            m_Number = number;
            m_Graphic = graphic;
        }

        public static SmallBulkEntry[] BlacksmithWeapons
        {
            get { return GetEntries("Blacksmith", "weapons"); }
        }

        public static SmallBulkEntry[] BlacksmithArmor
        {
            get { return GetEntries("Blacksmith", "armor"); }
        }

        public static SmallBulkEntry[] TailorCloth
        {
            get { return GetEntries("Tailoring", "cloth"); }
        }

        public static SmallBulkEntry[] TailorLeather
        {
            get { return GetEntries("Tailoring", "leather"); }
        }

        private static Hashtable m_Cache;

        public static SmallBulkEntry[] GetEntries(string type, string name)
        {
            if (m_Cache == null)
                m_Cache = new Hashtable();

            Hashtable table = (Hashtable)m_Cache[type];

            if (table == null)
                m_Cache[type] = table = new Hashtable();

            SmallBulkEntry[] entries = (SmallBulkEntry[])table[name];

            if (entries == null)
                table[name] = entries = LoadEntries(type, name);

            return entries;
        }

        public static SmallBulkEntry[] LoadEntries(string type, string name)
        {
            string filename = String.Format("{0}/{1}.cfg", type, name);
            return LoadEntries(Path.Combine(Core.DataDirectory, "Bulk Orders", filename)); //String.Format("Data/Bulk Orders/{0}/{1}.cfg", type, name));
        }

        public static SmallBulkEntry[] LoadEntries(string path)
        {
            path = Path.Combine(Core.BaseDirectory, path);

            ArrayList list = new ArrayList();

            if (File.Exists(path))
            {
                using (StreamReader ip = new StreamReader(path))
                {
                    string line;

                    while ((line = ip.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line.StartsWith("#"))
                            continue;

                        try
                        {
                            string[] split = line.Split('\t');

                            if (split.Length >= 2)
                            {
                                Type type = ScriptCompiler.FindTypeByName(split[0]);
                                int graphic = Utility.ToInt32(split[split.Length - 1]);

                                if (type != null && graphic > 0)
                                    list.Add(new SmallBulkEntry(type, 1020000 + graphic, graphic));
                            }
                        }
                        catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
                    }
                }
            }

            return (SmallBulkEntry[])list.ToArray(typeof(SmallBulkEntry));
        }
    }
}