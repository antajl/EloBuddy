using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        static Spells()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, (int) 250f, (int) 1700f, (int) 140f);
            Q2 = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, (int) 250f, (int) 1600f,
                (int) 140f);
            Q3 = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, (int) 250f, (int) 1600f,
                (int) 160f);
            W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, (int) 250f, (int) 1750f, (int) 275f);
            E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, (int) 500f, (int) 1750f, (int) 100f);
            R = new Spell.Skillshot(SpellSlot.R, 5000, SkillShotType.Circular, (int) 1000f, (int) 1750f, (int) 500f);
    {
    }
            }        
        }
    }
