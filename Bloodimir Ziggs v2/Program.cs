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
        public static Spell.Skillshot Q2;
        public static Spell.Skillshot Q3;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;
        public static AIHeroClient Ziggs = ObjectManager.Player;
        public static int UseSecondWTime;

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

        public static AIHeroClient SelectedHero { get; set; }

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
            if (Player.Instance.ChampionName != "Ziggs")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, 300, 1700, 130);
            Q2 = new Spell.Skillshot(SpellSlot.Q, 1125, SkillShotType.Circular, 250 + Q.CastDelay, 1700, 130);
            Q3 = new Spell.Skillshot(SpellSlot.Q, 1400, SkillShotType.Circular, 300 + Q2.CastDelay, 1700, 140);
            W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, 250, 1750, 275);
            E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 500, 1750, 100);
            R = new Spell.Skillshot(SpellSlot.R, 5300, SkillShotType.Circular, 2000, 1500, 500);

            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

            ZiggsMenu = MainMenu.AddMenu("BloodimirZiggs", "bloodimirziggs");
            ZiggsMenu.AddGroupLabel("Bloodimir Ziggs v2.0.2.0");
            ZiggsMenu.AddSeparator();
            ZiggsMenu.AddLabel("Bloodimir Ziggs v2.0.2.0");

            ComboMenu = ZiggsMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 1, 0, 5));
            ComboMenu.Add("wslider", new Slider("Enemy Health Percentage to use W", 15));
            ComboMenu.Add("waitAA", new CheckBox("wait for AA to finish", false));

            HarassMenu = ZiggsMenu.AddSubMenu("HarassMenu", "Harass");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("waitAA", new CheckBox("wait for AA to finish", false));

            LaneJungleClear = ZiggsMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCE", new CheckBox("Use E"));
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));

            LastHitMenu = ZiggsMenu.AddSubMenu("Last Hit", "lasthit");
            LastHitMenu.AddGroupLabel("Last Hit Settings");
            LastHitMenu.Add("LHQ", new CheckBox("Use Q"));

            DrawMenu = ZiggsMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("draww", new CheckBox("Draw W"));
            DrawMenu.Add("drawe", new CheckBox("Draw E"));
            DrawMenu.Add("drawaa", new CheckBox("Draw AA"));

            MiscMenu = ZiggsMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("KS");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS using Q"));
            MiscMenu.Add("int", new CheckBox("TRY to Interrupt spells"));
            MiscMenu.Add("gapw", new CheckBox("Anti Gapcloser W"));
            MiscMenu.Add("peel", new CheckBox("Peel From Melees"));

            FleeMenu = ZiggsMenu.AddSubMenu("Flee", "Flee");
            FleeMenu.Add("fleew", new CheckBox("Use W to mousePos"));

            PredMenu = ZiggsMenu.AddSubMenu("Prediction", "pred");
            PredMenu.AddGroupLabel("Q Hitchance");
            var qslider = PredMenu.Add("hQ", new Slider("Q HitChance", 2, 0, 2));
            var qMode = new[] {"Low (Fast Casting)", "Medium", "High (Slow Casting)"};
            qslider.DisplayName = qMode[qslider.CurrentValue];

            qslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = qMode[changeArgs.NewValue];
                };
            PredMenu.AddGroupLabel("E Hitchance");
            var eslider = PredMenu.Add("hE", new Slider("E HitChance", 2, 0, 2));
            var eMode = new[] {"Low (Fast Casting)", "Medium", "High (Slow Casting)"};
            eslider.DisplayName = eMode[eslider.CurrentValue];

            eslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = eMode[changeArgs.NewValue];
                };
            PredMenu.AddGroupLabel("W Hitchance");
            var wslider = PredMenu.Add("hW", new Slider("W HitChance", 1, 0, 2));
            var wMode = new[] {"Low (Fast Casting)", "Medium", "High (Slow Casting)"};
            wslider.DisplayName = wMode[wslider.CurrentValue];

            wslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = wMode[changeArgs.NewValue];
                };
            SkinMenu = ZiggsMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 4, 0, 5));
            var sID = new[] {"Default", "Mad Scientist", "Major", "Pool Party", "Snow Day", "Master Arcanist"};
            skinchange.DisplayName = sID[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                Combo();
            var target = TargetSelector.GetTarget(1200f, DamageType.Magical);
            if (target != null)
            {
                if (ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue
                    && Q.IsReady())
                {
                    CastQ(target);
                }

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
                                            a.Health < 50 + 20*Ziggs.Level - (a.HPRegenRate/5*3)))
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
            if (!MiscMenu["gapw"].Cast<CheckBox>().CurrentValue) return;
            if (ObjectManager.Player.Distance(gapcloser.Sender, true) <
                W.Range*W.Range && sender.IsValidTarget())
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
                    Drawing.DrawCircle(Ziggs.Position, Q2.Range, Color.Blue);
                    Drawing.DrawCircle(Ziggs.Position, Q3.Range, Color.Tomato);
                }
                {
                    if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && E.IsLearned)
                    {
                        Drawing.DrawCircle(Ziggs.Position, E.Range, Color.MediumVioletRed);
                    }
                    if (DrawMenu["draww"].Cast<CheckBox>().CurrentValue && W.IsLearned)
                    {
                        Drawing.DrawCircle(Ziggs.Position, W.Range, Color.DarkRed);
                    }
                    if (DrawMenu["drawaa"].Cast<CheckBox>().CurrentValue)
                    {
                        Drawing.DrawCircle(Ziggs.Position, Player.Instance.AttackRange, Color.DimGray);
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
                if (target.Distance(Ziggs) <= Q.Range)
                {
                    var predQ = Q.GetPrediction(target).CastPosition;
                    Q.Cast(predQ);
                }
            }

        }

        private static
            void Game_OnWndProc
            (WndEventArgs
                args)
        {
            if (args.Msg != (uint) WindowMessages.LeftButtonDown)
            {
                return;
            }
            SelectedHero =
                EntityManager.Heroes.Enemies
                    .FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 39999)
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();
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

            if (ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue && E.IsReady() &&
                E.GetPrediction(target).HitChance >= PredE())
            {
                var predE = E.GetPrediction(target).CastPosition;
                E.Cast(predE);
            }
            if (ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
            }
            var wpred = W.GetPrediction(target);
            if (wpred.HitChance <= PredW())
            {
                if ((W.IsInRange(wpred.UnitPosition) && target.HealthPercent >= ComboMenu["wslider"].Cast<Slider>().CurrentValue &&
                     ObjectManager.Player.ServerPosition.Distance(wpred.UnitPosition) > W.Range - 250 &&
                     wpred.UnitPosition.Distance(ObjectManager.Player.ServerPosition) >
                     target.Distance(ObjectManager.Player)))
                {
                    var pp =
                        ObjectManager.Player.ServerPosition.To2D()
                            .Extend(wpred.UnitPosition.To2D(), W.Range)
                            .To3D();
                    W.Cast(pp);
                    UseSecondWTime = Environment.TickCount;
                }
                if (ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue)
                    if (R.IsReady())
                    {
                        var predR = R.GetPrediction(target).CastPosition;
                       if (target.CountEnemiesInRange(R.Width) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue)
                        R.Cast(predR);
                    }

                if (MiscMenu["peel"].Cast<CheckBox>().CurrentValue)
                {
                    foreach (var pos in from enemy in ObjectManager.Get<Obj_AI_Base>()
                        where
                            enemy.IsValidTarget() &&
                            enemy.Distance(ObjectManager.Player) <=
                            enemy.BoundingRadius + enemy.AttackRange + ObjectManager.Player.BoundingRadius &&
                            enemy.IsMelee
                        let direction =
                            (enemy.ServerPosition.To2D() - ObjectManager.Player.ServerPosition.To2D()).Normalized()
                        let pos = ObjectManager.Player.ServerPosition.To2D()
                        select pos + Math.Min(200, Math.Max(50, enemy.Distance(ObjectManager.Player)/2))*direction)
                    {
                        W.Cast(pos.To3D());
                        UseSecondWTime = Environment.TickCount;
                    }
                }
            }
        }

        private static void CastQ(Obj_AI_Base target)
        {
           if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue)
            {
            PredictionResult prediction;

            if (Q.IsInRange(target))
            {
             prediction = Q.GetPrediction(target);
                Q.Cast(prediction.CastPosition);
            }
            else if (Q2.IsInRange(target))
            {
                prediction = Q2.GetPrediction(target);
                Q2.Cast(prediction.CastPosition);
            }
            else if (Q3.IsInRange(target))
            {
             prediction = Q3.GetPrediction(target);
                Q3.Cast(prediction.CastPosition);
            }
            else
            {
                return;
            }

            if (prediction.HitChance >= HitChance.High)
            {
                if (ObjectManager.Player.ServerPosition.Distance(prediction.CastPosition) <= Q.Range + Q.Width)
                {
                    Vector3 p;
                    if (ObjectManager.Player.ServerPosition.Distance(prediction.CastPosition) > 300)
                    {
                        p = prediction.CastPosition -
                            100*
                            (prediction.CastPosition.To2D() - ObjectManager.Player.ServerPosition.To2D()).Normalized()
                                .To3D();
                    }
                    else
                    {
                        p = prediction.CastPosition;
                    }

                    Q.Cast(p);
                }
                else if (ObjectManager.Player.ServerPosition.Distance(prediction.CastPosition) <=
                         ((Q.Range + Q2.Range)/2))
                {
                    var p = ObjectManager.Player.ServerPosition.To2D()
                        .Extend(prediction.CastPosition.To2D(), Q.Range - 100);

                    if (!CheckQCollision(target, prediction.UnitPosition, p.To3D()))
                    {
                        Q.Cast(p.To3D());
                    }
                }
                else
                {
                    var p = ObjectManager.Player.ServerPosition.To2D() +
                            Q.Range*
                            (prediction.CastPosition.To2D() - ObjectManager.Player.ServerPosition.To2D()).Normalized
                                ();

                    if (!CheckQCollision(target, prediction.UnitPosition, p.To3D()))
                    {
                        Q.Cast(p.To3D());
                    }
                }
            }
        }}
            

        private static bool CheckQCollision(Obj_AI_Base target, Vector3 targetPosition, Vector3 castPosition)
        {
            var direction = (castPosition.To2D() - ObjectManager.Player.ServerPosition.To2D()).Normalized();
            var firstBouncePosition = castPosition.To2D();
            var secondBouncePosition = firstBouncePosition +
                                       direction*0.4f*
                                       ObjectManager.Player.ServerPosition.To2D().Distance(firstBouncePosition);
            var thirdBouncePosition = secondBouncePosition +
                                      direction*0.6f*firstBouncePosition.Distance(secondBouncePosition);

            if (thirdBouncePosition.Distance(targetPosition.To2D()) < Q.Width + target.BoundingRadius)
            {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                {
                    if (minion.IsValidTarget(3000))
                    {
                        var predictedPos = Q2.GetPrediction(minion);
                        if (predictedPos.UnitPosition.To2D().Distance(secondBouncePosition) <
                            Q2.Width + minion.BoundingRadius)
                        {
                            return true;
                        }
                    }
                }
            }

            if (secondBouncePosition.Distance(targetPosition.To2D()) < Q.Width + target.BoundingRadius ||
                thirdBouncePosition.Distance(targetPosition.To2D()) < Q.Width + target.BoundingRadius)
            {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                {
                    if (minion.IsValidTarget(3000))
                    {
                        var predictedPos = Q.GetPrediction(minion);
                        if (predictedPos.UnitPosition.To2D().Distance(firstBouncePosition) <
                            Q.Width + minion.BoundingRadius)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            return true;
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
                            var qkspred = Q.GetPrediction(qtarget).CastPosition;
                            var qks2Pred = Q2.GetPrediction(qtarget).CastPosition;
                            var qks3Pred = Q3.GetPrediction(qtarget).CastPosition;
                            {
                                if (Q.IsInRange(qtarget))
                                {
                                    Q.Cast(qkspred);
                                }
                                else if (Q2.IsInRange(qtarget))
                                {
                                    Q2.Cast(qks2Pred);
                                }
                                else if (Q3.IsInRange(qtarget))
                                {
                                    Q3.Cast(qks3Pred);
                                }
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
                W.Cast(Ziggs.ServerPosition);
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