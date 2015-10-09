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
            Q,
            W
        };

        public static AIHeroClient Kennen
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
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
            }
        }

        public static Obj_AI_Base GetEnemy(GameObjectType type, AttackSpell spell)
        {
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
            var WCHECK = Program.LaneJungleClear["LCW"].Cast<CheckBox>().CurrentValue;
            var WREADY = Program.W.IsReady();

            if (QCHECK && QREADY)
            {
                var enemy = (Obj_AI_Minion) GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.Q.Cast(enemy.ServerPosition);
            }
            if (!WCHECK || !WREADY)
            {
                return;
            }
            var wminion = (Obj_AI_Minion)GetEnemy(Program.W.Range, GameObjectType.obj_AI_Minion);
            if (wminion != null)
            {
                if (wminion.HasBuff("kennenmarkofstorm"))
                {
                    Program.W.Cast();
                }
                if (Orbwalker.CanAutoAttack)
                {
                    var enemy = (Obj_AI_Minion) GetEnemy(Kennen.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                    if (enemy != null)
                        Orbwalker.ForcedTarget = enemy;
                }
            }
        }
    }
}