using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Renekton
{
    internal class LastHitA
    {
        public enum AttackSpell
        {
            Q,
            W,
            Hydra,
            Tiamat
        };

        public static AIHeroClient Renekton
        {
            get { return ObjectManager.Player; }
        }

        public static float Qcalc(Obj_AI_Base target)
        {
            return Renekton.CalculateDamageOnUnit(target, DamageType.Physical,
                (new float[] {0, 60, 90, 120, 150, 180}[Program.Q.Level] +
                 (0.80f*Renekton.FlatPhysicalDamageMod)));
        }

        public static Obj_AI_Base MinionLh(GameObjectType type, AttackSpell spell)
        {
            return ObjectManager.Get<Obj_AI_Base>().OrderBy(a => a.Health).FirstOrDefault(a => a.IsEnemy
                                                                                               && a.Type == type
                                                                                               &&
                                                                                               a.Distance(Renekton) <=
                                                                                               Program.Q.Range
                                                                                               && !a.IsDead
                                                                                               && !a.IsInvulnerable
                                                                                               &&
                                                                                               a.IsValidTarget(
                                                                                                   Program.Q.Range)
                                                                                               &&
                                                                                               a.Health <= Qcalc(a));
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
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
            }
        }

        public static void LastHitB()
        {
            var QCHECK = Program.LastHit["LHQ"].Cast<CheckBox>().CurrentValue;
            var WCHECK = Program.LastHit["LHW"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            var WREADY = Program.W.IsReady();

            if (!QCHECK || !QREADY)
            {
                return;
            }
            var minion = (Obj_AI_Minion)MinionLh(GameObjectType.obj_AI_Minion, AttackSpell.Q);
            if (minion != null)
            {
                Program.Q.Cast();
            }
                if (WCHECK || WREADY)
                {
                    return;
                }
                    var wminion = (Obj_AI_Minion) GetEnemy(Player.Instance.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);
                    if (wminion != null && Renekton.GetSpellDamage(wminion, SpellSlot.Q) >= wminion.Health)
                    {
                        Program.W.Cast();
                    }
                }
            
     
        public static
            void Items()
        {
            var ienemy =
                (Obj_AI_Minion) GetEnemy(Player.Instance.GetAutoAttackRange() + 335, GameObjectType.obj_AI_Minion);


            if (ienemy != null)
            {
                if (ienemy.IsValid && !ienemy.IsZombie)
                {
                    if (Program.LastHit["LHI"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Hydra.IsOwned() && Program.Hydra.IsReady() &&
                            Program.Hydra.IsInRange(ienemy))
                            Program.Hydra.Cast();
                    }
                    if (Program.LastHit["LHI"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Tiamat.IsOwned() && Program.Tiamat.IsReady() &&
                            Program.Tiamat.IsInRange(ienemy))
                            Program.Tiamat.Cast();
                    }
                }
            }
        }
    }
}