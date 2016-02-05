using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Kassadin
{
    internal static class LastHitA
    {
        public static AIHeroClient Kassawin
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

        public static void LastHitB()
        {
            var qcheck = Program.LastHitMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var qready = Program.Q.IsReady();
            var wcheck = Program.LastHitMenu["LHW"].Cast<CheckBox>().CurrentValue;
            var wready = Program.E.IsReady();
            if (!qcheck || !qready) return;
            var qenemy = (Obj_AI_Minion) GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);
            if (qenemy == null) return;
            {
                if (qenemy.Health < Calcs.QCalc(qenemy))
                Program.Q.Cast(qenemy);
            }
            if (!wcheck || !wready) return;
            var wenemy = (Obj_AI_Minion)GetEnemy(Program.W.Range, GameObjectType.obj_AI_Minion);
            if (wenemy == null) return;
            {
                if (wenemy.Health < Calcs.WCalc(wenemy))
                    Program.W.Cast();
            }
                 }
        }
    }
