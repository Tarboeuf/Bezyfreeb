using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BezyFreebGlobal
{
    public class MapFreebox
    {
        public string IP { get; set; }

        public string AppToken { get; set; }
    }

    public class MapFreeboxCollection : List<MapFreebox>
    {
    }
}