using Java.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidAppWear
{
    internal class HrSamples
    {
        public string date;
        public short value;

        public HrSamples(string date, short value) 
        { 
            this.date = date;
            this.value = value;
        }
    }
}
