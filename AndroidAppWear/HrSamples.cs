﻿using Java.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidAppWear
{
    internal class HrSamples
    {
        public string TimeStamp;
        public short HRValue;

        public HrSamples(string date, short value) 
        { 
            this.TimeStamp = date;
            this.HRValue = value;
        }
    }
}
