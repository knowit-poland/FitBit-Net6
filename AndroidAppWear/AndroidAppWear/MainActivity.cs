namespace AndroidAppWear
{
    using Android;
    using Android.Hardware;
    using Android.Nfc;
    using Android.OS;
    using Android.Runtime;
    using Android.Util;
    using Android.Webkit;
    using Java.Security;
    using Newtonsoft.Json;
    using System;
    using System.Security.Permissions;
    using System.Timers;
    using Xamarin;
    using Xamarin.Essentials;

    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity, ISensorEventListener
    {

        Button buttonStart;
        Button buttonStop;
        Button buttonPause;
        TextView textHR;
        TextView textTime;
        TextView textLocation;
        Timer timer = new Timer();
        
        SensorManager mSensorManager;
        Sensor mHeartRateSensor;

        short ms = 0;
        short s = 0;
        short m = 0;
        short h = 0;

        short hr;

        List<HrSample> hrSamples = new List<HrSample>(); 
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            await TryToGetPermissions();
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


            buttonStart = FindViewById<Button>(Resource.Id.button_start);
            buttonStop = FindViewById<Button>(Resource.Id.button_stop);
            buttonPause = FindViewById<Button>(Resource.Id.button_pause);
            textHR = FindViewById<TextView>(Resource.Id.hr_text);
            textTime = FindViewById<TextView>(Resource.Id.time_text);
            textLocation = FindViewById<TextView>(Resource.Id.location_text);

            buttonPause.Click += ButtonPause_Click;
            buttonStop.Click += ButtonStop_Click;
            buttonStart.Click += ButtonStart_Click;


            mSensorManager = ((SensorManager)GetSystemService(SensorService));
            if (checkSensor())
            {
                mHeartRateSensor = mSensorManager.GetDefaultSensor(SensorType.HeartRate);

            }

            ButtonStop_Click(null, null);
           
        }

        private bool checkSensor()
        {
            if (mSensorManager.GetSensorList(SensorType.HeartRate).Count != 0)
                return true;
            return false;
                
        }

        private void restartTimer()
        {
            ms = 0;
            s = 0;
            m = 0;
            h = 0;
        }

        private async void ButtonStart_Click(object? sender, EventArgs e)
        {
            if (checkSensor())
                mSensorManager.RegisterListener(this, mHeartRateSensor, SensorDelay.Normal);
            
            timer = new Timer();
            timer.Interval = 10;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            timer.Start();

            buttonStart.Enabled = false;
            buttonStart.Visibility = Android.Views.ViewStates.Invisible;
            buttonStop.Enabled = true;
            buttonStop.Visibility = Android.Views.ViewStates.Visible;
            buttonPause.Enabled = true;
            buttonPause.Visibility = Android.Views.ViewStates.Visible;

            
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            ms++;

            if (ms > 99)
            {
                s++;
                ms = 0;
            }
            if (s == 59)
            {
                m++;
                s = 0;
            }
            if (m == 59)
            {
                h++;
                m = 0;
            }

            RunOnUiThread(() =>
            {
                textTime.Text = String.Format("{0:00}:{1:00}:{2:00}:{3:00}", h, m, s, ms);
            });

        }

        

        private void ButtonStop_Click(object? sender, EventArgs e)
        {
            if (checkSensor())
                mSensorManager.UnregisterListener(this);
            
            RunOnUiThread(() =>
            {
                textHR.Text = "HR: 0" ;
            });

            buttonStop.Enabled = false;
            buttonStop.Visibility = Android.Views.ViewStates.Invisible;
            buttonPause.Enabled = false;
            buttonPause.Visibility = Android.Views.ViewStates.Invisible;
            buttonStart.Enabled = true;
            buttonStart.Visibility = Android.Views.ViewStates.Visible;


            timer.Enabled = false;
            timer.Stop();
            restartTimer();

            Data data = new Data()
            {
                hrSamples = hrSamples,
                locationSamples = "location"
            };

            string toWrite = JsonConvert.SerializeObject(data);
            File.WriteAllText(@"C:\Users\adakuc\source\repos\AndroidAppWear\AndroidAppWear\json\json.json", toWrite);




        }

        private void ButtonPause_Click(object? sender, EventArgs e)
        {
            timer.Enabled = !timer.Enabled;
            HrSample sample= new HrSample();
            sample.value = hr;
            hrSamples.Add(sample);  
           
        }

        public void OnAccuracyChanged(Sensor? sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            
        }

        public void OnSensorChanged(SensorEvent? e)
        {
            RunOnUiThread(() =>
            {
                hr = (short)e.Values[0];
                textHR.Text = "HR: " + (int)e.Values[0];
            });

        }

        

        #region RuntimePermissions

        async Task TryToGetPermissions()
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                await GetPermissionsAsync();
                return;
            }


        }
        const int RequestLocationId = 0;

        readonly string[] PermissionsGroupLocation =
            {
                            Manifest.Permission.BodySensors, Manifest.Permission.WriteExternalStorage
             };
        async Task GetPermissionsAsync()
        {
            const string permission = Manifest.Permission.BodySensors;

            if (CheckSelfPermission(permission) == (int)Android.Content.PM.Permission.Granted)
            {
                //TODO change the message to show the permissions name
                Toast.MakeText(this, "Special permissions granted1", ToastLength.Short).Show();
                return;
            }

            if (ShouldShowRequestPermissionRationale(permission))
            {
                //set alert for executing the task
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Permissions Needed");
                alert.SetMessage("The application need special permissions to continue");
                alert.SetPositiveButton("Request Permissions", (senderAlert, args) =>
                {
                    RequestPermissions(PermissionsGroupLocation, RequestLocationId);
                });

                alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                {
                    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                });

                Dialog dialog = alert.Create();
                dialog.Show();


                return;
            }

            RequestPermissions(PermissionsGroupLocation, RequestLocationId);

        }
        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestLocationId:
                    {
                        if (grantResults[0] == (int)Android.Content.PM.Permission.Granted)
                        {
                            Toast.MakeText(this, "Special permissions granted", ToastLength.Short).Show();

                        }
                        else
                        {
                            //Permission Denied :(
                            Toast.MakeText(this, "Special permissions denied", ToastLength.Short).Show();

                        }
                    }
                    break;
            }
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #endregion


    }
}