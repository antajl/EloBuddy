﻿using System;

namespace VolatileAIO.Organs.Brain
{
    class TickManager : Heart
    {
        public static int Index = 0;

        public static void Tick()
        {
            Index++;
            if (Index > 4)
                Index = 0;
        }

        public static bool NoLag(int offset)
        {
            return Index == offset;
        }
    }
}
