using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTesting
{
    public interface IRandomInterface
    {
        bool Setting1Enabled { get; set; }
        bool Setting2Enabled { get; set; }
        bool Setting3Enabled { get; set; }
    }
}
