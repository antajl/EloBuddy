using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Ziggs
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

        public static Obj_AI_Base GetEnemy(float range, GameObjectType type)
        {
            return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                               && a.Type == type
                                                                                               &&
                                                                                               a.Distance(Ziggs) <=
                                                                                               range
                                                                                               && !a.IsDead
                                                                                               && !a.IsInvulnerable
                                                                                               && a.IsValidTarget(range));
        }

        public static Obj_AI_Base GetEnemy(GameObjectType type, AttackSpell spell)
        {
            if (spell == AttackSpell.Q)
            {
                return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                                   && a.Type == type
                                                                                                   &&
                                                                                                   a.Distance(Ziggs) <=
                                                                                                   Spells.Q.Range
                                                                                                   && !a.IsDead
                                                                                                   && !a.IsInvulnerable
                                                                                                   &&
                                                                                                   a.IsValidTarget(
                                                                                                       Spells.Q.Range)
                                                                                                   &&
                                                                                                   a.Health <=
                                                                                                   Misc.Qcalc(a));
            }

            return null;
        }

        public static void LaneClear()
        {
            var QCHECK = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Spells.Q.IsReady();
            var WCHECK = Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue;
            var WREADY = Spells.W.IsReady();

            if (QCHECK && QREADY)
            {
                var enemy = (Obj_AI_Minion) GetEnemy(Spells.Q.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Spells.Q.Cast(enemy.ServerPosition);
            }
            if (!WCHECK || !WREADY)
            {
                return;
            }
            var eminion = (Obj_AI_Minion) GetEnemy(Spells.E.Range, GameObjectType.obj_AI_Minion);
            if (eminion != null)
            {
                Spells.E.Cast(eminion.ServerPosition);
            }
            if (Orbwalker.CanAutoAttack)
            {
                var enemy = (Obj_AI_Minion) GetEnemy(Ziggs.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Orbwalker.ForcedTarget = enemy;
            }
        }
    }
}