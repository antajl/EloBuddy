using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace Morgana
{
    class LaneClearA
    {
        public enum AttackSpell
        {
            W
        };
        public static AIHeroClient Morgana { get { return ObjectManager.Player; } }
        public static void LaneClear()
        {
            bool WCHECK = Program.LaneClear["LCW"].Cast<CheckBox>().CurrentValue;
            bool WREADY = Program.W.IsReady();

            if (WCHECK && WREADY)
            {
                Obj_AI_Base enemy = GetBestWLocation(GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.W.Cast(enemy.Position);
            }
        }
        public static Obj_AI_Base GetBestWLocation(GameObjectType type)
        {
            int numEnemiesInRange = 0;
            Obj_AI_Base enem = null;

            foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Base>()
                .OrderBy(a => a.Health)
                .Where(a => a.Distance(Morgana) <= Program.W.Range
                && a.IsEnemy
                && a.Type == type
                && !a.IsDead
                && !a.IsInvulnerable))
            {
                int tempNumEnemies = 0;
                foreach (Obj_AI_Base enemy2 in ObjectManager.Get<Obj_AI_Base>()
                    .OrderBy(a => a.Health)
                    .Where(a => a.Distance(Morgana) <= Program.W.Range
                    && a.IsEnemy
                    && !a.IsDead
                    && a.Type == type
                    && !a.IsInvulnerable))
                {
                    if (enemy != enemy2
                        && enemy2.Distance(enemy) <= 75)
                        tempNumEnemies++;
                }

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