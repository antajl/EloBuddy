using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace Bloodimir_Ziggs
{
    public static class Spells
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot Q2;
        public static Spell.Skillshot Q3;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;

        static Spells()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, 250, 1700, 140);
            Q2 = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, 250, 1600, 140);
            Q3 = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, 250, 1600, 160);
            W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, 250, 1750, 275);
            E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 500, 1750, 100);
            R = new Spell.Skillshot(SpellSlot.R, 5000, SkillShotType.Circular, 1000, 1750, 500);
            if (Program.HasSpell("summonerdot"))
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            {
            }
        }
    }
}