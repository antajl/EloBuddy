using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Evelynn
{
    internal static class Combo
    {
        public enum AttackSpell
        {
            Q,
            W,
            E
        };

        public static void EveCombo()
        {
            var qcheck = Program.ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var echeck = Program.ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue;
            var wcheck = Program.ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue;
            var qready = Program.Q.IsReady();
            var wready = Program.W.IsReady();
            var eready = Program.E.IsReady();

            if (qcheck && qready)
            {
                var enemy = TargetSelector.GetTarget(Program.Q.Range, DamageType.Magical);

                if (enemy != null)
                    Program.Q.Cast();
            }

            if (echeck && eready)
            {
                var enemy = TargetSelector.GetTarget(Program.E.Range, DamageType.Physical);

                if (enemy != null)
                    Program.E.Cast(enemy);
            }
            if (wcheck && wready)
            {
                Program.W.Cast();
            }
        }
    }
}