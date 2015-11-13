using EloBuddy;
using EloBuddy.SDK;

namespace Kennen
{
    internal static class Misc
    {
        private static AIHeroClient Kennen
        {
            get { return ObjectManager.Player; }
        }

        public static float Qcalc(Obj_AI_Base target)
        {
            return Kennen.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] {0, 75, 115, 155, 195, 235}[Program.Q.Level] +
                 (0.75f*Kennen.FlatMagicDamageMod)));
        }

        public static float Wcalc(Obj_AI_Base target)
        {
            return Kennen.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] {0, 65, 95, 125, 155, 185}[Program.W.Level] +
                 (0.55f*Kennen.FlatMagicDamageMod)));
        }

        public static float Ecalc(Obj_AI_Base target)
        {
            return Kennen.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] {0, 43, 63, 83, 102, 122}[Program.E.Level] +
                 (0.30f*Kennen.FlatMagicDamageMod)));
        }
    }
}