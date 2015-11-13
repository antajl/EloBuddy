using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;

namespace Kennen
{
    internal static class Combo
    {
        public enum AttackSpell
        {
            Q,
            W,
        };
        public static AIHeroClient Kennen
        {
            get { return ObjectManager.Player; }
        }
        public static void KennenCombo()
        {
            var qcheck = Program.ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var wcheck = Program.ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue;
            var qready = Program.Q.IsReady();
            var wready = Program.W.IsReady();

            if (qcheck && qready)   
                try
                {
                    var qTarget = TargetSelector.GetTarget(Program.Q.Range, DamageType.Magical);
                    if (qTarget.IsValidTarget(Program.Q.Range))
                    {
                        if (Program.Q.GetPrediction(qTarget).HitChance >= HitChance.High)
                        {
                                    Program.Q.Cast(qTarget);
                                }
                            }}
                    catch
                    {                      
                    }

            if (!wcheck || !wready) return;
            var wenemy = TargetSelector.GetTarget(Program.W.Range, DamageType.Magical);
            if (wenemy == null) return;
            if (wenemy.HasBuff("kennenmarkofstorm"))
            {
                Program.W.Cast();
            }
            if (!Orbwalker.CanAutoAttack) return;
            var enemy = TargetSelector.GetTarget(Player.Instance.GetAutoAttackRange(), DamageType.Physical);

            if (enemy != null)
                Orbwalker.ForcedTarget = enemy;
        }
    }
}