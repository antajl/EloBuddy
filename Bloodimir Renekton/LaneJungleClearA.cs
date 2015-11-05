using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Renekton
{
    internal class LaneJungleClearA
    {
        public enum AttackSpell
        {
            Q,
            E,
            W,
            Tiamat,
            Hydra
        };

        public static AIHeroClient Renekton
        {
            get { return ObjectManager.Player; }
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


        public static void LaneClear()
        {
            var ECHECK = Program.LaneJungleClear["LCE"].Cast<CheckBox>().CurrentValue;
            var EREADY = Program.E.IsReady();
            var QCHECK = Program.LaneJungleClear["LCQ"].Cast<CheckBox>().CurrentValue;
            var WCHECK = Program.LaneJungleClear["LCW"].Cast<CheckBox>().CurrentValue;
            var QREADY = Program.Q.IsReady();
            var WREADY = Program.W.IsReady();

            if (ECHECK && EREADY)
            {
            {
                var aenemy = (Obj_AI_Minion) GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);

                if (aenemy != null)
                    Program.E.Cast(aenemy.ServerPosition);
            }
            if (QCHECK && QREADY)
            {
            {
                var qenemy = (Obj_AI_Minion) GetEnemy(Program.Q.Range, GameObjectType.obj_AI_Minion);

                if (qenemy != null)
                    Program.Q.Cast();
            }
            if (WCHECK && WREADY)
            {
            {
                var wenemy =
                    (Obj_AI_Minion) GetEnemy(Player.Instance.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                if (wenemy != null &&  Renekton.GetSpellDamage(wenemy, SpellSlot.Q) >= wenemy.Health)
                    Program.W.Cast();
            }
            if (Orbwalker.CanAutoAttack)
            {
                var cenemy = (Obj_AI_Minion) GetEnemy(Renekton.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                if (cenemy != null)
                    Orbwalker.ForcedTarget = cenemy;
            }
            }
            }
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
                    if (Program.LaneJungleClear["LCI"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Hydra.IsOwned() && Program.Hydra.IsReady() &&
                            Program.Hydra.IsInRange(ienemy))
                            Program.Hydra.Cast();
                    }
                    if (Program.LaneJungleClear["LCI"].Cast<CheckBox>().CurrentValue)
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