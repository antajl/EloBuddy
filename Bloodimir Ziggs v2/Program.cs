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

namespace Bloodimir_Ziggs_v2
{
    internal class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;
        public static AIHeroClient Ziggs = ObjectManager.Player;

        public static Menu ZiggsMenu,
            ComboMenu,
            LaneJungleClear,
            SkinMenu,
            LastHitMenu,
            MiscMenu,
            HarassMenu,
            DrawMenu,
            FleeMenu,
            PredMenu;

        public static CheckBox SmartMode;
        public static AIHeroClient SelectedHero { get; set; }

        public static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }
        private static Vector3 mousePos
        {
            get { return Game.CursorPos; }
        }
        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Ziggs")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, 250, 1700, 140);
            W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, 250, 1750, 275);
            E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 500, 1750, 100);
            R = new Spell.Skillshot(SpellSlot.R, 2500, SkillShotType.Circular, 1000, 1750, 500);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

            ZiggsMenu = MainMenu.AddMenu("BloodimirZiggs", "bloodimirziggs");
            ZiggsMenu.AddGroupLabel("Bloodimir Ziggs v2.0.0.0");
            ZiggsMenu.AddSeparator();
            ZiggsMenu.AddLabel("Bloodimir Ziggs v2.0.0.0");

            ComboMenu = ZiggsMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("burst", new KeyBind("Burst Target", false, KeyBind.BindTypes.HoldActive, 'N'));
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 1, 0, 5));
            ComboMenu.Add("waitAA", new CheckBox("wait for AA to finish", false));

            HarassMenu = ZiggsMenu.AddSubMenu("HarassMenu", "Harass");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("useEHarass", new CheckBox("Use E"));
            HarassMenu.Add("waitAA", new CheckBox("wait for AA to finish", false));
            HarassMenu.AddLabel("Harass Smart Mana Mode");
            HarassMenu.Add("manamanager", new Slider("Harass Mana Manager %", 40));

            LaneJungleClear = ZiggsMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCE", new CheckBox("Use E"));
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));
            LaneJungleClear.Add("lcmanamanager", new Slider("Lane/Jungle Clear Mana Manager %", 35));


            LastHitMenu = ZiggsMenu.AddSubMenu("Last Hit", "lasthit");
            LastHitMenu.AddGroupLabel("Last Hit Settings");
            LastHitMenu.Add("LHQ", new CheckBox("Use Q"));
            LastHitMenu.Add("lhmanamanager", new Slider("Last Hit Mana Manager %", 35));

            DrawMenu = ZiggsMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("draww", new CheckBox("Draw W"));
            DrawMenu.Add("drawe", new CheckBox("Draw E"));

            MiscMenu = ZiggsMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("KS");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS using Q"));
            MiscMenu.Add("int", new CheckBox("TRY to Interrupt spells"));
            MiscMenu.Add("gapw", new CheckBox("Anti Gapcloser W"));
            SmartMode = MiscMenu.Add("smartMode", new CheckBox("Smart Mana Management"));

            FleeMenu = ZiggsMenu.AddSubMenu("Flee", "Flee");
            FleeMenu.Add("fleew", new CheckBox("Use W to mousePos"));

            PredMenu = ZiggsMenu.AddSubMenu("Prediction", "pred");
            PredMenu.AddGroupLabel("Q Hitchance");
            var qslider = PredMenu.Add("hQ", new Slider("Q HitChance", 1, 0, 2));
            var qMode = new[] { "Low (Fast Casting)", "Medium", "High (Slow Casting)" };
            qslider.DisplayName = qMode[qslider.CurrentValue];

            qslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = qMode[changeArgs.NewValue];
                };
            PredMenu.AddGroupLabel("E Hitchance");
            var eslider = PredMenu.Add("hE", new Slider("E HitChance", 2, 0, 2));
            var eMode = new[] { "Low (Fast Casting)", "Medium", "High (Slow Casting)" };
            eslider.DisplayName = eMode[eslider.CurrentValue];

            eslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = eMode[changeArgs.NewValue];
                };
            PredMenu.AddGroupLabel("W Hitchance");
            var wslider = PredMenu.Add("hW", new Slider("W HitChance", 2, 0, 2));
            var wMode = new[] { "Low (Fast Casting)", "Medium", "High (Slow Casting)" };
            wslider.DisplayName = wMode[wslider.CurrentValue];

            wslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = wMode[changeArgs.NewValue];
                };

            PredMenu.AddGroupLabel("R Hitchance");
            var rslider = PredMenu.Add("hR", new Slider("R HitChance", 2, 0, 2));
            var rMode = new[] { "Low (Fast Casting)", "Medium", "High (Slow Casting)" };
            rslider.DisplayName = rMode[rslider.CurrentValue];

            rslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = rMode[changeArgs.NewValue];
                };
            SkinMenu = ZiggsMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 4, 0, 5));
            var sID = new[] { "Default", "Mad Scientist", "Major", "Pool Party", "Snow Day", "Master Arcanist" };
            skinchange.DisplayName = sID[skinchange.CurrentValue];
            skinchange.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = sID[changeArgs.NewValue];
            };
            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interruptererer;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += OnDraw;
        }

        private static void Game_OnTick(EventArgs args)
        {
            Killsteal();
            SkinChange();
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                LaneJungleClearA.LaneClear();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                LastHitA.LastHitB();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) Flee();
            else if (ComboMenu["burst"].Cast<KeyBind>().CurrentValue) BurstCombo();
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
                                        a.Health < 50 + 20 * Ziggs.Level - (a.HPRegenRate / 5 * 3)))
                    {
                        Ignite.Cast(source);
                        return;
                    }
                }
            }
        }

        private static
            void Gapcloser_OnGapCloser
            (AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!MiscMenu["gapw"].Cast<CheckBox>().CurrentValue) return;
            if (ObjectManager.Player.Distance(gapcloser.Sender, true) <
                W.Range * W.Range && sender.IsValidTarget())
            {
                W.Cast(gapcloser.Sender);
                W.Cast(gapcloser.Sender);
            }
        }

        private static
            void Interruptererer
            (Obj_AI_Base sender,
                Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            {
                if (W.IsReady() && sender.IsValidTarget(W.Range) &&
                    MiscMenu["int"].Cast<CheckBox>().CurrentValue)
                    W.Cast(intTarget.ServerPosition);
            }
        }
        private static void OnDraw(EventArgs args)
        {
            if (!Ziggs.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Ziggs.Position, Q.Range, Color.Goldenrod);
                }
                {
                    if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && E.IsLearned)
                    {
                        Drawing.DrawCircle(Ziggs.Position, E.Range, Color.MediumVioletRed);
                    }
                }
            }
        }

        public static
            void Harass
            ()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Magical);
            var qpredvalue = Q.GetPrediction(target).HitChance >= PredQ();
            var epredvalue = E.GetPrediction(target).HitChance >= PredE();

            if (SelectedHero != null)
            {
                target = SelectedHero;
            }

            if (target == null || !target.IsValid())
            {
                return;
            }

            Orbwalker.OrbwalkTo(mousePos);
            if (Orbwalker.IsAutoAttacking && HarassMenu["waitAA"].Cast<CheckBox>().CurrentValue)
                return;
            if (HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue && Q.IsReady() && qpredvalue)
            {
                if (target.Distance(Ziggs) <= Q.Range ||
                    (Ziggs.ManaPercent > HarassMenu["manamanager"].Cast<Slider>().CurrentValue && SmartMode.CurrentValue))
                {
                    var predQ = Q.GetPrediction(target).CastPosition;
                    Q.Cast(predQ);
                    return;
                }
            }

            if (HarassMenu["useEHarass"].Cast<CheckBox>().CurrentValue && E.IsReady() && epredvalue)
            {
                if (target.Distance(Ziggs) <= E.Range ||
                    (Ziggs.ManaPercent > HarassMenu["manamanager"].Cast<Slider>().CurrentValue && SmartMode.CurrentValue))
                {
                    var predE = E.GetPrediction(target).CastPosition;
                    E.Cast(predE);
                }
            }
        }

        private static
            void Game_OnWndProc
            (WndEventArgs
                args)
        {
            if (args.Msg != (uint)WindowMessages.LeftButtonDown)
            {
                return;
            }
            SelectedHero =
                EntityManager.Heroes.Enemies
                    .FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 39999)
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();
        }

        public static
            void BurstCombo
            ()
        {
            var target = TargetSelector.GetTarget(860, DamageType.Magical);

            if (SelectedHero != null)
            {
                target = SelectedHero;
            }

            if (target == null || !target.IsValid())
            {
                return;
            }

            Orbwalker.OrbwalkTo(mousePos);
            if (R.IsReady() &&
                R.GetPrediction(target).HitChance >= PredR())
            {
                var predR = R.GetPrediction(target).CastPosition;
                R.Cast(predR);
            }

            if (E.IsReady() &&
                E.GetPrediction(target).HitChance >= PredE())
            {
                var predE = E.GetPrediction(target).CastPosition;
                E.Cast(predE);
            }

            if (Q.IsReady() &&
                Q.GetPrediction(target).HitChance >= PredQ())
            {
                var predQ = Q.GetPrediction(target).CastPosition;
                Q.Cast(predQ);
            }

            if (W.IsReady() &&
                W.GetPrediction(target).HitChance >= PredW())
            {
                var predW = W.GetPrediction(target).CastPosition;
                W.Cast(predW);
                if (W.IsReady())
                    W.Cast(predW);
            }
        }

        public static
            void Combo
            ()
        {
            var target = TargetSelector.GetTarget(1550, DamageType.Magical);
            if (target == null || !target.IsValid())
            {
                return;
            }

            if (Orbwalker.IsAutoAttacking && HarassMenu["waitAA"].Cast<CheckBox>().CurrentValue) return;

            {
                target = TargetSelector.GetTarget(1550, DamageType.Magical);

            }

            if (ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue && E.IsReady() &&
                E.GetPrediction(target).HitChance >= PredE())
            {
                var predE = E.GetPrediction(target).CastPosition;
                E.Cast(predE);
            }

            if (ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue && Q.IsReady() &&
                Q.GetPrediction(target).HitChance >= PredQ())
            {
                var predQ = Q.GetPrediction(target).CastPosition;
                Q.Cast(predQ);
            }
              if (ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                W.GetPrediction(target).HitChance >= PredW())
            {
                var predW = W.GetPrediction(target).CastPosition;
                W.Cast(predW);
                W.Cast(predW);
            }
            if (ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue)
                if (R.IsReady() &&
                    Ziggs.CountEnemiesInRange(1670) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue)
                {
                    var rtarget = TargetSelector.GetTarget(1670, DamageType.Magical);
                    R.Cast(rtarget.ServerPosition);
                }
        }

        private static
            void Killsteal
            ()
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
                        if (Ziggs.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                        {
                            {
                                var qkspred = Q.GetPrediction(qtarget).CastPosition;
                                Q.Cast(qkspred);
                            }
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

        public static
            void Flee
            ()
        {
            if (FleeMenu["fleew"].Cast<CheckBox>().CurrentValue)
            {
                Orbwalker.MoveTo(Game.CursorPos);
                W.Cast(Ziggs.Position);
            }
        }

        private static
            HitChance PredQ
            ()
        {
            var mode = PredMenu["hQ"].DisplayName;
            switch (mode)
            {
                case "Low (Fast Casting)":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High (Slow Casting)":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }

        private static
            HitChance PredE
            ()
        {
            var mode = PredMenu["hE"].DisplayName;
            switch (mode)
            {
                case "Low (Fast Casting)":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High (Slow Casting)":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }

        private static
            HitChance PredR
            ()
        {
            var mode = PredMenu["hR"].DisplayName;
            switch (mode)
            {
                case "Low (Fast Casting)":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High (Slow Casting)":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }

        private static
            HitChance PredW
            ()
        {
            var mode = PredMenu["hW"].DisplayName;
            switch (mode)
            {
                case "Low (Fast Casting)":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High (Slow Casting)":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }

        private static void SkinChange()
        {
            var style = SkinMenu["sID"].DisplayName;
            switch (style)
            {
                case "Default":
                    Player.SetSkinId(0);
                    break;
                case "Mad Scientist":
                    Player.SetSkinId(1);
                    break;
                case "Major":
                    Player.SetSkinId(2);
                    break;
                case "Pool Party":
                    Player.SetSkinId(3);
                    break;
                case "Snow Day":
                    Player.SetSkinId(4);
                    break;
                case "Master Arcanist":
                    Player.SetSkinId(5);
                    break;
            }
        }
    }
}