using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Evelynn
{
    internal static class LaneJungleClearA
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

        private static Obj_AI_Base GetLcEnemy(float range, GameObjectType t)
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
            var echeck = Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue;
            var eready = Program.E.IsReady();
            var qcheck = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var qready = Program.Q.IsReady();

            if (echeck && eready)
            {
                var enemy = (Obj_AI_Minion)GetLcEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.E.Cast(enemy);
            }

            if (!qcheck || !qready) return;
            {
                var enemy = GetLcEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.Q.Cast();
            }
        }
        public static void JungleClearB()
        {
                      foreach (var minion in EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Monster))
                      {
                          var echeck = Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue;
                          var eready = Program.E.IsReady();
                          var qcheck = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
                          var qready = Program.Q.IsReady();

            if (echeck && eready)
            {

                if (minion != null)
                    Program.E.Cast(minion);
            }

                          if (!qcheck || !qready) continue;
                          if (minion != null)
                              Program.Q.Cast();
                      }
    }
    }
}