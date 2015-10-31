using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Tryndamere
{
    internal class Combo
    {
        public enum AttackSpell
        {
            E,
            W
        };

        public static AIHeroClient Tryndamere
        {
            get { return ObjectManager.Player; }
        }

        public static void TrynCombo()
        {
            var WCHECK = Program.ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue;
            var ECHECK = Program.ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue;
            var WREADY = Program.W.IsReady();
            var EREADY = Program.E.IsReady();

            if (!ECHECK || !EREADY)
            {
                var enemy = TargetSelector.GetTarget(Program.E.Range, DamageType.Physical);
                if (enemy != null)
                    if (Tryndamere.Distance(enemy) <= Program.E.Range - Player.Instance.GetAutoAttackRange())
                    {
                        Program.E.Cast(enemy.ServerPosition);
                    }
            }

            if (!WCHECK || !WREADY)
            {
                return;
            }
            {
                var wenemy = TargetSelector.GetTarget(Program.W.Range, DamageType.Magical);
                {
                    if (!wenemy.IsFacing(Program.Tryndamere))
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

        public static
            void Items()
        {
            var ienemy = TargetSelector.GetTarget(Player.Instance.GetAutoAttackRange() + 425,
                DamageType.Physical);
            if (ienemy != null)
            {
                if (ienemy.IsValid && !ienemy.IsZombie)
                {
                    if (Program.MiscMenu["usebotrk"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Botrk.IsOwned() && Program.Botrk.IsReady() &&
                            Program.Botrk.IsInRange(ienemy))
                            Program.Botrk.Cast(ienemy);
                    }
                    if (Program.MiscMenu["usebilge"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Bilgewater.IsOwned() && Program.Bilgewater.IsReady())
                            Program.Bilgewater.Cast(ienemy);
                    }
                    if (Program.MiscMenu["usehydra"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Hydra.IsOwned() && Program.Hydra.IsReady() &&
                            Program.Hydra.IsInRange(ienemy))
                            Program.Hydra.Cast();
                    }
                    if (Program.MiscMenu["usetiamat"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Tiamat.IsOwned() && Program.Tiamat.IsReady() &&
                            Program.Tiamat.IsInRange(ienemy))
                            Program.Tiamat.Cast();
                    }
                    if (Program.MiscMenu["useyoumuu"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Youmuu.IsOwned() && Program.Youmuu.IsReady())
                            Program.Youmuu.Cast();
                    }
                }
            }
        }
    }
}