namespace YAZLAB2.Models
{
    using System.Collections.Generic;

    public class StatisticsViewModel
    {
        public Dictionary<string, int> UserCountByGender { get; set; }
        public Dictionary<int, int> EventCountByCategory { get; set; } 
        public Dictionary<int, int> UserCountByAgeGroup { get; set; }
        public Dictionary<string, int> UserPoints { get; set; } 

    }

}
