using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Annie
{
    internal class LaneJungleClearA
    {
        public enum AttackSpell
        {
            Q,
            W
        };

        public static AIHeroClient Annie
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

        public static void LaneClear()
        {
            var QCHECK = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            var WCHECK = Program.LaneJungleClear["LCW"].Cast<CheckBox>().CurrentValue;
            var WREADY = Program.W.IsReady();

            if (!QCHECK && !QREADY)
            {
                return;
            }
            var qenemy = (Obj_AI_Minion) GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);
            if (qenemy != null)
                {
                    Program.Q.Cast(qenemy);
                }
            if (!WCHECK || !WREADY)
            {
                return;
            }
            var wminion = (Obj_AI_Minion) GetEnemy(Program.W.Range, GameObjectType.obj_AI_Minion);
            if (wminion != null)
                {
                    Program.W.Cast(wminion.ServerPosition);
                }
            if (Orbwalker.CanAutoAttack)
            {
                var enemy = (Obj_AI_Minion) GetEnemy(Annie.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Orbwalker.ForcedTarget = enemy;
            }
        }
    }
}