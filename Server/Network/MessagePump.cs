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

/* Server\Network\MessagePump.cs
 * CHANGELOG:
 *  8/1/22, Adam
 *      Update Console messages to Yellow for "Client:" messages.
 *	12/23/08, Adam
 *		Add missing network and timer synchronization from RunUO 2.0
 */

using Server.Diagnostics;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.Network
{
    public class MessagePump
    {
        private Listener[] m_Listeners;
        private Queue<NetState> m_Queue;
        private Queue<NetState> m_WorkingQueue;
        private Queue<NetState> m_Throttled;

        public MessagePump()
        {
            IPEndPoint[] ipep = Listener.EndPoints;

            m_Listeners = new Listener[ipep.Length];

            bool success = false;

            do
            {
                for (int i = 0; i < ipep.Length; i++)
                {
                    Listener l = new Listener(ipep[i]);
                    if (!success && l != null)
                        success = true;
                    m_Listeners[i] = l;
                }

                if (!success)
                {
                    Utility.ConsoleWriteLine(String.Format("Retrying..."), ConsoleColor.Yellow);
                    Thread.Sleep(10000);
                }
            } while (!success);

            m_Queue = new Queue<NetState>();
            m_WorkingQueue = new Queue<NetState>();
            m_Throttled = new Queue<NetState>();
        }

        public Listener[] Listeners
        {
            get { return m_Listeners; }
            set { m_Listeners = value; }
        }

        public void AddListener(Listener l)
        {
            Listener[] old = m_Listeners;

            m_Listeners = new Listener[old.Length + 1];

            for (int i = 0; i < old.Length; ++i)
                m_Listeners[i] = old[i];

            m_Listeners[old.Length] = l;
        }

        private void CheckListener()
        {
            for (int j = 0; j < m_Listeners.Length; ++j)
            {
                Socket[] accepted = m_Listeners[j].Slice();

                for (int i = 0; i < accepted.Length; ++i)
                {
                    NetState ns = new NetState(accepted[i], this);
                    ns.Start();

                    if (ns.Running)
                        Utility.ConsoleWriteLine(String.Format("Client: {0}: Connected. [{1} Online]", ns, NetState.Instances.Count), ConsoleColor.Yellow);
                }
            }
        }

        public void OnReceive(NetState ns)
        {
            lock (this)
                m_Queue.Enqueue(ns);

            Core.Set();
        }

        public void Slice()
        {
            CheckListener();

            lock (this)
            {
                Queue<NetState> temp = m_WorkingQueue;
                m_WorkingQueue = m_Queue;
                m_Queue = temp;
            }

            while (m_WorkingQueue.Count > 0)
            {
                NetState ns = m_WorkingQueue.Dequeue();

                if (ns.Running)
                    HandleReceive(ns);
            }

            lock (this)
            {
                while (m_Throttled.Count > 0)
                    m_Queue.Enqueue(m_Throttled.Dequeue());
            }
        }

        private const int BufferSize = 4096;
        private BufferPool m_Buffers = new BufferPool("Processor", 4, BufferSize);

        private bool HandleSeed(NetState ns, ByteQueue buffer)
        {
            if (buffer.GetPacketID() == 0xEF)
            {
                // new packet in client	6.0.5.0	replaces the traditional seed method with a	seed packet
                // 0xEF	= 239 =	multicast IP, so this should never appear in a normal seed.	 So	this is	backwards compatible with older	clients.
                ns.Seeded = true;
                return true;
            }
            else if (buffer.Length >= 4)
            {
                byte[] m_Peek = new byte[4];

                buffer.Dequeue(m_Peek, 0, 4);

                int seed = (m_Peek[0] << 24) | (m_Peek[1] << 16) | (m_Peek[2] << 8) | m_Peek[3];

                if (seed == 0)
                {
                    Utility.ConsoleWriteLine(String.Format("Login: {0}: Invalid client detected, disconnecting", ns), ConsoleColor.Yellow);
                    ns.Dispose();
                    return false;
                }

                ns.m_Seed = seed;
                ns.Seeded = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckEncrypted(NetState ns, int packetID)
        {
            if (!ns.SentFirstPacket && packetID != 0xF0 && packetID != 0xF1 && packetID != 0xCF && packetID != 0x80 && packetID != 0x91 && packetID != 0xA4 && packetID != 0xEF)
            {
                Utility.ConsoleWriteLine(String.Format("Client: {0}: Encrypted client detected, disconnecting", ns), ConsoleColor.Yellow);
                ns.Dispose();
                return true;
            }
            return false;
        }

        public void HandleReceive(NetState ns)
        {
            ByteQueue buffer = ns.Buffer;

            if (buffer == null || buffer.Length <= 0)
                return;

            lock (buffer)
            {
                if (!ns.Seeded)
                {
                    if (!HandleSeed(ns, buffer))
                        return;
                }

                int length = buffer.Length;

                while (length > 0 && ns.Running)
                {
                    int packetID = buffer.GetPacketID();

                    if (CheckEncrypted(ns, packetID))
                        break;

                    PacketHandler handler = ns.GetHandler(packetID);

                    if (handler == null)
                    {
                        byte[] data = new byte[length];
                        length = buffer.Dequeue(data, 0, length);
                        new PacketReader(data, length, false).Trace(ns);
                        break;
                    }

                    int packetLength = handler.Length;

                    if (packetLength <= 0)
                    {
                        if (length >= 3)
                        {
                            packetLength = buffer.GetPacketLength();

                            if (packetLength < 3)
                            {
                                ns.Dispose();
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (length >= packetLength)
                    {
                        if (handler.Ingame)
                        {
                            if (ns.Mobile == null)
                            {
                                Utility.ConsoleWriteLine(String.Format("Client: {0}: Sent ingame packet (0x{1:X2}) before having been attached to a mobile", ns, packetID), ConsoleColor.Yellow);
                                ns.Dispose();
                                break;
                            }
                            else if (ns.Mobile.Deleted)
                            {
                                ns.Dispose();
                                break;
                            }
                        }

                        ThrottlePacketCallback throttler = handler.ThrottleCallback;

                        if (throttler != null && !throttler(ns))
                        {
                            m_Throttled.Enqueue(ns);
                            return;
                        }

                        PacketReceiveProfile prof = null;

                        if (Core.Profiling) prof = PacketReceiveProfile.Acquire(packetID);

                        if (prof != null)
                        {
                            prof.Start();
                        }

                        byte[] packetBuffer;

                        if (BufferSize >= packetLength)
                            packetBuffer = m_Buffers.AcquireBuffer();
                        else
                            packetBuffer = new byte[packetLength];

                        packetLength = buffer.Dequeue(packetBuffer, 0, packetLength);

                        PacketReader r = new PacketReader(packetBuffer, packetLength, handler.Length != 0);

                        handler.OnReceive(ns, r);
                        length = buffer.Length;

                        if (BufferSize >= packetLength)
                            m_Buffers.ReleaseBuffer(packetBuffer);

                        if (prof != null)
                        {
                            prof.Finish(packetLength);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}