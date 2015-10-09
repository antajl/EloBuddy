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

        public static Obj_AI_Base GetEnemy(GameObjectType type, AttackSpell spell)
        {
            var eminion =
                EntityManager.MinionsAndMonsters.GetJungleMonsters(Program.Eve.Position, Program.E.Range)
                    .FirstOrDefault(
                        m =>
                            m.Distance(Program.Eve) <= Program.E.Range &&
                            m.Health <= Misc.Ecalc(m) &&
                            m.IsValidTarget());

            if (Program.E.IsReady() && Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue && eminion != null &&
                !Orbwalker.IsAutoAttacking)
            {
                Program.E.Cast(eminion);
            }

            if (spell == AttackSpell.Q)
            {
                return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                                   && a.Type == type
                                                                                                   &&
                                                                                                   a.Distance(Evelynn) <=
                                                                                                   Program.Q.Range
                                                                                                   && !a.IsDead
                                                                                                   && !a.IsInvulnerable
                                                                                                   &&
                                                                                                   a.IsValidTarget(
                                                                                                       Program.Q.Range)
                                                                                                   &&
                                                                                                   a.Health <=
                                                                                                   Misc.Qcalc(a));
            }

            return null;
        }

        public static void LaneClear()
        {
            var ECHECK = Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue;
            var EREADY = Program.E.IsReady();
            var QCHECK = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();

            if (ECHECK && EREADY)
            {
                var enemy = (Obj_AI_Minion) GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.E.Cast(enemy);
            }

            if (QCHECK && QREADY)
            {
                var enemy = GetBestQLocation(GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.Q.Cast();
            }
            if (Orbwalker.CanAutoAttack)
            {
                var enemy = (Obj_AI_Minion) GetEnemy(Evelynn.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Orbwalker.ForcedTarget = enemy;
            }
        }

        public static Obj_AI_Base GetBestQLocation(GameObjectType type)
        {
            var numEnemiesInRange = 0;
            Obj_AI_Base enem = null;

            foreach (var enemy in ObjectManager.Get<Obj_AI_Base>()
                .OrderBy(a => a.Health)
                .Where(a => a.Distance(Evelynn) <= Program.E.Range
                            && a.IsEnemy
                            && a.Type == type
                            && !a.IsDead
                            && !a.IsInvulnerable))
            {
                var tempNumEnemies =
                    ObjectManager.Get<Obj_AI_Base>()
                        .OrderBy(a => a.Health)
                        .Where(
                            a =>
                                a.Distance(Evelynn) <= Program.E.Range && a.IsEnemy && !a.IsDead && a.Type == type &&
                                !a.IsInvulnerable)
                        .Count(enemy2 => enemy != enemy2 && enemy2.Distance(enemy) <= 77);
                if (tempNumEnemies > numEnemiesInRange)
                {
                    enem = enemy;
                    numEnemiesInRange = tempNumEnemies;
                }
            }

            return enem;
        }
    }
}