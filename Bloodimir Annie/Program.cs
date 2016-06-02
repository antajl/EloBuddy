using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using extend = EloBuddy.SDK.Extensions;

namespace Bloodimir_Annie
{
    internal static class Program
    {
        public static Spell.Targeted Q;
        private static Spell.Targeted _exhaust;
        public static Spell.Skillshot W;
        private static Spell.Skillshot _r;
        private static Spell.Skillshot _flash;
        private static Spell.Active _e;
        private static Menu _annieMenu;
        private static Menu _comboMenu;
        private static Menu _drawMenu;
        private static Menu _skinMenu;
        private static Menu _miscMenu;
        public static Menu LaneJungleClear, LastHit;
        private static Item _zhonia;
        private static AIHeroClient Annie = ObjectManager.Player;
        public static List<Obj_AI_Turret> Turrets = new List<Obj_AI_Turret>();
        private static int[] _abilitySequence;
        public static int QOff = 0, WOff = 0, EOff = 0, ROff = 0;
        private static GameObject TibbersObject { get; set; }

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

        private static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }

        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Annie")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 625);
            W = new Spell.Skillshot(SpellSlot.W, 550, SkillShotType.Cone, 500, int.MaxValue, 80);
            _e = new Spell.Active(SpellSlot.E);
            _r = new Spell.Skillshot(SpellSlot.R, 600, SkillShotType.Circular, 200, int.MaxValue, 251);
            _zhonia = new Item((int) ItemId.Zhonyas_Hourglass);
            _abilitySequence = new[] {1, 2, 1, 2, 3, 4, 1, 1, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};
            _exhaust = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerexhaust"), 650);
            var flashSlot = Annie.GetSpellSlotFromName("summonerflash");
            _flash = new Spell.Skillshot(flashSlot, 32767, SkillShotType.Linear);

            _annieMenu = MainMenu.AddMenu("BloodimirAnnie", "bloodimirannie");
            _annieMenu.AddGroupLabel("Bloodimir.Annie");
            _annieMenu.AddSeparator();
            _annieMenu.AddLabel("Bloodimir Annie V1.0.1.0");

            _comboMenu = _annieMenu.AddSubMenu("Combo", "sbtw");
            _comboMenu.AddGroupLabel("Combo Settings");
            _comboMenu.AddSeparator();
            _comboMenu.Add("usecomboq", new CheckBox("Use Q"));
            _comboMenu.Add("usecombow", new CheckBox("Use W"));
            _comboMenu.Add("usecomboe", new CheckBox("Use E "));
            _comboMenu.Add("usecombor", new CheckBox("Use R"));
            _comboMenu.Add("pilot", new CheckBox("Auto Pilot Tibbers"));
            _comboMenu.Add("comboOnlyExhaust", new CheckBox("Use Exhaust (Combo Only)"));
            _comboMenu.AddSeparator();
            _comboMenu.Add("rslider", new Slider("Minimum people for R", 2, 0, 5));
            _comboMenu.AddSeparator();
            _comboMenu.Add("flashr", new KeyBind("Flash R", false, KeyBind.BindTypes.HoldActive, 'Y'));
            _comboMenu.Add("flasher", new KeyBind("Ninja Flash E+R", false, KeyBind.BindTypes.HoldActive, 'N'));
            _comboMenu.Add("waitAA", new CheckBox("wait for AA to finish", false));

            _drawMenu = _annieMenu.AddSubMenu("Drawings", "drawings");
            _drawMenu.AddGroupLabel("Drawings");
            _drawMenu.AddSeparator();
            _drawMenu.Add("drawq", new CheckBox("Draw Q Range"));
            _drawMenu.Add("draww", new CheckBox("Draw W Range"));
            _drawMenu.Add("drawr", new CheckBox("Draw R Range"));
            _drawMenu.Add("drawaa", new CheckBox("Draw AA Range"));
            _drawMenu.Add("drawtf", new CheckBox("Draw Tibbers Flash Range"));

            LastHit = _annieMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q"));
            LastHit.Add("PLHQ", new CheckBox("Don't use Q farm if have stun"));

            LaneJungleClear = _annieMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));
            LaneJungleClear.Add("LCW", new CheckBox("Use W"));
            LaneJungleClear.Add("PLCQ", new CheckBox("Don't use Q farm if have stun"));

            _miscMenu = _annieMenu.AddSubMenu("Misc Menu", "miscmenu");
            _miscMenu.AddGroupLabel("MISC");
            _miscMenu.AddSeparator();
            _miscMenu.Add("ksq", new CheckBox("KS using Q"));
            _miscMenu.Add("ksw", new CheckBox("KS using W"));
            _miscMenu.Add("ksr", new CheckBox("KS using R"));
            _miscMenu.AddSeparator();
            _miscMenu.Add("estack", new CheckBox("Stack Passive E", false));
            _miscMenu.Add("wstack", new CheckBox("Stack Passive W ", false));
            _miscMenu.Add("useexhaust", new CheckBox("Use Exhaust"));
            foreach (var source in ObjectManager.Get<AIHeroClient>().Where(a => a.IsEnemy))
            {
                _miscMenu.Add(source.ChampionName + "exhaust",
                    new CheckBox("Exhaust " + source.ChampionName, false));
            }
            _miscMenu.AddSeparator();
            _miscMenu.Add("zhonias", new CheckBox("Use Zhonia"));
            _miscMenu.Add("zhealth", new Slider("Auto Zhonia Health %", 8));
            _miscMenu.AddSeparator();
            _miscMenu.Add("gapclose", new CheckBox("Gapcloser with Stun"));
            _miscMenu.Add("eaa", new CheckBox("Auto E on enemy AA's"));
            _miscMenu.Add("support", new CheckBox("Support Mode", false));
            _miscMenu.Add("lvlup", new CheckBox("Auto Level Up Spells"));


            _skinMenu = _annieMenu.AddSubMenu("Skin Changer", "skin");
            _skinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = _skinMenu.Add("skinid", new Slider("Skin", 8, 0, 9));
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
            Obj_AI_Base.OnBasicAttack += Auto_EOnBasicAttack;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Orbwalker.OnPreAttack += Support_Orbwalker;
            Core.DelayAction(Combo, 1);
            Core.DelayAction(TibbersFlash, 10);
        }

        private static void Interruptererer(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var qintTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (!Annie.HasBuff("pyromania_particle")) return;
            if (Q.IsReady() && sender.IsValidTarget(Q.Range) && _miscMenu["int"].Cast<CheckBox>().CurrentValue)
                Q.Cast(qintTarget);
            var wintTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (!Annie.HasBuff("pyromania_particle")) return;
            if (!Q.IsReady() && W.IsReady() && sender.IsValidTarget(W.Range) &&
                _miscMenu["int"].Cast<CheckBox>().CurrentValue)
                W.Cast(wintTarget);
        }

        private static
            void OnGapClose
            (AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!gapcloser.Sender.IsEnemy)
                return;
            var gapclose = _miscMenu["gapclose"].Cast<CheckBox>().CurrentValue;
            if (!gapclose)
                return;
            if (!Player.HasBuff("pyromania_particle")) return;
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

        private static void LevelUpSpells()
        {
            var qL = Annie.Spellbook.GetSpell(SpellSlot.Q).Level + QOff;
            var wL = Annie.Spellbook.GetSpell(SpellSlot.W).Level + WOff;
            var eL = Annie.Spellbook.GetSpell(SpellSlot.E).Level + EOff;
            var rL = Annie.Spellbook.GetSpell(SpellSlot.R).Level + ROff;
            if (qL + wL + eL + rL >= ObjectManager.Player.Level) return;
            int[] level = {0, 0, 0, 0};
            for (var i = 0; i < ObjectManager.Player.Level; i++)
            {
                level[_abilitySequence[i] - 1] = level[_abilitySequence[i] - 1] + 1;
            }
            if (qL < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
            if (wL < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
            if (eL < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            if (rL < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
        }

        private static void OnDraw(EventArgs args)
        {
            if (Annie.IsDead) return;
            if (_drawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
            {
                Circle.Draw(Color.Red, Q.Range, Player.Instance.Position);
            }
            if (_drawMenu["draww"].Cast<CheckBox>().CurrentValue && W.IsLearned)
            {
                Circle.Draw(Color.DarkGreen, W.Range, Player.Instance.Position);
            }
            if (_drawMenu["drawr"].Cast<CheckBox>().CurrentValue && _r.IsLearned)
            {
                Circle.Draw(Color.Purple, _r.Range, Player.Instance.Position);
            }
            if (_drawMenu["drawaa"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.DarkSlateGray, 653, Player.Instance.Position);
            }
            if (_drawMenu["drawtf"].Cast<CheckBox>().CurrentValue && _r.IsLearned)
            {
                Circle.Draw(Color.DarkBlue, _r.Range + 425, Player.Instance.Position);
            }
        }

        private static void Pyrostack()
        {
            var stacke = _miscMenu["estack"].Cast<CheckBox>().CurrentValue;
            var stackw = _miscMenu["wstack"].Cast<CheckBox>().CurrentValue;

            if (Player.HasBuff("pyromania_particle"))
                return;
            if (stacke && _e.IsReady())
            {
                _e.Cast();
            }

            if (stackw && W.IsReady())
            {
                W.Cast(MousePos);
            }
        }

        private static void Flee()
        {
            Orbwalker.MoveTo(MousePos);
            _e.Cast();
        }

        private static void Support_Orbwalker(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) &&
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) &&
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) return;
            var t = target as Obj_AI_Minion;
            if (t == null) return;
            {
                if (_miscMenu["support"].Cast<CheckBox>().CurrentValue)
                    args.Process = false;
            }
        }

        private static void Tick(EventArgs args)
        {
            Pyrostack();
            Zhonya();
            Killsteal();
            SkinChange();
            if (_miscMenu["lvlup"].Cast<CheckBox>().CurrentValue) LevelUpSpells();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
                MoveTibbers();
            }
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                    LaneJungleClearA.LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHitA.LastHitB();
            }
            if (_comboMenu["flashr"].Cast<KeyBind>().CurrentValue
                || _comboMenu["flasher"].Cast<KeyBind>().CurrentValue)
            {
                TibbersFlash();
            }
            if (!_miscMenu["useexhaust"].Cast<CheckBox>().CurrentValue ||
                _comboMenu["comboOnlyExhaust"].Cast<CheckBox>().CurrentValue &&
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                return;
            foreach (
                var enemy in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(a => a.IsEnemy && a.IsValidTarget(_exhaust.Range))
                        .Where(enemy => _miscMenu[enemy.ChampionName + "exhaust"].Cast<CheckBox>().CurrentValue))
            {
                if (enemy.IsFacing(Annie))
                {
                    if (!(Annie.HealthPercent < 50)) continue;
                    _exhaust.Cast(enemy);
                    return;
                }
                if (!(enemy.HealthPercent < 50)) continue;
                _exhaust.Cast(enemy);
                return;
            } }
        

        private static void Auto_EOnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (_miscMenu["eaa"].Cast<CheckBox>().CurrentValue &&
                sender.IsEnemy
                && args.SData.IsAutoAttack()
                && args.Target.IsMe)
            {
                _e.Cast();
            }
        }

        private static void Killsteal()
        {
            if (!_miscMenu["ksq"].Cast<CheckBox>().CurrentValue || !Q.IsReady()) return;
            foreach (var qtarget in EntityManager.Heroes.Enemies.Where(
                hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie)
                .Where(qtarget => Annie.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health))
            {
                {
                    Q.Cast(qtarget);
                }
                if (_miscMenu["ksw"].Cast<CheckBox>().CurrentValue && W.IsReady())
                {
                    foreach (var wtarget in EntityManager.Heroes.Enemies.Where(
                        hero =>
                            hero.IsValidTarget(W.Range) && !hero.IsDead && !hero.IsZombie)
                        .Where(wtarget => Annie.GetSpellDamage(wtarget, SpellSlot.W) >= wtarget.Health))
                    {
                        W.Cast(wtarget.ServerPosition);
                    }
                }

                if (!_miscMenu["ksr"].Cast<CheckBox>().CurrentValue || !_r.IsReady()) continue;
                {
                    foreach (var rtarget in EntityManager.Heroes.Enemies.Where(
                        hero =>
                            hero.IsValidTarget(_r.Range) && !hero.IsDead &&
                            !hero.IsZombie)
                        .Where(rtarget => Annie.GetSpellDamage(rtarget, SpellSlot.R) >= rtarget.Health))
                    {
                        _r.Cast(rtarget.ServerPosition);
                    }
                }
            }
        }

        private static void Zhonya()
        {
            var zhoniaon = _miscMenu["zhonias"].Cast<CheckBox>().CurrentValue;
            var zhealth = _miscMenu["zhealth"].Cast<Slider>().CurrentValue;

            if (!zhoniaon || !_zhonia.IsReady() || !_zhonia.IsOwned()) return;
            if (Annie.HealthPercent <= zhealth)
            {
                _zhonia.Cast();
            }
        }

        private static void TibbersFlash()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);

            var target = TargetSelector.GetTarget(_r.Range + 425, DamageType.Magical);
            if (target == null) return;
            var xpos = target.Position.Extend(target, 610);

            if (!_r.IsReady() || GetPassiveBuff == 1 || GetPassiveBuff == 2)
            {
                Combo();
            }

            var predrpos = _r.GetPrediction(target);
            if (_comboMenu["flashr"].Cast<KeyBind>().CurrentValue)
            {
                if (GetPassiveBuff == 4 && _flash.IsReady() && _r.IsReady() && _e.IsReady())
                    if (target.IsValidTarget(_r.Range + 425))
                    {
                        _flash.Cast((Vector3) xpos);
                        _r.Cast(predrpos.CastPosition);
                    }
            }

            if (!_comboMenu["flasher"].Cast<KeyBind>().CurrentValue) return;
            if (GetPassiveBuff == 3 && _flash.IsReady() && _r.IsReady() && _e.IsReady())
            {
                _e.Cast();
            }
            if (!Annie.HasBuff("pyromania_particle")) return;
            if (target.IsValidTarget(_r.Range + 425))
            {
                _flash.Cast((Vector3) xpos);
                _r.Cast(predrpos.CastPosition);
            }
        }

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "tibbers")
            {
                TibbersObject = sender;
            }
        }

        private static Obj_AI_Turret GetTurrets()
        {
            var turret =
                EntityManager.Turrets.Enemies.OrderBy(
                    x => x.Distance(TibbersObject.Position) <= 500 && !x.IsAlly && !x.IsDead)
                    .FirstOrDefault();
            return turret;
        }

        private static void MoveTibbers()
        {
            if (!_comboMenu["pilot"].Cast<CheckBox>().CurrentValue)
                return;

            var target = TargetSelector.GetTarget(2000, DamageType.Magical);

            if (Player.HasBuff("infernalguardiantime"))
            {
                Player.IssueOrder(GameObjectOrder.MovePet,
                    target.IsValidTarget(1500) ? target.Position : GetTurrets().Position);
            }
        }

        private static
            void Combo
            ()
        {
            var target = TargetSelector.GetTarget(700, DamageType.Magical);
            if (target == null || !target.IsValid())
            {
                return;
            }

            if (Orbwalker.IsAutoAttacking && _comboMenu["waitAA"].Cast<CheckBox>().CurrentValue)
                return;
            if (_comboMenu["usecomboq"].Cast<CheckBox>().CurrentValue)
            {
                Q.Cast(target);
            }
            if (_comboMenu["usecombow"].Cast<CheckBox>().CurrentValue)
                if (W.IsReady())
                {
                    var predW = W.GetPrediction(target).CastPosition;
                    if (target.CountEnemiesInRange(W.Range) >= 1)
                        W.Cast(predW);
                }
            if (_comboMenu["usecombor"].Cast<CheckBox>().CurrentValue)
                if (_r.IsReady())
                {
                    var predR = _r.GetPrediction(target).CastPosition;
                    if (target.CountEnemiesInRange(_r.Width) >= _comboMenu["rslider"].Cast<Slider>().CurrentValue)
                        _r.Cast(predR);
                }
            if (!_comboMenu["usecomboe"].Cast<CheckBox>().CurrentValue) return;
            if (_e.IsReady())
            {
                if (Annie.CountEnemiesInRange(Q.Range) >= 2 ||
                    Annie.HealthPercent >= 45 && Annie.CountEnemiesInRange(Q.Range) >= 1)
                    _e.Cast();
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
