using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Blitz
{
    internal class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Active W, E, R;
        public static Spell.Targeted Ignite, Exhaust;
        public static Menu MorgMenu, ComboMenu, DrawMenu, SkinMenu, MiscMenu, QMenu;
        public static AIHeroClient Blitz = ObjectManager.Player;
        public static HitChance QHitChance;

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
            if (Player.Instance.ChampionName != "Blitzcrank")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 250,1800,70);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Active(SpellSlot.R, 550);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Exhaust = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerexhaust"), 650);

            MorgMenu = MainMenu.AddMenu("BloodimirBlitz", "bloodimirblitz");
            MorgMenu.AddGroupLabel("Bloodimir Blitzcrank");
            MorgMenu.AddSeparator();
            MorgMenu.AddLabel("Bloodimir Blitzcrank v1.0.0.0");

            ComboMenu = MorgMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
            ComboMenu.Add("comboOnlyExhaust", new CheckBox("Use Exhaust (Combo Only)"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 1, 0, 5));

            QMenu = MorgMenu.AddSubMenu("Q Settings", "qsettings");
            QMenu.AddGroupLabel("Q Settings");
            QMenu.AddSeparator();
            QMenu.Add("qmin", new Slider("Min Range", 176, 0, (int)Q.Range));
            QMenu.Add("qmax", new Slider("Max Range", (int)Q.Range, 0, (int)Q.Range));
            QMenu.AddSeparator();
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team != Blitz.Team))
            {
                QMenu.Add("grab" + obj.ChampionName.ToLower(), new CheckBox("Grab " + obj.ChampionName));
            }
            QMenu.AddSeparator();
            QMenu.Add("mediumpred", new CheckBox("MEDIUM Bind Hitchance Prediction / Disabled = High", false));
            QMenu.AddSeparator();
            QMenu.Add("intq", new CheckBox("Q to Interrupt"));

            SkinMenu = MorgMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 4, 0, 8));
            var sID = new[] { "Default", "Rusty", "Goalkeeper", "Boom Boom", "Piltover Customs", "DefNotBlitz", "iBlitzCrank", "RiotCrank", "Battle Boss" };
            skinchange.DisplayName = sID[skinchange.CurrentValue];
            skinchange.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = sID[changeArgs.NewValue];
            };

            MiscMenu = MorgMenu.AddSubMenu("Misc", "misc");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS with Q"));
            MiscMenu.Add("ksr", new CheckBox("KS with R"));
            MiscMenu.Add("LHE", new CheckBox("Last Hit E"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("support", new CheckBox("Support Mode"));
            MiscMenu.Add("fleew", new CheckBox("Use W Flee"));
            MiscMenu.Add("useexhaust", new CheckBox("Use Exhaust"));
            foreach (var source in ObjectManager.Get<AIHeroClient>().Where(a => a.IsEnemy))
            {
                MiscMenu.Add(source.ChampionName + "exhaust",
                    new CheckBox("Exhaust " + source.ChampionName, false));
            }


            DrawMenu = MorgMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("drawr", new CheckBox("Draw R"));

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnTick += Tick;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            {
                if (Q.IsReady() && sender.IsValidTarget(Q.Range) && MiscMenu["intq"].Cast<CheckBox>().CurrentValue)
                    Q.Cast(intTarget.ServerPosition);
            }
        }
        private static void OnDraw(EventArgs args)
        {
            if (!Blitz.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Blitz.Position, Q.Range, Color.Red);
                }
                if (DrawMenu["drawr"].Cast<CheckBox>().CurrentValue && R.IsLearned)
                {
                    Drawing.DrawCircle(Blitz.Position, R.Range, Color.LightBlue);
                }
            }
        }
        public static
            void Flee
            ()
        {
            if (MiscMenu["fleew"].Cast<CheckBox>().CurrentValue)
            {
                Orbwalker.MoveTo(Game.CursorPos);
                W.Cast();
            }
        }
        private static void Tick(EventArgs args)
        {
            QHitChance = QMenu["mediumpred"].Cast<CheckBox>().CurrentValue ? HitChance.Medium : HitChance.High;
            SkinChange();
            Killsteal();
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    Combo(ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue,
                        ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue,
                        ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
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
                                    a.Health < 50 + 20 * Blitz.Level - (a.HPRegenRate / 5 * 3)))
                {
                    Ignite.Cast(source);
                    return;
                }
                 if (!MiscMenu["useexhaust"].Cast<CheckBox>().CurrentValue || ComboMenu["comboOnlyExhaust"].Cast<CheckBox>().CurrentValue &&
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                return;
            foreach (
                var enemy in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(a => a.IsEnemy && a.IsValidTarget(Exhaust.Range))
                        .Where(enemy => MiscMenu[enemy.ChampionName + "exhaust"].Cast<CheckBox>().CurrentValue))
            {
                if (enemy.IsFacing(Blitz))
                {
                    if (!(Blitz.HealthPercent < 50)) continue;
                    Exhaust.Cast(enemy);
                    return;
                }
                if (!(enemy.HealthPercent < 50)) continue;
                Exhaust.Cast(enemy);
                return;
            }
            }
        }




        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) ||
                 Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
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

                    foreach (
                        var qtarget in
                            EntityManager.Heroes.Enemies.Where(
                                hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
                    {
                        if (Blitz.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                        {
                            var poutput = Q.GetPrediction(qtarget);
                            if (poutput.HitChance >= HitChance.Medium)
                            {
                                Q.Cast(poutput.CastPosition);
                            }
                if (MiscMenu["ksr"].Cast<CheckBox>().CurrentValue && W.IsReady())
                            {
                                {
                                        foreach (
                                            var rtarget in
                                                EntityManager.Heroes.Enemies.Where(
                                                    hero =>
                                                        hero.IsValidTarget(R.Range) && !hero.IsDead && !hero.IsZombie))
                                        {
                                            if (Blitz.GetSpellDamage(rtarget, SpellSlot.R) >= rtarget.Health)
                                                R.Cast();
                                        }
                                    }
                                }
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
                case "Rusty":
                    Player.SetSkinId(1);
                    break;
                case "Goalkeeper":
                    Player.SetSkinId(2);
                    break;
                case "Boom Boom":
                    Player.SetSkinId(3);
                    break;
                case "Piltover Customs":
                    Player.SetSkinId(4);
                    break;
                case "DefNotBlitz":
                    Player.SetSkinId(5);
                    break;
                case "iBlitzCrank":
                    Player.SetSkinId(6);
                    break;
            case "RiotCrank":
                    Player.SetSkinId(7);
                    break;
                case "Battle Boss":
                    Player.SetSkinId(8);
                    break;

            }
        }
        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                var e = target as AIHeroClient;
                if (ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue && E.IsReady() && e.IsEnemy)
                    if (target != null)
                    {
                        if (e.IsValidTarget() && E.IsReady())
                        {
                            E.Cast();
                        }
                    }
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
        private static
            void LastHit()
        {
            var ECHECK = MiscMenu["LHE"].Cast<CheckBox>().CurrentValue;
            var EREADY = E.IsReady();
            if (ECHECK || EREADY)
                {
                    var eminion = (Obj_AI_Minion) GetEnemy(125, GameObjectType.obj_AI_Minion);
                    if (eminion != null)
                    {
                       E.Cast();
                    }
                }
        }
        private static
            void Combo(bool useW, bool useQ, bool useR)
        {
            if (useW && W.IsReady() && ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(350, DamageType.Physical);
                if (target.IsValidTarget(W.Range))
                {
                    {
                        if (target.Distance(Blitz.ServerPosition) > 350)
                        {
                            W.Cast();

                        }
                    }
                }
            }

                if (useQ && Q.IsReady() && ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue)
            {
                try
                {
                    var grabTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                    if (grabTarget.IsValidTarget(Q.Range))
                    {
                        if (Q.GetPrediction(grabTarget).HitChance >= QHitChance)
                        {
                            if (grabTarget.Distance(Blitz.ServerPosition) > QMenu["qmin"].Cast<Slider>().CurrentValue)
                            {
                                if (QMenu["grab" + grabTarget.ChampionName].Cast<CheckBox>().CurrentValue)
                                {
                                    Q.Cast(grabTarget);
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                if (useR && R.IsReady() &&
                    Blitz.CountEnemiesInRange(R.Range) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue && ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue)
                {
                    R.Cast();
                }
            }
        }
    }
    }
