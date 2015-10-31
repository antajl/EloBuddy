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
            E
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

        public static void VladCombo()
        {
            var QCHECK = Program.ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var ECHECK = Program.ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            var EREADY = Program.E.IsReady();

            if (!ECHECK || !EREADY)
            {
                return;
            }
            {
                var eenemy = TargetSelector.GetTarget(Program.E.Range, DamageType.Magical);

                if (eenemy != null)
                    Program.E.Cast();
                 }
    
        if (!QCHECK || !QREADY)
            {
                return;
            }
                var qenemy = (AIHeroClient) GetEnemy(Program.Q.Range, GameObjectType.AIHeroClient);

                if (qenemy != null)
                    Program.Q.Cast(qenemy);
            }
    }
}