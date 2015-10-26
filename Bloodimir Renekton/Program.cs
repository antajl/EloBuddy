using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using Color = System.Drawing.Color;

namespace Bloodimir_Renekton
{
    internal class Program
    {
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;
        public static Item Hydra;
        public static Item Tiamat;
        public static Menu RenekMenu, ComboMenu, SkinMenu, MiscMenu, DrawMenu, HarassMenu, LaneJungleClear, LastHit;
        public static Item Bilgewater, Youmuu, Botrk;
        public static AIHeroClient Renek = ObjectManager.Player;
        public static int[] AbilitySequence;
        public static int QOff = 0, WOff = 0, EOff = 0, ROff = 0;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

        public static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }
        private const string E2BuffName = "renektonsliceanddicedelay";
        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Renekton")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q, 225);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 450, SkillShotType.Linear);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            R = new Spell.Active(SpellSlot.R);
            Tiamat = new Item((int) ItemId.Tiamat_Melee_Only, 420);
            Hydra = new Item((int) ItemId.Ravenous_Hydra_Melee_Only, 420);
            Botrk = new Item(3153, 550f);
            Bilgewater = new Item(3144, 475f);
            Youmuu = new Item(3142, 10);
            AbilitySequence = new [] { 2, 1, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 };

            RenekMenu = MainMenu.AddMenu("BloodimiRenekton", "bloodimirrenekton");
            RenekMenu.AddGroupLabel("Bloodimir.enekton");
            RenekMenu.AddSeparator();
            RenekMenu.AddLabel("BloodimiRenekton v1.0.1.0");

            ComboMenu = RenekMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
            ComboMenu.Add("useitems", new CheckBox("Use Items"));
            ComboMenu.Add("autoult", new CheckBox("Auto Ult"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Health Percentage to Ult", 31));

            LaneJungleClear = RenekMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCE", new CheckBox("Use E"));
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));
            LaneJungleClear.Add("LCW", new CheckBox("Use W"));
            LaneJungleClear.Add("LCI", new CheckBox("Use Items"));

            DrawMenu = RenekMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("drawe", new CheckBox("Draw E"));

            LastHit = RenekMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q"));
            LastHit.Add("LHW", new CheckBox("Use W"));
            LastHit.Add("LHI", new CheckBox("Use Items"));
            
            HarassMenu = RenekMenu.AddSubMenu("Harass Menu", "harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("hq", new CheckBox("Harass Q"));
            HarassMenu.Add("hw", new CheckBox("Harass W"));
            HarassMenu.Add("hi", new CheckBox("Harass Items"));

            MiscMenu = RenekMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("KS");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS with Q"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("intw", new CheckBox("W to Interrupt"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("gapclose", new CheckBox("W to Interrupt"));
            MiscMenu.Add("lvlup", new CheckBox("Auto Level Up Spells", false));

            SkinMenu = RenekMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sid", new Slider("Skin", 5, 0, 7));
            var sid = new[]
            {"Classic", "Galactic", "Outback", "Bloodfury", "Rune Wars", "Scorched Earth", "Pool Party", "Prehistoric"};
            skinchange.DisplayName = sid[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sid[changeArgs.NewValue];
                };

            Game.OnTick += Tick;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            Gapcloser.OnGapcloser += OnGapClose;
        }

        public static void Flee()
        {
            Orbwalker.MoveTo(Game.CursorPos);
            E.Cast(Game.CursorPos);
        }
        private static void LevelUpSpells()
        {
            var qL = Renek.Spellbook.GetSpell(SpellSlot.Q).Level + QOff;
            var wL = Renek.Spellbook.GetSpell(SpellSlot.W).Level + WOff;
            var eL = Renek.Spellbook.GetSpell(SpellSlot.E).Level + EOff;
            var rL = Renek.Spellbook.GetSpell(SpellSlot.R).Level + ROff;
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
         private static
            void OnGapClose
            (AIHeroClient Sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!gapcloser.Sender.IsEnemy)
                return;
            var gapclose = MiscMenu["gapclose"].Cast<CheckBox>().CurrentValue;
            if (!gapclose)
                return;

                if (W.IsReady() && W.IsInRange(gapcloser.Start))
                {
                    W.Cast(gapcloser.Start);
                }
            }
        private static void OnDraw(EventArgs args)
        {
            if (!Renek.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Renek.Position, 225, Color.DarkGoldenrod);
                }
                if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Renek.Position, E.Range, Color.DarkCyan);
                }
            }
        }

        private static void Tick(EventArgs args)
        {
            Killsteal();
            SkinChange();
            if (MiscMenu["lvlup"].Cast<CheckBox>().CurrentValue) LevelUpSpells();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo.RenekCombo();
                Combo.Items();
                Combo.UseE();
                AutoUlt(ComboMenu["autoult"].Cast<CheckBox>().CurrentValue);
            }
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    LaneJungleClearA.LaneClear();
                    LaneJungleClearA.Items();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    Harass();
                    }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    LastHitA.LastHitB();
                    LastHitA.Items();
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
                                        a.Health < 50 + 20*Renek.Level - (a.HPRegenRate/5*3)))
                    {
                        Ignite.Cast(source);
                        return;
                    }
                }
            }
        }

        public static void AutoUlt(bool useR)
        {
            var autoR = ComboMenu["autoult"].Cast<CheckBox>().CurrentValue;
            var healthAutoR = ComboMenu["rslider"].Cast<Slider>().CurrentValue;
            if (autoR && _Player.HealthPercent < healthAutoR)
            {
                R.Cast();
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
        public static void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target != null && Player.Instance.Distance(target.Position) < E.Range && !Renek.HasBuff(E2BuffName) && E.IsReady())
                {
                    Player.CastSpell(SpellSlot.E, target.Position);
                }
        if (Renek.HasBuff(E2BuffName))
        { 
            var qtarget = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (qtarget.Distance(Renek.Position) <= 225 && Q.IsReady() && HarassMenu["hq"].Cast<CheckBox>().CurrentValue)
            Q.Cast();
            var wtarget = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (wtarget.Distance(Renek.Position) <= W.Range && HarassMenu["hw"].Cast<CheckBox>().CurrentValue)
            W.Cast();
            var itarget = TargetSelector.GetTarget(Hydra.Range, DamageType.Physical);
            if (itarget.Distance(Renek.Position) <= Hydra.Range && HarassMenu["hi"].Cast<CheckBox>().CurrentValue)
                Hydra.Cast();
            if (itarget.Distance(Renek.Position) <= Tiamat.Range && HarassMenu["hi"].Cast<CheckBox>().CurrentValue)
                Tiamat.Cast();
            if (Renek.HasBuff(E2BuffName) && E.IsReady())
            {
                if (!W.IsReady() || !Q.IsReady())
                {
                    Player.CastSpell(SpellSlot.E, qtarget.Position);
                }
            }
            }
        }
        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) ||
                (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                 (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))))
            {
                var e = target as AIHeroClient;
                if (ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue && W.IsReady() && e.IsEnemy)
                    if (target != null)
                    {
                        if (e.IsValidTarget() && W.IsReady())
                        {
                            W.Cast();
                        }
                        if (e.IsValidTarget() && Hydra.IsReady())
                        {
                            Hydra.Cast();
                        }
                        if (e.IsValidTarget() && Tiamat.IsReady())
                        {
                            Tiamat.Cast();
                        }
                    }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            {
                if (W.IsReady() && sender.IsValidTarget(Player.Instance.GetAutoAttackRange()) && MiscMenu["intw"].Cast<CheckBox>().CurrentValue)
                    W.Cast(sender);
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
                        if (Renek.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)

                            Q.Cast();
                    }
                }
            }
        

        private static void SkinChange()
        {
            var style = SkinMenu["sid"].DisplayName;
            switch (style)
            {
                case "Classic":
                    Player.SetSkinId(0);
                    break;
                case "Galactic":
                    Player.SetSkinId(1);
                    break;
                case "Outback":
                    Player.SetSkinId(2);
                    break;
                case "Bloodfury":
                    Player.SetSkinId(3);
                    break;
                case "Rune Wars":
                    Player.SetSkinId(4);
                    break;
                case "Scorched Earth":
                    Player.SetSkinId(5);
                    break;
                case "Pool Party":
                    Player.SetSkinId(6);
                    break;
                case "Prehistoric":
                    Player.SetSkinId(7);
                    break;
            }
        }
    }
}