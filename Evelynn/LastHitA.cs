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
            Q,
            E
        };

        public static AIHeroClient Evelynn
        {
            get { return ObjectManager.Player; }
        }


        public static float Qcalc(Obj_AI_Base target)
        {
            return Evelynn.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] { 0, 80, 115, 150, 185, 220 }[Program.Q.Level] +
                 (0.80f * Evelynn.FlatMagicDamageMod)));
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