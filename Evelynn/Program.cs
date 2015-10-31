using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Evelynn
{
    internal class Program
    {
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;
        public static Menu EveMenu, ComboMenu, DrawMenu, SkinMenu, MiscMenu, LaneJungleClear, LastHitMenu;
        public static AIHeroClient Eve = ObjectManager.Player;
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

        public static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }

        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Evelynn")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q, 475);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 225);
            R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Circular, 250, 1200, 150);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);


            EveMenu = MainMenu.AddMenu("BloodimirEve", "bloodimireve");
            EveMenu.AddGroupLabel("Bloodimir.Evelynn");
            EveMenu.AddSeparator();
            EveMenu.AddLabel("Bloodimir Evelynn V1.0.1.0");

            ComboMenu = EveMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 1, 0, 5));

            DrawMenu = EveMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("drawr", new CheckBox("Draw R"));
            DrawMenu.Add("drawe", new CheckBox("Draw R"));

            LaneJungleClear = EveMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCE", new CheckBox("Use E"));
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));

            LastHitMenu = EveMenu.AddSubMenu("Last Hit", "lasthit");
            LastHitMenu.AddGroupLabel("Last Hit Settings");
            LastHitMenu.Add("LHQ", new CheckBox("Use Q"));

            MiscMenu = EveMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("KS");
            MiscMenu.AddSeparator();
            MiscMenu.Add("kse", new CheckBox("KS using E"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS using Q"));
            MiscMenu.Add("asw", new CheckBox("Auto/Smart W"));


            SkinMenu = EveMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 2, 0, 4));
            var sid = new[] {"Default", "Shadow", "Masquerade", "Tango", "Safecracker"};
            skinchange.DisplayName = sid[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sid[changeArgs.NewValue];
                };

            Game.OnTick += Tick;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Eve.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Eve.Position, Q.Range, Color.DarkBlue);
                }
                if (DrawMenu["drawr"].Cast<CheckBox>().CurrentValue && R.IsLearned)
                {
                    Drawing.DrawCircle(Eve.Position, R.Range, Color.Red);
                }
                if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && E.IsLearned)
                {
                    Drawing.DrawCircle(Eve.Position, E.Range, Color.Green);
                }
            }
        }

        public static void Flee()
        {
            Orbwalker.MoveTo(Game.CursorPos);
            W.Cast();
        }

        public static void AutoW()
        {
            var useW = MiscMenu["asw"].Cast<CheckBox>().CurrentValue;

            if (Player.HasBuffOfType(BuffType.Slow) || Eve.CountEnemiesInRange(550) >= 3 && useW)
            {
                W.Cast();
            }
        }
        private static void Tick(EventArgs args)
        {
            Killsteal();
            SkinChange();
            AutoW();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo.EveCombo();
                Rincombo(ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
            }
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    LaneJungleClearA.LaneClearB();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    LastHitA.LastHitB();
                }
                {
                    if (!ComboMenu["useignite"].Cast<CheckBox>().CurrentValue ||
                        !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
                    foreach (
                        var source in
                            ObjectManager.Get<AIHeroClient>()
                                .Where(
                                    a =>
                                        a.IsEnemy && a.IsValidTarget(Ignite.Range) &&
                                        a.Health < 50 + 20*Eve.Level - (a.HPRegenRate/5*3)))
                    {
                        Ignite.Cast(source);
                        return;
                    }
                }
            }
        }

        public static void Rincombo(bool useR)
        {
            if (ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue)
                if (useR && R.IsReady() &&
                    Eve.CountEnemiesInRange(R.Range) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue)
                {
                    var rtarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);
                    R.Cast(rtarget.ServerPosition);
                }
        }

        private static void Killsteal()
        {
            var qenemy = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (MiscMenu["ksq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                try
                {
                    foreach (
                        var qtarget in
                            EntityManager.Heroes.Enemies.Where(
                                hero =>
                                    hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
                    {
                        if (Eve.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                        {
                            Q.Cast();
                        }
                        var eenemy = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                        if (MiscMenu["kse"].Cast<CheckBox>().CurrentValue && E.IsReady())
                        {
                            try
                            {
                                foreach (
                                    var etarget in
                                        EntityManager.Heroes.Enemies.Where(
                                            hero =>
                                                hero.IsValidTarget(E.Range) && !hero.IsDead && !hero.IsZombie))
                                {
                                    if (Eve.GetSpellDamage(qtarget, SpellSlot.E) >= etarget.Health)
                                    {
                                        E.Cast(eenemy);
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private static void SkinChange()
        {
            var style = SkinMenu["sID"].DisplayName;
            switch (style)
            {
                case "Default":
                    Player.SetSkinId(0);
                    break;
                case "Shadow":
                    Player.SetSkinId(1);
                    break;
                case "Masquerade":
                    Player.SetSkinId(2);
                    break;
                case "Tango":
                    Player.SetSkinId(3);
                    break;
                case "Safecracker":
                    Player.SetSkinId(4);
                    break;
            }
        }
    }
}