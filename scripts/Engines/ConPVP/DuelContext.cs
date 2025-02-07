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

/* Scripts\Engines\ConPVP\DuelContext.cs
 * Changelog:
 *  3/14/22, Yoar
 *      Implementation of RunUO's ConPVP system.
 */

using Server.Engines.PartySystem;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Seventh;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engines.ConPVP
{
    public delegate void CountdownCallback(int count);

    public class DuelContext
    {
        private Mobile m_Initiator;
        private ArrayList m_Participants;
        private Ruleset m_Ruleset;
        private Arena m_Arena;
        private bool m_Registered = true;
        private bool m_Finished, m_Started;

        private bool m_ReadyWait;
        private int m_ReadyCount;

        private bool m_Rematch;

        public bool Rematch { get { return m_Rematch; } }

        public bool ReadyWait { get { return m_ReadyWait; } }
        public int ReadyCount { get { return m_ReadyCount; } }

        public bool Registered { get { return m_Registered; } }
        public bool Finished { get { return m_Finished; } }
        public bool Started { get { return m_Started; } }

        public Mobile Initiator { get { return m_Initiator; } }
        public ArrayList Participants { get { return m_Participants; } }
        public Ruleset Ruleset { get { return m_Ruleset; } }
        public Arena Arena { get { return m_Arena; } }

        private bool CantDoAnything(Mobile mob)
        {
            if (m_EventGame != null)
                return m_EventGame.CantDoAnything(mob);
            else
                return false;
        }

        public static bool IsActivelyDueling(Mobile mob)
        {
            PlayerMobile pm = mob as PlayerMobile;

            if (pm == null || pm.DuelContext == null || !pm.DuelContext.m_StartedBeginCountdown)
                return false;

            DuelPlayer pl = pm.DuelContext.Find(mob);

            if (pl == null || pl.Eliminated)
                return false;

            return true;
        }

        public static bool IsFreeConsume(Mobile mob)
        {
            PlayerMobile pm = mob as PlayerMobile;

            if (pm == null || pm.DuelContext == null || !pm.DuelContext.m_StartedBeginCountdown)
                return false;

            DuelPlayer pl = pm.DuelContext.Find(mob);

            if (pl == null || pl.Eliminated)
                return false;

            return (pm.DuelContext.m_Ruleset.GetOption("Items", "Free Consume") || (pm.DuelContext.m_EventGame != null && pm.DuelContext.m_EventGame.FreeConsume));
        }

        public void DelayBounce(TimeSpan ts, Mobile mob, Container corpse)
        {
            Timer.DelayCall(ts, new TimerStateCallback(DelayBounce_Callback), new object[] { mob, corpse });
        }

        // Yoar: we don't have special moves
#if false
        public static bool AllowSpecialMove(Mobile from, string name, SpecialMove move)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return true;

            DuelContext dc = pm.DuelContext;

            return (dc == null || dc.InstAllowSpecialMove(from, name, move));
        }

        public bool InstAllowSpecialMove(Mobile from, string name, SpecialMove move)
        {

            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (CantDoAnything(from))
                return false;

            string title = null;

            if (move is NinjaMove)
                title = "Bushido";
            else if (move is SamuraiMove)
                title = "Ninjitsu";


            if (title == null || name == null || m_Ruleset.GetOption(title, name))
                return true;

            from.SendMessage("The dueling ruleset prevents you from using this move.");
            return false;
        }
#endif

        public bool AllowSpellCast(Mobile from, Spell spell)
        {
            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (CantDoAnything(from))
                return false;

            if (spell is Server.Spells.Fourth.RecallSpell)
                from.SendMessage("You may not cast this spell.");

            string title = null, option = null;

            // Yoar: we only have pre-AOS spells
#if false
            if (spell is ArcanistSpell)
            {
                title = "Spellweaving";
                option = spell.Name;
            }
            else if (spell is PaladinSpell)
            {
                title = "Chivalry";
                option = spell.Name;
            }
            else if (spell is NecromancerSpell)
            {
                title = "Necromancy";
                option = spell.Name;
            }
            else if (spell is NinjaSpell)
            {
                title = "Ninjitsu";
                option = spell.Name;
            }
            else if (spell is SamuraiSpell)
            {
                title = "Bushido";
                option = spell.Name;
            }
            else if (spell is MagerySpell)
            {
                switch (((MagerySpell)spell).Circle)
                {
                    case SpellCircle.First: title = "1st Circle"; break;
                    case SpellCircle.Second: title = "2nd Circle"; break;
                    case SpellCircle.Third: title = "3rd Circle"; break;
                    case SpellCircle.Fourth: title = "4th Circle"; break;
                    case SpellCircle.Fifth: title = "5th Circle"; break;
                    case SpellCircle.Sixth: title = "6th Circle"; break;
                    case SpellCircle.Seventh: title = "7th Circle"; break;
                    case SpellCircle.Eighth: title = "8th Circle"; break;
                }

                option = spell.Name;
            }
            else
            {
                title = "Other Spell";
                option = spell.Name;
            }
#else
            switch (spell.Circle)
            {
                case SpellCircle.First: title = "1st Circle"; break;
                case SpellCircle.Second: title = "2nd Circle"; break;
                case SpellCircle.Third: title = "3rd Circle"; break;
                case SpellCircle.Fourth: title = "4th Circle"; break;
                case SpellCircle.Fifth: title = "5th Circle"; break;
                case SpellCircle.Sixth: title = "6th Circle"; break;
                case SpellCircle.Seventh: title = "7th Circle"; break;
                case SpellCircle.Eighth: title = "8th Circle"; break;
            }

            option = spell.Name;
#endif

            if (title == null || option == null || m_Ruleset.GetOption(title, option))
                return true;

            from.SendMessage("The dueling ruleset prevents you from casting this spell.");
            return false;
        }

        public bool AllowItemEquip(Mobile from, Item item)
        {
            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (item is Dagger || CheckItemEquip(from, item))
                return true;

            from.SendMessage("The dueling ruleset prevents you from equiping this item.");
            return false;
        }

        public static bool AllowSpecialAbility(Mobile from, string name, bool message)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return true;

            DuelContext dc = pm.DuelContext;

            return (dc == null || dc.InstAllowSpecialAbility(from, name, message));
        }

        public bool InstAllowSpecialAbility(Mobile from, string name, bool message)
        {
            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (CantDoAnything(from))
                return false;

            if (m_Ruleset.GetOption("Combat Abilities", name))
                return true;

            if (message)
                from.SendMessage("The dueling ruleset prevents you from using this combat ability.");

            return false;
        }

        public bool CheckItemEquip(Mobile from, Item item)
        {
            if (item is Fists)
            {
                if (!m_Ruleset.GetOption("Weapons", "Wrestling"))
                    return false;
            }
            else if (item is BaseArmor)
            {
                BaseArmor armor = (BaseArmor)item;

                if (armor.ProtectionLevel > ArmorProtectionLevel.Regular && !m_Ruleset.GetOption("Armor", "Magical"))
                    return false;

                if (!Core.RuleSets.AOSRules() && armor.Resource != armor.DefaultResource && !m_Ruleset.GetOption("Armor", "Colored"))
                    return false;

                if (armor is BaseShield && !m_Ruleset.GetOption("Armor", "Shields"))
                    return false;
            }
            else if (item is BaseWeapon)
            {
                BaseWeapon weapon = (BaseWeapon)item;

                if ((weapon.DamageLevel > WeaponDamageLevel.Regular || weapon.AccuracyLevel > WeaponAccuracyLevel.Regular) && !m_Ruleset.GetOption("Weapons", "Magical"))
                    return false;

                if (!Core.RuleSets.AOSRules() && weapon.Resource != CraftResource.Iron && weapon.Resource != CraftResource.None && !m_Ruleset.GetOption("Weapons", "Runics"))
                    return false;

                if (weapon is BaseRanged && !m_Ruleset.GetOption("Weapons", "Ranged"))
                    return false;

                if (!(weapon is BaseRanged) && !m_Ruleset.GetOption("Weapons", "Melee"))
                    return false;

                if (weapon.PoisonCharges > 0 && weapon.Poison != null && !m_Ruleset.GetOption("Weapons", "Poisoned"))
                    return false;
            }

            if (item is IMagicItem && ((IMagicItem)item).MagicEffect != MagicItemEffect.None && !m_Ruleset.GetOption("Items", "Wands"))
                return false;

            if (item is IMagicEquip && ((IMagicEquip)item).MagicEffect != MagicEquipEffect.None && !m_Ruleset.GetOption("Items", "Wands"))
                return false;

            return true;
        }

        public bool AllowSkillUse(Mobile from, SkillName skill)
        {
            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (CantDoAnything(from))
                return false;

            if (Core.RuleSets.AOSRules() && skill == SkillName.SpiritSpeak && IsSuddenDeath)
            {
                from.SendMessage(0x22, "You may not use spirit speak in sudden death.");
                return false;
            }

            int id = (int)skill;

            if (id >= 0 && id < SkillInfo.Table.Length)
            {
                if (m_Ruleset.GetOption("Skills", SkillInfo.Table[id].Name))
                    return true;
            }

            from.SendMessage("The dueling ruleset prevents you from using this skill.");
            return false;
        }

        public bool AllowItemUse(Mobile from, Item item)
        {
            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (!(item is BaseRefreshPotion))
            {
                if (CantDoAnything(from))
                    return false;
            }

            string title = null, option = null;

            if (item is BasePotion)
            {
                title = "Potions";

                if (item is BaseAgilityPotion)
                    option = "Agility";
                else if (item is BaseCurePotion)
                    option = "Cure";
                else if (item is BaseHealPotion)
                    option = "Heal";
                else if (item is NightSightPotion)
                    option = "Nightsight";
                else if (item is BasePoisonPotion)
                    option = "Poison";
                else if (item is BaseStrengthPotion)
                    option = "Strength";
                else if (item is BaseExplosionPotion)
                    option = "Explosion";
                else if (item is BaseRefreshPotion)
                    option = "Refresh";
                // Yoar: we don't have these potions
#if false
                else if (item is BaseConfusionBlastPotion)
                    option = "Confusion Blast";
                else if (item is BaseConflagrationPotion)
                    option = "Conflagration";
                else if (item is InvisibilityPotion)
                    option = "Invisibility";
#endif
            }
            else if (item is Bandage)
            {
                title = "Items";
                option = "Bandages";
            }
            else if (item is TrapableContainer)
            {
                if (((TrapableContainer)item).TrapType != TrapType.None)
                {
                    title = "Items";
                    option = "Trapped Containers";
                }
            }
            else if (item is Bola)
            {
                title = "Items";
                option = "Bolas";
            }
            else if (item is OrangePetals)
            {
                title = "Items";
                option = "Orange Petals";
            }
            else if (item is EtherealMount || item.Layer == Layer.Mount)
            {
                title = "Items";
                option = "Mounts";
            }
            // Yoar: we don't have these items
#if false
            else if (item is LeatherNinjaBelt)
            {
                title = "Items";
                option = "Shurikens";
            }
            else if (item is Fukiya)
            {
                title = "Items";
                option = "Fukiya Darts";
            }
            else if (item is FireHorn)
            {
                title = "Items";
                option = "Fire Horns";
            }
#endif
            else if (item is IMagicItem && ((IMagicItem)item).MagicEffect != MagicItemEffect.None)
            {
                title = "Items";
                option = "Wands";
            }
            else if (item is IMagicEquip && ((IMagicEquip)item).MagicEffect != MagicEquipEffect.None)
            {
                title = "Items";
                option = "Wands";
            }
            else if (item is IDuelItem)
            {
                IDuelItem duelItem = (IDuelItem)item;

                title = duelItem.DuelCategory;
                option = duelItem.DuelOption;
            }

            if (title != null && option != null && m_StartedBeginCountdown && !m_Started)
            {
                from.SendMessage("You may not use this item before the duel begins.");
                return false;
            }
            else if (item is BasePotion && !(item is BaseExplosionPotion) && !(item is BaseRefreshPotion) /*&& !(item is BaseConfusionBlastPotion) && !(item is BaseConflagrationPotion)*/ && IsSuddenDeath)
            {
                from.SendMessage(0x22, "You may not drink potions in sudden death.");
                return false;
            }
            else if (item is Bandage && IsSuddenDeath)
            {
                from.SendMessage(0x22, "You may not use bandages in sudden death.");
                return false;
            }

            if (title == null || option == null || m_Ruleset.GetOption(title, option))
                return true;

            from.SendMessage("The dueling ruleset prevents you from using this item.");
            return false;
        }

        private void DelayBounce_Callback(object state)
        {
            object[] states = (object[])state;
            Mobile mob = (Mobile)states[0];
            Container corpse = (Container)states[1];

            RemoveAggressions(mob);
            SendOutside(mob);
            Refresh(mob, corpse);
            Debuff(mob);
            CancelSpell(mob);
            mob.Frozen = false;

            if (m_EventGame != null)
                m_EventGame.OnBounced(mob);
        }

        public void OnMapChanged(Mobile mob)
        {
            OnLocationChanged(mob);
        }

        public void OnLocationChanged(Mobile mob)
        {
            if (!m_Registered || !m_StartedBeginCountdown || m_Finished)
                return;

            Arena arena = m_Arena;

            if (arena == null)
                return;

            if (mob.Map == arena.Facet && arena.Bounds.Contains(mob.Location))
                return;

            DuelPlayer pl = Find(mob);

            if (pl == null || pl.Eliminated)
                return;

            if (mob.Map == Map.Internal)
            {
                // they've logged out

                if (mob.LogoutMap == arena.Facet && arena.Bounds.Contains(mob.LogoutLocation))
                {
                    // they logged out inside the arena.. set them to eject on login

                    mob.LogoutLocation = arena.Outside;
                }
            }

            pl.Eliminated = true;

            if (m_EventGame != null)
                m_EventGame.OnEliminated(mob, EliminationReason.LeftDuelingArena);

            mob.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have forfeited your position in the duel.");
            mob.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, String.Format("{0} has forfeited by leaving the dueling arena.", mob.Name));

            Participant winner = CheckCompletion();

            if (winner != null)
                Finish(winner);
        }

        private bool m_Yielding;

        public void OnDeath(Mobile mob, Container corpse)
        {
            if (!m_Registered || !m_Started)
                return;

            DuelPlayer pl = Find(mob);

            if (pl != null && !pl.Eliminated)
            {
                if (m_EventGame != null && !m_EventGame.OnDeath(mob, corpse))
                    return;

                pl.Eliminated = true;

                if (m_EventGame != null)
                    m_EventGame.OnEliminated(mob, EliminationReason.Defeated);

                Requip(mob, corpse);
                DelayBounce(TimeSpan.FromSeconds(4.0), mob, corpse);

                Participant winner = CheckCompletion();

                if (winner != null)
                {
                    Finish(winner);
                }
                else if (!m_Yielding)
                {
                    mob.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have been defeated.");
                    mob.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, String.Format("{0} has been defeated.", mob.Name));
                }
            }
        }

        public bool CheckFull()
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                if (p.HasOpenSlot)
                    return false;
            }

            return true;
        }

        public void Requip(Mobile from, Container cont)
        {
            if (m_EventGame != null && m_EventGame.LootableCorpses)
                return;

            Corpse corpse = cont as Corpse;

            if (corpse == null)
                return;

            List<Item> items = new List<Item>();

            foreach (Item item in corpse.Items)
                items.Add(item);

            bool gathered = false;
            bool didntFit = false;

            Container pack = from.Backpack;

            for (int i = 0; !didntFit && i < items.Count; ++i)
            {
                Item item = items[i];
                Point3D loc = item.Location;

                if ((item.Layer == Layer.Hair || item.Layer == Layer.FacialHair) || !item.Movable)
                    continue;

                if (pack != null)
                {
                    pack.DropItem(item);
                    gathered = true;
                }
                else
                {
                    didntFit = true;
                }
            }

            corpse.Carved = true;

            if (corpse.ItemID == 0x2006)
            {
                corpse.ProcessDelta();
                corpse.SendRemovePacket();
                corpse.ItemID = Utility.Random(0xECA, 9); // bone graphic
                corpse.Hue = 0;
                corpse.ProcessDelta();

                Mobile killer = from.FindMostRecentDamager(false);

                if (killer != null && killer.Player)
                    killer.AddToBackpack(new Head(from.Name, from as PlayerMobile, m_Tournament == null ? HeadType.Duel : HeadType.Tournament));
            }

            from.PlaySound(0x3E3);

            if (gathered && !didntFit)
                from.SendLocalizedMessage(1062471); // You quickly gather all of your belongings.
            else if (gathered && didntFit)
                from.SendLocalizedMessage(1062472); // You gather some of your belongings. The rest remain on the corpse.
        }

        public void Refresh(Mobile mob, Container cont)
        {
            if (!mob.Alive)
            {
                mob.Resurrect();

                DeathRobe robe = mob.FindItemOnLayer(Layer.OuterTorso) as DeathRobe;

                if (robe != null)
                    robe.Delete();

                if (cont is Corpse)
                {
                    Corpse corpse = (Corpse)cont;

                    for (int i = 0; i < corpse.EquipItems.Count; ++i)
                    {
                        Item item = (Item)corpse.EquipItems[i];

                        if (item.Movable && item.Layer != Layer.Hair && item.Layer != Layer.FacialHair && item.IsChildOf(mob.Backpack))
                            mob.EquipItem(item);
                    }
                }
            }

            mob.Hits = mob.HitsMax;
            mob.Stam = mob.StamMax;
            mob.Mana = mob.ManaMax;

            mob.Poison = null;
        }

        public void SendOutside(Mobile mob)
        {
            if (m_Arena == null)
                return;

            mob.Combatant = null;
            mob.MoveToWorld(m_Arena.Outside, m_Arena.Facet);
        }

        private Point3D m_GatePoint;
        private Map m_GateFacet;

        public void Finish(Participant winner)
        {
            if (m_Finished)
                return;

            EndAutoTie();
            StopSDTimers();

            m_Finished = true;

            for (int i = 0; i < winner.Players.Length; ++i)
            {
                DuelPlayer pl = winner.Players[i];

                if (pl != null && !pl.Eliminated)
                    DelayBounce(TimeSpan.FromSeconds(8.0), pl.Mobile, null);
            }

            winner.Broadcast(0x59, null, winner.Players.Length == 1 ? "{0} has won the duel." : "{0} and {1} team have won the duel.", winner.Players.Length == 1 ? "You have won the duel." : "Your team has won the duel.");

            if (m_Tournament != null && winner.TournyPart != null)
            {
                m_Match.Winner = winner.TournyPart;
                winner.TournyPart.WonMatch(m_Match);
                m_Tournament.HandleWon(m_Arena, m_Match, winner.TournyPart);
            }

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant loser = (Participant)m_Participants[i];

                if (loser != winner)
                {
                    loser.Broadcast(0x22, null, loser.Players.Length == 1 ? "{0} has lost the duel." : "{0} and {1} team have lost the duel.", loser.Players.Length == 1 ? "You have lost the duel." : "Your team has lost the duel.");

                    if (m_Tournament != null && loser.TournyPart != null)
                        loser.TournyPart.LostMatch(m_Match);
                }

                for (int j = 0; j < loser.Players.Length; ++j)
                {
                    if (loser.Players[j] != null)
                    {
                        RemoveAggressions(loser.Players[j].Mobile);
                        loser.Players[j].Mobile.Delta(MobileDelta.Noto);
                        loser.Players[j].Mobile.CloseGump(typeof(BeginGump));

                        if (m_Tournament != null)
                            loser.Players[j].Mobile.SendEverything();
                    }
                }

                if (loser.TournyPart != null)
                    loser.TournyPart.LastMatch = DateTime.UtcNow;
            }

            if (IsOneVsOne)
            {
                DuelPlayer dp1 = ((Participant)m_Participants[0]).Players[0];
                DuelPlayer dp2 = ((Participant)m_Participants[1]).Players[0];

                if (dp1 != null && dp2 != null)
                {
                    Award(dp1.Mobile, dp2.Mobile, dp1.Participant == winner);
                    Award(dp2.Mobile, dp1.Mobile, dp2.Participant == winner);
                }
            }

            if (m_EventGame != null)
                m_EventGame.OnStop();

            Timer.DelayCall(TimeSpan.FromSeconds(9.0), new TimerCallback(UnregisterRematch));
        }

        public void Award(Mobile us, Mobile them, bool won)
        {
            Ladder ladder = (m_Arena == null ? Ladder.Instance : m_Arena.AcquireLadder());

            if (ladder == null)
                return;

            LadderEntry ourEntry = ladder.Find(us);
            LadderEntry theirEntry = ladder.Find(them);

            if (ourEntry == null || theirEntry == null)
                return;

            int xpGain = Ladder.GetExperienceGain(ourEntry, theirEntry, won);

            if (xpGain == 0)
                return;

            if (m_Tournament != null)
                xpGain *= (xpGain > 0 ? 5 : 2);

            if (won)
                ++ourEntry.Wins;
            else
                ++ourEntry.Losses;

            int oldLevel = Ladder.GetLevel(ourEntry.Experience);

            ourEntry.Experience += xpGain;

            if (ourEntry.Experience < 0)
                ourEntry.Experience = 0;

            ladder.UpdateEntry(ourEntry);

            int newLevel = Ladder.GetLevel(ourEntry.Experience);

            if (newLevel > oldLevel)
                us.SendMessage(0x59, "You have achieved level {0}!", newLevel);
            else if (newLevel < oldLevel)
                us.SendMessage(0x22, "You have lost a level. You are now at {0}.", newLevel);
        }

        public void UnregisterRematch()
        {
            Unregister(true);
        }

        public void Unregister()
        {
            Unregister(false);
        }

        public void Unregister(bool queryRematch)
        {
            DestroyWall();

            if (!m_Registered)
                return;

            m_Registered = false;

            if (m_Arena != null)
                m_Arena.Evict();

            StopSDTimers();

            Type[] types = new Type[] { typeof(BeginGump), typeof(DuelContextGump), typeof(ParticipantGump), typeof(PickRulesetGump), typeof(ReadyGump), typeof(ReadyUpGump), typeof(RulesetGump) };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = (DuelPlayer)p.Players[j];

                    if (pl == null)
                        continue;

                    if (pl.Mobile is PlayerMobile)
                        ((PlayerMobile)pl.Mobile).DuelPlayer = null;

                    for (int k = 0; k < types.Length; ++k)
                        pl.Mobile.CloseGump(types[k]);
                }
            }

            if (queryRematch && m_Tournament == null)
                QueryRematch();
        }

        public void QueryRematch()
        {
            DuelContext dc = new DuelContext(m_Initiator, m_Ruleset.Layout, false);

            dc.m_Ruleset = m_Ruleset;
            dc.m_Rematch = true;

            dc.m_Participants.Clear();

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant oldPart = (Participant)m_Participants[i];
                Participant newPart = new Participant(dc, oldPart.Players.Length);

                for (int j = 0; j < oldPart.Players.Length; ++j)
                {
                    DuelPlayer oldPlayer = oldPart.Players[j];

                    if (oldPlayer != null)
                        newPart.Players[j] = new DuelPlayer(oldPlayer.Mobile, newPart);
                }

                dc.m_Participants.Add(newPart);
            }

            dc.CloseAllGumps();
            dc.SendReadyUpGump();
        }

        public DuelPlayer Find(Mobile mob)
        {
            if (mob is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)mob;

                if (pm.DuelContext == this)
                    return pm.DuelPlayer;

                return null;
            }

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];
                DuelPlayer pl = p.Find(mob);

                if (pl != null)
                    return pl;
            }

            return null;
        }

        public bool IsAlly(Mobile m1, Mobile m2)
        {
            DuelPlayer pl1 = Find(m1);
            DuelPlayer pl2 = Find(m2);

            return (pl1 != null && pl2 != null && pl1.Participant == pl2.Participant);
        }

        public Participant CheckCompletion()
        {
            Participant winner = null;

            bool hasWinner = false;
            int eliminated = 0;

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                if (p.Eliminated)
                {
                    ++eliminated;

                    if (eliminated == (m_Participants.Count - 1))
                        hasWinner = true;
                }
                else
                {
                    winner = p;
                }
            }

            if (hasWinner)
                return winner == null ? (Participant)m_Participants[0] : winner;

            return null;
        }

        private Timer m_Countdown;

        public void StartCountdown(int count, CountdownCallback cb)
        {
            cb(count);
            m_Countdown = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), count, new TimerStateCallback(Countdown_Callback), new object[] { count - 1, cb });
        }

        public void StopCountdown()
        {
            if (m_Countdown != null)
                m_Countdown.Stop();

            m_Countdown = null;
        }

        private void Countdown_Callback(object state)
        {
            object[] states = (object[])state;

            int count = (int)states[0];
            CountdownCallback cb = (CountdownCallback)states[1];

            if (count == 0)
            {
                if (m_Countdown != null)
                    m_Countdown.Stop();

                m_Countdown = null;
            }

            cb(count);

            states[0] = count - 1;
        }

        private Timer m_AutoTieTimer;
        private bool m_Tied;

        public bool Tied { get { return m_Tied; } }

        private bool m_IsSuddenDeath;

        public bool IsSuddenDeath { get { return m_IsSuddenDeath; } set { m_IsSuddenDeath = value; } }

        private Timer m_SDWarnTimer, m_SDActivateTimer;

        public void StopSDTimers()
        {
            if (m_SDWarnTimer != null)
                m_SDWarnTimer.Stop();

            m_SDWarnTimer = null;

            if (m_SDActivateTimer != null)
                m_SDActivateTimer.Stop();

            m_SDActivateTimer = null;
        }

        public void StartSuddenDeath(TimeSpan timeUntilActive)
        {
            if (m_SDWarnTimer != null)
                m_SDWarnTimer.Stop();

            m_SDWarnTimer = Timer.DelayCall(TimeSpan.FromMinutes(timeUntilActive.TotalMinutes * 0.9), new TimerCallback(WarnSuddenDeath));

            if (m_SDActivateTimer != null)
                m_SDActivateTimer.Stop();

            m_SDActivateTimer = Timer.DelayCall(timeUntilActive, new TimerCallback(ActivateSuddenDeath));
        }

        public void WarnSuddenDeath()
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null || pl.Eliminated)
                        continue;

                    pl.Mobile.SendSound(0x1E1);
                    pl.Mobile.SendMessage(0x22, "Warning! Warning! Warning!");
                    pl.Mobile.SendMessage(0x22, "Sudden death will be active soon!");
                }
            }

            if (m_Tournament != null)
                m_Tournament.Alert(m_Arena, "Sudden death will be active soon!");

            if (m_SDWarnTimer != null)
                m_SDWarnTimer.Stop();

            m_SDWarnTimer = null;
        }

        public static bool CheckSuddenDeath(Mobile mob)
        {
            if (mob is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)mob;

                if (pm.DuelPlayer != null && !pm.DuelPlayer.Eliminated && pm.DuelContext != null && pm.DuelContext.IsSuddenDeath)
                    return true;
            }

            return false;
        }

        public void ActivateSuddenDeath()
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null || pl.Eliminated)
                        continue;

                    pl.Mobile.SendSound(0x1E1);
                    pl.Mobile.SendMessage(0x22, "Warning! Warning! Warning!");
                    pl.Mobile.SendMessage(0x22, "Sudden death has ACTIVATED. You are now unable to perform any beneficial actions.");
                }
            }

            if (m_Tournament != null)
                m_Tournament.Alert(m_Arena, "Sudden death has been activated!");

            m_IsSuddenDeath = true;

            if (m_SDActivateTimer != null)
                m_SDActivateTimer.Stop();

            m_SDActivateTimer = null;
        }

        public void BeginAutoTie()
        {
            if (m_AutoTieTimer != null)
                m_AutoTieTimer.Stop();

            TimeSpan ts = (m_Tournament == null || m_Tournament.TournyType == TournyType.Standard)
                ? AutoTieDelay
                : TimeSpan.FromMinutes(90.0);

            m_AutoTieTimer = Timer.DelayCall(ts, new TimerCallback(InvokeAutoTie));
        }

        public void EndAutoTie()
        {
            if (m_AutoTieTimer != null)
                m_AutoTieTimer.Stop();

            m_AutoTieTimer = null;
        }

        public void InvokeAutoTie()
        {
            m_AutoTieTimer = null;

            if (!m_Started || m_Finished)
                return;

            m_Tied = true;
            m_Finished = true;

            StopSDTimers();

            ArrayList remaining = new ArrayList();

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                if (p.Eliminated)
                {
                    p.Broadcast(0x22, null, p.Players.Length == 1 ? "{0} has lost the duel." : "{0} and {1} team have lost the duel.", p.Players.Length == 1 ? "You have lost the duel." : "Your team has lost the duel.");
                }
                else
                {
                    p.Broadcast(0x59, null, p.Players.Length == 1 ? "{0} has tied the duel due to time expiration." : "{0} and {1} team have tied the duel due to time expiration.", p.Players.Length == 1 ? "You have tied the duel due to time expiration." : "Your team has tied the duel due to time expiration.");

                    for (int j = 0; j < p.Players.Length; ++j)
                    {
                        DuelPlayer pl = p.Players[j];

                        if (pl != null && !pl.Eliminated)
                            DelayBounce(TimeSpan.FromSeconds(8.0), pl.Mobile, null);
                    }

                    if (p.TournyPart != null)
                        remaining.Add(p.TournyPart);
                }

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl != null)
                    {
                        pl.Mobile.Delta(MobileDelta.Noto);
                        pl.Mobile.SendEverything();
                    }
                }

                if (p.TournyPart != null)
                    p.TournyPart.LastMatch = DateTime.UtcNow;
            }

            if (m_Tournament != null)
                m_Tournament.HandleTie(m_Arena, m_Match, remaining);

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), new TimerCallback(Unregister));
        }

        public bool IsOneVsOne
        {
            get
            {
                if (m_Participants.Count != 2)
                    return false;

                if (((Participant)m_Participants[0]).Players.Length != 1)
                    return false;

                if (((Participant)m_Participants[1]).Players.Length != 1)
                    return false;

                return true;
            }
        }

        public static void Initialize()
        {
            EventSink.Speech += new SpeechEventHandler(EventSink_Speech);
            EventSink.Login += new LoginEventHandler(EventSink_Login);

            CommandSystem.Register("vli", AccessLevel.GameMaster, new CommandEventHandler(vli_oc));
        }

        private static void vli_oc(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, false, Targeting.TargetFlags.None, new TargetCallback(vli_ot));
        }

        private static void vli_ot(Mobile from, object obj)
        {
            if (obj is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)obj;

                Ladder ladder = Ladder.Instance;

                if (ladder == null)
                    return;

                LadderEntry entry = ladder.Find(pm);

                if (entry != null)
                    from.SendGump(new PropertiesGump(from, entry));
            }
        }

        private static TimeSpan CombatDelay = TimeSpan.FromSeconds(30.0);
        private static TimeSpan AutoTieDelay = TimeSpan.FromMinutes(15.0);

        public static bool CheckCombat(Mobile m)
        {
            for (int i = 0; i < m.Aggressed.Count; ++i)
            {
                AggressorInfo info = m.Aggressed[i];

                if (info.Defender.Player && (DateTime.UtcNow - info.LastCombatTime) < CombatDelay)
                    return true;
            }

            for (int i = 0; i < m.Aggressors.Count; ++i)
            {
                AggressorInfo info = m.Aggressors[i];

                if (info.Attacker.Player && (DateTime.UtcNow - info.LastCombatTime) < CombatDelay)
                    return true;
            }

            return false;
        }

        private static void EventSink_Login(LoginEventArgs e)
        {
            if (!ConPVPSystem.Enabled)
                return;

            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            DuelContext dc = pm.DuelContext;

            if (dc == null)
                return;

            if (dc.ReadyWait && pm.DuelPlayer.Ready && !dc.Started && !dc.StartedBeginCountdown && !dc.Finished)
            {
                if (dc.m_Tournament == null)
                    pm.SendGump(new ReadyGump(pm, dc, dc.m_ReadyCount));
            }
            else if (dc.ReadyWait && !dc.StartedBeginCountdown && !dc.Started && !dc.Finished)
            {
                if (dc.m_Tournament == null)
                    pm.SendGump(new ReadyUpGump(pm, dc));
            }
            else if (dc.Initiator == pm && !dc.ReadyWait && !dc.StartedBeginCountdown && !dc.Started && !dc.Finished)
                pm.SendGump(new DuelContextGump(pm, dc));
        }

        private static void ViewLadder_OnTarget(Mobile from, object obj, object state)
        {
            if (obj is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)obj;
                Ladder ladder = (Ladder)state;

                LadderEntry entry = ladder.Find(pm);

                if (entry == null)
                    return; // sanity

                string text = String.Format("{{0}} are ranked {0} at level {1}.", LadderGump.Rank(entry.Index + 1), Ladder.GetLevel(entry.Experience));

                pm.PrivateOverheadMessage(MessageType.Regular, pm.SpeechHue, true, String.Format(text, from == pm ? "You" : "They"), from.NetState);
            }
            else if (obj is Mobile)
            {
                Mobile mob = (Mobile)obj;

                if (mob.Body.IsHuman)
                    mob.PrivateOverheadMessage(MessageType.Regular, mob.SpeechHue, false, "I'm not a duelist, and quite frankly, I resent the implication.", from.NetState);
                else
                    mob.PrivateOverheadMessage(MessageType.Regular, 0x3B2, true, "It's probably better than you.", from.NetState);
            }
            else
            {
                from.SendMessage("That's not a player.");
            }
        }

        private static void EventSink_Speech(SpeechEventArgs e)
        {
            if (!ConPVPSystem.Enabled)
                return;

            if (e.Handled)
                return;

            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            if (Insensitive.Contains(e.Speech, "i wish to duel"))
            {
                if (!pm.CheckAlive())
                {
                }
                else if (pm.Region.IsJailRules)
                {
                }
                else if (CheckCombat(pm))
                {
                    e.Mobile.SendMessage(0x22, "You have recently been in combat with another player and must wait before starting a duel.");
                }
                else if (pm.DuelContext != null)
                {
                    if (pm.DuelContext.Initiator == pm)
                        e.Mobile.SendMessage(0x22, "You have already started a duel.");
                    else
                        e.Mobile.SendMessage(0x22, "You have already been challenged in a duel.");
                }
                else if (TournamentController.PreventDuels && TournamentController.IsActive)
                {
                    e.Mobile.SendMessage(0x22, "You may not start a duel while a tournament is active.");
                }
                else if (TournamentController.IsParticipating(pm))
                {
                    e.Mobile.SendMessage(0x22, "You have already signed up for a tournament.");
                }
                else
                {
                    pm.SendGump(new DuelContextGump(pm, new DuelContext(pm, RulesetLayout.Root)));
                    e.Handled = true;
                }
            }
            else if (Insensitive.Equals(e.Speech, "change arena preferences"))
            {
                if (!pm.CheckAlive())
                {
                }
                else
                {
                    Preferences prefs = Preferences.Instance;

                    if (prefs != null)
                    {
                        e.Mobile.CloseGump(typeof(PreferencesGump));
                        e.Mobile.SendGump(new PreferencesGump(e.Mobile, prefs));
                    }
                }
            }
            else if (Insensitive.Equals(e.Speech, "showladder"))
            {
                e.Blocked = true;
                if (!pm.CheckAlive())
                {
                }
                else
                {
                    Ladder instance = Ladder.Instance;

                    if (instance == null)
                    {
                        //pm.SendMessage( "Ladder not yet initialized." );
                    }
                    else
                    {
                        LadderEntry entry = instance.Find(pm);

                        if (entry == null)
                            return; // sanity

                        string text = String.Format("{{0}} {{1}} ranked {0} at level {1}.", LadderGump.Rank(entry.Index + 1), Ladder.GetLevel(entry.Experience));

                        pm.LocalOverheadMessage(MessageType.Regular, pm.SpeechHue, true, String.Format(text, "You", "are"));
                        pm.NonlocalOverheadMessage(MessageType.Regular, pm.SpeechHue, true, String.Format(text, pm.Name, "is"));

                        //pm.PublicOverheadMessage( MessageType.Regular, pm.SpeechHue, true, String.Format( "Level {0} with {1} win{2} and {3} loss{4}.", Ladder.GetLevel( entry.Experience ), entry.Wins, entry.Wins==1?"":"s", entry.Losses, entry.Losses==1?"":"es" ) );
                        //pm.PublicOverheadMessage( MessageType.Regular, pm.SpeechHue, true, String.Format( "Level {0} with {1} win{2} and {3} loss{4}.", Ladder.GetLevel( entry.Experience ), entry.Wins, entry.Wins==1?"":"s", entry.Losses, entry.Losses==1?"":"es" ) );
                    }
                }
            }
            else if (Insensitive.Equals(e.Speech, "viewladder"))
            {
                e.Blocked = true;

                if (!pm.CheckAlive())
                {
                }
                else
                {
                    Ladder instance = Ladder.Instance;

                    if (instance == null)
                    {
                        //pm.SendMessage( "Ladder not yet initialized." );
                    }
                    else
                    {
                        pm.SendMessage("Target a player to view their ranking and level.");
                        pm.BeginTarget(16, false, Targeting.TargetFlags.None, new TargetStateCallback(ViewLadder_OnTarget), instance);
                    }
                }
            }
            else if (Insensitive.Contains(e.Speech, "i yield"))
            {
                if (!pm.CheckAlive())
                {
                }
                else if (pm.DuelContext == null)
                {
                }
                else if (pm.DuelContext.Finished)
                {
                    e.Mobile.SendMessage(0x22, "The duel is already finished.");
                }
                else if (!pm.DuelContext.Started)
                {
                    DuelContext dc = pm.DuelContext;
                    Mobile init = dc.Initiator;

                    if (pm.DuelContext.StartedBeginCountdown)
                    {
                        e.Mobile.SendMessage(0x22, "The duel has not yet started.");
                    }
                    else
                    {
                        DuelPlayer pl = pm.DuelContext.Find(pm);

                        if (pl == null)
                            return;

                        Participant p = pl.Participant;

                        if (!pm.DuelContext.ReadyWait) // still setting stuff up
                        {
                            p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

                            if (init == pm)
                            {
                                dc.Unregister();
                            }
                            else
                            {
                                p.Nullify(pl);
                                pm.DuelPlayer = null;

                                NetState ns = init.NetState;

                                if (ns != null)
                                {
                                    foreach (Gump g in ns.Gumps)
                                    {
                                        if (g is ParticipantGump)
                                        {
                                            ParticipantGump pg = (ParticipantGump)g;

                                            if (pg.Participant == p)
                                            {
                                                init.SendGump(new ParticipantGump(init, dc, p));
                                                break;
                                            }
                                        }
                                        else if (g is DuelContextGump)
                                        {
                                            DuelContextGump dcg = (DuelContextGump)g;

                                            if (dcg.Context == dc)
                                            {
                                                init.SendGump(new DuelContextGump(init, dc));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (!pm.DuelContext.StartedReadyCountdown) // at ready stage
                        {
                            p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

                            dc.m_Yielding = true;
                            dc.RejectReady(pm, null);
                            dc.m_Yielding = false;

                            if (init == pm)
                            {
                                dc.Unregister();
                            }
                            else if (dc.m_Registered)
                            {
                                p.Nullify(pl);
                                pm.DuelPlayer = null;

                                NetState ns = init.NetState;

                                if (ns != null)
                                {
                                    bool send = true;

                                    foreach (Gump g in ns.Gumps)
                                    {
                                        if (g is ParticipantGump)
                                        {
                                            ParticipantGump pg = (ParticipantGump)g;

                                            if (pg.Participant == p)
                                            {
                                                init.SendGump(new ParticipantGump(init, dc, p));
                                                send = false;
                                                break;
                                            }
                                        }
                                        else if (g is DuelContextGump)
                                        {
                                            DuelContextGump dcg = (DuelContextGump)g;

                                            if (dcg.Context == dc)
                                            {
                                                init.SendGump(new DuelContextGump(init, dc));
                                                send = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (send)
                                        init.SendGump(new DuelContextGump(init, dc));
                                }
                            }
                        }
                        else
                        {
                            if (pm.DuelContext.m_Countdown != null)
                                pm.DuelContext.m_Countdown.Stop();
                            pm.DuelContext.m_Countdown = null;

                            pm.DuelContext.m_StartedReadyCountdown = false;
                            p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

                            dc.m_Yielding = true;
                            dc.RejectReady(pm, null);
                            dc.m_Yielding = false;

                            if (init == pm)
                            {
                                dc.Unregister();
                            }
                            else if (dc.m_Registered)
                            {
                                p.Nullify(pl);
                                pm.DuelPlayer = null;

                                NetState ns = init.NetState;

                                if (ns != null)
                                {
                                    bool send = true;

                                    foreach (Gump g in ns.Gumps)
                                    {
                                        if (g is ParticipantGump)
                                        {
                                            ParticipantGump pg = (ParticipantGump)g;

                                            if (pg.Participant == p)
                                            {
                                                init.SendGump(new ParticipantGump(init, dc, p));
                                                send = false;
                                                break;
                                            }
                                        }
                                        else if (g is DuelContextGump)
                                        {
                                            DuelContextGump dcg = (DuelContextGump)g;

                                            if (dcg.Context == dc)
                                            {
                                                init.SendGump(new DuelContextGump(init, dc));
                                                send = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (send)
                                        init.SendGump(new DuelContextGump(init, dc));
                                }
                            }
                        }
                    }
                }
                else
                {
                    DuelPlayer pl = pm.DuelContext.Find(pm);

                    if (pl != null)
                    {
                        if (pm.DuelContext.IsOneVsOne)
                        {
                            e.Mobile.SendMessage(0x22, "You may not yield a 1 on 1 match.");
                        }
                        else if (pl.Eliminated)
                        {
                            e.Mobile.SendMessage(0x22, "You have already been eliminated.");
                        }
                        else
                        {
                            pm.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have yielded.");
                            pm.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, String.Format("{0} has yielded.", pm.Name));

                            pm.DuelContext.m_Yielding = true;
                            pm.Kill();
                            pm.DuelContext.m_Yielding = false;

                            if (pm.Alive) // invul, ...
                            {
                                pl.Eliminated = true;

                                if (pm.DuelContext.m_EventGame != null)
                                    pm.DuelContext.m_EventGame.OnEliminated(pm, EliminationReason.Yielded);

                                pm.DuelContext.RemoveAggressions(pm);
                                pm.DuelContext.SendOutside(pm);
                                pm.DuelContext.Refresh(pm, null);
                                Debuff(pm);
                                CancelSpell(pm);
                                pm.Frozen = false;

                                Participant winner = pm.DuelContext.CheckCompletion();

                                if (winner != null)
                                    pm.DuelContext.Finish(winner);
                            }
                        }
                    }
                    else
                    {
                        e.Mobile.SendMessage(0x22, "BUG: Unable to find duel context.");
                    }
                }
            }
        }

        public DuelContext(Mobile initiator, RulesetLayout layout) : this(initiator, layout, true)
        {
        }

        public DuelContext(Mobile initiator, RulesetLayout layout, bool addNew)
        {
            m_Initiator = initiator;
            m_Participants = new ArrayList();
            m_Ruleset = new Ruleset(layout);
            m_Ruleset.ApplyDefault(layout.Defaults[0]);

            if (addNew)
            {
                m_Participants.Add(new Participant(this, 1));
                m_Participants.Add(new Participant(this, 1));

                ((Participant)m_Participants[0]).Add(initiator);
            }
        }

        public void CloseAllGumps()
        {
            Type[] types = new Type[] { typeof(DuelContextGump), typeof(ParticipantGump), typeof(RulesetGump) };
            int[] defs = new int[] { -1, -1, -1 };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    Mobile mob = pl.Mobile;

                    for (int k = 0; k < types.Length; ++k)
                        mob.CloseGump(types[k]);
                    //mob.CloseGump( types[k], defs[k] );
                }
            }
        }

        public void RejectReady(Mobile rejector, string page)
        {
            if (m_StartedReadyCountdown)
                return; // sanity

            Type[] types = new Type[] { typeof(DuelContextGump), typeof(ReadyUpGump), typeof(ReadyGump) };
            int[] defs = new int[] { -1, -1, -1 };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    pl.Ready = false;

                    Mobile mob = pl.Mobile;

                    if (page == null) // yield
                    {
                        if (mob != rejector)
                            mob.SendMessage(0x22, "{0} has yielded.", rejector.Name);
                    }
                    else
                    {
                        if (mob == rejector)
                            mob.SendMessage(0x22, "You have rejected the {0}.", m_Rematch ? "rematch" : page);
                        else
                            mob.SendMessage(0x22, "{0} has rejected the {1}.", rejector.Name, m_Rematch ? "rematch" : page);
                    }

                    for (int k = 0; k < types.Length; ++k)
                        mob.CloseGump(types[k]);
                    //mob.CloseGump( types[k], defs[k] );
                }
            }

            if (m_Rematch)
                Unregister();
            else if (!m_Yielding)
                m_Initiator.SendGump(new DuelContextGump(m_Initiator, this));

            m_ReadyWait = false;
            m_ReadyCount = 0;
        }

        public void SendReadyGump()
        {
            SendReadyGump(-1);
        }

        public static void Debuff(Mobile mob)
        {
            mob.RemoveStatMod("[Magic] Str Offset");
            mob.RemoveStatMod("[Magic] Dex Offset");
            mob.RemoveStatMod("[Magic] Int Offset");
            mob.RemoveStatMod("Concussion");

            OrangePetals.RemoveContext(mob);

            // TODO
            //GrapesOfWrath.RemoveContext( mob );

            mob.Paralyzed = false;
            mob.Hidden = false;

            if (!Core.RuleSets.AOSRules())
            {
                mob.MagicDamageAbsorb = 0;
                mob.MeleeDamageAbsorb = 0;
                Spells.Second.ProtectionSpell.Registry.Remove(mob);

                Spells.Fourth.ArchProtectionSpell.RemoveEntry(mob);

                mob.EndAction(typeof(DefensiveSpell));
            }

            TransformationSpellHelper.RemoveContext(mob, true);
            // Yoar: we don't have animal form
#if false
            AnimalForm.RemoveContext(mob, true);
#endif

            if (DisguiseGump.IsDisguised(mob))
                DisguiseGump.StopTimer(mob);

            if (!mob.CanBeginAction(typeof(PolymorphSpell)))
            {
                mob.BodyMod = 0;
                mob.HueMod = -1;
                mob.EndAction(typeof(PolymorphSpell));
            }

            BaseArmor.ValidateMobile(mob);
            BaseClothing.ValidateMobile(mob);

            mob.Hits = mob.HitsMax;
            mob.Stam = mob.StamMax;
            mob.Mana = mob.ManaMax;

            mob.Poison = null;
        }

        public static void CancelSpell(Mobile mob)
        {
            if (mob.Spell is Spells.Spell)
                ((Spells.Spell)mob.Spell).Disturb(Spells.DisturbType.Kill);

            Targeting.Target.Cancel(mob);
        }

        private bool m_StartedBeginCountdown;
        private bool m_StartedReadyCountdown;

        public bool StartedBeginCountdown { get { return m_StartedBeginCountdown; } }
        public bool StartedReadyCountdown { get { return m_StartedReadyCountdown; } }

        private class InternalWall : Item
        {
            public InternalWall() : base(0x80)
            {
                Movable = false;
            }

            public void Appear(Point3D loc, Map map)
            {
                MoveToWorld(loc, map);

                Effects.SendLocationParticles(this, 0x376A, 9, 10, 5025);
            }

            public InternalWall(Serial serial) : base(serial)
            {
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                Delete();
            }
        }

        private ArrayList m_Walls = new ArrayList();

        public void DestroyWall()
        {
            for (int i = 0; i < m_Walls.Count; ++i)
                ((Item)m_Walls[i]).Delete();

            m_Walls.Clear();
        }

        public void CreateWall()
        {
            if (m_Arena == null)
                return;

            Point3D start = m_Arena.Points.EdgeWest;
            Point3D wall = m_Arena.Wall;

            int dx = start.X - wall.X;
            int dy = start.Y - wall.Y;
            int rx = dx - dy;
            int ry = dx + dy;

            bool eastToWest;

            if (rx >= 0 && ry >= 0)
                eastToWest = false;
            else if (rx >= 0)
                eastToWest = true;
            else if (ry >= 0)
                eastToWest = true;
            else
                eastToWest = false;

            Effects.PlaySound(wall, m_Arena.Facet, 0x1F6);

            for (int i = -1; i <= 1; ++i)
            {
                Point3D loc = new Point3D(eastToWest ? wall.X + i : wall.X, eastToWest ? wall.Y : wall.Y + i, wall.Z);

                InternalWall created = new InternalWall();

                created.Appear(loc, m_Arena.Facet);

                m_Walls.Add(created);
            }
        }

        public void BuildParties()
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                if (p.Players.Length > 1)
                {
                    ArrayList players = new ArrayList();

                    for (int j = 0; j < p.Players.Length; ++j)
                    {
                        DuelPlayer dp = p.Players[j];

                        if (dp == null)
                            continue;

                        players.Add(dp.Mobile);
                    }

                    if (players.Count > 1)
                    {
                        for (int leaderIndex = 0; (leaderIndex + 1) < players.Count; leaderIndex += Party.Capacity)
                        {
                            Mobile leader = (Mobile)players[leaderIndex];
                            Party party = Party.Get(leader);

                            if (party == null)
                            {
                                leader.Party = party = new Party(leader);
                            }
                            else if (party.Leader != leader)
                            {
                                party.SendPublicMessage(leader, "I leave this party to fight in a duel.");
                                party.Remove(leader);
                                leader.Party = party = new Party(leader);
                            }

                            for (int j = leaderIndex + 1; j < players.Count && j < leaderIndex + Party.Capacity; ++j)
                            {
                                Mobile player = (Mobile)players[j];
                                Party existing = Party.Get(player);

                                if (existing == party)
                                    continue;

                                if ((party.Members.Count + party.Candidates.Count) >= Party.Capacity)
                                {
                                    player.SendMessage("You could not be added to the team party because it is at full capacity.");
                                    leader.SendMessage("{0} could not be added to the team party because it is at full capacity.", player.Name);
                                }
                                else
                                {
                                    if (existing != null)
                                    {
                                        existing.SendPublicMessage(player, "I leave this party to fight in a duel.");
                                        existing.Remove(player);
                                    }

                                    party.OnAccept(player, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ClearIllegalItems()
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    ClearIllegalItems(pl.Mobile);
                }
            }
        }

        public void ClearIllegalItems(Mobile mob)
        {
            if (mob.StunReady && !AllowSpecialAbility(mob, "Stun", false))
                mob.StunReady = false;

            if (mob.DisarmReady && !AllowSpecialAbility(mob, "Disarm", false))
                mob.DisarmReady = false;

            Container pack = mob.Backpack;

            if (pack == null)
                return;

            for (int i = mob.Items.Count - 1; i >= 0; --i)
            {
                if (i >= mob.Items.Count)
                    continue; // sanity

                Item item = (Item)mob.Items[i];

                if (!CheckItemEquip(mob, item))
                {
                    pack.DropItem(item);

                    if (item is BaseWeapon)
                        mob.SendLocalizedMessage(1062001, item.Name == null ? "#" + item.LabelNumber.ToString() : item.Name); // You can no longer wield your ~1_WEAPON~
                    else if (item is BaseArmor && !(item is BaseShield))
                        mob.SendLocalizedMessage(1062002, item.Name == null ? "#" + item.LabelNumber.ToString() : item.Name); // You can no longer wear your ~1_ARMOR~
                    else
                        mob.SendLocalizedMessage(1062003, item.Name == null ? "#" + item.LabelNumber.ToString() : item.Name); // You can no longer equip your ~1_SHIELD~
                }
            }

            Item inHand = mob.Holding;

            if (inHand != null && !CheckItemEquip(mob, inHand))
            {
                mob.Holding = null;

                BounceInfo bi = inHand.GetBounce();

                if (bi.m_Parent == mob)
                    pack.DropItem(inHand);
                else
                    inHand.Bounce(mob);

                inHand.ClearBounce();
            }
        }

        public void SendBeginGump(int count)
        {
            if (!m_Registered || m_Finished)
                return;

            if (count == 10)
            {
                CreateWall();
                BuildParties();
                ClearIllegalItems();
            }
            else if (count == 0)
            {
                DestroyWall();
            }

            m_StartedBeginCountdown = true;

            if (count == 0)
            {
                m_Started = true;
                BeginAutoTie();
            }

            Type[] types = new Type[] { typeof(ReadyGump), typeof(ReadyUpGump), typeof(BeginGump) };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    Mobile mob = pl.Mobile;

                    if (count > 0)
                    {
                        if (count == 10)
                            CloseAndSendGump(mob, new BeginGump(count), types);

                        mob.Frozen = true;
                    }
                    else
                    {
                        mob.CloseGump(typeof(BeginGump));
                        mob.Frozen = false;
                    }
                }
            }
        }

        private ArrayList m_Entered = new ArrayList();

        private class ReturnEntry
        {
            private Mobile m_Mobile;
            private Point3D m_Location;
            private Map m_Facet;
            private DateTime m_Expire;

            public Mobile Mobile { get { return m_Mobile; } }
            public Point3D Location { get { return m_Location; } }
            public Map Facet { get { return m_Facet; } }

            public void Return()
            {
                if (m_Facet == Map.Internal || m_Facet == null)
                    return;

                if (m_Mobile.Map == Map.Internal)
                {
                    m_Mobile.LogoutLocation = m_Location;
                    m_Mobile.LogoutMap = m_Facet;
                }
                else
                {
                    m_Mobile.Location = m_Location;
                    m_Mobile.Map = m_Facet;
                }
            }

            public ReturnEntry(Mobile mob)
            {
                m_Mobile = mob;

                Update();
            }

            public ReturnEntry(Mobile mob, Point3D loc, Map facet)
            {
                m_Mobile = mob;
                m_Location = loc;
                m_Facet = facet;
                m_Expire = DateTime.UtcNow + TimeSpan.FromMinutes(30.0);
            }

            public bool Expired { get { return (DateTime.UtcNow >= m_Expire); } }

            public void Update()
            {
                m_Expire = DateTime.UtcNow + TimeSpan.FromMinutes(30.0);

                if (m_Mobile.Map == Map.Internal)
                {
                    m_Facet = m_Mobile.LogoutMap;
                    m_Location = m_Mobile.LogoutLocation;
                }
                else
                {
                    m_Facet = m_Mobile.Map;
                    m_Location = m_Mobile.Location;
                }
            }
        }

        private class ExitTeleporter : Item
        {
            private ArrayList m_Entries;

            public override string DefaultName
            {
                get { return "return teleporter"; }
            }

            public ExitTeleporter() : base(0x1822)
            {
                m_Entries = new ArrayList();

                Hue = 0x482;
                Movable = false;
            }

            public void Register(Mobile mob)
            {
                ReturnEntry entry = Find(mob);

                if (entry != null)
                {
                    entry.Update();
                    return;
                }

                m_Entries.Add(new ReturnEntry(mob));
            }

            private ReturnEntry Find(Mobile mob)
            {
                for (int i = 0; i < m_Entries.Count; ++i)
                {
                    ReturnEntry entry = (ReturnEntry)m_Entries[i];

                    if (entry.Mobile == mob)
                        return entry;
                    else if (entry.Expired)
                        m_Entries.RemoveAt(i--);
                }

                return null;
            }

            public override bool OnMoveOver(Mobile m)
            {
                if (!base.OnMoveOver(m))
                    return false;

                ReturnEntry entry = Find(m);

                if (entry != null)
                {
                    entry.Return();

                    Effects.PlaySound(GetWorldLocation(), Map, 0x1FE);
                    Effects.PlaySound(m.Location, m.Map, 0x1FE);

                    m_Entries.Remove(entry);

                    return false;
                }
                else
                {
                    m.SendLocalizedMessage(1049383); // The teleporter doesn't seem to work for you.
                    return true;
                }
            }

            public ExitTeleporter(Serial serial) : base(serial)
            {
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0);

                writer.WriteEncodedInt((int)m_Entries.Count);

                for (int i = 0; i < m_Entries.Count; ++i)
                {
                    ReturnEntry entry = (ReturnEntry)m_Entries[i];

                    writer.Write((Mobile)entry.Mobile);
                    writer.Write((Point3D)entry.Location);
                    writer.Write((Map)entry.Facet);

                    if (entry.Expired)
                        m_Entries.RemoveAt(i--);
                }
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                switch (version)
                {
                    case 0:
                        {
                            int count = reader.ReadEncodedInt();

                            m_Entries = new ArrayList(count);

                            for (int i = 0; i < count; ++i)
                            {
                                Mobile mob = reader.ReadMobile();
                                Point3D loc = reader.ReadPoint3D();
                                Map map = reader.ReadMap();

                                m_Entries.Add(new ReturnEntry(mob, loc, map));
                            }

                            break;
                        }
                }
            }
        }

        private class ArenaMoongate : ConfirmationMoongate
        {
            private ExitTeleporter m_Teleporter;

            public override string DefaultName
            {
                get { return "spectator moongate"; }
            }

            public ArenaMoongate(Point3D target, Map map, ExitTeleporter tp) : base(target, map)
            {
                m_Teleporter = tp;

                ItemID = 0x1FD4;
                Dispellable = false;

                GumpWidth = 300;
                GumpHeight = 150;
                MessageColor = 0xFFC000;
                MessageString = "Are you sure you wish to spectate this duel?";
                TitleColor = 0x7800;
                TitleNumber = 1062051; // Gate Warning

                Timer.DelayCall(TimeSpan.FromSeconds(10.0), new TimerCallback(Delete));
            }

            public override void CheckGate(Mobile m, int range)
            {
                if (DuelContext.CheckCombat(m))
                {
                    m.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
                }
                else
                {
                    base.CheckGate(m, range);
                }
            }

            public override void UseGate(Mobile m)
            {
                if (DuelContext.CheckCombat(m))
                {
                    m.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
                }
                else
                {
                    if (m_Teleporter != null && !m_Teleporter.Deleted)
                        m_Teleporter.Register(m);

                    base.UseGate(m);
                }
            }

            public void Appear(Point3D loc, Map map)
            {
                Effects.PlaySound(loc, map, 0x20E);
                MoveToWorld(loc, map);
            }

            public ArenaMoongate(Serial serial) : base(serial)
            {
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                Delete();
            }
        }

        public void RemoveAggressions(Mobile mob)
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer dp = (DuelPlayer)p.Players[j];

                    if (dp == null || dp.Mobile == mob)
                        continue;

                    mob.RemoveAggressed(dp.Mobile);
                    mob.RemoveAggressor(dp.Mobile);
                    dp.Mobile.RemoveAggressed(mob);
                    dp.Mobile.RemoveAggressor(mob);
                }
            }
        }

        public void SendReadyUpGump()
        {
            if (!m_Registered)
                return;

            m_ReadyWait = true;
            m_ReadyCount = -1;

            Type[] types = new Type[] { typeof(ReadyUpGump) };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    Mobile mob = pl.Mobile;

                    if (mob != null)
                    {
                        if (m_Tournament == null)
                            CloseAndSendGump(mob, new ReadyUpGump(mob, this), types);
                    }
                }
            }
        }

        public string ValidateStart()
        {
            if (m_Tournament == null && TournamentController.PreventDuels && TournamentController.IsActive)
                return "a tournament is active";

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer dp = p.Players[j];

                    if (dp == null)
                        return "a slot is empty";

                    if (dp.Mobile.Region.IsJailRules)
                        return String.Format("{0} is in jail", dp.Mobile.Name);

                    if (Sigil.ExistsOn(dp.Mobile))
                        return String.Format("{0} is holding a sigil", dp.Mobile.Name);

                    if (Engines.Alignment.TheFlag.ExistsOn(dp.Mobile))
                        return String.Format("{0} is holding a flag", dp.Mobile.Name);

                    if (!dp.Mobile.Alive)
                    {
                        if (m_Tournament == null)
                            return String.Format("{0} is dead", dp.Mobile.Name);
                        else
                            dp.Mobile.Resurrect();
                    }

                    if (m_Tournament == null && CheckCombat(dp.Mobile))
                        return String.Format("{0} is in combat", dp.Mobile.Name);

                    if (dp.Mobile.Mounted)
                    {
                        IMount mount = dp.Mobile.Mount;

                        if (m_Tournament != null && mount != null)
                            mount.Rider = null;
                        else
                            return String.Format("{0} is mounted", dp.Mobile.Name);
                    }
                }
            }

            return null;
        }

        public Arena m_OverrideArena;
        public Tournament m_Tournament;
        public TournyMatch m_Match;
        public EventGame m_EventGame;

        public Tournament Tournament { get { return m_Tournament; } }

        public void SendReadyGump(int count)
        {
            if (!m_Registered)
                return;

            if (count != -1)
                m_StartedReadyCountdown = true;

            m_ReadyCount = count;

            if (count == 0)
            {
                string error = ValidateStart();

                if (error != null)
                {
                    for (int i = 0; i < m_Participants.Count; ++i)
                    {
                        Participant p = (Participant)m_Participants[i];

                        for (int j = 0; j < p.Players.Length; ++j)
                        {
                            DuelPlayer dp = p.Players[j];

                            if (dp != null)
                                dp.Mobile.SendMessage("The duel could not be started because {0}.", error);
                        }
                    }

                    StartCountdown(10, new CountdownCallback(SendReadyGump));

                    return;
                }

                m_ReadyWait = false;

                List<Mobile> players = new List<Mobile>();

                for (int i = 0; i < m_Participants.Count; ++i)
                {
                    Participant p = (Participant)m_Participants[i];

                    for (int j = 0; j < p.Players.Length; ++j)
                    {
                        DuelPlayer dp = p.Players[j];

                        if (dp != null)
                            players.Add(dp.Mobile);
                    }
                }

                Arena arena = m_OverrideArena;

                if (arena == null)
                    arena = Arena.FindArena(players);

                if (arena == null)
                {
                    for (int i = 0; i < m_Participants.Count; ++i)
                    {
                        Participant p = (Participant)m_Participants[i];

                        for (int j = 0; j < p.Players.Length; ++j)
                        {
                            DuelPlayer dp = p.Players[j];

                            if (dp != null)
                                dp.Mobile.SendMessage("The duel could not be started because there are no arenas. If you want to stop waiting for a free arena, yield the duel.");
                        }
                    }

                    StartCountdown(10, new CountdownCallback(SendReadyGump));
                    return;
                }

                if (!arena.IsOccupied)
                {
                    m_Arena = arena;

                    if (m_Initiator.Map == Map.Internal)
                    {
                        m_GatePoint = m_Initiator.LogoutLocation;
                        m_GateFacet = m_Initiator.LogoutMap;
                    }
                    else
                    {
                        m_GatePoint = m_Initiator.Location;
                        m_GateFacet = m_Initiator.Map;
                    }

                    ExitTeleporter tp = arena.Teleporter as ExitTeleporter;

                    if (tp == null)
                    {
                        arena.Teleporter = tp = new ExitTeleporter();
                        tp.MoveToWorld(arena.GateOut == Point3D.Zero ? arena.Outside : arena.GateOut, arena.Facet);
                    }

                    ArenaMoongate mg = new ArenaMoongate(arena.GateIn == Point3D.Zero ? arena.Outside : arena.GateIn, arena.Facet, tp);

                    m_StartedBeginCountdown = true;

                    for (int i = 0; i < m_Participants.Count; ++i)
                    {
                        Participant p = (Participant)m_Participants[i];

                        for (int j = 0; j < p.Players.Length; ++j)
                        {
                            DuelPlayer pl = p.Players[j];

                            if (pl == null)
                                continue;

                            tp.Register(pl.Mobile);

                            pl.Mobile.Frozen = false; // reset timer just in case
                            pl.Mobile.Frozen = true;

                            Debuff(pl.Mobile);
                            CancelSpell(pl.Mobile);

                            pl.Mobile.Delta(MobileDelta.Noto);
                        }

                        arena.MoveInside(p.Players, i);
                    }

                    if (m_EventGame != null)
                        m_EventGame.OnStart();

                    StartCountdown(10, new CountdownCallback(SendBeginGump));

                    mg.Appear(m_GatePoint, m_GateFacet);
                }
                else
                {
                    for (int i = 0; i < m_Participants.Count; ++i)
                    {
                        Participant p = (Participant)m_Participants[i];

                        for (int j = 0; j < p.Players.Length; ++j)
                        {
                            DuelPlayer dp = p.Players[j];

                            if (dp != null)
                                dp.Mobile.SendMessage("The duel could not be started because all arenas are full. If you want to stop waiting for a free arena, yield the duel.");
                        }
                    }

                    StartCountdown(10, new CountdownCallback(SendReadyGump));
                }

                return;
            }

            m_ReadyWait = true;

            bool isAllReady = true;

            Type[] types = new Type[] { typeof(ReadyGump) };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    Mobile mob = pl.Mobile;

                    if (pl.Ready)
                    {
                        if (m_Tournament == null)
                            CloseAndSendGump(mob, new ReadyGump(mob, this, count), types);
                    }
                    else
                    {
                        isAllReady = false;
                    }
                }
            }

            if (count == -1 && isAllReady)
                StartCountdown(3, new CountdownCallback(SendReadyGump));
        }

        public static void CloseAndSendGump(Mobile mob, Gump g, params Type[] types)
        {
            CloseAndSendGump(mob.NetState, g, types);
        }

        public static void CloseAndSendGump(NetState ns, Gump g, params Type[] types)
        {
            if (ns != null)
            {
                Mobile mob = ns.Mobile;

                if (mob != null)
                {
                    foreach (Type type in types)
                    {
                        mob.CloseGump(type);
                    }

                    mob.SendGump(g);
                }
            }

            /*if ( ns == null )
                return;

            for ( int i = 0; i < types.Length; ++i )
                ns.Send( new CloseGump( Gump.GetTypeID( types[i] ), 0 ) );

            g.SendTo( ns );

            ns.AddGump( g );

            Packet[] packets = new Packet[types.Length + 1];

            for ( int i = 0; i < types.Length; ++i )
                packets[i] = new CloseGump( Gump.GetTypeID( types[i] ), 0 );

            packets[types.Length] = (Packet) typeof( Gump ).InvokeMember( "Compile", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, g, null, null );

            bool compress = ns.CompressionEnabled;
            ns.CompressionEnabled = false;
            ns.Send( BindPackets( compress, packets ) );
            ns.CompressionEnabled = compress;*/
        }

        /*public static Packet BindPackets( bool compress, params Packet[] packets )
        {
            if ( packets.Length == 0 )
                throw new ArgumentException( "No packets to bind", "packets" );

            byte[][] compiled = new byte[packets.Length][];
            int[] lengths = new int[packets.Length];

            int length = 0;

            for ( int i = 0; i < packets.Length; ++i )
            {
                compiled[i] = packets[i].Compile( compress, out lengths[i] );
                length += lengths[i];
            }

            return new BoundPackets( length, compiled, lengths );
        }

        private class BoundPackets : Packet
        {
            public BoundPackets( int length, byte[][] compiled, int[] lengths ) : base( 0, length )
            {
                m_Stream.Seek( 0, System.IO.SeekOrigin.Begin );

                for ( int i = 0; i < compiled.Length; ++i )
                    m_Stream.Write( compiled[i], 0, lengths[i] );
            }
        }*/
    }

    public interface IDuelItem
    {
        string DuelCategory { get; }
        string DuelOption { get; }
    }
}