using Android.Content;
using Android.Gestures;
using Android.OS;
using Android.Runtime;
using Java.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace AndroidAppWear.Services
{
    [Service(Label = "TimerService")]
    internal class TimerService : Service
    {
        System.Timers.Timer timer;
        public static string BROADCAST_ACTION = "timer.elapsed";
        public override void OnCreate()
        {
            base.OnCreate();
            timer = new System.Timers.Timer(1000) { AutoReset = true};
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
        }

        public override void OnDestroy()
        {
            timer.Stop();
            timer.Dispose();
            base.OnDestroy();
        }

        

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var intent = new Intent(BROADCAST_ACTION);
            SendBroadcast(intent);

        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            timer.Start();
            return StartCommandResult.Sticky;
        }

        public override IBinder? OnBind(Intent? intent)
        {
             return null;
        }
    }
}
