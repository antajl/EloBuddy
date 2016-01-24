using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Kennen
{
    internal static class LaneJungleClearA
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

        private static Obj_AI_Base GetEnemy(float range, GameObjectType t)
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
            var qcheck = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var qready = Program.Q.IsReady();
            var wcheck = Program.LaneJungleClear["LCW"].Cast<CheckBox>().CurrentValue;
            var wready = Program.W.IsReady();

            if (!qcheck || !qready) return;
            {
                var enemy = (Obj_AI_Minion) GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.Q.Cast(enemy.ServerPosition);
            }
            if (!wcheck || !wready) return;
            var wminion = (Obj_AI_Minion) GetEnemy(Program.W.Range, GameObjectType.obj_AI_Minion);
            if (wminion == null) return;
            if (wminion.HasBuff("kennenmarkofstorm"))
            {
                Program.W.Cast();
            }
        }
    }
}