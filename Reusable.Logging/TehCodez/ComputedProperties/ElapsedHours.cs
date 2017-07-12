﻿using System;

namespace Reusable.Logging.Loggex.ComputedProperties
{
    public class ElapsedHours : Elapsed
    {
        protected override double Compute(TimeSpan elapsed) => elapsed.TotalHours;
    }
}