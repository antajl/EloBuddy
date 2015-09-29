using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace BloodimirVladimir
{
	internal class Program
	{
		public static Spell.Targeted Q;
		public static Spell.Active W;
		public static Spell.Active E;
		public static Spell.Skillshot R;
		public static Spell.Targeted Ignite;
		public static Menu VladMenu, ComboMenu, DrawMenu, SkinMenu, MiscMenu, LaneClear, LastHit;
		public static AIHeroClient Vlad = ObjectManager.Player;

		public static AIHeroClient _Player
		{
			get { return ObjectManager.Player; }
		}

		public static int health
		{
			get { return (int) _Player.Health; }
		}

		private static void Main(string[] args)
		{
			Loading.OnLoadingComplete += OnLoaded;
		}

		private static void OnLoaded(EventArgs args)
		{
			if (Player.Instance.ChampionName != "Vladimir")
				return;
			Bootstrap.Init(null);
			Q = new Spell.Targeted(SpellSlot.Q, 600);
			W = new Spell.Active(SpellSlot.W);
			E = new Spell.Active(SpellSlot.E, 610);
			R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Circular, (int) 250f, (int) 1200f, (int) 150f);
			var summoner1 = _Player.Spellbook.GetSpell(SpellSlot.Summoner1);
			var summoner2 = _Player.Spellbook.GetSpell(SpellSlot.Summoner2);
			if (summoner1.Name == "summonerdot")
				Ignite = new Spell.Targeted(SpellSlot.Summoner1, 599);
			else if (summoner2.Name == "summonerdot")
				Ignite = new Spell.Targeted(SpellSlot.Summoner2, 599);
			VladMenu = MainMenu.AddMenu("Bloodimir", "bloodimir");
			VladMenu.AddGroupLabel("Bloodimir.Bloodimir");
			VladMenu.AddSeparator();
			VladMenu.AddLabel("Bloodimir c what i did there?");

			ComboMenu = VladMenu.AddSubMenu("Combo", "sbtw");
			ComboMenu.AddGroupLabel("Combo Settings");
			ComboMenu.AddSeparator();
			ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
			ComboMenu.Add("usecomboe", new CheckBox("Use E"));
			ComboMenu.Add("usecombor", new CheckBox("Use R"));
			ComboMenu.Add("useignite", new CheckBox("Use Ignite"));
			ComboMenu.AddSeparator();
			ComboMenu.Add("rslider", new Slider("Minimum people for R", 1, 0, 5));

			DrawMenu = VladMenu.AddSubMenu("Drawings", "drawings");
			DrawMenu.AddGroupLabel("Drawings");
			DrawMenu.AddSeparator();
			DrawMenu.Add("drawq", new CheckBox("Draw Q"));
			DrawMenu.Add("drawe", new CheckBox("Draw E"));

			LaneClear = VladMenu.AddSubMenu("Lane Clear", "laneclear");
			LaneClear.AddGroupLabel("Lane Clear Settings");
			LaneClear.Add("LCE", new CheckBox("Use E"));
			LaneClear.Add("LCQ", new CheckBox("Use Q"));

			LastHit = VladMenu.AddSubMenu("Last Hit", "lasthit");
			LastHit.AddGroupLabel("Last Hit Settings");
			LastHit.Add("LHQ", new CheckBox("Use Q"));

			MiscMenu = VladMenu.AddSubMenu("Misc Menu", "miscmenu");
			MiscMenu.AddGroupLabel("KS");
			MiscMenu.AddSeparator();
			MiscMenu.Add("ksq", new CheckBox("KS with Q"));
			MiscMenu.AddSeparator();
			MiscMenu.Add("ksignite", new CheckBox("Ks with Ignite"));
			MiscMenu.AddSeparator();
			MiscMenu.Add("dodgew", new CheckBox("Use W to Dodge WIP"));
			MiscMenu.AddSeparator();
			MiscMenu.Add("debug", new CheckBox("Debug", false));
			MiscMenu.Add("ehealth", new Slider("E Health Manager (%)", 50));

			SkinMenu = VladMenu.AddSubMenu("Skin Changer", "skin");
			SkinMenu.AddGroupLabel("Choose the desired skin");

			var skinchange = SkinMenu.Add("sID", new Slider("Skin", 0, 0, 7));
			var sID = new[]
			{"Default", "Count", "Marquius", "Nosferatu", "Vandal", "Blood Lord", "Soulstealer", "Academy"};
			skinchange.DisplayName = sID[skinchange.CurrentValue];
			skinchange.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
			{
				sender.DisplayName = sID[changeArgs.NewValue];
				if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
				{
					Chat.Print("skin-changed");
				}
			};

			Game.OnTick += Tick;
			Drawing.OnDraw += OnDraw;
		}

		private static void OnDraw(EventArgs args)
		{
			if (!Vlad.IsDead)
			{
				if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
				{
					Drawing.DrawCircle(Vlad.Position, Q.Range, Color.DarkBlue);
				}
				{
					if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && E.IsLearned)
					{
						Drawing.DrawCircle(Vlad.Position, E.Range, Color.DarkGreen);
					}
				}
			}
		}

		public static void Flee()
		{
			Orbwalker.MoveTo(Game.CursorPos);
			W.Cast();
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
				Combo.VladCombo();
			}
			{
				if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
				    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
				{
					LaneClearA.LaneClear();
				}
				if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
				{
					LastHitA.LastHitB();
				}
				Misc.EHslider();
			}
			SkinChange();
		}

		private static void Killsteal()
		{
			var enemy = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
			if (MiscMenu["ksq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
			{
				try
				{
					foreach (
						var qtarget in
							HeroManager.Enemies.Where(
								hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
					{
						if (Vlad.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
						{
							Q.Cast(enemy);
							if (MiscMenu["debug"].Cast<CheckBox>().CurrentValue)
							{
								Chat.Print("q-ks");
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
				case "Count":
					Player.SetSkinId(1);
					break;
				case "Marquius":
					Player.SetSkinId(2);
					break;
				case "Nosferatu":
					Player.SetSkinId(3);
					break;
				case "Vandal":
					Player.SetSkinId(4);
					break;
				case "Blood Lord":
					Player.SetSkinId(5);
					break;
				case "Soulstealer":
					Player.SetSkinId(6);
					break;
				case "Academy":
					Player.SetSkinId(7);
					break;
			}
		}
	}
}