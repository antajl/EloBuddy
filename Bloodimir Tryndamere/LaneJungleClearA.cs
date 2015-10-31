using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Tryndamere
{
    internal class LaneJungleClearA
    {
        public enum AttackSpell
        {
            E
        };

        public static AIHeroClient Tryndamere
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

        public static void LaneClearA()
        {
            var ECHECK = Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue;
            var EREADY = Program.E.IsReady();

            if (!ECHECK || !EREADY)
            {
                return;
            }
            {
                var aenemy = (Obj_AI_Minion)GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);

                if (aenemy != null)
                    Program.E.Cast(aenemy.ServerPosition);
            }
            var benemy = (Obj_AI_Minion)GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);
            if (Program.MiscMenu["usehydra"].Cast<CheckBox>().CurrentValue)
            {
                if (Program.Hydra.IsOwned() && Program.Hydra.IsReady() &&
                    Program.Hydra.IsInRange(benemy))
                    Program.Hydra.Cast();
            }
            if (Program.MiscMenu["useTiamat"].Cast<CheckBox>().CurrentValue)
            {
                if (Program.Tiamat.IsOwned() && Program.Tiamat.IsReady() &&
                    Program.Tiamat.IsInRange(benemy))
                    Program.Tiamat.Cast();
            }
            if (Orbwalker.CanAutoAttack)
            {
                var cenemy = (Obj_AI_Minion)GetEnemy(Tryndamere.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                if (cenemy != null)
                    Orbwalker.ForcedTarget = cenemy;
            }
        }
    }
}