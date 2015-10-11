using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Evelynn
{
    internal class LaneJungleClearA
    {
        public enum AttackSpell
        {
            E,
            Q
        };

        public static AIHeroClient Evelynn
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

        public static void LaneClearB()
        {
            var ECHECK = Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue;
            var EREADY = Program.E.IsReady();
            var QCHECK = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();

            if (ECHECK && EREADY)
            {
                var enemy = (Obj_AI_Minion)GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.E.Cast(enemy);
            }

            if (QCHECK && QREADY)
            {
                var enemy = GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.Q.Cast();
            }
        }

    }
}