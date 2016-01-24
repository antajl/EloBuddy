using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace Bloodimir_Blitz
{
    internal static class Program
    {
        private static Spell.Skillshot _q;
        private static Spell.Skillshot _flash;
        private static Spell.Active _w;
        private static Spell.Active _e;
        private static Spell.Active _r;

        private static Menu _blitzMenu,
            _comboMenu,
            _drawMenu,
            _skinMenu,
            _miscMenu;

        private static Menu _qMenu;
        private static readonly AIHeroClient Blitz = ObjectManager.Player;
        private static Item _talisman;
        private static HitChance _qHitChance;

        private static Vector3 MousePos
        {
            get { return Game.CursorPos; }
        }

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
            _q = new Spell.Skillshot(SpellSlot.Q, 925, SkillShotType.Linear, 250, 1800, 70);
            _w = new Spell.Active(SpellSlot.W);
            _e = new Spell.Active(SpellSlot.E);
            _r = new Spell.Active(SpellSlot.R, 550);
            var FlashSlot = Blitz.GetSpellSlotFromName("summonerflash");
            _flash = new Spell.Skillshot(FlashSlot, 32767, SkillShotType.Linear);
            _talisman = new Item((int) ItemId.Talisman_of_Ascension);

            _blitzMenu = MainMenu.AddMenu("BloodimirBlitz", "bloodimirblitz");
            _blitzMenu.AddGroupLabel("Bloodimir Blitzcrank");
            _blitzMenu.AddSeparator();
            _blitzMenu.AddLabel("Bloodimir Blitzcrank v1.0.2.0");

            _comboMenu = _blitzMenu.AddSubMenu("Combo", "sbtw");
            _comboMenu.AddGroupLabel("Combo Settings");
            _comboMenu.AddSeparator();
            _comboMenu.Add("usecomboq", new CheckBox("Use Q"));
            _comboMenu.Add("usecombow", new CheckBox("Use W"));
            _comboMenu.Add("usecomboe", new CheckBox("Use E"));
            _comboMenu.Add("usecombor", new CheckBox("Use R"));
            _comboMenu.AddSeparator();
            _comboMenu.Add("rslider", new Slider("Minimum people for R", 2, 0, 5));
            _comboMenu.AddSeparator();
            _comboMenu.Add("flashq", new KeyBind("Flash Q", false, KeyBind.BindTypes.HoldActive, 'Y'));

            _qMenu = _blitzMenu.AddSubMenu("Q Settings", "qsettings");
            _qMenu.AddGroupLabel("Q Settings");
            _qMenu.AddSeparator();
            _qMenu.Add("qmin", new Slider("Min Range", 125, 0, (int) _q.Range));
            _qMenu.Add("qmax", new Slider("Max Range", (int) _q.Range, 0, (int) _q.Range));
            _qMenu.AddSeparator();
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team != Blitz.Team))
            {
                _qMenu.Add("grab" + obj.ChampionName.ToLower(), new CheckBox("Grab " + obj.ChampionName));
            }
            _qMenu.AddSeparator();
            _qMenu.Add("mediumpred", new CheckBox("MEDIUM Bind Hitchance Prediction / Disabled = High", false));
            _qMenu.Add("intq", new CheckBox("Q to Interrupt"));
            _qMenu.AddSeparator();

            _skinMenu = _blitzMenu.AddSubMenu("Skin Changer", "skin");
            _skinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = _skinMenu.Add("sID", new Slider("Skin", 4, 0, 8));
            var sID = new[]
            {
                "Default", "Rusty", "Goalkeeper", "Boom Boom", "Piltover Customs", "DefNotBlitz", "iBlitzCrank",
                "RiotCrank", "Battle Boss"
            };
            skinchange.DisplayName = sID[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sID[changeArgs.NewValue];
                };

            _miscMenu = _blitzMenu.AddSubMenu("Misc", "misc");
            _miscMenu.AddGroupLabel("Misc");
            _miscMenu.AddSeparator();
            _miscMenu.Add("ksq", new CheckBox("KS with Q"));
            _miscMenu.Add("ksr", new CheckBox("KS with R"));
            _miscMenu.Add("LHE", new CheckBox("Last Hit E"));
            _miscMenu.AddSeparator();
            _miscMenu.Add("support", new CheckBox("Support Mode"));
            _miscMenu.Add("fleew", new CheckBox("Use W Flee"));
            _miscMenu.AddSeparator();
            _miscMenu.Add("talisman", new CheckBox("Use Talisman of Ascension"));


            _drawMenu = _blitzMenu.AddSubMenu("Drawings", "drawings");
            _drawMenu.AddGroupLabel("Drawings");
            _drawMenu.AddSeparator();
            _drawMenu.Add("drawq", new CheckBox("Draw Q"));
            _drawMenu.Add("drawr", new CheckBox("Draw R"));
            _drawMenu.Add("drawfq", new CheckBox("Draw FlashQ"));
            _drawMenu.Add("predictions", new CheckBox("Visualize prediction"));

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnUpdate += Tick;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            Core.DelayAction(FlashQ, 1);
            Drawing.OnDraw += delegate
            {
                if (_drawMenu["drawr"].Cast<CheckBox>().CurrentValue && _r.IsLearned)
                {
                    Circle.Draw(Color.LightBlue, _r.Range, Player.Instance.Position);
                }
                if (_drawMenu["drawfq"].Cast<CheckBox>().CurrentValue && _q.IsLearned)
                {
                    Circle.Draw(Color.DarkBlue, 850 + 425, Player.Instance.Position);
                }
                var predictedPositions = new Dictionary<int, Tuple<int, PredictionResult>>();
                var predictions = _drawMenu["predictions"].Cast<CheckBox>().CurrentValue;
                var qRange = _drawMenu["drawq"].Cast<CheckBox>().CurrentValue;

                foreach (
                    var enemy in
                        EntityManager.Heroes.Enemies.Where(
                            enemy => _qMenu["grab" + enemy.ChampionName].Cast<CheckBox>().CurrentValue &&
                                     enemy.IsValidTarget(_q.Range + 150) &&
                                     !enemy.HasBuffOfType(BuffType.SpellShield)))
                {
                    var predictionsq = _q.GetPrediction(enemy);
                    predictedPositions[enemy.NetworkId] = new Tuple<int, PredictionResult>(Environment.TickCount,
                        predictionsq);
                    if (qRange && _q.IsLearned)
                    {
                        Circle.Draw(_q.IsReady() ? Color.Blue : Color.Red, _q.Range,
                            Player.Instance.Position);
                    }

                    if (!predictions)
                    {
                        return;
                    }

                    foreach (var prediction in predictedPositions.ToArray())
                    {
                        if (Environment.TickCount - prediction.Value.Item1 > 2000)
                        {
                            predictedPositions.Remove(prediction.Key);
                            continue;
                        }

                        Circle.Draw(Color.Red, 75, prediction.Value.Item2.CastPosition);
                        Line.DrawLine(System.Drawing.Color.GreenYellow, Player.Instance.Position,
                            prediction.Value.Item2.CastPosition);
                        Line.DrawLine(System.Drawing.Color.CornflowerBlue,
                            EntityManager.Heroes.Enemies.Find(o => o.NetworkId == prediction.Key).Position,
                            prediction.Value.Item2.CastPosition);
                        Drawing.DrawText(prediction.Value.Item2.CastPosition.WorldToScreen() + new Vector2(0, -20),
                            System.Drawing.Color.LimeGreen,
                            string.Format("Hitchance: {0}%", Math.Ceiling(prediction.Value.Item2.HitChancePercent)),
                            10);
                    }
                }
                ;
            };
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
            {
                if (_q.IsReady() && sender.IsValidTarget(_q.Range) && _miscMenu["intq"].Cast<CheckBox>().CurrentValue)
                    _q.Cast(intTarget.ServerPosition);
            }
        }

        private static
            void Flee
            ()
        {
            if (!_miscMenu["fleew"].Cast<CheckBox>().CurrentValue) return;
            Orbwalker.MoveTo(Game.CursorPos);
            _w.Cast();
        }

        private static void Tick(EventArgs args)
        {
            _qHitChance = _qMenu["mediumpred"].Cast<CheckBox>().CurrentValue ? HitChance.Medium : HitChance.High;
            SkinChange();
            Killsteal();
            Ascension();
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    Combo(useW: _comboMenu["usecombow"].Cast<CheckBox>().CurrentValue,
                        useQ: _comboMenu["usecomboq"].Cast<CheckBox>().CurrentValue,
                        useR: _comboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (_comboMenu["flashq"].Cast<KeyBind>().CurrentValue)
            {
                FlashQ();
            }
        }

        private static void Ascension()
        {
            if (!_talisman.IsReady() || !_talisman.IsOwned()) return;
            var ascension = _miscMenu["talisman"].Cast<CheckBox>().CurrentValue;
            if (ascension && Blitz.HealthPercent <= 15 && Blitz.CountEnemiesInRange(800) >= 1 ||
                Blitz.CountEnemiesInRange(_q.Range) >= 3)
            {
                _talisman.Cast();
            }
        }

        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
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

        private static void FlashQ()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);
            var ftarget = TargetSelector.GetTarget(850 + 425, DamageType.Magical);
            if (ftarget == null) return;
            var xpos = ftarget.Position.Extend(ftarget, 850);
            var predqpos = _q.GetPrediction(ftarget).CastPosition;
            if (!_comboMenu["flashq"].Cast<KeyBind>().CurrentValue) return;
            if (!_q.IsReady() || !_flash.IsReady()) return;
            if (!ftarget.IsValidTarget(850 + 425)) return;
            _flash.Cast((Vector3) xpos);
            _q.Cast(predqpos);
        }

        private static void Killsteal()
        {
            if (!_miscMenu["ksq"].Cast<CheckBox>().CurrentValue || !_q.IsReady()) return;
            foreach (var poutput in from qtarget in EntityManager.Heroes.Enemies.Where(
                hero => hero.IsValidTarget(_q.Range) && !hero.IsDead && !hero.IsZombie)
                where Blitz.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health
                select _q.GetPrediction(qtarget))
            {
                if (poutput.HitChance >= HitChance.Medium)
                {
                    _q.Cast(poutput.CastPosition);
                }
                if (!_miscMenu["ksr"].Cast<CheckBox>().CurrentValue || !_r.IsReady()) continue;
                {
                    foreach (var rtarget in EntityManager.Heroes.Enemies.Where(
                        hero =>
                            hero.IsValidTarget(_r.Range) && !hero.IsDead && !hero.IsZombie)
                        .Where(rtarget => Blitz.GetSpellDamage(rtarget, SpellSlot.R) >= rtarget.Health))
                    {
                        _r.Cast();
                    }
                }
            }
        }

        private static void SkinChange()
        {
            var style = _skinMenu["sID"].DisplayName;
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
            if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
            var e = target as AIHeroClient;
            if (!_comboMenu["usecomboe"].Cast<CheckBox>().CurrentValue || !_e.IsReady() || !e.IsEnemy) return;
            if (target == null) return;
            if (e.IsValidTarget() && _e.IsReady())
            {
                _e.Cast();
            }
        }

        private static Obj_AI_Base GetEnemy(float range, GameObjectType t)
        {
            switch (t)
            {
                case GameObjectType.AIHeroClient:
                    return EntityManager.Heroes.Enemies.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
                default:
                    return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(
                        a =>
                            a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable &&
                            Blitz.GetSpellDamage(a, SpellSlot.E) >= a.Health);
            }
        }

        private static
            void LastHit()
        {
            var echeck = _miscMenu["LHE"].Cast<CheckBox>().CurrentValue;
            var eready = _e.IsReady();
            if (!echeck && !eready) return;
            var eminion = (Obj_AI_Minion) GetEnemy(Player.Instance.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);
            if (eminion != null)
            {
                _e.Cast();
            }
        }

        private static
            void Combo(bool useQ, bool useW, bool useR)
        {
            if (!useQ || !_q.IsReady()) return;
            try
            {
                var grabTarget = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
                if (grabTarget.IsValidTarget(_q.Range))
                {
                    if (_q.GetPrediction(grabTarget).HitChance >= _qHitChance)
                    {
                        if (grabTarget.Distance(Blitz.ServerPosition) > _qMenu["qmin"].Cast<Slider>().CurrentValue)
                        {
                            if (_qMenu["grab" + grabTarget.ChampionName].Cast<CheckBox>().CurrentValue)
                            {
                                _q.Cast(grabTarget);
                            }
                        }
                    }
                }
                if (!useW || !_w.IsReady() || !_comboMenu["usecombow"].Cast<CheckBox>().CurrentValue) return;
                var target = TargetSelector.GetTarget(_q.Range, DamageType.Physical);
                if (target.IsValidTarget(_q.Range))
                {
                    if (target.Distance(Blitz) > 800)
                    {
                        _w.Cast();
                    }
                    if (target.Distance(Blitz) < 425)
                    {
                        _w.Cast();
                    }
                }
                if (!useR || !_r.IsReady()) return;
                if (Blitz.CountEnemiesInRange(_r.Range) >= _comboMenu["rslider"].Cast<Slider>().CurrentValue)
                {
                    _r.Cast();
                }
            }
            catch
            {
            }
        }
    }
}