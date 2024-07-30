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

/* Scripts/Spells/Initializer.cs
 * ChangeLog:
 *  9/23/22, Yoar
 *      Now building spell registry on Configure, not Initialize
 *      This way we can refer to the spell registry during world load
 *	7/23/05, Adam
 *		Remove all Necromancy, and Chivalry nonsense
 *	6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;

namespace Server.Spells
{
    public class Initializer
    {
        public static void Configure()
        {
            // First circle
            Register(00, typeof(First.ClumsySpell));
            Register(01, typeof(First.CreateFoodSpell));
            Register(02, typeof(First.FeeblemindSpell));
            Register(03, typeof(First.HealSpell));
            Register(04, typeof(First.MagicArrowSpell));
            Register(05, typeof(First.NightSightSpell));
            Register(06, typeof(First.ReactiveArmorSpell));
            Register(07, typeof(First.WeakenSpell));

            // Second circle
            Register(08, typeof(Second.AgilitySpell));
            Register(09, typeof(Second.CunningSpell));
            Register(10, typeof(Second.CureSpell));
            Register(11, typeof(Second.HarmSpell));
            Register(12, typeof(Second.MagicTrapSpell));
            Register(13, typeof(Second.RemoveTrapSpell));
            Register(14, typeof(Second.ProtectionSpell));
            Register(15, typeof(Second.StrengthSpell));

            // Third circle
            Register(16, typeof(Third.BlessSpell));
            Register(17, typeof(Third.FireballSpell));
            Register(18, typeof(Third.MagicLockSpell));
            Register(19, typeof(Third.PoisonSpell));
            Register(20, typeof(Third.TelekinesisSpell));
            Register(21, typeof(Third.TeleportSpell));
            Register(22, typeof(Third.UnlockSpell));
            Register(23, typeof(Third.WallOfStoneSpell));

            // Fourth circle
            Register(24, typeof(Fourth.ArchCureSpell));
            Register(25, typeof(Fourth.ArchProtectionSpell));
            Register(26, typeof(Fourth.CurseSpell));
            Register(27, typeof(Fourth.FireFieldSpell));
            Register(28, typeof(Fourth.GreaterHealSpell));
            Register(29, typeof(Fourth.LightningSpell));
            Register(30, typeof(Fourth.ManaDrainSpell));
            Register(31, typeof(Fourth.RecallSpell));

            // Fifth circle
            Register(32, typeof(Fifth.BladeSpiritsSpell));
            Register(33, typeof(Fifth.DispelFieldSpell));
            Register(34, typeof(Fifth.IncognitoSpell));
            Register(35, typeof(Fifth.MagicReflectSpell));
            Register(36, typeof(Fifth.MindBlastSpell));
            Register(37, typeof(Fifth.ParalyzeSpell));
            Register(38, typeof(Fifth.PoisonFieldSpell));
            Register(39, typeof(Fifth.SummonCreatureSpell));

            // Sixth circle
            Register(40, typeof(Sixth.DispelSpell));
            Register(41, typeof(Sixth.EnergyBoltSpell));
            Register(42, typeof(Sixth.ExplosionSpell));
            Register(43, typeof(Sixth.InvisibilitySpell));
            Register(44, typeof(Sixth.MarkSpell));
            Register(45, typeof(Sixth.MassCurseSpell));
            Register(46, typeof(Sixth.ParalyzeFieldSpell));
            Register(47, typeof(Sixth.RevealSpell));

            // Seventh circle
            Register(48, typeof(Seventh.ChainLightningSpell));
            Register(49, typeof(Seventh.EnergyFieldSpell));
            Register(50, typeof(Seventh.FlameStrikeSpell));
            Register(51, typeof(Seventh.GateTravelSpell));
            Register(52, typeof(Seventh.ManaVampireSpell));
            Register(53, typeof(Seventh.MassDispelSpell));
            Register(54, typeof(Seventh.MeteorSwarmSpell));
            Register(55, typeof(Seventh.PolymorphSpell));

            // Eighth circle
            Register(56, typeof(Eighth.EarthquakeSpell));
            Register(57, typeof(Eighth.EnergyVortexSpell));
            Register(58, typeof(Eighth.ResurrectionSpell));
            Register(59, typeof(Eighth.AirElementalSpell));
            Register(60, typeof(Eighth.SummonDaemonSpell));
            Register(61, typeof(Eighth.EarthElementalSpell));
            Register(62, typeof(Eighth.FireElementalSpell));
            Register(63, typeof(Eighth.WaterElementalSpell));
            /*
                        if ( Core.AOS )
                        {
                            // Necromancy spells
                            Register( 100, typeof( Necromancy.AnimateDeadSpell ) );
                            Register( 101, typeof( Necromancy.BloodOathSpell ) );
                            Register( 102, typeof( Necromancy.CorpseSkinSpell ) );
                            Register( 103, typeof( Necromancy.CurseWeaponSpell ) );
                            Register( 104, typeof( Necromancy.EvilOmenSpell ) );
                            Register( 105, typeof( Necromancy.HorrificBeastSpell ) );
                            Register( 106, typeof( Necromancy.LichFormSpell ) );
                            Register( 107, typeof( Necromancy.MindRotSpell ) );
                            Register( 108, typeof( Necromancy.PainSpikeSpell ) );
                            Register( 109, typeof( Necromancy.PoisonStrikeSpell ) );
                            Register( 110, typeof( Necromancy.StrangleSpell ) );
                            Register( 111, typeof( Necromancy.SummonFamiliarSpell ) );
                            Register( 112, typeof( Necromancy.VampiricEmbraceSpell ) );
                            Register( 113, typeof( Necromancy.VengefulSpiritSpell ) );
                            Register( 114, typeof( Necromancy.WitherSpell ) );
                            Register( 115, typeof( Necromancy.WraithFormSpell ) );

                            // Paladin abilities
                            Register( 200, typeof( Chivalry.CleanseByFireSpell ) );
                            Register( 201, typeof( Chivalry.CloseWoundsSpell ) );
                            Register( 202, typeof( Chivalry.ConsecrateWeaponSpell ) );
                            Register( 203, typeof( Chivalry.DispelEvilSpell ) );
                            Register( 204, typeof( Chivalry.DivineFurySpell ) );
                            Register( 205, typeof( Chivalry.EnemyOfOneSpell ) );
                            Register( 206, typeof( Chivalry.HolyLightSpell ) );
                            Register( 207, typeof( Chivalry.NobleSacrificeSpell ) );
                            Register( 208, typeof( Chivalry.RemoveCurseSpell ) );
                            Register( 209, typeof( Chivalry.SacredJourneySpell ) );
                        }
            */
        }

        public static void Register(int spellID, Type type)
        {
            SpellRegistry.Register(spellID, type);
        }
    }
}