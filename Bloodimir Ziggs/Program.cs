using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Ziggs
{
    internal class Program
    {
        public static Menu ZiggsMenu, ComboMenu, LaneJungleClear, SkinMenu,  LastHit, MiscMenu;
        public static AIHeroClient Ziggs = ObjectManager.Player;

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {

            Loading.OnLoadingComplete += Yuklendi;
            Interrupter.OnInterruptableSpell += Interruptererer;
            Game.OnTick += Tick;
        }

        private static void Yuklendi(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Ziggs")
                return;
            Bootstrap.Init(null);

            ZiggsMenu = MainMenu.AddMenu("BloodimirZiggs", "bloodimirziggs");
            ZiggsMenu.AddGroupLabel("Bloodimir.Ziggs");
            ZiggsMenu.AddSeparator();
            ZiggsMenu.AddLabel("Bloodimir Ziggs v1.0.0.0");

            ComboMenu = ZiggsMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecombor", new CheckBox("Use R"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rslider", new Slider("Minimum people for R", 1, 0, 5));

            LaneJungleClear = ZiggsMenu.AddSubMenu("Lane Jungle Clear", "lanejungleclear");
            LaneJungleClear.AddGroupLabel("Lane Jungle Clear Settings");
            LaneJungleClear.Add("LCE", new CheckBox("Use E"));
            LaneJungleClear.Add("LCQ", new CheckBox("Use Q"));


            LastHit = ZiggsMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q"));

            MiscMenu = ZiggsMenu.AddSubMenu("Misc Menu", "miscmenu");
            MiscMenu.AddGroupLabel("KS");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS using Q"));
            MiscMenu.Add("int", new CheckBox("TRY to Interrupt spells"));
            
            SkinMenu = ZiggsMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 4, 0, 5));
            var sID = new[] {"Default", "Mad Scientist", "Major", "Pool Party", "Snow Day", "Master Arcanist"};
            skinchange.DisplayName = sID[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sID[changeArgs.NewValue];
                };
        }

        private static void Interruptererer(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            var intTarget = TargetSelector.GetTarget(Spells.W.Range, DamageType.Magical);
            {
                if (Spells.W.IsReady() && sender.IsValidTarget(Spells.W.Range) && MiscMenu["int"].Cast<CheckBox>().CurrentValue)
                    Spells.W.Cast(intTarget.ServerPosition);
                if (Spells.W.IsReady())
                   Spells.W.Cast();
            }
        }

        public static void Flee()
        {
            Orbwalker.MoveTo(Game.CursorPos);
            Spells.W.Cast(Ziggs.Position);
        }

        private static void Tick(EventArgs args)
        {
            Killsteal();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo.ZiggsCombo();
                Rincombo(ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue);
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
            }
            SkinChange();
        }

        public static void Rincombo(bool useR)
        {
            if (ComboMenu["usecombor"].Cast<CheckBox>().CurrentValue)
                if (useR && Spells.R.IsReady() &&
                    Ziggs.CountEnemiesInRange(Spells.R.Range) >= ComboMenu["rslider"].Cast<Slider>().CurrentValue)
                {
                    var rtarget = TargetSelector.GetTarget(1975, DamageType.Magical);
                    Spells.R.Cast(rtarget.ServerPosition);
                }
        }
        private static void Killsteal()
        {
            if (MiscMenu["ksq"].Cast<CheckBox>().CurrentValue && Spells.Q.IsReady())
            {
                try
                {
                    foreach (
                        var qtarget in
                            EntityManager.Heroes.Enemies.Where(
                                hero => hero.IsValidTarget(Spells.Q.Range) && !hero.IsDead && !hero.IsZombie))
                    {
                        if (Ziggs.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                        {
                            {
                                Spells.Q.Cast(qtarget.ServerPosition);
                            }   
                                    {
                                    }
                                }
                            }
                }
                catch
                {
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
                case "Mad Scientist":
                    Player.SetSkinId(1);
                    break;
                case "Major":
                    Player.SetSkinId(2);
                    break;
                case "Pool Party":
                    Player.SetSkinId(3);
                    break;
                case "Snow Day":
                    Player.SetSkinId(4);
                    break;
                case "Master Arcanist":
                    Player.SetSkinId(5);
                    break;
            }
        }
    }
}