using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using Kennen;

namespace Kennen
{
    internal class Combo
    {
        public enum AttackSpell
        {
            Q,
            W,
        };

        public static void KennenCombo()
        {
            var QCHECK = Program.ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var WCHECK = Program.ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            var WREADY = Program.W.IsReady();

            if (QCHECK && QREADY)
            {
                var enemy = TargetSelector.GetTarget(Program.Q.Range, DamageType.Magical);

                if (enemy != null)
                    Program.Q.Cast(enemy.ServerPosition);
            }

            if (WCHECK && WREADY)
            { 
                var wenemy = TargetSelector.GetTarget(Program.W.Range, DamageType.Magical);
                if (wenemy != null)
                {
                    if (wenemy.HasBuff("kennenmarkofstorm"))
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
    }
}