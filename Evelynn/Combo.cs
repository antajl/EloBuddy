using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Evelynn
{
    internal class Combo
    {
        public enum AttackSpell
        {
            Q,
            W,
            E
        };

        public static void EveCombo()
        {
            var QCHECK = Program.ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var ECHECK = Program.ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue;
            var WCHECK = Program.ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            var WREADY = Program.W.IsReady();
            var EREADY = Program.E.IsReady();

            if (QCHECK && QREADY)
            {
                var enemy = TargetSelector.GetTarget(Program.Q.Range, DamageType.Magical);

                if (enemy != null)
                    Program.Q.Cast();
            }

            if (ECHECK && EREADY)
            {
                var enemy = TargetSelector.GetTarget(Program.E.Range, DamageType.Physical);

                if (enemy != null)
                    Program.E.Cast(enemy);
            }
            if (WCHECK && WREADY)
            {
                Program.W.Cast();
            }
            if (Orbwalker.CanAutoAttack)
            {
                var enemy = TargetSelector.GetTarget(Player.Instance.GetAutoAttackRange(), DamageType.Physical);

                if (enemy != null)
                    Orbwalker.ForcedTarget = enemy;
            }
        }
    }
}