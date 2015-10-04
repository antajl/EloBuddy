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

            if (ECHECK && EREADY)
            {
                var enemy = TargetSelector.GetTarget(Program.E.Range, DamageType.Physical);
                if (enemy != null)
                    if (Program.E.IsReady() &&
                        Tryndamere.Distance(enemy) <= Program.E.Range - Player.Instance.GetAutoAttackRange())
                    {
                        Program.E.Cast(enemy.ServerPosition);
                    }
            }

            if (WCHECK && WREADY)
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
                        if (Program.botrk.IsOwned() && Program.botrk.IsReady() &&
                            Program.botrk.IsInRange(ienemy))
                            Program.botrk.Cast(ienemy);
                    }
                    if (Program.MiscMenu["usebilge"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.bilgewater.IsOwned() && Program.bilgewater.IsReady())
                            Program.bilgewater.Cast(ienemy);
                    }
                    if (Program.MiscMenu["usehydra"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.hydra.IsOwned() && Program.hydra.IsReady() &&
                            Program.hydra.IsInRange(ienemy))
                            Program.hydra.Cast();
                    }
                    if (Program.MiscMenu["useTiamat"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.tiamat.IsOwned() && Program.tiamat.IsReady() &&
                            Program.tiamat.IsInRange(ienemy))
                            Program.tiamat.Cast();
                    }
                    if (Program.MiscMenu["useyoumuu"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.youmuu.IsOwned() && Program.youmuu.IsReady())
                            Program.youmuu.Cast();
                    }
                }
            }
        }
    }
}