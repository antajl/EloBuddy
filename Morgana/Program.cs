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
        public static Menu MorgMenu, ComboMenu, DrawMenu, SkinMenu, MiscMenu, QMenu, WMenu, LaneClear;
        public static AIHeroClient Me = ObjectManager.Player;
        public static HitChance QHitChance;
        public static HitChance WHitChance;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Morgana")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, (int) 250f, (int) 1200f, (int) 80f);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, (int) 250f, (int) 20f, (int) 0f);
            E = new Spell.Targeted(SpellSlot.E, 750);
            R = new Spell.Active(SpellSlot.R, 600);

            MorgMenu = MainMenu.AddMenu("B.Morgana", "bloodimirmorgana");
            MorgMenu.AddGroupLabel("Bloodimir.Morgana");
            MorgMenu.AddSeparator();
            MorgMenu.AddLabel("An Addon made my Bloodimir/turkey");

            ComboMenu = MorgMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
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
            QMenu.AddLabel("EB's common prediction and hitchance is still beta and sometimes it wont cast Q." +
                           Environment.NewLine + "But it works just fine if you use Medium hitchance prediction." +
                           Environment.NewLine +
                           "This allows Q to cast more but also a slightly smaller bind success percentage.");
            QMenu.AddSeparator();
            QMenu.Add("mediumpred", new CheckBox("MEDIUM Bind Hitchance Prediction / Disabled = High", false));

            WMenu = MorgMenu.AddSubMenu("W Settings", "wsettings");
            WMenu.AddGroupLabel("W Settings");
            WMenu.AddSeparator();
            WMenu.Add("wmax", new Slider("Max Range", (int) W.Range, 0, (int) W.Range));
            WMenu.Add("wmin", new Slider("Min Range", 124, 0, (int) W.Range));
            WMenu.AddSeparator();
            WMenu.Add("mediumpred", new CheckBox("MEDIUM Soil Hitchance Prediction / Disabled = High"));

            SkinMenu = MorgMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 0, 0, 6));
            var sID = new[]
            {"Default", "Exiled", "Sinful Succulence", "Blade Mistress", "Blackthorn", "Ghost Bride", "Victorius"};
            skinchange.DisplayName = sID[skinchange.CurrentValue];
            skinchange.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = sID[changeArgs.NewValue];
                if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                {
                    Chat.Print("skin-changed");
                }
            };

            MiscMenu = MorgMenu.AddSubMenu("Misc", "misc");
            MiscMenu.AddGroupLabel("KS");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS with Q"));
            MiscMenu.AddSeparator();
            MiscMenu.AddGroupLabel("Interrupt");
            MiscMenu.AddSeparator();
            MiscMenu.Add("intq", new CheckBox("Q to Interrupt"));
            MiscMenu.Add("dashq", new CheckBox("Q on Dashing"));
            MiscMenu.Add("immoq", new CheckBox("Q on Immobile"));
            MiscMenu.Add("immow", new CheckBox("W on Immobile"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("debug", new CheckBox("Debug", false));

            DrawMenu = MorgMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("draww", new CheckBox("Draw W"));

            LaneClear = MorgMenu.AddSubMenu("Lane Clear", "laneclear");
            LaneClear.AddGroupLabel("Lane Clear Settings");
            LaneClear.Add("LCW", new CheckBox("Use W"));

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnTick += Tick;
            Drawing.OnDraw += OnDraw;
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            {
                if (Q.IsReady() && sender.IsValidTarget(Q.Range) && MiscMenu["intq"].Cast<CheckBox>().CurrentValue)
                    Q.Cast(intTarget);
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
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                Combo(ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue,
                    ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue,
                    ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                LaneClearA.LaneClear();
            }
            SkinChange();
        }

        private static void AutoCast()
        {
            if (Q.IsReady())
            {
                try
                {
                    foreach (
                        var enemy in
                            ObjectManager.Get<AIHeroClient>()
                                .Where(x => x.IsValidTarget(MiscMenu["qmax"].Cast<Slider>().CurrentValue)))
                    {
                        if (MiscMenu["dashq"].Cast<CheckBox>().CurrentValue &&
                            MiscMenu["bind" + enemy.ChampionName].Cast<CheckBox>().CurrentValue)
                            if (enemy.Distance(Me.ServerPosition) > MiscMenu["qmin"].Cast<Slider>().CurrentValue)
                                if (Q.GetPrediction(enemy).HitChance == HitChance.Dashing)
                                {
                                    Q.Cast(enemy);

                                    if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                                    {
                                        Chat.Print("q-dash");
                                    }
                                }
                        if (MiscMenu["immoq"].Cast<CheckBox>().CurrentValue &&
                            MiscMenu["bind" + enemy.ChampionName].Cast<CheckBox>().CurrentValue)
                            if (enemy.Distance(Me.ServerPosition) > MiscMenu["qmin"].Cast<Slider>().CurrentValue)
                                if (Q.GetPrediction(enemy).HitChance == HitChance.Immobile)
                                {
                                    Q.Cast(enemy);

                                    if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                                    {
                                        Chat.Print("q-immo");
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
                            var enemy in
                                ObjectManager.Get<AIHeroClient>()
                                    .Where(x => x.IsValidTarget(MiscMenu["wmax"].Cast<Slider>().CurrentValue)))
                        {
                            if (MiscMenu["immow"].Cast<CheckBox>().CurrentValue &&
                                MiscMenu["bind" + enemy.ChampionName].Cast<CheckBox>().CurrentValue)
                                if (enemy.Distance(Me.ServerPosition) > MiscMenu["wmin"].Cast<Slider>().CurrentValue)
                                    if (W.GetPrediction(enemy).HitChance == HitChance.Immobile)
                                    {
                                        W.Cast(enemy);

                                        if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                                        {
                                            Chat.Print("w-immo");
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

        private static void Killsteal()
        {
            if (MiscMenu["ksq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                try
                {
                    foreach (
                        var qtarget in
                            HeroManager.Enemies.Where(
                                hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
                    {
                        if (Me.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                        {
                            var poutput = Q.GetPrediction(qtarget);
                            if (poutput.HitChance >= HitChance.Medium)
                            {
                                Q.Cast(poutput.CastPosition);
                                if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                                {
                                    Chat.Print("q-ks");
                                }
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

        private static void Combo(bool useW, bool useQ, bool useR)
        {
            if (useW && W.IsReady())
            {
                var soilTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                if (soilTarget.IsValidTarget(W.Range))
                {
                    if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                    {
                        Chat.Print("Valid soil target found");
                    }
                    if (W.GetPrediction(soilTarget).HitChance >= WHitChance)
                    {
                        if (soilTarget.Distance(Me.ServerPosition) > WMenu["wmin"].Cast<Slider>().CurrentValue)
                        {
                            W.Cast(soilTarget);

                            if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                            {
                                Chat.Print("w-combo");
                            }
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
                        if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                        {
                            Chat.Print("Valid target found");
                        }
                        if (Q.GetPrediction(bindTarget).HitChance >= QHitChance)
                        {
                            if (bindTarget.Distance(Me.ServerPosition) > QMenu["qmin"].Cast<Slider>().CurrentValue)
                            {
                                if (QMenu["bind" + bindTarget.ChampionName].Cast<CheckBox>().CurrentValue)
                                {
                                    Q.Cast(bindTarget);

                                    if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                                    {
                                        Chat.Print("q-combo");
                                    }
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
                    if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                    {
                        Chat.Print("r-combo");
                    }
                }
            }
        }
    }
}