using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace BloodimirVladimir
{
	internal static class LastHitA
	{
	    private enum AttackSpell
		{
			Q
		};

	    private static AIHeroClient Vladimir
		{
			get { return ObjectManager.Player; }
		}

	    private static Obj_AI_Base MinionLh(GameObjectType type, AttackSpell spell)
		{
            return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
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
			var qcheck = Program.LastHit["LHQ"].Cast<CheckBox>().CurrentValue;
			var qready = Program.Q.IsReady();
			if (qcheck && qready)
			{
			var minion = (Obj_AI_Minion) MinionLh(GameObjectType.obj_AI_Minion, AttackSpell.Q);
			if (minion != null)
			{
				Program.Q.Cast(minion);
			}
		}
	}
}	}
