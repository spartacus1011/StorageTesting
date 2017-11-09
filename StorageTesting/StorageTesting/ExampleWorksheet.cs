using System;
using System.Collections.Generic;

namespace StorageTesting
{
    public class ExampleWorksheet //worksheet record
    {
        public string WorksheetName;
        public List<ElementWavelength> ElementWavelengths = new List<ElementWavelength>();
        public List<Solution> Solutions = new List<Solution>();

        public ExampleWorksheet()
        {
        }
    }

    public class ElementWavelength
    {
        public string ElementName;
        public float Wavelength;

        public ElementWavelength(string elementName = "DefaultElementName",float wavelength = 0.0f)
        {
            ElementName = elementName;
            Wavelength = wavelength;

        }
    }

    public class Replicate
    {
        public readonly float MeasuredValue;

        private static Random RNG = new Random();

        public Replicate(float measuredValue = -1)
        {
            if (measuredValue == -1)
                MeasuredValue = RNG.Next(0,100000);
            else
                MeasuredValue = measuredValue;
        }

    }

    public class Solution
    {
        public string SolutionName;
        public int NumberOfReplicates;
        public List<Replicate> Replicates = new List<Replicate>();
        public SolutionType SolutionType;
        public Solution(string solutionName,SolutionType solutionType , int numberOfReplicates = 3)
        {
            SolutionName = solutionName;
            SolutionType = solutionType;

            NumberOfReplicates = numberOfReplicates;

            for (int i = 0; i < numberOfReplicates; i++)
            {
                Replicates.Add(new Replicate());
            }
        }
    }
}
