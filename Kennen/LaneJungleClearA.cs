using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Kennen
{
    internal class LaneJungleClearA
    {
        public enum AttackSpell
        {
            Q
        };

        public static AIHeroClient Kennen
        {
            get { return ObjectManager.Player; }
        }

        public static Obj_AI_Base GetEnemy(float range, GameObjectType type)
        {
            return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                               && a.Type == type
                                                                                               &&
                                                                                               a.Distance(Kennen) <=
                                                                                               range
                                                                                               && !a.IsDead
                                                                                               && !a.IsInvulnerable
                                                                                               && a.IsValidTarget(range));
        }

        public static Obj_AI_Base GetEnemy(GameObjectType type, AttackSpell spell)
        {
            var eminion =
                EntityManager.GetJungleMonsters(Program.Kennen.Position.To2D(), Program.E.Range)
                    .FirstOrDefault(
                        m =>
                            m.Distance(Program.Kennen) <= Program.E.Range &&
                            m.Health <= Misc.Ecalc(m) &&
                            m.IsValidTarget());

            if (spell == AttackSpell.Q)
            {
                return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                                   && a.Type == type
                                                                                                   &&
                                                                                                   a.Distance(Kennen) <=
                                                                                                   Program.Q.Range
                                                                                                   && !a.IsDead
                                                                                                   && !a.IsInvulnerable
                                                                                                   &&
                                                                                                   a.IsValidTarget(
                                                                                                       Program.Q.Range)
                                                                                                   &&
                                                                                                   a.Health <=
                                                                                                   Misc.Qcalc(a));
            }

            return null;
        }

        public static void LaneClear()
        {
            var QCHECK = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            if (QCHECK && QREADY)
            {
                var enemy = (Obj_AI_Minion)GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.Q.Cast(enemy.ServerPosition);
            }
            if (Orbwalker.CanAutoAttack)
            {
                var enemy = (AIHeroClient)GetEnemy(Kennen.GetAutoAttackRange(), GameObjectType.AIHeroClient);

                if (enemy != null)
                    Orbwalker.ForcedTarget = enemy;
            }
        }
    }
}