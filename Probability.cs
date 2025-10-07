using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveFunctionCollapseTest
{
    internal class Probability
    {
        public Probability(char thisType, List<char> types) {
            Type = thisType;

            // Initialize the dictionaries with all types set to 0
            foreach (var type in types)
            {
                Top[type] = 0;
                Right[type] = 0;
                Bottom[type] = 0;
                Left[type] = 0;
            }
        }

        public char Type { get; set; }
        public Dictionary<char, int> Top { get; set; } = new();
        public Dictionary<char, int> Right { get; set; } = new();
        public Dictionary<char, int> Bottom { get; set; } = new();
        public Dictionary<char, int> Left { get; set; } = new();


    }
}
