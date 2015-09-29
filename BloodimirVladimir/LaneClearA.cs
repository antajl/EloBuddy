using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace BloodimirVladimir
{
	internal class LaneClearA
	{
		public enum AttackSpell
		{
			E,
			Q
		};

		public static AIHeroClient Vladimir
		{
			get { return ObjectManager.Player; }
		}

		public static Obj_AI_Base GetEnemy(float range, GameObjectType type)
		{
			return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
			                                                                                   && a.Type == type
			                                                                                   &&
			                                                                                   a.Distance(Vladimir) <=
			                                                                                   range
			                                                                                   && !a.IsDead
			                                                                                   && !a.IsInvulnerable
			                                                                                   && a.IsValidTarget(range));
		}

		public static Obj_AI_Base GetEnemy(GameObjectType type, AttackSpell spell)
		{
			if (spell == AttackSpell.E)
			{
				return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
				                                                                                   && a.Type == type
				                                                                                   &&
				                                                                                   a.Distance(Vladimir) <=
				                                                                                   Program.E.Range
				                                                                                   && !a.IsDead
				                                                                                   && !a.IsInvulnerable
				                                                                                   &&
				                                                                                   a.IsValidTarget(
					                                                                                   Program.E.Range)
				                                                                                   &&
				                                                                                   a.Health <=
				                                                                                   Misc.Edmg(a));
			}

			if (spell == AttackSpell.Q)
			{
				return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
				                                                                                   && a.Type == type
				                                                                                   &&
				                                                                                   a.Distance(Vladimir) <=
				                                                                                   Program.Q.Range
				                                                                                   && !a.IsDead
				                                                                                   && !a.IsInvulnerable
				                                                                                   &&
				                                                                                   a.IsValidTarget(
					                                                                                   Program.Q.Range)
				                                                                                   &&
				                                                                                   a.Health <=
				                                                                                   Misc.Qdmg(a));
			}

			return null;
		}

		public static void LaneClear()
		{
			var ECHECK = Program.LaneClear["LCE"].Cast<CheckBox>().CurrentValue;
			var EREADY = Program.E.IsReady();
			var QCHECK = Program.LaneClear["LCQ"].Cast<CheckBox>().CurrentValue;
			var QREADY = Program.Q.IsReady();
			if (QCHECK && QREADY)
			{
				var enemy = (Obj_AI_Minion) GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

				if (enemy != null)
					Program.Q.Cast(enemy);
			}

			if (ECHECK && EREADY)
			{
				var enemy = GetBestELocation(GameObjectType.obj_AI_Minion);

				if (enemy != null)
					Program.E.Cast();
			}
			if (Orbwalker.CanAutoAttack)
			{
				var enemy = (AIHeroClient) GetEnemy(Vladimir.GetAutoAttackRange(), GameObjectType.AIHeroClient);

				if (enemy != null)
					Orbwalker.ForcedTarget = enemy;
			}
		}

		public static Obj_AI_Base GetBestELocation(GameObjectType type)
		{
			var numEnemiesInRange = 0;
			Obj_AI_Base enem = null;

			foreach (var enemy in ObjectManager.Get<Obj_AI_Base>()
				.OrderBy(a => a.Health)
				.Where(a => a.Distance(Vladimir) <= Program.E.Range
				            && a.IsEnemy
				            && a.Type == type
				            && !a.IsDead
				            && !a.IsInvulnerable))
			{
				var tempNumEnemies =
					ObjectManager.Get<Obj_AI_Base>()
						.OrderBy(a => a.Health)
						.Where(
							a =>
								a.Distance(Vladimir) <= Program.E.Range && a.IsEnemy && !a.IsDead && a.Type == type &&
								!a.IsInvulnerable)
						.Count(enemy2 => enemy != enemy2 && enemy2.Distance(enemy) <= 75);
				if (tempNumEnemies > numEnemiesInRange)
				{
					enem = enemy;
					numEnemiesInRange = tempNumEnemies;
				}
			}

			return enem;
		}
	}
}