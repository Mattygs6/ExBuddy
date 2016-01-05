using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExBuddy.Offsets;

namespace ExBuddy.SeconadryOffsets
{

    public static class Bait
    {

        //dword_1442828
        [Offset("Search 3B 05 ? ? ? ? 74 D8 Add 2 Read32")]
        public static IntPtr SelectedBaitItemIdPointer;
    }


}
