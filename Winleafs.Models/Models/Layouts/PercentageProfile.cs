﻿using System.Collections.Generic;

namespace Winleafs.Models.Models.Layouts
{
    public class PercentageProfile
    {
        public List<PercentageStep> Steps { get; set; }

        public PercentageProfile()
        {
            Steps = new List<PercentageStep>();
        }
    }
}
