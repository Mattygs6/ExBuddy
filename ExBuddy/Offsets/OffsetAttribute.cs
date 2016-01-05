using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExBuddy.Offsets
{
    [AttributeUsage(AttributeTargets.All)]
    public class Offset : Attribute
    {
        public string Pattern;
        public bool IsOffset;
        public int Modifier;
        public bool MultipleResults;
        public Offset(string pattern, bool isoffset = false, int modifier = 0, bool multresults = false)
        {
            Pattern = pattern;
            IsOffset = isoffset;
            Modifier = modifier;
            MultipleResults = multresults;
        }
    }


    [AttributeUsage(AttributeTargets.All)]
    public class OffsetCN : Offset
    {

        public OffsetCN(string pattern, bool isoffset = false, int modifier = 0, bool multresults = false)
            : base(pattern, isoffset, modifier, multresults)
        {
        }
    }
}
