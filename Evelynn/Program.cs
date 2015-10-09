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
        public static Spell.Targeted Smite;
        public static Spell.Targeted Ignite;
        public static Menu EveMenu, ComboMenu, DrawMenu, SkinMenu, MiscMenu, LaneJungleClear, LastHit;
        public static AIHeroClient Eve = ObjectManager.Player;

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

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
            Q = new Spell.Active(SpellSlot.Q, 500);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 225);
            R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Circular, (int) 250f, (int) 1200f, (int) 150f);
            var summoner1 = _Player.Spellbook.GetSpell(SpellSlot.Summoner1);
            var summoner2 = _Player.Spellbook.GetSpell(SpellSlot.Summoner2);
            if (summoner1.Name == "summonerdot")
                Smite = new Spell.Targeted(SpellSlot.Summoner1, 500);
            else if (summoner2.Name == "summonerdot")
                Smite = new Spell.Targeted(SpellSlot.Summoner2, 500);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);


            EveMenu = MainMenu.AddMenu("BloodimirEve", "bloodimireve");
            EveMenu.AddGroupLabel("Bloodimir.Evelynn");
            EveMenu.AddSeparator();
            EveMenu.AddLabel("Bloodimir Evelynn V1.0.0.0");

            ComboMenu = EveMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 1, 0, 5));

            DrawMenu = EveMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("drawr", new CheckBox("Draw R"));

            LaneJungleClear = EveMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCE", new CheckBox("Use E"));
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));

            LastHit = EveMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q"));

            MiscMenu = EveMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("KS");
            MiscMenu.AddSeparator();
            MiscMenu.Add("kse", new CheckBox("KS using E"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS using Q"));

            SkinMenu = EveMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("skinid", new Slider("Skin", 0, 0, 4));
            var skinid = new[]
            {"Default", "Shadow", "Masquerade", "Tango", "Safecracker"};
            skinchange.DisplayName = skinid[skinchange.CurrentValue];
            skinchange.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = skinid[changeArgs.NewValue];
                if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                {
                    Chat.Print("skin-changed");
                }
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
            }
        }

        public static void Flee()
        {
            Orbwalker.MoveTo(Game.CursorPos);
            W.Cast();
        }

        private static void Tick(EventArgs args)
        {
            Killsteal();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
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
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Combo.EveCombo();
                    Rincombo(ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
                }
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                        Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                    {
                        LaneJungleClearA.LaneClear();
                    }
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                    {
                        LastHitA.LastHitB();
                    }
                }
                SkinChange();
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
                            Q.Cast(qenemy);
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
            var style = SkinMenu["skinid"].DisplayName;
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