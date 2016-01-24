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

namespace Kennen
{
    internal static class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        private static Spell.Active _r;
        private static Spell.Targeted _ignite;
        private static Menu _kennenMenu;
        public static Menu ComboMenu;
        private static Menu _drawMenu;
        private static Menu _skinMenu;
        private static Menu _miscMenu;
        public static Menu LaneJungleClear, LastHit;
        private static readonly AIHeroClient Kennen = ObjectManager.Player;
        private static readonly int LastE = 0;

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
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
            if (Player.Instance.ChampionName != "Kennen")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 1050, SkillShotType.Linear, 250, 1700, 50);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E);
            _r = new Spell.Active(SpellSlot.R, 565);
            if (HasSpell("summonerdot"))
                _ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

            _kennenMenu = MainMenu.AddMenu("BloodimirKennen", "bloodimirkennen");
            _kennenMenu.AddGroupLabel("Bloodimir.Kennen");
            _kennenMenu.AddSeparator();
            _kennenMenu.AddLabel("Bloodimir Kennen V1.0.1.0");

            ComboMenu = _kennenMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E "));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite "));
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 2, 0, 5));

            _drawMenu = _kennenMenu.AddSubMenu("Drawings", "drawings");
            _drawMenu.AddGroupLabel("Drawings");
            _drawMenu.AddSeparator();
            _drawMenu.Add("drawq", new CheckBox("Draw Q"));
            _drawMenu.Add("draww", new CheckBox("Draw W"));

            LaneJungleClear = _kennenMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCW", new CheckBox("Use W"));
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));

            LastHit = _kennenMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q"));
            LastHit.Add("LHW", new CheckBox("Use W"));

            _miscMenu = _kennenMenu.AddSubMenu("Misc Menu", "miscmenu");
            _miscMenu.AddGroupLabel("KS");
            _miscMenu.AddSeparator();
            _miscMenu.Add("ksq", new CheckBox("KS using Q"));
            _miscMenu.Add("ksw", new CheckBox("KS using W"));
            _miscMenu.Add("int", new CheckBox("TRY to Interrupt spells"));

            _skinMenu = _kennenMenu.AddSubMenu("Skin Changer", "skin");
            _skinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = _skinMenu.Add("skinid", new Slider("Skin", 1, 0, 5));
            var skinid = new[] {"Default", "Deadly", "Swamp Master", "Karate", "Doctor", "Arctic Ops"};
            skinchange.DisplayName = skinid[skinchange.CurrentValue];
            skinchange.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = skinid[changeArgs.NewValue];
                if (_miscMenu["debug"].Cast<CheckBox>().CurrentValue)
                {
                    Chat.Print("skin-changed");
                }
            };
            Interrupter.OnInterruptableSpell += Interruptererer;
            Game.OnUpdate += Tick;
            Drawing.OnDraw += OnDraw;
        }

        private static void Interruptererer(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (!intTarget.HasBuff("kennenmarkofstorm") && intTarget.CountEnemiesInRange(500) >= 2) return;
            if (Q.IsReady() && sender.IsValidTarget(Q.Range) && _miscMenu["int"].Cast<CheckBox>().CurrentValue)
                Q.Cast(intTarget.ServerPosition);
            if (W.IsReady() && sender.IsValidTarget(W.Range))
                W.Cast();
            if (E.IsReady() && sender.IsValidTarget(E.Range))
                E.Cast();
            Orbwalker.DisableMovement = Kennen.HasBuff("KennenLightningRush");
            Player.IssueOrder(GameObjectOrder.MoveTo, intTarget);
        }

        private static void OnDraw(EventArgs args)
        {
            if (Kennen.IsDead) return;
            if (_drawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
            {
                Circle.Draw(Color.Red, Q.Range, Player.Instance.Position);
            }
            if (_drawMenu["draww"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
            {
                Circle.Draw(Color.DarkGreen, W.Range, Player.Instance.Position);
            }
        }

        private static void Flee()
        {
            Orbwalker.MoveTo(Game.CursorPos);
            E.Cast();
            if (!Player.Instance.HasBuff("KennenLightingRushBuff")) return;
            if (Environment.TickCount - LastE >= 1950)
            {
                E.Cast();
            }
        }

        private static void Tick(EventArgs args)
        {
            Killsteal();
            SkinChange();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
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
                {
                    if (!ComboMenu["useignite"].Cast<CheckBox>().CurrentValue ||
                        !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
                    foreach (
                        var source in
                            ObjectManager.Get<AIHeroClient>()
                                .Where(
                                    a =>
                                        a.IsEnemy && a.IsValidTarget(_ignite.Range) &&
                                        a.Health < 50 + 20*Kennen.Level - (a.HPRegenRate/5*3)))
                    {
                        _ignite.Cast(source);
                        return;
                    }
                }
            }
        }

        private static
            void Rincombo
            (bool
                useR)
        {
            if (!ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue) return;
            if (useR && _r.IsReady() &&
                Kennen.CountEnemiesInRange(_r.Range) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue)
            {
                _r.Cast();
            }
        }

        private static
            void Eincombo(bool useE)
        {
            if (!ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue) return;
            if (useE && E.IsReady() && Kennen.CountEnemiesInRange(W.Range) >= 2)
            {
                E.Cast();
            }
        }

        private static void Killsteal()
        {
            if (!_miscMenu["ksq"].Cast<CheckBox>().CurrentValue || !Q.IsReady()) return;
            try
            {
                foreach (var poutput in from qtarget in EntityManager.Heroes.Enemies.Where(
                    hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie)
                    where Kennen.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health
                    select Q.GetPrediction(qtarget))
                {
                    if (poutput.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(poutput.CastPosition);
                    }
                    if (!_miscMenu["ksw"].Cast<CheckBox>().CurrentValue || !W.IsReady()) continue;
                    {
                        try
                        {
                            foreach (var wtarget in EntityManager.Heroes.Enemies.Where(
                                hero =>
                                    hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie)
                                .Where(wtarget => Kennen.GetSpellDamage(wtarget, SpellSlot.W) >= wtarget.Health)
                                .Where(wtarget => wtarget.HasBuff("kennenmarkofstorm")))
                            {
                                W.Cast();
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
            void SkinChange()
        {
            var style = _skinMenu["skinid"].DisplayName;
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