using System;
using System.Collections.Generic;

namespace PayrollAnalytics.Api.DTOs
{
    public class HeadcountTrendDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Headcount { get; set; }
        public int NewHires { get; set; }
        public int Terminations { get; set; }
    }
}
