using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace Bloodimir_Shen
{
    internal class Program
    {
        public static AIHeroClient Shen = ObjectManager.Player;
        public static Spell.Targeted Q, R, Ignite, Exhaust;
        public static Spell.Active W;
        public static Spell.Skillshot E, Flash;
        public static Item Randuin;
        public static Menu ShenMenu, ComboMenu, UltMenu, MiscMenu, EMenu, DrawMenu, SkinMenu;
        public static int[] AbilitySequence;
        public static int QOff = 0, WOff = 0, EOff = 0, ROff = 0;
        public static HitChance EHitChance;

        private static Vector3 MousePos
        {
            get { return Game.CursorPos; }
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
            if (Player.Instance.ChampionName != "Shen")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 475);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Linear, 250, 1600, 50);
            R = new Spell.Targeted(SpellSlot.R, 31000);
            if (HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Exhaust = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerexhaust"), 650);
            var FlashSlot = Shen.GetSpellSlotFromName("summonerflash");
            Flash = new Spell.Skillshot(FlashSlot, 32767, SkillShotType.Linear);
            Randuin = new Item((int) ItemId.Randuins_Omen);
            AbilitySequence = new int[] { 1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };

            ShenMenu = MainMenu.AddMenu("BloodimirShen", "bloodimirshen");
            ShenMenu.AddGroupLabel("Bloodimir Shen");
            ShenMenu.AddSeparator();
            ShenMenu.AddLabel("Bloodimir Shen v1.0.0.0");

            ComboMenu = ShenMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("useignite", new CheckBox("Use Ignite"));

            SkinMenu = ShenMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 5, 0, 6));
            var sID = new[]
            {
                "Default", "Yellow Jacket", "Frozen", "Surgeon", "Blood Moon", "Warlord", "TPA"
            };
            skinchange.DisplayName = sID[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sID[changeArgs.NewValue];
                };

            EMenu = ShenMenu.AddSubMenu("Taunt", "etaunt");
            EMenu.AddGroupLabel("E Settings");
            EMenu.Add("eslider", new Slider("Minimum Enemy to Taunt", 1, 0, 5));
            EMenu.Add("fleee", new CheckBox("Use E Flee"));
            EMenu.AddSeparator();
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team != Shen.Team))
            {
                EMenu.Add("taunt" + obj.ChampionName.ToLower(), new CheckBox("Taunt " + obj.ChampionName));
            }
            EMenu.Add("flashe", new KeyBind("Flash E", false, KeyBind.BindTypes.HoldActive, 'Y'));
            EMenu.Add("e", new KeyBind("E", false, KeyBind.BindTypes.HoldActive, 'E'));
            EMenu.Add("mediumpred", new CheckBox("MEDIUM E Hitchance | Disabled = High", false));

            UltMenu = ShenMenu.AddSubMenu("ULT", "ultmenu");
            UltMenu.AddGroupLabel("ULT");
            UltMenu.AddSeparator();
            UltMenu.Add("autoult", new CheckBox("Auto Ult on Key Press"));
            UltMenu.Add("rslider", new Slider("Health Percent for Ult", 20));
            UltMenu.AddSeparator();
            UltMenu.Add("ult", new KeyBind("ULT", false, KeyBind.BindTypes.HoldActive, 'R'));

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
            Game.OnTick += Tick;
            Drawing.OnDraw += OnDraw;
            Gapcloser.OnGapcloser += OnGapClose;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Obj_AI_Base.OnProcessSpellCast += Auto_WOnProcessSpell;
            Core.DelayAction(FlashE, 1);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            {
                if (E.IsReady() && sender.IsValidTarget(E.Range) && MiscMenu["inte"].Cast<CheckBox>().CurrentValue)
                    E.Cast(intTarget.ServerPosition);
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
                if (E.IsReady()
                    && E.IsInRange(gapcloser.Start))
                {
                    E.Cast(MousePos);
                }
            }
        private static void Auto_WOnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var shieldHealthPercent = MiscMenu["WHPPercent"].Cast<Slider>().CurrentValue;
            var shieldSelf = MiscMenu["autow"].Cast<CheckBox>().CurrentValue;
            if (shieldSelf)
            {
                if (Shen.CountEnemiesInRange(850) >= 1 && Shen.HealthPercent < shieldHealthPercent)
                {
                    W.Cast();
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Shen.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Shen.Position, Q.Range, Color.Red);
                }
                if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && R.IsLearned)
                {
                    Drawing.DrawCircle(Shen.Position, E.Range, Color.LightBlue);
                }
                if (DrawMenu["drawfq"].Cast<CheckBox>().CurrentValue && E.IsLearned && Flash.IsReady())
                {
                    Drawing.DrawCircle(Shen.Position, 575 + 425, Color.DarkBlue);
                }
                {
                    DrawAllyHealths();
                }
                {
                    Danger();
                }
            }
        }

        public static
            void Flee
            ()
        {
            if (EMenu["fleee"].Cast<CheckBox>().CurrentValue)
            {
                Orbwalker.MoveTo(Game.CursorPos);
                E.Cast(MousePos);
                W.Cast();
            }
        }

        public static void HighestAuthority()
        {
            var autoult = UltMenu["autoult"].Cast<CheckBox>().CurrentValue;
            if (autoult)
            {
                foreach (
                    var ally in
                        EntityManager.Heroes.Allies.Where(
                            x => x.IsValidTarget(R.Range) && x.HealthPercent < 7)
                    )
                    if (ally != null && R.IsReady() && ally.CountEnemiesInRange(650) >= 1)
                    {
                        R.Cast(ally);
                    }
            }
        }

        private static void Tick(EventArgs args)
        {
            EHitChance = EMenu["mediumpred"].Cast<CheckBox>().CurrentValue ? HitChance.Medium : HitChance.High;
            SkinChange();
            Killsteal();
            RanduinU();
            HighestAuthority();
            if (MiscMenu["lvlup"].Cast<CheckBox>().CurrentValue) LevelUpSpells();
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    Combo(useW: ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue,
                        useE: ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue,
                        useQ: ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue);
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
            if (EMenu["flashe"].Cast<KeyBind>().CurrentValue)
            {
                FlashE();
            }
            if (EMenu["e"].Cast<KeyBind>().CurrentValue)
            {
                EE();
            }
            if (UltMenu["ult"].Cast<KeyBind>().CurrentValue)
            {
                Ult();
            }
            {
                if (ComboMenu["useignite"].Cast<CheckBox>().CurrentValue)
                    foreach (
                        var source in
                            ObjectManager.Get<AIHeroClient>()
                                .Where(
                                    a =>
                                        a.IsEnemy && a.IsValidTarget(Ignite.Range) &&
                                        a.Health < 50 + 20*Shen.Level - (a.HPRegenRate/5*3)))
                    {
                        Ignite.Cast(source);
                        return;
                    }
                if (MiscMenu["useexhaust"].Cast<CheckBox>().CurrentValue)
                    foreach (
                        var enemy in
                            ObjectManager.Get<AIHeroClient>()
                                .Where(a => a.IsEnemy && a.IsValidTarget(Exhaust.Range))
                                .Where(enemy => MiscMenu[enemy.ChampionName + "exhaust"].Cast<CheckBox>().CurrentValue))
                    {
                        if (enemy.IsFacing(Shen))
                        {
                            if (!(Shen.HealthPercent < 50)) continue;
                            Exhaust.Cast(enemy);
                            return;
                        }
                        if (!(enemy.HealthPercent < 50)) continue;
                        Exhaust.Cast(enemy);
                        return;
                    }
            }
        }

        private static void RanduinU()
        {
            if (Randuin.IsReady() && Randuin.IsOwned())
            {
                var randuin = MiscMenu["randuin"].Cast<CheckBox>().CurrentValue;
                if (randuin && Shen.HealthPercent <= 15 && Shen.CountEnemiesInRange(Randuin.Range) >= 1 ||
                    Shen.CountEnemiesInRange(Randuin.Range) >= 2)
                {
                    Randuin.Cast();
                }
            }
        }
        private static void LevelUpSpells()
        {
            var qL = Shen.Spellbook.GetSpell(SpellSlot.Q).Level + QOff;
            var wL = Shen.Spellbook.GetSpell(SpellSlot.W).Level + WOff;
            var eL = Shen.Spellbook.GetSpell(SpellSlot.E).Level + EOff;
            var rL = Shen.Spellbook.GetSpell(SpellSlot.R).Level + ROff;
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

        private static void FlashE()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);
            var fetarget = TargetSelector.GetTarget(575 + 425, DamageType.Magical);
            if (fetarget == null) return;
            var xpos = fetarget.Position.Extend(fetarget, 575);
            var predepos = E.GetPrediction(fetarget).CastPosition;
            {
                if (E.IsReady() && Flash.IsReady())
                    if (fetarget.IsValidTarget(575 + 425))
                    {
                        Flash.Cast((Vector3) xpos);
                        E.Cast(predepos);
                    }
            }
        }

        private static void EE()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);
            var etarget = TargetSelector.GetTarget(600, DamageType.Magical);
            if (etarget == null) return;
            var predepos = E.GetPrediction(etarget).CastPosition;
            if (EMenu["e"].Cast<KeyBind>().CurrentValue)
            {
                if (E.IsReady())
                    if (etarget.IsValidTarget(600))
                    {
                        E.Cast(predepos);
                    }
            }
        }

        private static void Ult()
        {
            var autoult = UltMenu["autoult"].Cast<CheckBox>().CurrentValue;
            var rslider = UltMenu["rslider"].Cast<Slider>().CurrentValue;
            if (autoult && (UltMenu["ult"].Cast<KeyBind>().CurrentValue))
            {
                foreach (
                    var ally in
                        EntityManager.Heroes.Allies.Where(
                            x => x.IsValidTarget(R.Range) && x.HealthPercent < rslider)
                    )
                    if (ally != null && R.IsReady() && ally.CountEnemiesInRange(850) >= 1)
                    {
                        R.Cast(ally);
                    }
            }
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
                    var color = Color.Red;
                    if (percent > 25)
                    {
                        color = Color.Orange;
                    }
                    if (percent > 50)
                    {
                        color = Color.Yellow;
                    }
                    if (percent > 75)
                    {
                        color = Color.LimeGreen;
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
            var rslider = UltMenu["rslider"].Cast<Slider>().CurrentValue;
            foreach (
                var ally in
                    EntityManager.Heroes.Allies.Where(
                        x => x.IsValidTarget(R.Range) && x.HealthPercent < rslider)
                )
            {
                float i = 0;
                {
                    var champion = ally.ChampionName;
                    if (champion.Length > 12)
                    {
                        champion = champion.Remove(7) + "..";
                    }
                    var percent = (int) (ally.Health/ally.MaxHealth*100);
                    var color = Color.Red;
                    if (percent > 25)
                    {
                        color = Color.Orange;
                    }
                    if (percent > 50)
                    {
                        color = Color.Yellow;
                    }
                    if (percent > 75)
                    {
                        color = Color.LimeGreen;
                    }
                    if (ally != null && ally.CountEnemiesInRange(850) >= 1 && R.IsReady())
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
            if (MiscMenu["ksq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                foreach (
                    var qtarget in
                        EntityManager.Heroes.Enemies.Where(
                            hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
                {
                    if (Shen.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                    {
                        Q.Cast(qtarget);
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
                        a =>
                            a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable &&
                            Shen.GetSpellDamage(a, SpellSlot.Q) >= a.Health);
            }
        }

        private static
            void LastHit()
        {
            var qcheck = MiscMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var qready = Q.IsReady();
            if (!qcheck || !qready)
            {
                return;
            }
            var qminion = (Obj_AI_Minion) GetEnemy(Q.Range, GameObjectType.obj_AI_Minion);
            if (qminion != null)
            {
                Q.Cast(qminion);
            }
        }

        private static
            void LaneClear()
        {
            var qcheck = MiscMenu["LCQ"].Cast<CheckBox>().CurrentValue;
            var qready = Q.IsReady();
            if (!qcheck || !qready)
            {
                return;
            }
            var qminion = (Obj_AI_Minion) GetEnemy(Q.Range, GameObjectType.obj_AI_Minion);
            if (qminion != null)
            {
                Q.Cast(qminion);
            }
        }

        private static
            void Combo(bool useE, bool useW, bool useQ)
        {
            if (useE && E.IsReady())
            {
                var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                var predE = E.GetPrediction(eTarget).CastPosition;
                if (eTarget.IsValidTarget(E.Range))
                    if (EMenu["taunt" + eTarget.ChampionName].Cast<CheckBox>().CurrentValue)
                        if (Shen.CountEnemiesInRange(E.Range) <= 1)
                            if (E.GetPrediction(eTarget).HitChance >= EHitChance)
                            {
                                E.Cast(eTarget);
                            }
                            else if (Shen.CountEnemiesInRange(E.Width) >=
                                     EMenu["eslider"].Cast<Slider>().CurrentValue)
                            {
                                E.Cast(predE);
                            }

            if (useE && Q.IsReady())
            {
                var grabTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (grabTarget.IsValidTarget(Q.Range))
                {
                    Q.Cast(grabTarget);
                }
            }
            if (useW && W.IsReady())
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (target.IsValidTarget(Q.Range))
                {
                    if (target.Distance(Shen) < Q.Range)

                    {
                        W.Cast();
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
                case "Yellow Jacket":
                    Player.SetSkinId(1);
                    break;
                case "Frozen":
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