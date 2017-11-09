using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTesting
{
    /* All the classes in this file are essentially structs. The only reason they arent is because the XML serializer requires the classes to have parameterless constructors
     * These classes have been created to simulate the real worksheet record 
     */

    public class ExampleWorksheet2: IRandomInterface
    {
        public string WorksheetName { get; set; }
        public List<ElementWavelength2> Elements { get; set; }
        public List<Solution2> Solutions { get; set; }
        public bool IECEnabled { get; set; }
        public bool QCEnabled { get; set; }
        public bool IQEnabled { get; set; }
        public bool AutosamplerEnabled { get; set; }
        public bool AutoDilutorEnabled { get; set; }
        public bool OxygenInjectionEnabled { get; set; }
        public bool CustomReplicatesEnabled { get; set; }
        public bool IsoMistEnabled { get; set; }
        public bool DriftCorrectionEnabled { get; set; }
        public bool Setting1Enabled { get; set; }
        public bool Setting2Enabled { get; set; }
        public bool Setting3Enabled { get; set; }

        public ExampleWorksheet2()
        {
            WorksheetName = "DefaultWorksheetName";
            Elements = new List<ElementWavelength2>();
            Solutions = new List<Solution2>();
        }

        public bool FunctionTest(int foo)
        {
            return IECEnabled || QCEnabled || IQEnabled || AutosamplerEnabled || AutoDilutorEnabled || OxygenInjectionEnabled || CustomReplicatesEnabled || IsoMistEnabled || DriftCorrectionEnabled;
        }
    }

    public class ElementWavelength2
    {
        public string ElementName { get; set; }
        public float Wavelength { get; set; }
        public float ElementId { get; set; }

        public ElementWavelength2()
        {
        }
    }

    public class Replicate2
    {
        public  float MeasuredValue { get; set; }

        private static Random RNG = new Random();

        public Replicate2()
        {
            MeasuredValue = RNG.Next(0, 100000);
        }

    }

    public class Measurement2
    {
        public int SolutionId { get; set; }
        public int ElementId { get; set; }
        public decimal Intensity { get; set; }
        public decimal Conc { get; set; }
    }

    public class Solution2
    {
        public string SolutionName { get; set; }
        public int NumberOfReplicates { get; set; }
        public List<Replicate2> Replicates { get; set; }
        public List<Measurement2> Measurements { get; set; }

        public int SolutionTypeId
        {
            get { return (int) this.SolutionType; }
            set { SolutionType = (SolutionType) value; }
        }
        public SolutionType SolutionType { get; set; }
        public int SolutionID { get; set; }
        private static int SolutionIDCounter;

        public Solution2()
        {
            SolutionName = "Default Solution Name";
            SolutionType = SolutionType.Sample;
            Replicates = new List<Replicate2>();

            NumberOfReplicates = 3;

            for (int i = 0; i < NumberOfReplicates; i++)
            {
                Replicates.Add(new Replicate2());
            }

            SolutionID = SolutionIDCounter;
            SolutionIDCounter++;
            
        }
    }

    public class Preferences2
    {

    }

}
