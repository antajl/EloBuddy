using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace BloodimirVladimir
{
    internal class Combo
    {
        public enum AttackSpell
        {
            Q,
            E,
        };

        public static AIHeroClient Vladimir
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
            var target = TargetSelector.GetTarget(1500, DamageType.True);
            if (target == null || !target.IsValid())
                if (Program.Ignite.IsInRange(target) &&
                    target.Health < 50 + 20*Program._Player.Level - (target.HPRegenRate/5*3) &&
                    Program.ComboMenu["useignite"].Cast<CheckBox>().CurrentValue)
                {
                    Program.Ignite.Cast(target);
                }
            {
                Chat.Print("igniteused");
            }
            return null;
        }

        public static void VladCombo()
        {
            var QCHECK = Program.ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var ECHECK = Program.ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            var EREADY = Program.E.IsReady();

            if (QCHECK && QREADY)
            {
                var enemy = (AIHeroClient) GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Hero);

                if (enemy != null)
                    Program.Q.Cast(enemy);
            }

            if (ECHECK && EREADY)
            {
                var enemy = (AIHeroClient) GetBestELocation(GameObjectType.obj_AI_Hero);

                if (enemy != null)
                    Program.E.Cast();
            }
            if (Orbwalker.CanAutoAttack)
            {
                var enemy = (AIHeroClient) GetEnemy(Vladimir.GetAutoAttackRange(), GameObjectType.obj_AI_Hero);

                if (enemy != null)
                    Orbwalker.ForcedTarget = enemy;
            }
        }

        public static Obj_AI_Base GetBestELocation(GameObjectType type)
        {
            var numEnemiesInRange = 0;
            Obj_AI_Base enem = null;

            foreach (var enemy in ObjectManager.Get<Obj_AI_Base>()
                .OrderBy(a => a.Health)
                .Where(a => a.Distance(Vladimir) <= Program.E.Range
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
                                a.Distance(Vladimir) <= Program.E.Range && a.IsEnemy && !a.IsDead && a.Type == type &&
                                !a.IsInvulnerable)
                        .Count(enemy2 => enemy != enemy2 && enemy2.Distance(enemy) <= 75);
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