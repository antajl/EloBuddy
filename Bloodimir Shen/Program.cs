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
using extend = EloBuddy.SDK.Extensions;

namespace Bloodimir_Shen
{
    internal static class Program
    {
        private static readonly AIHeroClient Shen = ObjectManager.Player;
        private static Spell.Targeted _q;
        public static Spell.Targeted R, Ignite, Exhaust;
        private static Spell.Active _w;
        private static Spell.Skillshot _e;
        private static Spell.Skillshot _flash;
        private static Item _randuin;
        public static Menu ShenMenu;
        private static Menu _comboMenu;
        private static Menu _ultMenu;
        public static Menu MiscMenu;
        private static Menu _eMenu;
        public static Menu DrawMenu;
        private static Menu _skinMenu;
        private static int[] _abilitySequence;
        public static int QOff = 0, WOff = 0, EOff = 0, ROff = 0;

        private static Vector3 MousePos
        {
            get { return Game.CursorPos; }
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
            if (Player.Instance.ChampionName != "Shen")
                return;
            Bootstrap.Init(null);
            _q = new Spell.Targeted(SpellSlot.Q, 475);
            _w = new Spell.Active(SpellSlot.W);
            _e = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Linear, 250, 1600, 50);
            R = new Spell.Targeted(SpellSlot.R, 31000);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Exhaust = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerexhaust"), 650);
            var flashSlot = Shen.GetSpellSlotFromName("summonerflash");
            _flash = new Spell.Skillshot(flashSlot, 32767, SkillShotType.Linear);
            _randuin = new Item((int) ItemId.Randuins_Omen);
            _abilitySequence = new[] {1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};

            ShenMenu = MainMenu.AddMenu("BloodimirShen", "bloodimirshen");
            ShenMenu.AddGroupLabel("Bloodimir Shen");
            ShenMenu.AddSeparator();
            ShenMenu.AddLabel("Bloodimir Shen v1.0.0.0");

            _comboMenu = ShenMenu.AddSubMenu("Combo", "sbtw");
            _comboMenu.AddGroupLabel("Combo Settings");
            _comboMenu.AddSeparator();
            _comboMenu.Add("usecomboq", new CheckBox("Use Q"));
            _comboMenu.Add("usecombow", new CheckBox("Use W"));
            _comboMenu.Add("usecomboe", new CheckBox("Use E"));
            _comboMenu.Add("useignite", new CheckBox("Use Ignite"));

            _skinMenu = ShenMenu.AddSubMenu("Skin Changer", "skin");
            _skinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = _skinMenu.Add("sID", new Slider("Skin", 5, 0, 6));
            var sid = new[]
            {
                "Default", "Frozen", "Yellow Jacket", "Surgeon", "Blood Moon", "Warlord", "TPA"
            };
            skinchange.DisplayName = sid[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sid[changeArgs.NewValue];
                };

            _eMenu = ShenMenu.AddSubMenu("Taunt", "etaunt");
            _eMenu.AddGroupLabel("E Settings");
            _eMenu.Add("eslider", new Slider("Minimum Enemy to Taunt", 1, 0, 5));
            _eMenu.Add("fleee", new CheckBox("Use E Flee"));
            _eMenu.AddSeparator();
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team != Shen.Team))
            {
                _eMenu.Add("taunt" + obj.ChampionName.ToLower(), new CheckBox("Taunt " + obj.ChampionName));
            }
            _eMenu.Add("flashe", new KeyBind("Flash E", false, KeyBind.BindTypes.HoldActive, 'Y'));
            _eMenu.Add("e", new KeyBind("E", false, KeyBind.BindTypes.HoldActive, 'E'));

            _ultMenu = ShenMenu.AddSubMenu("ULT", "ultmenu");
            _ultMenu.AddGroupLabel("ULT");
            _ultMenu.AddSeparator();
            _ultMenu.Add("autoult", new CheckBox("Auto Ult on Key Press"));
            _ultMenu.Add("rslider", new Slider("Health Percent for Ult", 20));
            _ultMenu.AddSeparator();
            _ultMenu.Add("ult", new KeyBind("ULT", false, KeyBind.BindTypes.HoldActive, 'R'));

            MiscMenu = ShenMenu.AddSubMenu("Misc", "misc");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS with Q"));
            MiscMenu.Add("LHQ", new CheckBox("Last Hit Q"));
            MiscMenu.Add("LCQ", new CheckBox("LaneClear Q"));
            MiscMenu.Add("int", new CheckBox("Interrupt Spells"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("support", new CheckBox("Support Mode", false));
            MiscMenu.Add("useexhaust", new CheckBox("Use Exhaust"));
            MiscMenu.Add("randuin", new CheckBox("Use Randuin"));
            MiscMenu.Add("autow", new CheckBox("Auto W"));
            MiscMenu.AddSeparator();
            MiscMenu.Add("WHPPercent", new Slider("Auto W HP %", 45));
            MiscMenu.AddSeparator();
            MiscMenu.Add("lvlup", new CheckBox("Auto Level Up Spells", false));
            foreach (var source in ObjectManager.Get<AIHeroClient>().Where(a => a.IsEnemy))
            {
                MiscMenu.Add(source.ChampionName + "exhaust",
                    new CheckBox("Exhaust " + source.ChampionName, false));
            }


            DrawMenu = ShenMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("drawe", new CheckBox("Draw E"));
            DrawMenu.Add("drawfq", new CheckBox("Draw FlashQ"));

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Obj_AI_Base.OnProcessSpellCast += Auto_WOnProcessSpell;
            Core.DelayAction(FlashE, 60);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
            {
                if (_e.IsReady() && sender.IsValidTarget(_e.Range) && MiscMenu["inte"].Cast<CheckBox>().CurrentValue)
                    _e.Cast(intTarget.ServerPosition);
            }
        }

        private static void Auto_WOnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var shieldHealthPercent = MiscMenu["WHPPercent"].Cast<Slider>().CurrentValue;
            var shieldSelf = MiscMenu["autow"].Cast<CheckBox>().CurrentValue;
            if (!shieldSelf) return;
            if (Shen.CountEnemiesInRange(850) >= 1 && Shen.HealthPercent < shieldHealthPercent)
            {
                _w.Cast();
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Shen.IsDead) return;
            if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && _q.IsLearned)
            {
                Circle.Draw(Color.Red, _q.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && R.IsLearned)
            {
                Circle.Draw(Color.LightBlue, _e.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawfq"].Cast<CheckBox>().CurrentValue && _e.IsLearned && _flash.IsReady() && _e.IsReady())
            {
                Circle.Draw(Color.DarkBlue, _e.Range + 425, Player.Instance.Position);
            }
            {
                DrawAllyHealths();
            }
            {
                Danger();
            }
        }

        private static
            void Flee
            ()
        {
            if (!_eMenu["fleee"].Cast<CheckBox>().CurrentValue) return;
            Orbwalker.MoveTo(Game.CursorPos);
            _e.Cast(MousePos);
        }

        private static void HighestAuthority()
        {
            var autoult = _ultMenu["autoult"].Cast<CheckBox>().CurrentValue;
            if (!autoult) return;
            foreach (var ally in EntityManager.Heroes.Allies.Where(
                x => x.IsValidTarget(R.Range) && x.HealthPercent < 7)
                .Where(ally => R.IsReady() && ally.CountEnemiesInRange(650) >= 1))
                R.Cast(ally);
        }

        private static void OnUpdate(EventArgs args)
        {
            if (_eMenu["flashe"].Cast<KeyBind>().CurrentValue)
            {
                FlashE();
            }
            SkinChange();
            Killsteal();
            if (MiscMenu["randuin"].Cast<CheckBox>().CurrentValue) RanduinU();
            HighestAuthority();
            if (MiscMenu["lvlup"].Cast<CheckBox>().CurrentValue) LevelUpSpells();
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    Combo(useW: _comboMenu["usecombow"].Cast<CheckBox>().CurrentValue,
                        useE: _comboMenu["usecomboe"].Cast<CheckBox>().CurrentValue,
                        useQ: _comboMenu["usecomboq"].Cast<CheckBox>().CurrentValue);
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (_eMenu["e"].Cast<KeyBind>().CurrentValue)
            {
                Ee();
            }
            if (_ultMenu["ult"].Cast<KeyBind>().CurrentValue)
            {
                Ult();
            }
        }

        private static void RanduinU()
        {
            if (!_randuin.IsReady() || !_randuin.IsOwned()) return;
            var randuin = MiscMenu["randuin"].Cast<CheckBox>().CurrentValue;
            if (randuin && Shen.HealthPercent <= 15 && Shen.CountEnemiesInRange(_randuin.Range) >= 1 ||
                Shen.CountEnemiesInRange(_randuin.Range) >= 2)
            {
                _randuin.Cast();
            }
        }

        private static void LevelUpSpells()
        {
            var qL = Shen.Spellbook.GetSpell(SpellSlot.Q).Level + QOff;
            var wL = Shen.Spellbook.GetSpell(SpellSlot.W).Level + WOff;
            var eL = Shen.Spellbook.GetSpell(SpellSlot.E).Level + EOff;
            var rL = Shen.Spellbook.GetSpell(SpellSlot.R).Level + ROff;
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

        private static void FlashE()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);
            var fetarget = TargetSelector.GetTarget(_e.Range + 425, DamageType.Magical);
            if (fetarget == null) return;
            var xpos = fetarget.Position.Extend(fetarget, _e.Range);
            var predepos = _e.GetPrediction(fetarget).CastPosition;
            {
                if (!_e.IsReady() || !_flash.IsReady()) return;
                if (fetarget.IsValidTarget(_e.Range + 425))
                {
                    _flash.Cast((Vector3) xpos);
                    _e.Cast(predepos);
                }
            }
        }

        private static void Ee()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);
            var etarget = TargetSelector.GetTarget(600, DamageType.Magical);
            if (etarget == null) return;
            var predepos = _e.GetPrediction(etarget).CastPosition;
            if (!_eMenu["e"].Cast<KeyBind>().CurrentValue) return;
            if (!_e.IsReady()) return;
            if (etarget.IsValidTarget(600))
            {
                _e.Cast(predepos);
            }
        }

        private static void Ult()
        {
            var autoult = _ultMenu["autoult"].Cast<CheckBox>().CurrentValue;
            var rslider = _ultMenu["rslider"].Cast<Slider>().CurrentValue;
            if (!autoult || (!_ultMenu["ult"].Cast<KeyBind>().CurrentValue)) return;
            foreach (var ally in EntityManager.Heroes.Allies.Where(
                x => x.IsValidTarget(R.Range) && x.HealthPercent < rslider)
                .Where(ally => R.IsReady() && ally.CountEnemiesInRange(850) >= 1))
                R.Cast(ally);
        }

        private static void DrawAllyHealths()
        {
            {
                float i = 0;
                foreach (
                    var hero in EntityManager.Heroes.Allies.Where(hero => hero.IsAlly && !hero.IsMe && !hero.IsDead))
                {
                    var playername = hero.Name;
                    if (playername.Length > 13)
                    {
                        playername = playername.Remove(9) + "..";
                    }
                    var champion = hero.ChampionName;
                    if (champion.Length > 12)
                    {
                        champion = champion.Remove(7) + "..";
                    }
                    var percent = (int) (hero.Health/hero.MaxHealth*100);
                    var color = System.Drawing.Color.Red;
                    if (percent > 25)
                    {
                        color = System.Drawing.Color.Orange;
                    }
                    if (percent > 50)
                    {
                        color = System.Drawing.Color.Yellow;
                    }
                    if (percent > 75)
                    {
                        color = System.Drawing.Color.LimeGreen;
                    }
                    Drawing.DrawText(
                        Drawing.Width*0.8f, Drawing.Height*0.1f + i, color, playername + " (" + champion + ") ");
                    Drawing.DrawText(
                        Drawing.Width*0.9f, Drawing.Height*0.1f + i, color,
                        ((int) hero.Health) + " (" + percent + "%)");
                    i += 20f;
                }
            }
        }

        private static void Danger()
        {
            var rslider = _ultMenu["rslider"].Cast<Slider>().CurrentValue;
            foreach (
                var ally in
                    EntityManager.Heroes.Allies.Where(
                        x => x.IsValidTarget(R.Range) && x.HealthPercent < rslider)
                )
            {
                const float i = 0;
                {
                    var champion = ally.ChampionName;
                    if (champion.Length > 12)
                    {
                        champion = champion.Remove(7) + "..";
                    }
                    var percent = (int) (ally.Health/ally.MaxHealth*100);
                    var color = System.Drawing.Color.Red;
                    if (percent > 25)
                    {
                        color = System.Drawing.Color.Orange;
                    }
                    if (percent > 50)
                    {
                        color = System.Drawing.Color.Yellow;
                    }
                    if (percent > 75)
                    {
                        color = System.Drawing.Color.LimeGreen;
                    }
                    if (ally.CountEnemiesInRange(850) >= 1 && R.IsReady())
                    {
                        Drawing.DrawText(
                            Drawing.Width*0.4f, Drawing.Height*0.4f + i, color, " (" + champion + ")"
                                                                                + "-   (IS DYING - PRESS R TO AUTO ULT)");
                    }
                }
            }
        }

        private static void Killsteal()
        {
            if (!MiscMenu["ksq"].Cast<CheckBox>().CurrentValue || !_q.IsReady()) return;
            foreach (var qtarget in EntityManager.Heroes.Enemies.Where(
                hero => hero.IsValidTarget(_q.Range) && !hero.IsDead && !hero.IsZombie)
                .Where(qtarget => Shen.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health))
            {
                _q.Cast(qtarget);
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
                            Shen.GetSpellDamage(a, SpellSlot.Q) >= a.Health);
            }
        }

        private static
            void LastHit()
        {
            var qcheck = MiscMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var qready = _q.IsReady();
            if (!qcheck || !qready) return;
            var qminion = (Obj_AI_Minion) GetEnemy(_q.Range, GameObjectType.obj_AI_Minion);
            if (qminion != null)
            {
                _q.Cast(qminion);
            }
        }

        private static
            void LaneClear()
        {
            var qcheck = MiscMenu["LCQ"].Cast<CheckBox>().CurrentValue;
            var qready = _q.IsReady();
            if (!qcheck || !qready) return;
            var qminion = (Obj_AI_Minion) GetEnemy(_q.Range, GameObjectType.obj_AI_Minion);
            if (qminion != null)
            {
                _q.Cast(qminion);
            }
        }

        private static
            void Combo(bool useE, bool useW, bool useQ)
        {
            if (useQ && _q.IsReady())
            {
                var qtarget = GetEnemy(_q.Range, GameObjectType.AIHeroClient);
                if (qtarget.IsValidTarget(_q.Range))
                {
                    _q.Cast(qtarget);
                }
            }
            if (!useE || !_e.IsReady()) return;
            var eTarget = TargetSelector.GetTarget(_e.Range, DamageType.Magical);
            var predE = _e.GetPrediction(eTarget).CastPosition;
            if (eTarget.IsValidTarget(_e.Range))
                if (_eMenu["taunt" + eTarget.ChampionName].Cast<CheckBox>().CurrentValue)
                    if (Shen.CountEnemiesInRange(_e.Range) <= 1)
                        if (_e.GetPrediction(eTarget).HitChance >= HitChance.High)
                        {
                            _e.Cast(eTarget);
                        }
                        else if (Shen.CountEnemiesInRange(_e.Width) >=
                                 _eMenu["eslider"].Cast<Slider>().CurrentValue)
                        {
                            _e.Cast(predE);
                        }
            if (!useW || !_w.IsReady()) return;
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Physical);
            if (!target.IsValidTarget(_q.Range)) return;
            if (target.Distance(Shen) < _q.Range)

            {
                _w.Cast();
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
                case "Frozen":
                    Player.SetSkinId(1);
                    break;
                case "Yellow Jacket":
                    Player.SetSkinId(2);
                    break;
                case "Surgeon":
                    Player.SetSkinId(3);
                    break;
                case "Blood Moon":
                    Player.SetSkinId(4);
                    break;
                case "Warlord":
                    Player.SetSkinId(5);
                    break;
                case "TPA":
                    Player.SetSkinId(6);
                    break;
            }
        }
    }
}