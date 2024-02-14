﻿using System;
using System.Diagnostics;

namespace Orangebeard.SpecFlowPlugin.Util
{
    public class PreciseUtcTime
    {
        private static readonly Stopwatch Stopwatch = new Stopwatch();
        private static DateTime _startTime;

        static PreciseUtcTime()
        {
            Reset();
        }

        private static void Reset()
        {
            _startTime = DateTime.UtcNow;
            Stopwatch.Restart();
        }

        public static DateTime UtcNow => _startTime.Add(Stopwatch.Elapsed);
    }
}