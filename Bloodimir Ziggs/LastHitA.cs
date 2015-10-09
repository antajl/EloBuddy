using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Ziggs
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
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable && a.Health <= Misc.Passivecalc(a));
        }
            }
        public static Obj_AI_Base MinionLh(GameObjectType type, AttackSpell spell)
        {
            return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                               && a.Type == type
                                                                                               &&
                                                                                               a.Distance(Ziggs) <=
                                                                                               Spells.Q.Range
                                                                                               && !a.IsDead
                                                                                               && !a.IsInvulnerable
                                                                                               &&
                                                                                               a.IsValidTarget(
                                                                                                   Spells.Q.Range)
                                                                                               &&
                                                                                               a.Health <= Misc.Qcalc(a));
        }

        public static void LastHitB()
        {
            var QCHECK = Program.LastHit["LHQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Spells.Q.IsReady();
            if (!QCHECK || !QREADY)
            {
                return;
            }

            var minion = (Obj_AI_Minion) MinionLh(GameObjectType.obj_AI_Minion, AttackSpell.Q);
            if (minion != null)
            {
                if (Spells.Q.MinimumHitChance >= HitChance.Low)
                {
                    Spells.Q.Cast(minion.ServerPosition);
                }
                if (Orbwalker.CanAutoAttack)
                {
                    if (Player.HasBuff("ziggsShortFuse"))
            {
                    var enemy = (Obj_AI_Minion) GetEnemy(Ziggs.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);
                    if (enemy.Health <= Misc.Passivecalc(enemy))
                    if (enemy != null)
                        Orbwalker.ForcedTarget = enemy;
                }

            }
        }
    }
}
}