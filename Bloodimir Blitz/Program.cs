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
        private static Spell.Skillshot Q;
        private static Spell.Skillshot Flash;
        private static Spell.Active W;
        private static Spell.Active E;
        private static Spell.Active R;
        private static Menu BlitzMenu;
        private static Menu ComboMenu;
        private static Menu DrawMenu;
        private static Menu SkinMenu;
        private static Menu MiscMenu;
        private static Menu QMenu;
        private static AIHeroClient Blitz = ObjectManager.Player;
        private static Item Talisman;
        private static HitChance QHitChance;

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
            Q = new Spell.Skillshot(SpellSlot.Q, 925, SkillShotType.Linear, 250, 1800, 70);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Active(SpellSlot.R, 550);
            var FlashSlot = Blitz.GetSpellSlotFromName("summonerflash");
            Flash = new Spell.Skillshot(FlashSlot, 32767, SkillShotType.Linear);
            Talisman = new Item((int)ItemId.Talisman_of_Ascension);

            BlitzMenu = MainMenu.AddMenu("BloodimirBlitz", "bloodimirblitz");
            BlitzMenu.AddGroupLabel("Bloodimir Blitzcrank");
            BlitzMenu.AddSeparator();
            BlitzMenu.AddLabel("Bloodimir Blitzcrank v1.0.2.0");

            ComboMenu = BlitzMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 2, 0, 5));
            ComboMenu.AddSeparator();
            ComboMenu.Add("flashq", new KeyBind("Flash Q", false, KeyBind.BindTypes.HoldActive, 'Y'));

            QMenu = BlitzMenu.AddSubMenu("Q Settings", "qsettings");
            QMenu.AddGroupLabel("Q Settings");
            QMenu.AddSeparator();
            QMenu.Add("qmin", new Slider("Min Range", 125, 0, (int) Q.Range));
            QMenu.Add("qmax", new Slider("Max Range", (int) Q.Range, 0, (int) Q.Range));
            QMenu.AddSeparator();
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team != Blitz.Team))
            {
                QMenu.Add("grab" + obj.ChampionName.ToLower(), new CheckBox("Grab " + obj.ChampionName));
            }
            QMenu.AddSeparator();
            QMenu.Add("mediumpred", new CheckBox("MEDIUM Bind Hitchance Prediction / Disabled = High", false));
            QMenu.Add("intq", new CheckBox("Q to Interrupt"));
            QMenu.AddSeparator();

            SkinMenu = BlitzMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 4, 0, 8));
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

            MiscMenu = BlitzMenu.AddSubMenu("Misc", "misc");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS with Q"));
            MiscMenu.Add("ksr", new CheckBox("KS with R"));
            MiscMenu.Add("LHE", new CheckBox("Last Hit E"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("support", new CheckBox("Support Mode"));
            MiscMenu.Add("fleew", new CheckBox("Use W Flee"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("talisman", new CheckBox("Use Talisman of Ascension"));


            DrawMenu = BlitzMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("drawr", new CheckBox("Draw R"));
            DrawMenu.Add("drawfq", new CheckBox("Draw FlashQ"));
            DrawMenu.Add("predictions", new CheckBox("Visualize prediction"));

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnUpdate += Tick;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            Core.DelayAction(FlashQ, 1);
            Drawing.OnDraw += delegate
            {
                 if (DrawMenu["drawr"].Cast<CheckBox>().CurrentValue && R.IsLearned)
                {
                    Drawing.DrawCircle(Blitz.Position, R.Range, System.Drawing.Color.LightBlue);
                }
                if (DrawMenu["drawfq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Blitz.Position, 850 + 425, System.Drawing.Color.DarkBlue);
                }
                var predictedPositions = new Dictionary<int, Tuple<int, PredictionResult>>();
                var predictions = DrawMenu["predictions"].Cast<CheckBox>().CurrentValue;
                var qRange = DrawMenu["drawq"].Cast<CheckBox>().CurrentValue;

                foreach (
                    var enemy in
                        EntityManager.Heroes.Enemies.Where(
                            enemy => QMenu["grab" + enemy.ChampionName].Cast<CheckBox>().CurrentValue &&
                                     enemy.IsValidTarget(Q.Range + 150) &&
                                     !enemy.HasBuffOfType(BuffType.SpellShield)))
                {
                    var predictionsq = Q.GetPrediction(enemy);
                    predictedPositions[enemy.NetworkId] = new Tuple<int, PredictionResult>(Environment.TickCount,
                        predictionsq);
                    if (qRange && Q.IsLearned)
                    {
                        Circle.Draw(Q.IsReady() ? Color.Blue : Color.Red, Q.Range, Player.Instance.Position);
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
            var intTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            {
                if (Q.IsReady() && sender.IsValidTarget(Q.Range) && MiscMenu["intq"].Cast<CheckBox>().CurrentValue)
                    Q.Cast(intTarget.ServerPosition);
            }}

        private static
            void Flee
            ()
        {
            if (!MiscMenu["fleew"].Cast<CheckBox>().CurrentValue) return;
            Orbwalker.MoveTo(Game.CursorPos);
            W.Cast();
        }
   

        private static void Tick(EventArgs args)
        {
            QHitChance = QMenu["mediumpred"].Cast<CheckBox>().CurrentValue ? HitChance.Medium : HitChance.High;
            SkinChange();
            Killsteal();
            Ascension();
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    Combo(useW:ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue, useQ:ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue, useR:ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (ComboMenu["flashq"].Cast<KeyBind>().CurrentValue)
            {
                FlashQ();
            }
        }
        
        private static void Ascension()
        {
            if (!Talisman.IsReady() || !Talisman.IsOwned()) return;
            var ascension = MiscMenu["talisman"].Cast<CheckBox>().CurrentValue;
            if (ascension && Blitz.HealthPercent <= 15 && Blitz.CountEnemiesInRange(800) >= 1 || Blitz.CountEnemiesInRange(Q.Range) >= 3)
            {
                Talisman.Cast();
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
                if (MiscMenu["support"].Cast<CheckBox>().CurrentValue)
                    args.Process = false;
            }
        }

        private static void FlashQ()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);
            var ftarget = TargetSelector.GetTarget(850 + 425, DamageType.Magical);
            if (ftarget == null) return;
            var xpos = ftarget.Position.Extend(ftarget, 850);
            var predqpos = Q.GetPrediction(ftarget).CastPosition;
            if (!ComboMenu["flashq"].Cast<KeyBind>().CurrentValue) return;
            if (!Q.IsReady() || !Flash.IsReady()) return;
            if (!ftarget.IsValidTarget(850 + 425)) return;
            Flash.Cast((Vector3) xpos);
            Q.Cast(predqpos);
        }

        private static void Killsteal()
        {
            if (!MiscMenu["ksq"].Cast<CheckBox>().CurrentValue || !Q.IsReady()) return;
            foreach (var poutput in from qtarget in EntityManager.Heroes.Enemies.Where(
                hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie) where Blitz.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health select Q.GetPrediction(qtarget))
            {
                if (poutput.HitChance >= HitChance.Medium)
                {
                    Q.Cast(poutput.CastPosition);
                }
                if (!MiscMenu["ksr"].Cast<CheckBox>().CurrentValue || !R.IsReady()) continue;
                {
                    foreach (var rtarget in EntityManager.Heroes.Enemies.Where(
                        hero =>
                            hero.IsValidTarget(R.Range) && !hero.IsDead && !hero.IsZombie).Where(rtarget => Blitz.GetSpellDamage(rtarget, SpellSlot.R) >= rtarget.Health))
                    {
                        R.Cast();
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
            if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
            var e = target as AIHeroClient;
            if (!ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue || !E.IsReady() || !e.IsEnemy) return;
            if (target == null) return;
            if (e.IsValidTarget() && E.IsReady())
            {
                E.Cast();
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
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable && Blitz.GetSpellDamage(a, SpellSlot.E) >= a.Health);
            }
        }

        private static
            void LastHit()
        {
            var echeck = MiscMenu["LHE"].Cast<CheckBox>().CurrentValue;
            var eready = E.IsReady();
            if (!echeck && !eready) return;
            var eminion = (Obj_AI_Minion) GetEnemy(Player.Instance.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);
            if (eminion != null)
            {
                E.Cast();
            }
        }

        private static
            void Combo(bool useQ, bool useW, bool useR)
        {
            if (!useQ || !Q.IsReady()) return;
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
                if (!useW || !W.IsReady() || !ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue) return;
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (target.IsValidTarget(Q.Range))
                {
                    if (target.Distance(Blitz) > 800)
                    {
                        W.Cast();
                    }
                    if (target.Distance(Blitz) < 425)
                    {
                        W.Cast();
                    }
                }
                if (!useR || !R.IsReady()) return;
                if  (Blitz.CountEnemiesInRange(R.Range) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue)
                {
                    R.Cast();
                }
            }
            catch {}
        }
    }
    }
