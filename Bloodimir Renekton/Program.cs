using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Renekton
{
    internal class Program
    {
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Active R;
        public static Item Hydra;
        public static Item Tiamat;
        public static Menu RenekMenu, ComboMenu, SkinMenu, MiscMenu, DrawMenu, LaneJungleClear, LastHit;
        public static Item Bilgewater, Youmuu, Botrk;
        public static AIHeroClient Renek = ObjectManager.Player;

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Renekton")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q, 225);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 450, SkillShotType.Linear);
            R = new Spell.Active(SpellSlot.R);
            Tiamat = new Item((int) ItemId.Tiamat_Melee_Only, 420);
            Hydra = new Item((int) ItemId.Ravenous_Hydra_Melee_Only, 420);
            Botrk = new Item(3153, 550f);
            Bilgewater = new Item(3144, 475f);
            Youmuu = new Item(3142, 10);

            RenekMenu = MainMenu.AddMenu("BloodimiRenekton", "bloodimirrenekton");
            RenekMenu.AddGroupLabel("Bloodimir.enekton");
            RenekMenu.AddSeparator();
            RenekMenu.AddLabel("BloodimiRenekton v1.0.0.");

            ComboMenu = RenekMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("useitems", new CheckBox("Use Items"));
            ComboMenu.Add("autoult", new CheckBox("Auto Ult"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Health Percentage to Ult", 31, 0, 100));

            LaneJungleClear = RenekMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCE", new CheckBox("Use E"));
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));
            LaneJungleClear.Add("LCW", new CheckBox("Use W"));
            LaneJungleClear.Add("LCI", new CheckBox("Use Items"));

            DrawMenu = RenekMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("drawe", new CheckBox("Draw E"));

            LastHit = RenekMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q"));
            LastHit.Add("LHW", new CheckBox("Use W"));
            LastHit.Add("LHI", new CheckBox("Use Items"));

            MiscMenu = RenekMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("KS");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS with Q"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("intw", new CheckBox("W to Interrupt"));

            SkinMenu = RenekMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sid", new Slider("Skin", 5, 0, 7));
            var sid = new[]
            {"Classic", "Galactic", "Outback", "Bloodfury", "Rune Wars", "Scorched Earth", "Pool Party", "Prehistoric"};
            skinchange.DisplayName = sid[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sid[changeArgs.NewValue];
                };

            Game.OnTick += Tick;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
        }

        public static void Flee()
        {
            Orbwalker.MoveTo(Game.CursorPos);
            E.Cast(Game.CursorPos);
            E.Cast(Game.CursorPos);
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Renek.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Renek.Position, 225, Color.DarkGoldenrod);
                }
                if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Renek.Position, E.Range, Color.DarkCyan);
                }
            }
        }

        private static void Tick(EventArgs args)
        {
            Killsteal();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo.RenekCombo();
                Combo.Items();
                Combo.UseE();
                AutoUlt(ComboMenu["autoult"].Cast<CheckBox>().CurrentValue);
            }
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    LaneJungleClearA.LaneClear();
                    LaneJungleClearA.Items();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    LastHitA.LastHitB();
                    LastHitA.Items();
                }
                SkinChange();
            }
        }

        public static void AutoUlt(bool UseR)
        {
            var autoR = ComboMenu["autoult"].Cast<CheckBox>().CurrentValue;
            var healthAutoR = ComboMenu["rslider"].Cast<Slider>().CurrentValue;
            if (autoR && _Player.HealthPercent < healthAutoR)
            {
                R.Cast();
            }
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) ||
                (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                 (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))))
            {
                var e = target as AIHeroClient;
                if (ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue && W.IsReady() && e.IsEnemy)
                    if (target != null)
                    {
                        if (e.IsValidTarget() && W.IsReady())
                        {
                            W.Cast();
                        }
                    }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(Player.Instance.GetAutoAttackRange(), DamageType.Physical);
            {
                if (W.IsReady() && sender.IsValidTarget(W.Range) && MiscMenu["intw"].Cast<CheckBox>().CurrentValue)
                    W.Cast(intTarget);
            }
        }

        private static void Killsteal()
        {
            if (MiscMenu["ksq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                try
                {
                    foreach (
                        var qtarget in
                            EntityManager.Heroes.Enemies.Where(
                                hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
                    {
                        if (Renek.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)

                            Q.Cast();
                    }
                }
                catch
                {
                }
            }
        }

        private static void SkinChange()
        {
            var style = SkinMenu["sid"].DisplayName;
            switch (style)
            {
                case "Classic":
                    Player.SetSkinId(0);
                    break;
                case "Galactic":
                    Player.SetSkinId(1);
                    break;
                case "Outback":
                    Player.SetSkinId(2);
                    break;
                case "Bloodfury":
                    Player.SetSkinId(3);
                    break;
                case "Rune Wars":
                    Player.SetSkinId(4);
                    break;
                case "Scorched Earth":
                    Player.SetSkinId(5);
                    break;
                case "Pool Party":
                    Player.SetSkinId(6);
                    break;
                case "Prehistoric":
                    Player.SetSkinId(7);
                    break;
            }
        }
    }
}