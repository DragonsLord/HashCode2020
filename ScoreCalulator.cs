using System;
using System.Collections.Generic;
using System.Linq;

namespace HashCodeProblem
{
    public static class ScoreCalulator
    {
        public static int ShowResult(List<LibraryScanPlan> plans)
        {
            return plans.SelectMany(_ => _.BooksToScan)
             .Distinct()
             .Select(_ => _.Score)
             .Sum();
        }
    }
}
