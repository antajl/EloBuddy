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
            R,
            Ignite
        };

        public static AIHeroClient Vladimir
        {
            get { return ObjectManager.Player; }
        }

        public static Obj_AI_Base GetEnemy(float range, GameObjectType type)
        {
            return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                               && a.Type == type
                                                                                               &&
                                                                                               a.Distance(Vladimir) <=
                                                                                               range
                                                                                               && !a.IsDead
                                                                                               && !a.IsInvulnerable
                                                                                               && a.IsValidTarget(range));
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

            if (spell == AttackSpell.R)
            {
                return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                                   && a.Type == type
                                                                                                   &&
                                                                                                   a.Distance(Vladimir) <=
                                                                                                   Program.R.Range
                                                                                                   && !a.IsDead
                                                                                                   && !a.IsInvulnerable
                                                                                                   &&
                                                                                                   a.IsValidTarget(
                                                                                                       Program.R.Range)
                                                                                                   &&
                                                                                                   a.Health <=
                                                                                                   Misc.Rdmg(a));
            }
           if (spell == AttackSpell.Ignite)
            {
                return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).Where(a => a.IsEnemy
                && a.Type == type
                && a.Distance(Vladimir) <= Program.Ignite.Range
                && !a.IsDead
                && !a.IsInvulnerable
                && a.IsValidTarget(Program.Ignite.Range)
                && a.Health <= Misc.Ignitedmg(a)).FirstOrDefault();
            }
            return null;
        }

        public static void VladCombo()
        {
            var QCHECK = Program.ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var ECHECK = Program.ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue;
            var RCHECK = Program.ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            var EREADY = Program.E.IsReady();
            var RREADY = Program.R.IsReady();
            var IgniteCHECK = Program.ComboMenu["useignite"].Cast<CheckBox>().CurrentValue;

            if (RCHECK && RREADY)
            {
                var enemy = (AIHeroClient) GetEnemy(Program.R.Range, GameObjectType.AIHeroClient);

                if (enemy != null)
                    Program.R.Cast(enemy.Position);
            }

            if (QCHECK && QREADY)
            {
                var enemy = (AIHeroClient) GetEnemy(Program.Q.Range, GameObjectType.AIHeroClient);

                if (enemy != null)
                    Program.Q.Cast(enemy);
            }

            if (ECHECK && EREADY)
            {
                var enemy = (AIHeroClient) GetBestELocation(GameObjectType.AIHeroClient);

                if (enemy != null)
                    Program.E.Cast();
            }
            if (IgniteCHECK && Program.Ignite != null && Program.Ignite.IsReady())
            {
                AIHeroClient enemy = (AIHeroClient)GetEnemy(Program.Ignite.Range, GameObjectType.AIHeroClient);
                if (enemy != null)
                    Program.Ignite.Cast(enemy);
            }

            if (Orbwalker.CanAutoAttack)
            {
                var enemy = (AIHeroClient) GetEnemy(Vladimir.GetAutoAttackRange(), GameObjectType.AIHeroClient);

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