using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Tryndamere
{
    internal static class Program
    {
        private static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        private static Spell.Active R;
        private static Spell.Targeted Ignite;
        private static Menu TrynMenu;
        public static Menu ComboMenu;
        private static Menu DrawMenu;
        private static Menu SkinMenu;
        public static Menu MiscMenu, LaneJungleClear;
        public static Item Tiamat, Hydra, Bilgewater, Youmuu, Botrk;
        public static AIHeroClient Tryndamere = ObjectManager.Player;

        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

        private static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }

        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Tryndamere")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W, 400);
            E = new Spell.Skillshot(SpellSlot.E, 660, SkillShotType.Linear, 250, 700, (int) 92.5);
            R = new Spell.Active(SpellSlot.R);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

            Botrk = new Item(3153, 550f);
            Bilgewater = new Item(3144, 475f);
            Hydra = new Item(3074, 250f);
            Tiamat = new Item(3077, 250f);
            Youmuu = new Item(3142, 10);

            TrynMenu = MainMenu.AddMenu("BloodimirTryn", "bloodimirtry");
            TrynMenu.AddGroupLabel("Bloodimir Tryndamere");
            TrynMenu.AddSeparator();
            TrynMenu.AddLabel("Bloodimir Tryndamere V1.0.0.0");

            ComboMenu = TrynMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum HP% for Ult", 20, 0, 95));
            ComboMenu.AddSeparator();
            ComboMenu.Add("qhp", new Slider("Q % HP", 25, 0, 95));


            DrawMenu = TrynMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawe", new CheckBox("Draw E"));

            LaneJungleClear = TrynMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCE", new CheckBox("Use E"));

            MiscMenu = TrynMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddSeparator();
            MiscMenu.Add("kse", new CheckBox("KS using E"));
            MiscMenu.Add("ksbotrk", new CheckBox("KS using Botrk"));
            MiscMenu.Add("kshydra", new CheckBox("KS using Hydra"));
            MiscMenu.Add("usehydra", new CheckBox("Use Hydra"));
            MiscMenu.Add("usetiamat", new CheckBox("Use Tiamat"));
            MiscMenu.Add("usebotrk", new CheckBox("Use Botrk"));
            MiscMenu.Add("usebilge", new CheckBox("Use Bilgewater"));
            MiscMenu.Add("useyoumuu", new CheckBox("Use Youmuu"));


            SkinMenu = TrynMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("skinid", new Slider("Skin", 4, 0, 7));
            var skinid = new[] { "Default", "Highland", "King", "Viking", "Demon Blade", "Sultan", "Warring Kingdoms", "Nightmare" };
            skinchange.DisplayName = skinid[skinchange.CurrentValue];
            skinchange.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = skinid[changeArgs.NewValue];
                if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                {
                    Chat.Print("skin-changed");
                }
            };
            Game.OnUpdate += Tick;
            Drawing.OnDraw += OnDraw;
        }

        private static void AutoQ(bool useR)
        {
            var autoQ = ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var healthAutoR = ComboMenu["qhp"].Cast<Slider>().CurrentValue;
            if (autoQ && _Player.HealthPercent < healthAutoR)
            {
                Q.Cast();
            }
        }

        private static void AutoUlt(bool useR)
        {
            var autoR = ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue;
            var healthAutoR = ComboMenu["rslider"].Cast<Slider>().CurrentValue;
            if (!autoR || !(_Player.HealthPercent < healthAutoR)) return;
            if (
                ObjectManager
                    .Get<AIHeroClient>().Any(x => x.IsEnemy && x.Distance(Tryndamere.Position) <= 1100))
            {
                R.Cast();
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Tryndamere.IsDead) return;
            if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && E.IsLearned)
            {
                Drawing.DrawCircle(Tryndamere.Position, E.Range, Color.DarkBlue);
            }
        }

        private static void Flee()
        {
            Orbwalker.MoveTo(Game.CursorPos);
            E.Cast(Game.CursorPos);
        }

        private static void Tick(EventArgs args)
        {
            Killsteal();
            SkinChange();
            AutoQ(ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue);
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo.TrynCombo();
                Combo.Items();
            }
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    LaneJungleClearA.LaneClearA();
                }
                AutoUlt(ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
            }
            if (!ComboMenu["useignite"].Cast<CheckBox>().CurrentValue ||
              !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
            foreach (
                var source in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(
                            a =>
                                a.IsEnemy && a.IsValidTarget(Ignite.Range) &&
                                a.Health < 50 + 20 * Tryndamere.Level - (a.HPRegenRate / 5 * 3)))
            {
                Ignite.Cast(source);
                return;
            }
        }

        private static void Killsteal()
        {
            if (!MiscMenu["kse"].Cast<CheckBox>().CurrentValue || !E.IsReady()) return;
            try
            {
                foreach (var etarget in EntityManager.Heroes.Enemies.Where(
                    hero => hero.IsValidTarget(E.Range) && !hero.IsDead && !hero.IsZombie).Where(etarget => Tryndamere.GetSpellDamage(etarget, SpellSlot.E) >= etarget.Health))
                {
                    {
                        E.Cast(etarget.ServerPosition);
                    }
                    if ((!MiscMenu["ksbotrk"].Cast<CheckBox>().CurrentValue || !Botrk.IsReady()) &&
                        !Bilgewater.IsReady() && !Tiamat.IsReady()) continue;
                    {
                        try
                        {
                            foreach (var itarget in EntityManager.Heroes.Enemies.Where(
                                hero =>
                                    hero.IsValidTarget(Botrk.Range) && !hero.IsDead &&
                                    !hero.IsZombie).Where(itarget => Tryndamere.GetItemDamage(itarget, ItemId.Blade_of_the_Ruined_King) >=
                                                                     itarget.Health))
                            {
                                {
                                    Botrk.Cast(itarget);
                                }
                                if ((!MiscMenu["kshydra"].Cast<CheckBox>().CurrentValue ||
                                     !Botrk.IsReady()) && !Bilgewater.IsReady() && !Tiamat.IsReady())
                                    continue;
                                {
                                    try
                                    {
                                        foreach (var htarget in EntityManager.Heroes.Enemies.Where(
                                            hero =>
                                                hero.IsValidTarget(Hydra.Range) &&
                                                !hero.IsDead && !hero.IsZombie).Where(htarget => Tryndamere.GetItemDamage(htarget,
                                                    ItemId.Ravenous_Hydra_Melee_Only) >=
                                                                                                 htarget.Health))
                                        {
                                            Hydra.Cast();
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
            }
            catch
            {
            }
        }

        private static
            void SkinChange
            ()
        {
            var style = SkinMenu["skinid"].DisplayName;
            switch (style)
            {
                case "Default":
                    Player.SetSkinId(0);
                    break;
                case "Highland":
                    Player.SetSkinId(1);
                    break;
                case "King":
                    Player.SetSkinId(2);
                    break;
                case "Viking":
                    Player.SetSkinId(3);
                    break;
                case "Demon Blade":
                    Player.SetSkinId(4);
                    break;
                case "Sultan":
                    Player.SetSkinId(5);
                    break;
                case "Warring Kingdoms":
                    Player.SetSkinId(5);
                    break;
                case "Nightmare":
                    Player.SetSkinId(5);
                    break;
            }
        }
    }
}
