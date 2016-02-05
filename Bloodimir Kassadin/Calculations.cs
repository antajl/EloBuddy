using EloBuddy;
using EloBuddy.SDK;


namespace Bloodimir_Kassadin
{
    internal class Calcs
    {
        private static readonly AIHeroClient Kassawin = ObjectManager.Player;
        public static float QCalc(Obj_AI_Base target)
        {
            return Kassawin.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 70, 95, 120, 145, 170 }[Program.Q.Level] + 0.7 * Kassawin.FlatMagicDamageMod
                    ));
        }

        public static float WCalc(Obj_AI_Base target)
        {
            return Kassawin.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 20, 45, 70, 95, 120 }[Program.W.Level] + 0.6 * Kassawin.FlatMagicDamageMod
                    ));
        }

        public static float ECalc(Obj_AI_Base target)
        {
            return Kassawin.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 80, 105, 130, 155, 180 }[Program.E.Level] + 0.7 * Kassawin.FlatMagicDamageMod
                    ));
        }

        public static float RCalc(Obj_AI_Base target)
        {
            return Kassawin.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 80, 100, 120 }[Program.R.Level] + 0.2 * Kassawin.MaxMana + 0.1 * Kassawin.FlatMagicDamageMod
                    ));
        }

        public static float DmgCalc(AIHeroClient target)
        {
            var damage = 0f;
            if (Program.Q.IsReady() && target.IsValidTarget(Program.Q.Range))
                damage += QCalc(target);
            if (Program.W.IsReady())
                damage += WCalc(target);
            if (Program.E.IsReady() && target.IsValidTarget(Program.E.Range))
                damage += ECalc(target);
            if (Program.R.IsReady() && target.IsValidTarget(Program.R.Range))
                damage += RCalc(target);
            damage += Kassawin.GetAutoAttackDamage(target, true) * 2;
            return damage;
        }
    }
}