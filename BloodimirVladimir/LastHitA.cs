using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace BloodimirVladimir
{
	internal class LastHitA
	{
		public enum AttackSpell
		{
			Q
		};

		public static AIHeroClient Vladimir
		{
			get { return ObjectManager.Player; }
		}

		public static Obj_AI_Base MinionLH(GameObjectType type, AttackSpell spell)
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
			                                                                                   a.Health <= Misc.Qdmg(a));
		}

		public static void LastHitB()
		{
			var QCHECK = Program.LastHit["LHQ"].Cast<CheckBox>().CurrentValue;
			var QREADY = Program.Q.IsReady();
			if (!QCHECK || !QREADY)
			{
				return;
			}

			var minion = (Obj_AI_Minion) MinionLH(GameObjectType.obj_AI_Minion, AttackSpell.Q);
			if (minion != null)
			{
				Program.Q.Cast(minion);
			}
		}
	}
}