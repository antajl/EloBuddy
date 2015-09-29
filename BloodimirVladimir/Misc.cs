using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace BloodimirVladimir
{
    internal class Misc
    {
        public static void EHslider()
        {
            var ehealth = Program.MiscMenu["ehealth"].Cast<Slider>().CurrentValue;
            if (Program._Player.HealthPercent < ehealth)
            {
                return;
            }

        }
        private static AIHeroClient Vladimir { get { return ObjectManager.Player; } }
        public static float Qdmg(Obj_AI_Base target)
        {
            return Vladimir.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] { 0, 90, 125, 160, 195, 230 }[Program.Q.Level] + (0.6f * Vladimir.FlatMagicDamageMod)));
        }
        public static float Edmg(Obj_AI_Base target)
        {
            return Vladimir.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] { 0, 60, 85, 110, 135, 160 }[Program.E.Level] + (0.45f * Vladimir.FlatMagicDamageMod)));
        }
        public static float Rdmg(Obj_AI_Base target)
        {
            return Vladimir.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] { 0, 168, 280, 392 }[Program.E.Level] + (0.78f * Vladimir.FlatMagicDamageMod)));
        }
    }
}
