using System;
using System.Collections.Generic;

namespace RipCheck
{
    public class Warnings
    {
        private readonly List<string> warnings;

        public Warnings()
        {
            warnings = new List<string>();
        }

        public void Add(string warning)
        {
            warnings.Add(warning);
        }

        public void AddRange(Warnings newWarningSet)
        {
            if (newWarningSet is not null)
            {
                warnings.AddRange(newWarningSet.warnings);
            }
        }

        public void PrintWarnings()
        {
            foreach (string warning in warnings)
            {
                Console.WriteLine(warning);
            }
        }

        public bool IsEmpty()
        {
            return warnings.Count == 0;
        }
    }
}
