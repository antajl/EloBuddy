using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
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
        public static Menu MorgMenu, ComboMenu, DrawMenu, MiscMenu, QMenu;
        public static AIHeroClient Me = ObjectManager.Player;
        public static HitChance QHitChance;
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }
        private static void OnLoaded(EventArgs args)
        {
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, (int)250f, (int)1200f, (int)80f);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, (int)250f, (int)20f, (int)0f);
            E = new Spell.Targeted(SpellSlot.E, 750);
            R = new Spell.Active(SpellSlot.R, 1000);

            MorgMenu = MainMenu.AddMenu("O.Morgana", "omorgana");
            MorgMenu.AddGroupLabel("O.Morgana");
            MorgMenu.AddSeparator();
            MorgMenu.AddLabel("An Addon made my Bloodimir");

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
            QMenu.Add("qmin", new Slider("Min Range", 195, 0, (int)Q.Range));
            QMenu.Add("qmax", new Slider("Max Range", (int)Q.Range, 0, (int)Q.Range));
            QMenu.AddSeparator();
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team != Me.Team))
            {
                QMenu.Add("bind" + obj.ChampionName.ToLower(), new CheckBox("Bind " + obj.ChampionName));
            }
            QMenu.AddSeparator();
            QMenu.AddLabel("EB's common prediction and hitchance is still beta and sometimes it wont cast Q." + Environment.NewLine + "But it works just fine if you use Medium hitchance prediction." + Environment.NewLine + "This allows Q to cast more but also a slightly smaller bind success percentage.");
            QMenu.AddSeparator();
            QMenu.Add("mediumpred", new CheckBox("Medium Hitchance Prediction"));


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
            MiscMenu.AddSeparator();
            MiscMenu.Add("debug", new CheckBox("Debug", false));

            DrawMenu = MorgMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));


            Interrupter.OnInterruptableSpell += Interrupt;
            Game.OnTick += Tick;
            Drawing.OnDraw += OnDraw;
        }

        private static void Interrupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (MiscMenu["intq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                if (sender.Distance(Me, true) <= Q.RangeSquared)
                {
                    var pred = Q.GetPrediction(sender);
                    if (pred.HitChance >= HitChance.Low)
                    {
                        Q.Cast(pred.CastPosition);
                        if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                        {
                            Chat.Print("q-int");
                        }
                    }
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Me.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Me.Position, Q.Range, Color.White);
                }
            }
        }
        private static void Tick(EventArgs args)
        {
            QHitChance = QMenu["mediumpred"].Cast<CheckBox>().CurrentValue ? HitChance.Medium : HitChance.High;
            Killsteal();
            AutoCast();
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                Combo(ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue,
                    ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
            }
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
                        if (MiscMenu["imoq"].Cast<CheckBox>().CurrentValue &&
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
            }
        }
        private static void Killsteal()
        {
            if (MiscMenu["ksq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                try
                {
                    foreach (var qtarget in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
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
        private static void Combo(bool shoulduseQ, bool shoulduseR)
        {

            if (shoulduseQ && Q.IsReady())
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
                if (shoulduseR && R.IsReady() &&
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
