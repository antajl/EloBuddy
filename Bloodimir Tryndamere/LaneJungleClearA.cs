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

        public static Obj_AI_Base GetEnemy(float range, GameObjectType type)
        {
            return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                               && a.Type == type
                                                                                               &&
                                                                                               a.Distance(Tryndamere) <=
                                                                                               range
                                                                                               && !a.IsDead
                                                                                               && !a.IsInvulnerable
                                                                                               && a.IsValidTarget(range));
        }

        public static void LaneClear()
        {
            var ECHECK = Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue;
            var EREADY = Program.E.IsReady();

            if (ECHECK && EREADY)
            {
                var aenemy = (Obj_AI_Minion) GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);

                if (aenemy != null)
                    Program.E.Cast(aenemy.ServerPosition);
            }
            var benemy = (Obj_AI_Minion) GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);
            if (Program.MiscMenu["usehydra"].Cast<CheckBox>().CurrentValue)
            {
                if (Program.hydra.IsOwned() && Program.hydra.IsReady() &&
                    Program.hydra.IsInRange(benemy))
                    Program.hydra.Cast();
            }
            if (Program.MiscMenu["useTiamat"].Cast<CheckBox>().CurrentValue)
            {
                if (Program.tiamat.IsOwned() && Program.tiamat.IsReady() &&
                    Program.tiamat.IsInRange(benemy))
                    Program.tiamat.Cast();
            }
            if (Orbwalker.CanAutoAttack)
            {
                var cenemy = (AIHeroClient) GetEnemy(Tryndamere.GetAutoAttackRange(), GameObjectType.AIHeroClient);

                if (cenemy != null)
                    Orbwalker.ForcedTarget = cenemy;
            }
        }
    }
}