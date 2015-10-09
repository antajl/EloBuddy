using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Morgana
{
    internal class LaneClearA
    {
        public enum AttackSpell
        {
            W
        };

        public static AIHeroClient Morgana
        {
            get { return ObjectManager.Player; }
        }

        public static void LaneClear()
        {
            var WCHECK = Program.LaneClear["LCW"].Cast<CheckBox>().CurrentValue;
            var WREADY = Program.W.IsReady();

            if (WCHECK && WREADY)
            {
                var enemy = GetBestWLocation(GameObjectType.obj_AI_Minion);

                if (enemy != null)
                    Program.W.Cast(enemy.ServerPosition);
            }
        }

        public static Obj_AI_Base GetBestWLocation(GameObjectType type)
        {
            var numEnemiesInRange = 0;
            Obj_AI_Base enem = null;

            foreach (var enemy in ObjectManager.Get<Obj_AI_Base>()
                .OrderBy(a => a.Health)
                .Where(a => a.Distance(Morgana) <= Program.W.Range
                            && a.IsEnemy
                            && a.Type == type
                            && !a.IsDead
                            && !a.IsInvulnerable))
            {
                var tempNumEnemies = 0;
                foreach (var enemy2 in ObjectManager.Get<Obj_AI_Base>()
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