using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace BloodimirVladimir
{
    internal class Program
    {
        public static Spell.Active W, E;
        public static Spell.Skillshot R, Flash;
        public static Spell.Targeted Ignite, Q;
        public static Item Zhonia;
        public static Menu VladMenu, ComboMenu, DrawMenu, SkinMenu, MiscMenu, LaneClear, HarassMenu, LastHit;
        public static AIHeroClient Vlad = ObjectManager.Player;

        private static Vector3 mousePos
        {
            get { return Game.CursorPos; }
        }

        public static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Vladimir")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 600);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E, 610);
            R = new Spell.Skillshot(SpellSlot.R, 899, SkillShotType.Circular, (int) 250f, (int) 1200f, (int) 150f);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Zhonia = new Item((int)ItemId.Zhonyas_Hourglass);
            var FlashSlot = Vlad.GetSpellSlotFromName("summonerflash");
            Flash = new Spell.Skillshot(FlashSlot, 32767, SkillShotType.Linear);
            
            VladMenu = MainMenu.AddMenu("Bloodimir", "bloodimir");
            VladMenu.AddGroupLabel("Bloodimir.Bloodimir");
            VladMenu.AddSeparator();
            VladMenu.AddLabel("Bloodimir c what i did there? version 1.0.5.0");

            ComboMenu = VladMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for Combo R", 2, 0, 5));
            DrawMenu = VladMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q Range"));
            DrawMenu.Add("drawe", new CheckBox("Draw E Range"));
            DrawMenu.Add("drawr", new CheckBox("Draw R Range"));
            DrawMenu.Add("drawaa", new CheckBox("Draw AA Range"));

            LaneClear = VladMenu.AddSubMenu("Lane Clear", "laneclear");
            LaneClear.AddGroupLabel("Lane Clear Settings");
            LaneClear.Add("LCE", new CheckBox("Use E"));
            LaneClear.Add("LCQ", new CheckBox("Use Q"));

            LastHit = VladMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q"));

            HarassMenu = VladMenu.AddSubMenu("Harass Menu", "harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("hq", new CheckBox("Harass Q"));
            HarassMenu.Add("he", new CheckBox("Harass E"));

            MiscMenu = VladMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS with Q"));
            MiscMenu.Add("kse", new CheckBox("KS with E"));
            MiscMenu.Add("zhonias", new CheckBox("Use Zhonia"));
            MiscMenu.Add("zhealth", new Slider("Auto Zhonia Health %", 8));
            MiscMenu.AddSeparator();
            MiscMenu.Add("gapcloserw", new CheckBox("Anti Gapcloser W"));
            MiscMenu.Add("gapcloserhp", new Slider("Gapcloser W Health %", 25));
            MiscMenu.AddSeparator();

            SkinMenu = VladMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 5, 0, 7));
            var sID = new[]
            {"Default", "Count", "Marquius", "Nosferatu", "Vandal", "Blood Lord", "Soulstealer", "Academy"};
            skinchange.DisplayName = sID[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sID[changeArgs.NewValue];
                };

            Game.OnTick += Tick;
            Drawing.OnDraw += OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Vlad.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Vlad.Position, Q.Range, Color.Red);
                }
                {
                    if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && E.IsLearned)
                    {
                        Drawing.DrawCircle(Vlad.Position, E.Range, Color.DarkGreen);
                    }
                    if (DrawMenu["drawr"].Cast<CheckBox>().CurrentValue && R.IsLearned)
                    {
                        Drawing.DrawCircle(Vlad.Position, R.Range, Color.DarkMagenta);
                    }
                    }
                if (DrawMenu["drawaa"].Cast<CheckBox>().CurrentValue)
                {
                    Drawing.DrawCircle(Vlad.Position, 518, Color.DarkSlateGray);
                }
                }
            }

        private static
            void Gapcloser_OnGapCloser
            (AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!gapcloser.Sender.IsEnemy || (!MiscMenu["gapcloserw"].Cast<CheckBox>().CurrentValue))
                return;
            if (ObjectManager.Player.Distance(gapcloser.Sender, true) <
                W.Range && sender.IsValidTarget() && W.IsReady() &&
                Vlad.HealthPercent <= MiscMenu["gapcloserhp"].Cast<Slider>().CurrentValue)
            {
                W.Cast();
            }
        }

        public static void Flee()
        {
            Orbwalker.MoveTo(Game.CursorPos);
            W.Cast();
        }
         private static void Zhonya()
        {
            var zhoniaon = MiscMenu["zhonias"].Cast<CheckBox>().CurrentValue;
            var zhealth = MiscMenu["zhealth"].Cast<Slider>().CurrentValue;

            if (zhoniaon && Zhonia.IsReady() && Zhonia.IsOwned())
            {
                if (Vlad.HealthPercent <= zhealth)
                {
                    Zhonia.Cast();
                }}}
        private static void Tick(EventArgs args)
        {
            Killsteal();
            SkinChange();
            Zhonya();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo.VladCombo();
                Rincombo(ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
            } if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    LaneClearA.LaneClear();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                Harass();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    LastHitA.LastHitB();
                }
                else if (!ComboMenu["useignite"].Cast<CheckBox>().CurrentValue ||
                    !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
                foreach (
                    var source in
                        ObjectManager.Get<AIHeroClient>()
                            .Where(
                                a =>
                                    a.IsEnemy && a.IsValidTarget(Ignite.Range) &&
                                    a.Health < 50 + 20*Vlad.Level - (a.HPRegenRate/5*3)))
                {
                    Ignite.Cast(source);
                    return;
                }
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
        public static void Harass()
        {
            Orbwalker.OrbwalkTo(mousePos);
            if (HarassMenu["he"].Cast<CheckBox>().CurrentValue)
            {
                var enemy = TargetSelector.GetTarget(E.Range, DamageType.Magical);

                if (enemy != null)
                    E.Cast();
            }
            if (HarassMenu["hq"].Cast<CheckBox>().CurrentValue)
            {
                var enemy = (AIHeroClient)GetEnemy(Q.Range, GameObjectType.AIHeroClient);

                if (enemy != null)
                    Q.Cast(enemy);
            }
            }
        public static void Rincombo(bool useR)
        {
            foreach (
                        var qtarget in
                            EntityManager.Heroes.Enemies.Where(
                                hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
                                {
            if (Vlad.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health || (Vlad.GetSpellDamage(qtarget, SpellSlot.E) >= qtarget.Health))
                                    return;
            if (ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue)
                if (useR && R.IsReady() &&
                    Vlad.CountEnemiesInRange(R.Width) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue)
                {
                    var rtarget = TargetSelector.GetTarget(1250, DamageType.Magical);
                    R.Cast(rtarget.ServerPosition);
                }
        }
            }

        private static void Killsteal()
        {
            var enemy = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (MiscMenu["ksq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                    foreach (
                        var qtarget in
                            EntityManager.Heroes.Enemies.Where(
                                hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
                    {
                        if (Vlad.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health && qtarget.Distance(Vlad) <= Q.Range)
                        {
                            Q.Cast(enemy);
                        }
                        if (MiscMenu["kse"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                    foreach (
                        var etarget in
                            EntityManager.Heroes.Enemies.Where(
                                hero => hero.IsValidTarget(E.Range) && !hero.IsDead && !hero.IsZombie))
                    {
                        if (Vlad.GetSpellDamage(etarget, SpellSlot.E) >= etarget.Health && etarget.Distance(Vlad) <= E.Range)
                        {
                           E.Cast();
                        }
                    }
                }
                {
                }
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
                case "Count":
                    Player.SetSkinId(1);
                    break;
                case "Marquius":
                    Player.SetSkinId(2);
                    break;
                case "Nosferatu":
                    Player.SetSkinId(3);
                    break;
                case "Vandal":
                    Player.SetSkinId(4);
                    break;
                case "Blood Lord":
                    Player.SetSkinId(5);
                    break;
                case "Soulstealer":
                    Player.SetSkinId(6);
                    break;
                case "Academy":
                    Player.SetSkinId(7);
                    break;
            }
        }
    }
}