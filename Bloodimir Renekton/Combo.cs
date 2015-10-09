﻿using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Renekton
{
    internal class Combo
    {
        public enum AttackSpell
        {
            Q,
            E,
            W
        };

        private const string E2BuffName = "renektonsliceanddicedelay";

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

        public static void RenekCombo()
        {
            var qcheck = Program.ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var qready = Program.Q.IsReady();

            if (qcheck && qready)
            {
                var enemy = TargetSelector.GetTarget(Program.Q.Range, DamageType.Physical);

                if (enemy != null)
                    Program.Q.Cast();
            }
            if (Orbwalker.CanAutoAttack)
            {
                var cenemy = (AIHeroClient) GetEnemy(Player.Instance.GetAutoAttackRange(), GameObjectType.AIHeroClient);

                if (cenemy != null)
                    Orbwalker.ForcedTarget = cenemy;
            }
        }

        public static
            void Items()
        {
            var ienemy = TargetSelector.GetTarget(Player.Instance.GetAutoAttackRange() + 425,
                DamageType.Physical);
            if (ienemy != null)
            {
                if (ienemy.IsValid && !ienemy.IsZombie)
                {
                    if (Program.ComboMenu["useitems"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Botrk.IsOwned() && Program.Botrk.IsReady() &&
                            Program.Botrk.IsInRange(ienemy))
                            Program.Botrk.Cast(ienemy);
                    }
                    if (Program.ComboMenu["useitems"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Bilgewater.IsOwned() && Program.Bilgewater.IsReady())
                            Program.Bilgewater.Cast(ienemy);
                    }
                    if (Program.ComboMenu["useitems"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Hydra.IsOwned() && Program.Hydra.IsReady() &&
                            Program.Hydra.IsInRange(ienemy))
                            Program.Hydra.Cast();
                    }
                    if (Program.ComboMenu["useitems"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Tiamat.IsOwned() && Program.Tiamat.IsReady() &&
                            Program.Tiamat.IsInRange(ienemy))
                            Program.Tiamat.Cast();
                    }
                    if (Program.ComboMenu["useitems"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.Youmuu.IsOwned() && Program.Youmuu.IsReady())
                            Program.Youmuu.Cast();
                    }
                }
            }
        }

        public static void UseE()

        {
            var target = TargetSelector.GetTarget(Program.E.Range, DamageType.Physical);
            var eenemy = (Obj_AI_Minion) GetEnemy(Program.E.Range, GameObjectType.obj_AI_Minion);
            if (target != null && Player.Instance.Distance(target.Position) < Program.E.Range)
                if (Program.E.IsReady())
                {
                    Player.CastSpell(SpellSlot.E, target.Position);
                }
                else if (target != null && Player.Instance.Distance(target.Position) < 800 && eenemy != null &&
                         Player.Instance.Distance(eenemy.Position) < Program.E.Range &&
                         target.Distance(eenemy.Position) < Program.E.Range)
                {
                    Player.CastSpell(SpellSlot.E, eenemy.Position);
                    if (Player.HasBuff(E2BuffName))
                        Player.CastSpell(SpellSlot.E, target.Position);
                }
        }
    }
}