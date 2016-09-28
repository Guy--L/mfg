using System;
using System.Collections.Generic;
using NPoco;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public partial class SolutionRecipe
    {
        [Ignore] public static Dictionary<int, string> Solutions;

        static SolutionRecipe()
        {
            Solutions = repo.Fetch<SolutionRecipe>().ToDictionary(k => k.SolutionRecipeId, v => v.SolutionType);
        }
    }
}