using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Kennen
{
    internal class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;
        public static Menu KennenMenu, ComboMenu, DrawMenu, SkinMenu, MiscMenu, LaneJungleClear, LastHit;
        public static AIHeroClient Kennen = ObjectManager.Player;

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
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
            if (Player.Instance.ChampionName != "Kennen")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 1050, SkillShotType.Linear, (int) 250f, (int) 1700f, (int) 50f);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Active(SpellSlot.R, 565);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

            KennenMenu = MainMenu.AddMenu("BloodimirKennen", "bloodimirkennen");
            KennenMenu.AddGroupLabel("Bloodimir.Kennen");
            KennenMenu.AddSeparator();
            KennenMenu.AddLabel("Bloodimir Kennen V1.0.1.0");

            ComboMenu = KennenMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E "));
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 2, 0, 5));

            DrawMenu = KennenMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("draww", new CheckBox("Draw W"));

            LaneJungleClear = KennenMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCW", new CheckBox("Use W"));
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));

            LastHit = KennenMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q"));
            LastHit.Add("LHW", new CheckBox("Use W"));

            MiscMenu = KennenMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("KS");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS using Q"));
            MiscMenu.Add("ksw", new CheckBox("KS using W"));
            MiscMenu.Add("int", new CheckBox("TRY to Interrupt spells"));

            SkinMenu = KennenMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("skinid", new Slider("Skin", 1, 0, 5));
            var skinid = new[] {"Default", "Deadly", "Swamp Master", "Karate", "Doctor", "Arctic Ops"};
            skinchange.DisplayName = skinid[skinchange.CurrentValue];
            skinchange.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = skinid[changeArgs.NewValue];
                if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
                {
                    Chat.Print("skin-changed");
                }
            };
            Interrupter.OnInterruptableSpell += Interruptererer;
            Game.OnTick += Tick;
            Drawing.OnDraw += OnDraw;
        }

        private static void Interruptererer(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (intTarget.HasBuff("kennenmarkofstorm"))
            {
                if (Q.IsReady() && sender.IsValidTarget(Q.Range) && MiscMenu["int"].Cast<CheckBox>().CurrentValue)
                    Q.Cast(intTarget.ServerPosition);
                if (W.IsReady() && sender.IsValidTarget(W.Range))
                    W.Cast();
                if (E.IsReady() && sender.IsValidTarget(E.Range))
                    E.Cast();
                Orbwalker.DisableMovement = Kennen.HasBuff("KennenLightningRush");
                Player.IssueOrder(GameObjectOrder.MoveTo, intTarget);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Kennen.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Kennen.Position, Q.Range, Color.DarkBlue);
                }
                if (DrawMenu["draww"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Kennen.Position, W.Range, Color.DarkGreen);
                }
            }
        }

        public static void Flee()
        {
            Orbwalker.MoveTo(Game.CursorPos);
            E.Cast();
        }

        private static void Tick(EventArgs args)
        {
            Killsteal();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
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
                                    a.Health < 50 + 20*Kennen.Level - (a.HPRegenRate/5*3)))
                {
                    Ignite.Cast(source);
                    return;
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Combo.KennenCombo();
                    Rincombo(ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
                    Eincombo(ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue);
                }
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                        Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                    {
                        LaneJungleClearA.LaneClear();
                    }
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                    {
                        LastHitA.LastHitB();
                    }
                    SkinChange();
                }
            }
        }

        public static
            void Rincombo
            (bool
                useR)
        {
            if (ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue)
                if (useR && R.IsReady() &&
                    Kennen.CountEnemiesInRange(R.Range) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue)
                {
                    R.Cast();
                }
        }

        public static
            void Eincombo(bool useE)
        {
            if (ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue)
                if (useE && E.IsReady() && Kennen.CountEnemiesInRange(W.Range) >= 2)
                {
                    E.Cast();
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
                        if (Kennen.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                        {
                            var poutput = Q.GetPrediction(qtarget);
                            if (poutput.HitChance >= HitChance.Medium)
                            {
                                Q.Cast(poutput.CastPosition);
                            }
                            if (MiscMenu["ksw"].Cast<CheckBox>().CurrentValue && W.IsReady())
                            {
                                {
                                    try
                                    {
                                        foreach (
                                            var wtarget in
                                                EntityManager.Heroes.Enemies.Where(
                                                    hero =>
                                                        hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
                                        {
                                            if (Kennen.GetSpellDamage(wtarget, SpellSlot.W) >= wtarget.Health)
                                                if (wtarget.HasBuff("kennenmarkofstorm"))
                                                {
                                                    W.Cast();
                                                }
                                        }
                                    }
                                    catch
                                    {
                                    }
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

        private static
            void SkinChange()
        {
            var style = SkinMenu["skinid"].DisplayName;
            switch (style)
            {
                case "Default":
                    Player.SetSkinId(0);
                    break;
                case "Deadly":
                    Player.SetSkinId(1);
                    break;
                case "Swamp Master":
                    Player.SetSkinId(2);
                    break;
                case "Karate":
                    Player.SetSkinId(3);
                    break;
                case "Doctor":
                    Player.SetSkinId(4);
                    break;
                case "Arctic Ops":
                    Player.SetSkinId(5);
                    break;
            }
        }
    }
}