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

        public static Obj_AI_Base GetEnemy(float range, GameObjectType t)
        {
            switch (t)
            {
                case GameObjectType.AIHeroClient:
                    return EntityManager.Heroes.Enemies.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
                default:
                    return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(
                        a =>
                            a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable &&
                            a.Health <= Calculations.Passivecalc(a));
            }
        }

        public static Obj_AI_Base MinionLh(GameObjectType type, AttackSpell spell)
        {
            return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                                            &&
                                                                                                            a.Type ==
                                                                                                            type
                                                                                                            &&
                                                                                                            a.Distance(
                                                                                                                Ziggs) <=
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
                                                                                                            Calculations
                                                                                                                .Qcalc(a));
        }

        public static void LastHitB()
        {
            var QCHECK = Program.LastHitMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            if (!QCHECK || !QREADY)
            {
                return;
            }

            var minion = (Obj_AI_Minion) MinionLh(GameObjectType.obj_AI_Minion, AttackSpell.Q);
            if (minion != null)
                if (Ziggs.ManaPercent > Program.LastHitMenu["lhmanamanager"].Cast<Slider>().CurrentValue)
            {
                {
                    var predQ = Program.Q.GetPrediction(minion).CastPosition;
                    Program.Q.Cast(predQ);
                }
            }
        }
    }
}