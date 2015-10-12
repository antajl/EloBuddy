using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Morgana
{
    internal class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;
        public static Menu MorgMenu, ComboMenu, DrawMenu, SkinMenu, MiscMenu, QMenu, WMenu, LaneClear, LastHit;
        public static AIHeroClient Me = ObjectManager.Player;
        public static HitChance QHitChance;
        public static HitChance WHitChance;

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
            if (Player.Instance.ChampionName != "Morgana")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, (int) 250f, (int) 1200f, (int) 80f);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, (int) 250f, (int) 2200f, (int) 400f);
            E = new Spell.Targeted(SpellSlot.E, 750);
            R = new Spell.Active(SpellSlot.R, 600);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

            MorgMenu = MainMenu.AddMenu("B.Morgana", "bloodimirmorgana");
            MorgMenu.AddGroupLabel("Bloodimir.Morgana");
            MorgMenu.AddSeparator();
            MorgMenu.AddLabel("Bloodimir Morgana v1.0.4.0");

            ComboMenu = MorgMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 1, 0, 5));

            QMenu = MorgMenu.AddSubMenu("Q Settings", "qsettings");
            QMenu.AddGroupLabel("Q Settings");
            QMenu.AddSeparator();
            QMenu.Add("qmin", new Slider("Min Range", 200, 0, (int) Q.Range));
            QMenu.Add("qmax", new Slider("Max Range", (int) Q.Range, 0, (int) Q.Range));
            QMenu.AddSeparator();
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team != Me.Team))
            {
                QMenu.Add("bind" + obj.ChampionName.ToLower(), new CheckBox("Bind " + obj.ChampionName));
            }
            QMenu.AddSeparator();
            QMenu.Add("mediumpred", new CheckBox("MEDIUM Bind Hitchance Prediction / Disabled = High", false));
            QMenu.AddSeparator();
            QMenu.Add("intq", new CheckBox("Q to Interrupt"));
            QMenu.Add("dashq", new CheckBox("Q on Dashing"));
            QMenu.Add("immoq", new CheckBox("Q on Immobile"));
            QMenu.Add("gapq", new CheckBox("Q on Gapcloser"));

            WMenu = MorgMenu.AddSubMenu("W Settings", "wsettings");
            WMenu.AddGroupLabel("W Settings");
            WMenu.AddSeparator();
            WMenu.Add("wmax", new Slider("Max Range", (int) W.Range, 0, (int) W.Range));
            WMenu.Add("wmin", new Slider("Min Range", 124, 0, (int) W.Range));
            WMenu.AddSeparator();
            WMenu.Add("mediumpred", new CheckBox("MEDIUM Soil Hitchance Prediction / Disabled = High"));
            WMenu.AddSeparator();
            WMenu.Add("immow", new CheckBox("W on Immobile"));

            SkinMenu = MorgMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 5, 0, 6));
            var sID = new[]
            {"Default", "Exiled", "Sinful Succulence", "Blade Mistress", "Blackthorn", "Ghost Bride", "Victorius"};
            skinchange.DisplayName = sID[skinchange.CurrentValue];
            skinchange.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = sID[changeArgs.NewValue];
            };

            MiscMenu = MorgMenu.AddSubMenu("Misc", "misc");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS with Q"));
            MiscMenu.Add("peel", new CheckBox("Peel from Melee Champions"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("ELowAllies", new CheckBox("Use E on % Hp Allies"));
            MiscMenu.Add("EHPPercent", new Slider("Ally HP %", 45));
            MiscMenu.AddSeparator();
            MiscMenu.Add("support", new CheckBox("Support Mode", false));

            DrawMenu = MorgMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("draww", new CheckBox("Draw W"));

            LaneClear = MorgMenu.AddSubMenu("Lane Clear", "laneclear");
            LaneClear.AddGroupLabel("Lane Clear Settings");
            LaneClear.Add("LCW", new CheckBox("Use W"));
            
            LastHit = MorgMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q"));

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnTick += Tick;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            {
                if (Q.IsReady() && sender.IsValidTarget(Q.Range) && MiscMenu["intq"].Cast<CheckBox>().CurrentValue)
                    Q.Cast(intTarget.ServerPosition);
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
                        Q.Cast(pos.To3D());
                    }
                }
            }
        private static void AutoE()
        {
            var shieldAllies = MiscMenu["ELowAllies"].Cast<CheckBox>().CurrentValue;
            var shieldHealthPercent = MiscMenu["EHPPercent"].Cast<Slider>().CurrentValue;

            if (shieldAllies)
            {
                var ally =
                    EntityManager.Heroes.Allies.Where(
                        x => x.IsValidTarget(W.Range) && x.HealthPercent < shieldHealthPercent)
                        .FirstOrDefault();    
                if (ally != null && ally.CountEnemiesInRange(650) >= 1)
                {
                    E.Cast(ally);
                }
            }
        }
        private static
           void Gapcloser_OnGapCloser
           (AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!QMenu["gapq"].Cast<CheckBox>().CurrentValue) return;
            if (ObjectManager.Player.Distance(gapcloser.Sender, true) <
                Q.Range * Q.Range && sender.IsValidTarget())
            {
                Q.Cast(gapcloser.Sender);
            }
        }
        private static void OnDraw(EventArgs args)
        {
            if (!Me.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Me.Position, Q.Range, Color.LightYellow);
                }
                if (DrawMenu["draww"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Me.Position, W.Range, Color.LightBlue);
                }
            }
        }

        private static void Tick(EventArgs args)
        {
            QHitChance = QMenu["mediumpred"].Cast<CheckBox>().CurrentValue ? HitChance.Medium : HitChance.High;
            WHitChance = WMenu["mediumpred"].Cast<CheckBox>().CurrentValue ? HitChance.Medium : HitChance.High;
            Killsteal();
            AutoCast();
            SkinChange();
            AutoE();
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    Combo(ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue,
                        ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue,
                        ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                LaneClearA.LaneClear();
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
                                        a.Health < 50 + 20*Me.Level - (a.HPRegenRate/5*3)))
                    {
                        Ignite.Cast(source);
                        return;
                    }
        }
            }

        private static void AutoCast()
        {
            if (Q.IsReady())
            {
                try
                {
                    foreach (
                        var enemy in EntityManager.Heroes.Enemies
                            .Where(x => x.IsValidTarget(QMenu["qmax"].Cast<Slider>().CurrentValue)))
                    {
                        if (QMenu["dashq"].Cast<CheckBox>().CurrentValue &&
                            QMenu["bind" + enemy.ChampionName].Cast<CheckBox>().CurrentValue)
                        {
                            var pred = Q.GetPrediction(enemy);
                            if (pred.HitChance >= HitChance.Dashing)
                            {
                                Q.Cast(pred.CastPosition);
                            }
                        }
                    }
                }
                catch
                {
                }
                if (W.IsReady())
                {
                    try
                    {
                        foreach (
                            var enemy in EntityManager.Heroes.Enemies
                                .Where(x => x.IsValidTarget(WMenu["wmax"].Cast<Slider>().CurrentValue)))
                        {
                            if (WMenu["immow"].Cast<CheckBox>().CurrentValue &&
                                QMenu["bind" + enemy.ChampionName].Cast<CheckBox>().CurrentValue)
                            {
                                var pred = W.GetPrediction(enemy);
                                if (pred.HitChance >= HitChance.Immobile)
                                {
                                    W.Cast(pred.CastPosition);
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

        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) ||
                (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) ||
                 Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)))
            {
                var t = target as Obj_AI_Minion;
                if (t != null)
                {
                    {
                        if (MiscMenu["support"].Cast<CheckBox>().CurrentValue)
                            args.Process = false;
                    }
                }
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
                        if (Me.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                        {
                            var poutput = Q.GetPrediction(qtarget);
                            if (poutput.HitChance >= HitChance.Medium)
                            {
                                Q.Cast(poutput.CastPosition);
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
                case "Exiled":
                    Player.SetSkinId(1);
                    break;
                case "Sinful Succulence":
                    Player.SetSkinId(2);
                    break;
                case "Blade Mistress":
                    Player.SetSkinId(3);
                    break;
                case "Blackthorn":
                    Player.SetSkinId(4);
                    break;
                case "Ghost Bride":
                    Player.SetSkinId(5);
                    break;
                case "Victorius":
                    Player.SetSkinId(6);
                    break;
            }
        }

        private static
            void Combo(bool useW, bool useQ, bool useR)
        {
            if (useW && W.IsReady())
            {
                var soilTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                if (soilTarget.IsValidTarget(W.Range))
                {
                    if (W.GetPrediction(soilTarget).HitChance >= WHitChance)
                    {
                        if (soilTarget.Distance(Me.ServerPosition) > WMenu["wmin"].Cast<Slider>().CurrentValue)
                        {
                            W.Cast(soilTarget);

                        }
                    }
                }
            }

            if (useQ && Q.IsReady())
            {
                try
                {
                    var bindTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                    if (bindTarget.IsValidTarget(Q.Range))
                    {
                        if (Q.GetPrediction(bindTarget).HitChance >= QHitChance)
                        {
                            if (bindTarget.Distance(Me.ServerPosition) > QMenu["qmin"].Cast<Slider>().CurrentValue)
                            {
                                if (QMenu["bind" + bindTarget.ChampionName].Cast<CheckBox>().CurrentValue)
                                {
                                    Q.Cast(bindTarget);
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                if (useR && R.IsReady() &&
                    Me.CountEnemiesInRange(R.Range) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue)
                {
                    R.Cast();
                }
            }
        }
    }
}