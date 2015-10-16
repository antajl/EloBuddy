using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Ziggs_v2
{
    internal class LastHitA
    {
        public enum AttackSpell
        {
            Q
        };

        public static AIHeroClient Ziggs
        {
            get { return ObjectManager.Player; }
        }
        public static float Qcalc(Obj_AI_Base target)
        {
            return Ziggs.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] { 0, 75, 120, 165, 210, 255 }[Program.Q.Level] +
                 (0.75f * Ziggs.FlatMagicDamageMod)));
        }

        public static Obj_AI_Base MinionLh(GameObjectType type, AttackSpell spell)
        {
            return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                               && a.Type == type
                                                                                               &&
                                                                                               a.Distance(Ziggs) <=
                                                                                               Program.Q.Range
                                                                                               && !a.IsDead
                                                                                               && !a.IsInvulnerable
                                                                                               &&
                                                                                               a.IsValidTarget(
                                                                                                   Program.Q.Range)
                                                                                               &&
                                                                                               a.Health <= Qcalc(a));
        }

   
        public static void LastHitB()
        {
            var QCHECK = Program.LastHitMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            if (!QCHECK || !QREADY)
            {
                return;
            }

            var minion = (Obj_AI_Minion)MinionLh(GameObjectType.obj_AI_Minion, AttackSpell.Q);
            if (minion != null)
            {
                {
                    var predQ = Program.Q.GetPrediction(minion).CastPosition;
                    Program.Q.Cast(predQ);
                }
            }
        }
    }
}