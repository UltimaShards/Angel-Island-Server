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

/* Scripts/Engines/Chat/ChatLogger.cs
 * ChangeLog:
 *	4/23/23, Yoar
 *	    Initial version.
 *	    
 *	    Added chat logging.
 */

using Server.Diagnostics;
using Server.Network;
using System;

namespace Server.Engines.Chat
{
    public static class ChatLogger
    {
        public static void Log(NetState state, string lang, int actionID, string param)
        {
            LogHelper logger = new LogHelper("Chat.log", false, true);
            string text = String.Format("Client: {0}: Chat action 0x{1:X}: {2}", state, actionID, param);
            logger.Log(text);
            logger.Finish();
        }
    }
}