﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;
using extend = EloBuddy.SDK.Extensions;

namespace Bloodimir_Annie
{
    internal class Program
    {
        public static Spell.Targeted Q, Ignite, Exhaust;
        public static Spell.Skillshot W, R, Flash;
        public static Spell.Active E;
        public static Menu AnnieMenu, ComboMenu, DrawMenu, SkinMenu, MiscMenu, LaneJungleClear, LastHit;
        public static Item Zhonia;
        public static AIHeroClient Annie = ObjectManager.Player;
        public static List<Obj_AI_Turret> Turrets = new List<Obj_AI_Turret>();
        public static int[] AbilitySequence;
        public static int QOff = 0, WOff = 0, EOff = 0, ROff = 0;
        public static GameObject TibbersObject { get; set; }

        public static int GetPassiveBuff
        {
            get
            {
                var data = Player.Instance.Buffs
                    .FirstOrDefault(b => b.DisplayName == "Pyromania");

                return data != null ? data.Count : 0;
            }
        }

        private static Vector3 MousePos
        {
            get { return Game.CursorPos; }
        }

        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

        public static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }

        public static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Annie")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 625);
            W = new Spell.Skillshot(SpellSlot.W, 550, SkillShotType.Cone, 500, int.MaxValue, 80);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Skillshot(SpellSlot.R, 600, SkillShotType.Circular, 200, int.MaxValue, 251);
            Zhonia = new Item((int)ItemId.Zhonyas_Hourglass);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            AbilitySequence = new[] { 1, 2, 1, 2, 3, 4, 1, 1, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
            Exhaust = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerexhaust"), 650);
            var FlashSlot = Annie.GetSpellSlotFromName("summonerflash");
            Flash = new Spell.Skillshot(FlashSlot, 32767, SkillShotType.Linear);

            AnnieMenu = MainMenu.AddMenu("BloodimirAnnie", "bloodimirannie");
            AnnieMenu.AddGroupLabel("Bloodimir.Annie");
            AnnieMenu.AddSeparator();
            AnnieMenu.AddLabel("Bloodimir Annie V1.0.1.0");

            ComboMenu = AnnieMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E "));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
            ComboMenu.Add("comboOnlyExhaust", new CheckBox("Use Exhaust (Combo Only)"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 2, 0, 5));
            ComboMenu.AddSeparator();
            ComboMenu.Add("flashr", new KeyBind("Flash R", false, KeyBind.BindTypes.HoldActive, 'Y'));
            ComboMenu.Add("flasher", new KeyBind("Ninja Flash E+R", false, KeyBind.BindTypes.HoldActive, 'N'));
            ComboMenu.Add("waitAA", new CheckBox("wait for AA to finish", false));

            DrawMenu = AnnieMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q Range"));
            DrawMenu.Add("draww", new CheckBox("Draw W Range"));
            DrawMenu.Add("drawr", new CheckBox("Draw R Range"));
            DrawMenu.Add("drawaa", new CheckBox("Draw AA Range"));
            DrawMenu.Add("drawtf", new CheckBox("Draw Tibbers Flash Range"));

            LastHit = AnnieMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q"));

            LaneJungleClear = AnnieMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));
            LaneJungleClear.Add("LCW", new CheckBox("Use W"));

            MiscMenu = AnnieMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("MISC");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS using Q"));
            MiscMenu.Add("ksw", new CheckBox("KS using W"));
            MiscMenu.Add("ksr", new CheckBox("KS using R"));
            MiscMenu.Add("ksignite", new CheckBox("KS using Ignite"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("estack", new CheckBox("Stack Passive E", false));
            MiscMenu.Add("wstack", new CheckBox("Stack Passive W ", false));
            MiscMenu.Add("useexhaust", new CheckBox("Use Exhaust"));
            foreach (var source in ObjectManager.Get<AIHeroClient>().Where(a => a.IsEnemy))
            {
                MiscMenu.Add(source.ChampionName + "exhaust",
                    new CheckBox("Exhaust " + source.ChampionName, false));
            }
            MiscMenu.AddSeparator();
            MiscMenu.Add("zhonias", new CheckBox("Use Zhonia"));
            MiscMenu.Add("zhealth", new Slider("Auto Zhonia Health %", 8));
            MiscMenu.AddSeparator();
            MiscMenu.Add("gapclose", new CheckBox("Gapcloser with Stun"));
            MiscMenu.Add("eaa", new CheckBox("Auto E on enemy AA's"));
            MiscMenu.Add("support", new CheckBox("Support Mode", false));
            MiscMenu.Add("lvlup", new CheckBox("Auto Level Up Spells"));


            SkinMenu = AnnieMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("skinid", new Slider("Skin", 8, 0, 9));
            var skinid = new[]
            {
                "Default", "Goth", "Red Riding", "Annie in Wonderland", "Prom Queen", "Frostfire", "Franken Tibbers",
                "Reverse", "Panda", "Sweetheart"
            };
            skinchange.DisplayName = skinid[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = skinid[changeArgs.NewValue];
                };
            Interrupter.OnInterruptableSpell += Interruptererer;
            Game.OnUpdate += Tick;
            Drawing.OnDraw += OnDraw;
            Gapcloser.OnGapcloser += OnGapClose;
            Obj_AI_Base.OnProcessSpellCast += Auto_EOnProcessCastSpell;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Orbwalker.OnPreAttack += Support_Orbwalker;
            Core.DelayAction(Combo, 1);
            Core.DelayAction(TibbersFlash, 10);
        }

        private static void Interruptererer(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var qintTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (Annie.HasBuff("pyromania_particle"))
            {
                if (Q.IsReady() && sender.IsValidTarget(Q.Range) && MiscMenu["int"].Cast<CheckBox>().CurrentValue)
                    Q.Cast(qintTarget);
                var wintTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                if (Annie.HasBuff("pyromania_particle"))
                    if (!Q.IsReady() && W.IsReady() && sender.IsValidTarget(W.Range) &&
                        MiscMenu["int"].Cast<CheckBox>().CurrentValue)
                        W.Cast(wintTarget);
            }
        }

        private static
            void OnGapClose
            (AIHeroClient Sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!gapcloser.Sender.IsEnemy)
                return;
            var gapclose = MiscMenu["gapclose"].Cast<CheckBox>().CurrentValue;
            if (!gapclose)
                return;
            if (Player.HasBuff("pyromania_particle"))
            {
                if (Q.IsReady()
                    && Q.IsInRange(gapcloser.Start))
                {
                    Q.Cast(gapcloser.Start);
                }

                if (W.IsReady() && W.IsInRange(gapcloser.Start))
                {
                    W.Cast(gapcloser.Start);
                }
            }
        }

        private static void LevelUpSpells()
        {
            var qL = Annie.Spellbook.GetSpell(SpellSlot.Q).Level + QOff;
            var wL = Annie.Spellbook.GetSpell(SpellSlot.W).Level + WOff;
            var eL = Annie.Spellbook.GetSpell(SpellSlot.E).Level + EOff;
            var rL = Annie.Spellbook.GetSpell(SpellSlot.R).Level + ROff;
            if (qL + wL + eL + rL < ObjectManager.Player.Level)
            {
                int[] level = { 0, 0, 0, 0 };
                for (var i = 0; i < ObjectManager.Player.Level; i++)
                {
                    level[AbilitySequence[i] - 1] = level[AbilitySequence[i] - 1] + 1;
                }
                if (qL < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (wL < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (eL < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (rL < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Annie.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Annie.Position, Q.Range, Color.Red);
                }
                if (DrawMenu["draww"].Cast<CheckBox>().CurrentValue && W.IsLearned)
                {
                    Drawing.DrawCircle(Annie.Position, W.Range, Color.DarkGreen);
                }
                if (DrawMenu["drawr"].Cast<CheckBox>().CurrentValue && R.IsLearned)
                {
                    Drawing.DrawCircle(Annie.Position, R.Range, Color.Purple);
                }
                if (DrawMenu["drawaa"].Cast<CheckBox>().CurrentValue)
                {
                    Drawing.DrawCircle(Annie.Position, 633, Color.DarkSlateGray);
                }
                if (DrawMenu["drawtf"].Cast<CheckBox>().CurrentValue && R.IsLearned)
                {
                    Drawing.DrawCircle(Annie.Position, R.Range + 425, Color.DarkBlue);
                }
            }
        }

        private static void Pyrostack()
        {
            var stacke = MiscMenu["estack"].Cast<CheckBox>().CurrentValue;
            var stackw = MiscMenu["wstack"].Cast<CheckBox>().CurrentValue;

            if (Player.HasBuff("pyromania_particle"))
                return;
            if (stacke && E.IsReady())
            {
                E.Cast();
            }

            if (stackw && W.IsReady())
            {
                W.Cast(MousePos);
            }
        }

        public static void Flee()
        {
            Orbwalker.MoveTo(MousePos);
            E.Cast();
        }

        private static void Support_Orbwalker(AttackableUnit target, Orbwalker.PreAttackArgs args)
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

        private static void Tick(EventArgs args)
        {
            Pyrostack();
            Zhonya();
            Killsteal();
            SkinChange();
            MoveTibbers();
            if (MiscMenu["lvlup"].Cast<CheckBox>().CurrentValue) LevelUpSpells();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                    LaneJungleClearA.LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHitA.LastHitB();
            }
            if (ComboMenu["flashr"].Cast<KeyBind>().CurrentValue
                || ComboMenu["flasher"].Cast<KeyBind>().CurrentValue)
            {
                TibbersFlash();
            }
            if (!ComboMenu["useignite"].Cast<CheckBox>().CurrentValue ||
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
            foreach (
                var source in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(
                            a =>
                                a.IsEnemy && a.IsValidTarget(Ignite.Range) &&
                                a.Health < 50 + 20 * Annie.Level - (a.HPRegenRate / 5 * 3)))
            {
                Ignite.Cast(source);
                return;
            }
            if (!MiscMenu["useexhaust"].Cast<CheckBox>().CurrentValue ||
                ComboMenu["comboOnlyExhaust"].Cast<CheckBox>().CurrentValue &&
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                return;
            foreach (
                var enemy in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(a => a.IsEnemy && a.IsValidTarget(Exhaust.Range))
                        .Where(enemy => MiscMenu[enemy.ChampionName + "exhaust"].Cast<CheckBox>().CurrentValue))
            {
                if (enemy.IsFacing(Annie))
                {
                    if (!(Annie.HealthPercent < 50)) continue;
                    Exhaust.Cast(enemy);
                    return;
                }
                if (!(enemy.HealthPercent < 50)) continue;
                Exhaust.Cast(enemy);
                return;
            }
        }

        private static void Auto_EOnProcessCastSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (MiscMenu["eaa"].Cast<CheckBox>().CurrentValue &&
                sender.IsEnemy
                && args.SData.IsAutoAttack()
                && args.Target.IsMe)
            {
                E.Cast();
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
                    if (Annie.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                    {
                        {
                            Q.Cast(qtarget);
                        }
                        if (MiscMenu["ksw"].Cast<CheckBox>().CurrentValue && W.IsReady())
                        {
                            foreach (
                                var wtarget in
                                    EntityManager.Heroes.Enemies.Where(
                                        hero =>
                                            hero.IsValidTarget(W.Range) && !hero.IsDead && !hero.IsZombie))
                            {
                                if (Annie.GetSpellDamage(wtarget, SpellSlot.W) >= wtarget.Health)
                                    W.Cast(wtarget.ServerPosition);
                            }
                        }

                        if (MiscMenu["ksr"].Cast<CheckBox>().CurrentValue && R.IsReady())
                        {
                            foreach (
                                var rtarget in
                                    EntityManager.Heroes.Enemies.Where(
                                        hero =>
                                            hero.IsValidTarget(R.Range) && !hero.IsDead &&
                                            !hero.IsZombie))
                            {
                                if (Annie.GetSpellDamage(rtarget, SpellSlot.R) >= rtarget.Health)
                                    R.Cast(rtarget.ServerPosition);
                            }
                            if (MiscMenu["ksignite"].Cast<CheckBox>().CurrentValue && Ignite.IsReady())
                                foreach (
                                    var source in
                                        ObjectManager.Get<AIHeroClient>()
                                            .Where(
                                                a =>
                                                    a.IsEnemy && a.IsValidTarget(Ignite.Range) &&
                                                    a.Health < 50 + 20 * Annie.Level - (a.HPRegenRate / 5 * 3)))
                                {
                                    Ignite.Cast(source);
                                    return;
                                }
                            {
                            }
                        }
                    }
                }
            }
        }

        private static void Zhonya()
        {
            var zhoniaon = MiscMenu["zhonias"].Cast<CheckBox>().CurrentValue;
            var zhealth = MiscMenu["zhealth"].Cast<Slider>().CurrentValue;

            if (zhoniaon && Zhonia.IsReady() && Zhonia.IsOwned())
            {
                if (Annie.HealthPercent <= zhealth)
                {
                    Zhonia.Cast();
                }
            }
        }

        private static void TibbersFlash()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);

            var target = TargetSelector.GetTarget(R.Range + 425, DamageType.Magical);
            if (target == null) return;
            var xpos = target.Position.Extend(target, 610);

            if (!R.IsReady() || GetPassiveBuff == 1 || GetPassiveBuff == 2)
            {
                Combo();
            }

            var predrpos = R.GetPrediction(target);
            if (ComboMenu["flashr"].Cast<KeyBind>().CurrentValue)
            {
                if (GetPassiveBuff == 4 && Flash.IsReady() && R.IsReady() && E.IsReady())
                    if (target.IsValidTarget(R.Range + 425))
                    {
                        Flash.Cast((Vector3)xpos);
                        R.Cast(predrpos.CastPosition);
                    }
            }

            if (ComboMenu["flasher"].Cast<KeyBind>().CurrentValue)
            {
                if (GetPassiveBuff == 3 && Flash.IsReady() && R.IsReady() && E.IsReady())
                {
                    E.Cast();
                }
                if (Annie.HasBuff("pyromania_particle"))
                    if (target.IsValidTarget(R.Range + 425))
                    {
                        Flash.Cast((Vector3)xpos);
                        R.Cast(predrpos.CastPosition);
                    }
            }
        }

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "tibbers")
            {
                TibbersObject = sender;
            }
        }

        public static Obj_AI_Turret GetTurrets()
        {
            var turret =
                EntityManager.Turrets.Enemies.OrderBy(
                    x => x.Distance(TibbersObject.Position) <= 500 && !x.IsAlly && !x.IsDead)
                    .FirstOrDefault();
            return turret;
        }

        private static void MoveTibbers()
        {
            var target = TargetSelector.GetTarget(2000, DamageType.Magical);

            if (Player.HasBuff("infernalguardiantime"))
            {
                Player.IssueOrder(GameObjectOrder.MovePet,
                    target.IsValidTarget(1500) ? target.Position : GetTurrets().Position);
            }
        }

        public static
            void Combo
            ()
        {
            var target = TargetSelector.GetTarget(700, DamageType.Magical);
            if (target == null || !target.IsValid())
            {
                return;
            }

            if (Orbwalker.IsAutoAttacking && ComboMenu["waitAA"].Cast<CheckBox>().CurrentValue)
                return;
            if (ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue)
            {
                Q.Cast(target);
            }
            if (ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue)
                if (W.IsReady())
                {
                    var predW = W.GetPrediction(target).CastPosition;
                    if (target.CountEnemiesInRange(W.Range) >= 1)
                        W.Cast(predW);
                }
            if (ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue)
                if (R.IsReady())
                {
                    var predR = R.GetPrediction(target).CastPosition;
                    if (target.CountEnemiesInRange(R.Width) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue)
                        R.Cast(predR);
                }
            if (ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue)
                if (E.IsReady())
                {
                    if (Annie.CountEnemiesInRange(Q.Range) >= 1)
                        E.Cast();
                }
        }

        private static
            void SkinChange()
        {
            var style = SkinMenu["skinid"].DisplayName;
            switch (style)
            {
                case "Default":
                    Player.SetSkinId(0);
                    break;
                case "Goth":
                    Player.SetSkinId(1);
                    break;
                case "Red Riding":
                    Player.SetSkinId(2);
                    break;
                case "Annie in Wonderland":
                    Player.SetSkinId(3);
                    break;
                case "Prom Queen":
                    Player.SetSkinId(4);
                    break;
                case "Frostfire":
                    Player.SetSkinId(5);
                    break;
                case "Franken Tibbers":
                    Player.SetSkinId(6);
                    break;
                case "Reverse":
                    Player.SetSkinId(7);
                    break;
                case "Panda":
                    Player.SetSkinId(8);
                    break;
                case "Sweetheart":
                    Player.SetSkinId(9);
                    break;
            }
        }
    }
}