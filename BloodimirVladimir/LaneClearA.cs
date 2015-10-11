using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace BloodimirVladimir
{
    internal class LaneClearA
    {
        public enum AttackSpell
        {
            E,
            Q
        };

        public static AIHeroClient Vladimir
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

        public static Obj_AI_Base GetEnemy(GameObjectType type, AttackSpell spell)
        {
            if (spell == AttackSpell.E)
            {
                return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                                   && a.Type == type
                                                                                                   &&
                                                                                                   a.Distance(Vladimir) <=
                                                                                                   Program.E.Range
                                                                                                   && !a.IsDead
                                                                                                   && !a.IsInvulnerable
                                                                                                   &&
                                                                                                   a.IsValidTarget(
                                                                                                       Program.E.Range)
                                                                                                   &&
                                                                                                   a.Health <=
                                                                                                   Misc.Edmg(a));
            }

            if (spell == AttackSpell.Q)
            {
                return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                                   && a.Type == type
                                                                                                   &&
                                                                                                   a.Distance(Vladimir) <=
                                                                                                   Program.Q.Range
                                                                                                   && !a.IsDead
                                                                                                   && !a.IsInvulnerable
                                                                                                   &&
                                                                                                   a.IsValidTarget(
                                                                                                       Program.Q.Range)
                                                                                                   &&
                                                                                                   a.Health <=
                                                                                                   Misc.Qdmg(a));
            }

            return null;
        }

        public static void LaneClear()
        {
            var ECHECK = Program.LaneClear["LCE"].Cast<CheckBox>().CurrentValue;
            var EREADY = Program.E.IsReady();
            var QCHECK = Program.LaneClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            if (QCHECK && QREADY)
            {
                var enemy = (Obj_AI_Minion)GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.Q.Cast(enemy);
            }

            if (ECHECK && EREADY)
            {
                var enemy = (Obj_AI_Minion)GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.E.Cast();
            }
            if (Orbwalker.CanAutoAttack)
            {
                var enemy = (Obj_AI_Minion)GetEnemy(Vladimir.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Orbwalker.ForcedTarget = enemy;
            }
        }
    }
}