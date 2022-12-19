using Android.Content;
using Android.OS;
using Android.Runtime;
using Java.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AndroidAppWear.Services
{
    [Service(Label = "TimerService")]
    internal class TimerService : Service
    {
        int counter = 0;
        bool isRunningTimer = true;

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                MessagingCenter.Send<string>(counter.ToString(), "counterValues");
                // MessagingCenter.Send<string>("lol", "counterValues");
                //counter++;

                return isRunningTimer;
            });
            return StartCommandResult.Sticky;
        }
        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            StopSelf();
            counter = 0;
            isRunningTimer= false;
            base.OnDestroy();
        }
    }
}
