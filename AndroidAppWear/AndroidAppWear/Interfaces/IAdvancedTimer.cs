using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidAppWear.Interfaces
{
    internal interface IAdvancedTimer
    {
        void InitTimer(int interval, EventHandler e, bool autoReset);
        void StartTimer();
        void StopTimer();
        void PauseTimer();
        bool IsTimerEnabled { get; }
        int Interval { get; set; }

    }
}
