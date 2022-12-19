using Android.OS;
using AndroidAppWear;
using AndroidAppWear.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(AdvancedTimer))]
namespace AndroidAppWear
{
    internal class AdvancedTimer : IAdvancedTimer
    {
        System.Timers.Timer timer;
        int interval;
        bool autoReset;
        

        public void InitTimer(int interval, EventHandler e, bool autoReset)
        {
            if (this.timer == null)
            {
                this.timer = new System.Timers.Timer(interval);
                this.timer.Elapsed += new System.Timers.ElapsedEventHandler(e);
            }

            this.interval = interval;
            this.autoReset = autoReset;

            this.timer.AutoReset = autoReset;
        }

        public void PauseTimer()
        {
            if (this.timer != null)
            {
                
                   this.timer.Enabled = !timer.Enabled;

            }
            
        }

        public void StartTimer()
        {
            timer.Enabled = true;
            timer.Start();
        }

        public void StopTimer()
        {
            this.timer.Enabled = false;
            this.timer.Stop();
        }

        public bool IsTimerEnabled => this.timer.Enabled;

        public int Interval 
        { 
            get => this.interval; 
            set
            {
                this.interval = value;
                this.timer.Interval = value;
            }
        }
    }
}
