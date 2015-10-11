using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Ziggs_v2
{
    internal class LaneJungleClearA
    {
        public enum AttackSpell
        {
            Q,
            E
        };

        public static AIHeroClient Ziggs
        {
            get { return ObjectManager.Player; }
        }

        public static Obj_AI_Base GetEnemy(float range, GameObjectType t)
        {
            switch (t)
            {
                case GameObjectType.obj_AI_Hero:
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
            var ECHECK = Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue;
            var EREADY = Program.E.IsReady();

            if (QCHECK && QREADY)
            {
                var qenemy = (Obj_AI_Minion)GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

                if (qenemy != null)
                    if (Ziggs.ManaPercent > Program.LaneJungleClear["lcmanamanager"].Cast<Slider>().CurrentValue)
                    {
                        var predQ = Program.Q.GetPrediction(qenemy).CastPosition;
                        Program.Q.Cast(predQ);
                    }
                if (!ECHECK || !EREADY)
                {
                    return;
                }
                var eminion = (Obj_AI_Minion)GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);
                if (eminion != null)
                    if (Ziggs.ManaPercent > Program.LaneJungleClear["lcmanamanager"].Cast<Slider>().CurrentValue)
                    {
                        var predE = Program.Q.GetPrediction(eminion).CastPosition;
                        Program.E.Cast(predE);
                    }
                if (Orbwalker.CanAutoAttack)
                {
                    var enemy = (Obj_AI_Minion)GetEnemy(Ziggs.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                    if (enemy != null)
                        Orbwalker.ForcedTarget = enemy;
                }
            }
        }
    }
}