using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;

namespace Kennen
{
    internal static class LastHitA
    {
        public enum AttackSpell
        {
            Q,
            W
        };

        private static AIHeroClient Kennen
        {
            get { return ObjectManager.Player; }
        }

        private static Obj_AI_Base MinionLh(GameObjectType type, AttackSpell spell)
        {
            return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                                            &&
                                                                                                            a.Type ==
                                                                                                            type
                                                                                                            &&
                                                                                                            a.Distance(
                                                                                                                Kennen) <=
                                                                                                            Program.Q
                                                                                                                .Range
                                                                                                            && !a.IsDead
                                                                                                            &&
                                                                                                            !a
                                                                                                                .IsInvulnerable
                                                                                                            &&
                                                                                                            a
                                                                                                                .IsValidTarget
                                                                                                                (
                                                                                                                    Program
                                                                                                                        .Q
                                                                                                                        .Range)
                                                                                                            &&
                                                                                                            a.Health <=
                                                                                                            Misc.Qcalc(a));
        }

        private static Obj_AI_Base MinionWlh(GameObjectType type, AttackSpell spell)
        {
            return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                                            &&
                                                                                                            a.Type ==
                                                                                                            type
                                                                                                            &&
                                                                                                            a.Distance(
                                                                                                                Kennen) <=
                                                                                                            Program.W
                                                                                                                .Range
                                                                                                            && !a.IsDead
                                                                                                            &&
                                                                                                            !a
                                                                                                                .IsInvulnerable
                                                                                                            &&
                                                                                                            a
                                                                                                                .IsValidTarget
                                                                                                                (
                                                                                                                    Program
                                                                                                                        .W
                                                                                                                        .Range)
                                                                                                            &&
                                                                                                            a.Health <=
                                                                                                            Misc.Wcalc(a));
        }

        public static void LastHitB()
        {
            var qcheck = Program.LastHit["LHQ"].Cast<CheckBox>().CurrentValue;
            var qready = Program.Q.IsReady();
            var wcheck = Program.LastHit["LHW"].Cast<CheckBox>().CurrentValue;
            var wready = Program.W.IsReady();

            if (!qcheck || !qready) return;
            {
                var minion = (Obj_AI_Minion) MinionLh(GameObjectType.obj_AI_Minion, AttackSpell.Q);
                if (minion != null)
                {
                    if (Program.Q.MinimumHitChance >= HitChance.Low)
                    {
                        Program.Q.Cast(minion.ServerPosition);
                    }

                    if (!wcheck || !wready) return;
                    {
                        var wminion = (Obj_AI_Minion) MinionWlh(GameObjectType.obj_AI_Minion, AttackSpell.W);
                        if (wminion != null)
                        {
                            if (wminion.HasBuff("kennenmarkofstorm"))
                            {
                                Program.W.Cast();
                            }
                        }
                    }
                }
            }
        }
    }
}