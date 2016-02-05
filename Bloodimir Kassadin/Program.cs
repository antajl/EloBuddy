using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace Bloodimir_Kassadin
{
    internal class Program
    {
        public static Spell.Skillshot E, R;
        public static Spell.Targeted Q, Flash, Ignite;
        public static Spell.Active W;
        private static readonly AIHeroClient Kassawin = ObjectManager.Player;

        public static Menu KassaMenu,
            ComboMenu,
            LaneJungleClear,
            SkinMenu,
            LastHitMenu,
            MiscMenu,
            HarassMenu,
            DrawMenu,
            FleeMenu;

        private static Vector3 MousePos
        {
            get { return Game.CursorPos; }
        }

        private static AIHeroClient SelectedHero { get; set; }

        private static int ECount
        {
            get { return Kassawin.GetBuffCount("forcepulsecounter"); }
        }

        private static float RMana
        {
            get { return Kassawin.Spellbook.GetSpell(SpellSlot.R).SData.Mana; }
        }

        private static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Kassadin")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 650);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 400, SkillShotType.Cone, 500, int.MaxValue, 10);
            R = new Spell.Skillshot(SpellSlot.R, 700, SkillShotType.Circular, 500, int.MaxValue, 150);

            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

            KassaMenu = MainMenu.AddMenu("BloodimirKassadin", "bloodimirkassa");
            KassaMenu.AddGroupLabel("Bloodimir Kassadin v1.0.0.0");
            KassaMenu.AddSeparator();
            KassaMenu.AddLabel("Bloodimir Kassadin v1.0.0.0");

            ComboMenu = KassaMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Maximum enemy to R", 2, 0, 5));

            HarassMenu = KassaMenu.AddSubMenu("HarassMenu", "Harass");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("useEHarass", new CheckBox("Use E"));

            LaneJungleClear = KassaMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));
            LaneJungleClear.Add("LCW", new CheckBox("Use W"));
            LaneJungleClear.Add("LCE", new CheckBox("Use E"));

            LastHitMenu = KassaMenu.AddSubMenu("Last Hit", "lasthit");
            LastHitMenu.AddGroupLabel("Last Hit Settings");
            LastHitMenu.Add("LHQ", new CheckBox("Use Q"));
            LastHitMenu.Add("LHW", new CheckBox("Use W"));

            DrawMenu = KassaMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("drawe", new CheckBox("Draw E"));
            DrawMenu.Add("drawr", new CheckBox("Draw R"));

            MiscMenu = KassaMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("KS");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS using Q"));
            MiscMenu.Add("int", new CheckBox("TRY to Interrupt Channeled Spells"));
            MiscMenu.Add("gape", new CheckBox("Anti Gapcloser E"));

            FleeMenu = KassaMenu.AddSubMenu("Flee", "Flee");
            FleeMenu.Add("fleer", new CheckBox("Use R to Mouse Pos"));

            SkinMenu = KassaMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 5, 0, 5));
            var sid = new[] {"Default", "Festival", "Deep One", "Pre-Void", "Harbinger", "Cosmic Reaver"};
            skinchange.DisplayName = sid[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sid[changeArgs.NewValue];
                };
            Game.OnUpdate += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interruptererer;
            Orbwalker.OnPostAttack += Reset;
            Drawing.OnDraw += OnDraw;
        }

        private static void Game_OnTick(EventArgs args)
        {
            SkinChange();
            Killsteal();
            { 
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                Harass();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                LaneJungleClearA.LaneClear();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                LastHitA.LastHitB();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                Flee();
            {
                {
                    if (!ComboMenu["useignite"].Cast<CheckBox>().CurrentValue ||
                        !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
                    foreach (
                        var source in
                            ObjectManager.Get<AIHeroClient>()
                                .Where(
                                    a =>
                                        a.IsEnemy && a.IsValidTarget(Ignite.Range) &&
                                        a.Health < 50 + 20*Kassawin.Level - a.HPRegenRate/5*3))
                    {
                        Ignite.Cast(source);
                        return;
                    }
                }
            }
            }
        }

        private static
            void Gapcloser_OnGapCloser
            (AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (MiscMenu["gape"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                sender.Distance(Kassawin) < E.Range &&
                E.IsReady())
            {
                E.Cast(sender);
            }
        }

        private static
            void Interruptererer
            (Obj_AI_Base sender,
                Interrupter.InterruptableSpellEventArgs args)
        {
            if (args.DangerLevel == DangerLevel.High && MiscMenu["int"].Cast<CheckBox>().CurrentValue &&
                sender.IsEnemy &&
                sender is AIHeroClient &&
                sender.Distance(Kassawin) < Q.Range &&
                Q.IsReady() && Q.IsLearned)
            {
                Q.Cast(sender);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Kassawin.IsDead) return;
            if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
            {
                Circle.Draw(Color.Goldenrod, Q.Range, Kassawin.Position);
            }
            {
                if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && E.IsLearned)
                {
                    Circle.Draw(Color.MediumVioletRed, E.Range, Player.Instance.Position);
                }
                if (DrawMenu["drawr"].Cast<CheckBox>().CurrentValue)
                {
                    Circle.Draw(Color.DimGray, R.Range, Kassawin.Position);
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (SelectedHero != null)
            {
                target = SelectedHero;
            }

            if (target == null || !target.IsValid())
            {
                return;
            }
            if (HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue && Q.IsReady() && target.IsValidTarget(Q.Range))
                Q.Cast(target);
            if (HarassMenu["useEHarass"].Cast<CheckBox>().CurrentValue && E.IsReady() &&
                target.Distance(Kassawin) < E.Range)
                E.Cast(target.ServerPosition);
        }

        private static
            void Combo
            ()
        {
            var target = TargetSelector.GetTarget(750, DamageType.Magical);
            if (target == null || !target.IsValid())
            {
                return;
            }
            if (ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue && Q.IsReady() && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
            if (ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue)
                if (W.IsReady())
                {
                    var enemies =
                        EntityManager.Heroes.Enemies.Where(x => x.IsEnemy && x.Distance(Kassawin) < 250).Count();
                    if (enemies > 0)
                    {
                        W.Cast();
                    }
                    if (ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue && E.IsReady())
                    {
                        E.Cast(target.ServerPosition);
                    }
                }
            if (ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                if (target.CountEnemiesInRange(550) < ComboMenu["rslider"].Cast<Slider>().CurrentValue && RMana < 400)
                    R.Cast(target.ServerPosition);
                else if (ECount >= 3 ||
                         Calcs.DmgCalc(target) >= target.Health &&
                         target.CountEnemiesInRange(550) < ComboMenu["rslider"].Cast<Slider>().CurrentValue - 1)
                {
                    R.Cast(target.ServerPosition);
                }
            }
        }

        private static
            void Killsteal
            ()
        {
            if (!MiscMenu["ksq"].Cast<CheckBox>().CurrentValue || !Q.IsReady()) return;
            foreach (var qtarget in EntityManager.Heroes.Enemies.Where(
                hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie)
                .Where(qtarget => Kassawin.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health))
            {
                Q.Cast(qtarget);
            }
        }

        private static void Reset(AttackableUnit target, EventArgs args)
        {
            if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) &&
                (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) &&
                 (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)))) return;
            var e = target as AIHeroClient;
            if (!ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue || !W.IsReady() || !e.IsEnemy) return;
            if (target == null) return;
            if (e.IsValidTarget() && W.IsReady())
            {
                W.Cast();
            }
        }

        private static
            void Flee
            ()
        {
            if (!FleeMenu["fleer"].Cast<CheckBox>().CurrentValue) return;
            Orbwalker.MoveTo(Game.CursorPos);
            R.Cast(MousePos);
        }

        private static void SkinChange()
        {
            var style = SkinMenu["sID"].DisplayName;
            switch (style)
            {
                case "Default":
                    Player.SetSkinId(0);
                    break;
                case "Festival":
                    Player.SetSkinId(1);
                    break;
                case "Deep One":
                    Player.SetSkinId(2);
                    break;
                case "Pre-Void":
                    Player.SetSkinId(3);
                    break;
                case "Harbinger":
                    Player.SetSkinId(4);
                    break;
                case "Cosmic Reaver":
                    Player.SetSkinId(5);
                    break;
            }
        }
    }
}