namespace AndroidAppWear
{
    using Android;
    using Android.Content;
    using Android.Hardware;
    using Android.Nfc;
    using Android.OS;
    using Android.Runtime;
    using Android.Util;
    using Android.Webkit;
    using AndroidAppWear.Interfaces;
    using AndroidAppWear.Services;
    using Java.Security;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics.Metrics;
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
        
        
        static IAdvancedTimer timer;
      

        SensorManager mSensorManager;
        Sensor mHeartRateSensor;
        

        short ms = 0;
        short s = 0;
        short m = 0;
        short h = 0;

        short hr;
        double latitude;
        double longitude;

        List<HrSamples> hrSamples = new List<HrSamples>();
        List<LocationSamples> locationSamples = new List<LocationSamples>();
        
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            await TryToGetPermissions();
            base.OnCreate(savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);


            buttonStart = FindViewById<Button>(Resource.Id.button_start);
            buttonStop = FindViewById<Button>(Resource.Id.button_stop);
            buttonPause = FindViewById<Button>(Resource.Id.button_pause);
            textHR = FindViewById<TextView>(Resource.Id.hr_text);
            textTime = FindViewById<TextView>(Resource.Id.time_text);

            buttonPause.Click += ButtonPause_Click;
            buttonStop.Click += ButtonStop_Click;
            buttonStart.Click += ButtonStart_Click;


            mSensorManager = ((SensorManager)GetSystemService(SensorService));
            if (checkSensor())
            {
                mHeartRateSensor = mSensorManager.GetDefaultSensor(SensorType.HeartRate);

            }

            timer = Xamarin.Forms.DependencyService.Get<IAdvancedTimer>();
            timer.InitTimer(1000, TimerElapsed, true);

            ButtonStop_Click(null, null);
            
           
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

            buttonStart.Enabled = false;
            buttonStart.Visibility = Android.Views.ViewStates.Invisible;
            buttonStop.Enabled = true;
            buttonStop.Visibility = Android.Views.ViewStates.Visible;
            buttonPause.Enabled = true;
            buttonPause.Visibility = Android.Views.ViewStates.Visible;

            timer.StartTimer();



        }

        private void ButtonStop_Click(object? sender, EventArgs e)
        {
            timer.StopTimer();

            if (checkSensor())
                mSensorManager.UnregisterListener(this);
            
            RunOnUiThread(() =>
            {
                textHR.Text = "HR: " + hr;
            });

            buttonStop.Enabled = false;
            buttonStop.Visibility = Android.Views.ViewStates.Invisible;
            buttonPause.Enabled = false;
            buttonPause.Visibility = Android.Views.ViewStates.Invisible;
            buttonStart.Enabled = true;
            buttonStart.Visibility = Android.Views.ViewStates.Visible;


            restartTimer();
        }

        private void ButtonPause_Click(object? sender, EventArgs e)
        {
            timer.PauseTimer();


            if (buttonPause.Text == "PAUSE")
            {
                buttonPause.Text = "START";


            }
            else
            {
                buttonPause.Text = "PAUSE";
            }
        }


        private void TimerElapsed(object? sender, EventArgs e)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                //timer.Interval = (timer.Interval);
                s++;

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
                textTime.Text = String.Format("{0:00}:{1:00}:{2:00}", h, m, s);

                readLocation();
                var date = DateTime.Now.ToString();

                HrSamples hrSample = new HrSamples(date, hr);
                LocationSamples locationSample = new LocationSamples(date, latitude, longitude);




                hrSamples.Add(hrSample);
                locationSamples.Add(locationSample);

                if (s % 5 == 0)
                {
                    Data data = new Data()
                    {
                        hrSamples = hrSamples,
                        locationSamples = locationSamples
                    };

                    string toWrite = JsonConvert.SerializeObject(data, Formatting.Indented);


                    Xamarin.Forms.DependencyService.Get<IFileService>().CreateFile(toWrite);

                    string filePath = Xamarin.Forms.DependencyService.Get<FileService>().GetRootPath();
                    string fileName = Xamarin.Forms.DependencyService.Get<FileService>().Filename;
                    string printData = File.ReadAllText(filePath + "/" + fileName);
                    System.Diagnostics.Debug.WriteLine(printData);
                }
                else
                    System.Diagnostics.Debug.WriteLine(String.Format("{0:00}:{1:00}:{2:00}", h, m, s));
            });


        }

        private async void readLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromMinutes(1)));
                
                if(location == null)
                {
                    System.Diagnostics.Debug.WriteLine("noGPS");
                }
                else
                {
                        RunOnUiThread(() =>
                        {
                            latitude = location.Latitude;
                            longitude = location.Longitude;
                        
                        });
                }
            }
            catch (Exception e) 
            {
                System.Diagnostics.Debug.WriteLine("Can't read location: " + e.Message);
            }
        }



        #region Sensores

        private bool checkSensor()
        {
            if (mSensorManager.GetSensorList(SensorType.HeartRate).Count != 0)
                return true;
            return false;

        }
        public void OnAccuracyChanged(Sensor? sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            
        }

        public void OnSensorChanged(SensorEvent? e)
        {
            hr = (short)e.Values[0];
            RunOnUiThread(() =>
            {
                textHR.Text = "HR: " + (int)e.Values[0];
            });

        }

        #endregion



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
            Manifest.Permission.BodySensors, Manifest.Permission.WriteExternalStorage, 
            Manifest.Permission.AccessBackgroundLocation, Manifest.Permission.AccessCoarseLocation, 
            Manifest.Permission.AccessFineLocation
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

                            Toast.MakeText(this, "Special permissions denied", ToastLength.Short).Show();
                        }
                    }
                    break;
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #endregion


    }
}