using System;
using System.Collections.Generic;
using System.Text;

namespace Shizukanawa.KaeruBot.Objects
{
    public class Champions
    {
        public class RootChampions
        {
            public string type { get; set; }
            public string format { get; set; }
            //The string in Dictionary is the champions name. i.e the objects name where the first key is Aatrox.
            public Dictionary<string, Champion> Data { get; set; }
        }

        public class Champion
        {
            public string version { get; set; }
            public string id { get; set; }
            public string key { get; set; }
            public string name { get; set; }
            public string title { get; set; }
            public string blurb { get; set; }
        }
    }

}
