using Android;
using Android.Content;
using Android.Hardware;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Android.Gms.Common.Apis;
using Android.Bluetooth;
using Java.Util;
using AndroidAppWear.Services;

namespace AndroidAppWear
{



    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity, ISensorEventListener
    {
        
        private Button buttonStart;
        private Button buttonStop;
        private TextView textHR;
        private TextView textTime;

        private Intent timerIntent;

        private AudioService audioService;

        private SensorManager mSensorManager;
        private Sensor mHeartRateSensor;

        private short s = 0;
        private short m = 0;
        private short h = 0;

        private short hr = 0;
        private double latitude;
        private double longitude;

        private const int RequestLocationId = 0;

        private List<HrSamples> hrSamples = new List<HrSamples>();
        private List<GPSSamples> locationSamples = new List<GPSSamples>();
        private List<AudioSamples> audioSamples = new List<AudioSamples>();
        
        private BluetoothAdapter adapter;
        private BluetoothDevice phone;
        private BluetoothSocket phoneSocketString = null;
        private const string MY_PHONE_ADDRESS = "B8:3B:CC:3F:9A:03";
        


        protected override async void OnCreate(Bundle? savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            TryToGetPermissions();
            SetContentView(Resource.Layout.activity_main);
            
            buttonStart = FindViewById<Button>(Resource.Id.button_start);
            buttonStop = FindViewById<Button>(Resource.Id.button_stop);
            textHR = FindViewById<TextView>(Resource.Id.hr_text);
            textTime = FindViewById<TextView>(Resource.Id.time_text);
            
            buttonStop.Click += ButtonStop_Click;
            buttonStart.Click += ButtonStart_Click;
            
            mSensorManager = ((SensorManager)GetSystemService(SensorService));
            if (checkSensor())
            {
                mHeartRateSensor = mSensorManager.GetDefaultSensor(SensorType.HeartRate);
            }
            
            timerIntent = new Intent(this, typeof(TimerService));
            var filter = new IntentFilter(TimerService.BROADCAST_ACTION);
            RegisterReceiver(new TimerReceiver(this), filter);

            audioService = new AudioService();
            
            textTime.Text = String.Format("{0:00}:{1:00}:{2:00}", h, m, s);
            textHR.Text = "HR: " + hr;
            buttonStop.Enabled = false;
            buttonStop.Visibility = Android.Views.ViewStates.Invisible;
            buttonStart.Enabled = true;
            buttonStart.Visibility = Android.Views.ViewStates.Visible;
        
        }


        #region Buttons
        private void ButtonStart_Click(object? sender, EventArgs e)
        {
            checkBluetoothConnection();
            blSocketString();
            if (!adapter.IsEnabled)
            {
                Toast.MakeText(this, "Turn on Bluetooth", ToastLength.Short).Show();
                adapter = null;
                phone = null;
                phoneSocketString = null;
                return;
            }
            if(phoneSocketString == null || !phoneSocketString.IsConnected)
            {
                Toast.MakeText(this, "Turn on app on phone", ToastLength.Short).Show();
                adapter = null;
                phone = null;
                phoneSocketString = null;
                return;
            }
                

                restartTimer();
            if (checkSensor())
                mSensorManager.RegisterListener(this, mHeartRateSensor, SensorDelay.Normal);

            buttonStart.Enabled = false;
            buttonStart.Visibility = Android.Views.ViewStates.Invisible;
            buttonStop.Enabled = true;
            buttonStop.Visibility = Android.Views.ViewStates.Visible;
            
            StartService(timerIntent);
        
        }

        private void ButtonStop_Click(object? sender, EventArgs e)
        {
            StopService(timerIntent);

            if (checkSensor())
                mSensorManager.UnregisterListener(this);

            RunOnUiThread(() =>
            {
                textHR.Text = "HR: " + hr;
            });

            buttonStop.Enabled = false;
            buttonStop.Visibility = Android.Views.ViewStates.Invisible;
            buttonStart.Enabled = true;
            buttonStart.Visibility = Android.Views.ViewStates.Visible;
            
            SendMessage("stop");

        }

        #endregion
        
        #region Timer
        public async void TimerElapsed()
        {
            
                textTime.Text = String.Format("{0:00}:{1:00}:{2:00}", h, m, s);
                s++;
            
            if (s == 59)
            {
                Task.Run(() => MakeMessage());
            }
            else if (s == 25 || s == 50)
            {
                Task.Run(() => RecordSound());
            }
            if (m == 59)
            {
                h++;
                m = 0;
            }
            
            readLocation();
            var date = DateTime.Now.ToString("s") + "+00:00";
            HrSamples hrSample = new HrSamples(date, hr);
            GPSSamples locationSample = new GPSSamples(date, latitude, longitude);

            hrSamples.Add(hrSample);
            locationSamples.Add(locationSample);
        
        }

        private void MakeMessage()
        {
            m++;
            s = 0;

            Data data = new Data()
            {
                AudioSamples = audioSamples,
                HRSamples = hrSamples,
                GPSSamples = locationSamples,
                UserID = "SamsungWear",
                id = Guid.NewGuid().ToString()
            };


            string toWrite = JsonConvert.SerializeObject(data);
            SendMessage(toWrite + "key");

            hrSamples.Clear();
            locationSamples.Clear();
            audioSamples.Clear();
        }

        private void RecordSound()
        {
            var sample = audioService.RecordSound();
            var d = DateTime.Now.ToString("s") + "+00:00";
            AudioSamples audioSample = new AudioSamples(d, sample);
            audioSamples.Add(audioSample);
        }

        private void restartTimer()
        {
            s = 0;
            m = 0;
            h = 0;
        }

        public class TimerReceiver : BroadcastReceiver
        {
            MainActivity _mainActivity;

            public TimerReceiver(MainActivity mainActivity)
            {
                _mainActivity = mainActivity;
            }
            public override void OnReceive(Context context, Intent intent)
            {
                if (intent.Action == TimerService.BROADCAST_ACTION)
                {

                    _mainActivity.TimerElapsed();
                    
                }
            }

           
        }
        
        #endregion
        
        #region Bluetooth
        private void checkBluetoothConnection()
        {
            adapter = BluetoothAdapter.DefaultAdapter;
            phone = null;

            if (adapter.IsEnabled)
            {
                var pairedDevices = adapter.BondedDevices;

                if (pairedDevices.Count > 0)
                {
                    foreach (var device in pairedDevices)
                    {
                        var deviceMAC = device.ToString();
                        if (deviceMAC == MY_PHONE_ADDRESS)
                        {
                            phone = device;
                        }
                    }
                }
                else
                {
                    Toast.MakeText(Application.Context, "There are no devices", ToastLength.Long).Show();
                }
            }
            else
            {
                Toast.MakeText(Application.Context, "Bluetooth adapter is not enabled", ToastLength.Long).Show();
            }

        }

        private void blSocketString()
        {
            if (phone == null)
            {
                Toast.MakeText(Application.Context, "Phone is not avaible", ToastLength.Long).Show();
                return;
            }

            phoneSocketString = phone.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));
            try
            {
                phoneSocketString.Connect();
            }
            catch (Exception ex)
            {
                phoneSocketString = null;
            }




        }
        
        public void SendMessage(string message)
        {
            

            if (phoneSocketString != null && phoneSocketString.IsConnected)
            {
                try
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
                    phoneSocketString.OutputStream.WriteAsync(bytes);
                }
                catch (Exception e)
                {
                    Toast.MakeText(Application.Context, "Error", ToastLength.Short).Show();
                }
                
            }
            else
            {
                Toast.MakeText(Application.Context, "Phone lost", ToastLength.Short).Show();
            }
            if(message == "stop")
            {
                adapter = null;
                phone = null;
                phoneSocketString = null;
            }
        }
        #endregion
        
        #region Sensores

        private async void readLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromMinutes(1)));

                if (location == null)
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

        private async void TryToGetPermissions()
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                GetPermissionsAsync();
                return;
            }


        }


        private readonly string[] PermissionsGroupLocation =
            {
            Manifest.Permission.BodySensors,  Manifest.Permission.AccessBackgroundLocation,
            Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation, Manifest.Permission.RecordAudio,
            Manifest.Permission.Internet, Manifest.Permission.WakeLock, Manifest.Permission.Bluetooth, Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.WriteExternalStorage, Manifest.Permission.ReadExternalStorage,
        };
        private async Task GetPermissionsAsync()
        {
            var permissions = PermissionsGroupLocation;

            var grantedPermissions = new List<string>();
            var deniedPermissions = new List<string>();

            foreach (var permission in permissions)
            {
                if (CheckSelfPermission(permission) == (int)Android.Content.PM.Permission.Granted)
                {
                    grantedPermissions.Add(permission);
                }
                else
                {
                    deniedPermissions.Add(permission);
                }
            }

            if (grantedPermissions.Count == permissions.Length)
            {
                Toast.MakeText(this, "All permissions granted", ToastLength.Short).Show();
                return;
            }

            if (deniedPermissions.Count > 0)
            {
                if (ShouldShowRequestPermissionRationale(deniedPermissions[0]))
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Permissions Needed");
                    alert.SetMessage("The application needs special permissions to continue");
                    alert.SetPositiveButton("Request Permissions", (senderAlert, args) =>
                    {
                        RequestPermissions(deniedPermissions.ToArray(), RequestLocationId);
                    });

                    alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                    {
                        Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                    });

                    Dialog dialog = alert.Create();
                    dialog.Show();

                    return;
                }

                RequestPermissions(deniedPermissions.ToArray(), RequestLocationId);
            }
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestLocationId:
                    {
                        var grantedPermissions = new List<string>();
                        var deniedPermissions = new List<string>();

                        for (int i = 0; i < grantResults.Length; i++)
                        {
                            if (grantResults[i] == (int)Android.Content.PM.Permission.Granted)
                            {
                                grantedPermissions.Add(permissions[i]);
                            }
                            else
                            {
                                deniedPermissions.Add(permissions[i]);
                            }
                        }

                        if (grantedPermissions.Count == permissions.Length)
                        {
                            Toast.MakeText(this, "All permissions granted", ToastLength.Short).Show();
                        }
                        else if (deniedPermissions.Count > 0)
                        {
                            Toast.MakeText(this, "Some permissions denied", ToastLength.Short).Show();
                        }
                    }
                    break;
            }
        }

        #endregion


    }
}