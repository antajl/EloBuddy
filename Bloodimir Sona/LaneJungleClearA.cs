using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Sona
{
    internal class LaneJungleClearA
    {
        public enum AttackSpell
        {
            Q
        };

        public static AIHeroClient Sona
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
                case GameObjectType.obj_AI_Minion:
                    return EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderBy(a => a.Health).FirstOrDefault(
                            a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
                default:
                    return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
            }
        }
        public static void LaneClear()
        {
            var QCHECK = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();

            if (!QCHECK || !QREADY)
            {
                var qenemy = (Obj_AI_Minion)GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

                if (qenemy != null)
                    {
                        Program.Q.Cast();
                    }
                if (Orbwalker.CanAutoAttack)
                {
                    var enemy = (Obj_AI_Minion)GetEnemy(Sona.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                    if (enemy != null)
                        Orbwalker.ForcedTarget = enemy;
                }
            }
        }
    }
}