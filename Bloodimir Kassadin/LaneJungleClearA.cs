using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Kassadin
{
    internal static class LaneJungleClearA
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

        public static void LaneClear()
        {
            var qcheck = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var qready = Program.Q.IsReady();
            var echeck = Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue;
            var eready = Program.E.IsReady();
            var rcheck = Program.LaneJungleClear["LCR"].Cast<CheckBox>().CurrentValue;
            var rready = Program.R.IsReady();

            if (!qcheck || !qready) return;
            var qenemy = (Obj_AI_Minion)GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

            if (qenemy != null)
            {
                Program.Q.Cast(qenemy);
            }

            {
            if (!echeck || !eready) return;
            var eminion = (Obj_AI_Minion)GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);
                    Program.E.Cast(eminion.Position);
                 }
    {
        if (!rcheck || !rready) return;
            var renemy = (Obj_AI_Minion)GetEnemy(Program.R.Range, GameObjectType.obj_AI_Minion);
            if (renemy != null && Program.RMana < 400)
            {
                Program.R.Cast(renemy.ServerPosition);
            }}
    }
    }
}