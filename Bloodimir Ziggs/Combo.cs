using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Ziggs
{
    internal class Combo
    {
        public enum AttackSpell
        {
            Q,
            E,
            W
        };

        public static AIHeroClient Ziggs
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
                                                                                                   a.Distance(Ziggs) <=
                                                                                                   Spells.E.Range
                                                                                                   && !a.IsDead
                                                                                                   && !a.IsInvulnerable
                                                                                                   &&
                                                                                                   a.IsValidTarget(
                                                                                                       Spells.E.Range)
                                                                                                   &&
                                                                                                   a.Health <=
                                                                                                   Misc.Ecalc(a));
            }

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

        public static void ZiggsCombo()
        {
            var QCHECK = Program.ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var ECHECK = Program.ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue;
            var WCHECK = Program.ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue;
            var QREADY = Spells.Q.IsReady();
            var EREADY = Spells.E.IsReady();
            var WREADY = Spells.W.IsReady();

            if (QCHECK && QREADY)
            {
                var enemy = (AIHeroClient) GetEnemy(Spells.Q.Range, GameObjectType.AIHeroClient);

                var pred = Spells.Q.GetPrediction(enemy);
                if (pred.HitChance >= HitChance.High)
                    if (enemy != null)
                { 
                    Spells.Q.Cast(pred.CastPosition);
            }
                }
            if (ECHECK && EREADY)
            {
                var enemy = (AIHeroClient) GetBestEWLocation(GameObjectType.AIHeroClient);

                if (enemy != null)
                    Spells.E.Cast(enemy.ServerPosition);
            }
            if (WCHECK && WREADY)
            {
                var enemy = (AIHeroClient) GetBestEWLocation(GameObjectType.AIHeroClient);

                if (enemy != null)
                    Spells.W.Cast(enemy.ServerPosition);
            }
            if (Orbwalker.CanAutoAttack)
            {
                var enemy = (AIHeroClient) GetEnemy(Ziggs.GetAutoAttackRange(), GameObjectType.AIHeroClient);

                if (enemy != null)
                    Orbwalker.ForcedTarget = enemy;
            }
        }

        public static Obj_AI_Base GetBestEWLocation(GameObjectType type)
        {
            var numEnemiesInRange = 0;
            Obj_AI_Base enem = null;

            foreach (var enemy in ObjectManager.Get<Obj_AI_Base>()
                .OrderBy(a => a.Health)
                .Where(a => a.Distance(Ziggs) <= Spells.E.Range
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
                                a.Distance(Ziggs) <= Spells.E.Range && a.IsEnemy && !a.IsDead && a.Type == type &&
                                !a.IsInvulnerable)
                        .Count(enemy2 => enemy != enemy2 && enemy2.Distance(enemy) <= 80);
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