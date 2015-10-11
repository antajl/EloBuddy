using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Evelynn
{
    internal class LastHitA
    {
        public enum AttackSpell
        {
            Q
        };

        public static AIHeroClient Evelynn
        {
            get { return ObjectManager.Player; }
        }

        public static float Qcalc(Obj_AI_Base target)
        {
            return Evelynn.CalculateDamageOnUnit(target, DamageType.Magical,
                new float[] {0, 40, 50, 60, 70, 80}[Program.Q.Level] +
                new float[] {0, 35, 40, 45, 50, 55}[Program.Q.Level]/100*Player.Instance.FlatMagicDamageMod +
                new float[] {0, 50, 55, 60, 65, 70}[Program.Q.Level]/100*Player.Instance.FlatPhysicalDamageMod);
        }

        public static Obj_AI_Base GetEnemy(float range, GameObjectType t)
        {
            switch (t)
            {
                case GameObjectType.obj_AI_Hero:
                    return EntityManager.Heroes.Enemies.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
                default:
                    return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(
                        a =>
                            a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable &&
                            a.Health <= Qcalc(a));
            }
        }

        public static void LastHitB()
        {
            var QCHECK = Program.LastHitMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            if (!QCHECK || !QREADY)
            {
                return;
            }

            var minion = (Obj_AI_Minion) GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);
            if (minion != null)
            {
                Program.Q.Cast();
            }
        }
    }
}