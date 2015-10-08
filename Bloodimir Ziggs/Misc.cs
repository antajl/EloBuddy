using EloBuddy;
using EloBuddy.SDK;

namespace Bloodimir_Ziggs
{
    internal class Misc
    {
        private static AIHeroClient Ziggs
        {
            get { return ObjectManager.Player; }
        }

        public static float Qcalc(Obj_AI_Base target)
        {
            return Ziggs.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] {0, 75, 120, 165, 210, 255}[Spells.Q.Level] +
                 (0.65f*Ziggs.FlatMagicDamageMod)));
        }

        public static float Wcalc(Obj_AI_Base target)
        {
            return Ziggs.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] {0, 70, 105, 140, 175, 210}[Spells.W.Level] +
                 (0.35f*Ziggs.FlatMagicDamageMod)));
        }

        public static float Ecalc(Obj_AI_Base target)
        {
            return Ziggs.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] {0, 40, 65, 90, 115, 140}[Spells.E.Level] +
                 (0.30f*Ziggs.FlatMagicDamageMod)) + (new float[] {0, 16, 26, 36, 46, 56}[Spells.E.Level] +
                                                      (0.12f*Ziggs.FlatMagicDamageMod)));
        }
        public static float Rcalc(Obj_AI_Base target)
        {
            return Ziggs.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] {0, 200, 300, 400,}[Spells.R.Level] +
                 (0.72f*Ziggs.FlatMagicDamageMod)));
        }
        public static float Passivecalc(Obj_AI_Base target)
        {
            return Ziggs.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] {20, 24, 28, 32, 36, 40, 44, 48, 56, 64, 72, 80, 88, 100, 112, 124, 136, 148, 160}[
                    Ziggs.Level] +
                 (0.38f*Ziggs.FlatMagicDamageMod)));
            
        }
    }
}