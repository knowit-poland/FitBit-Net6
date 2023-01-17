using Android;
using Android.Content;
using Android.Hardware;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using AndroidAppWear.Interfaces;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Android.Gms.Common.Apis;
using Android.Gms.Wearable;
using Android.Gms.Common;
using Android.Bluetooth;
using Java.Util;
using Android.Media;
using Plugin.AudioRecorder;
using AndroidAppWear.Services;
using Android.Graphics;
using static Android.Provider.CalendarContract;
using Java.Sql;
//using Abp.Events.Bus;
//using static Xamarin.Essentials.Platform;
//using static Xamarin.Essentials.Platform;
//using static Xamarin.Essentials.Platform;
//using Java.IO;

namespace AndroidAppWear
{



    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity, ISensorEventListener
    {

        Button buttonStart;
        Button buttonStop;
        Button buttonPause;
        TextView textHR;
        TextView textTime;


        //static IAdvancedTimer timer;
        Intent timerIntent;
        Intent btStringIntent;
        Intent btAudioIntent;

        SensorManager mSensorManager;
        Sensor mHeartRateSensor;


        short s = 0;
        short m = 0;
        short h = 0;

        short hr;
        double latitude;
        double longitude;
        string audioS;

        List<HrSamples> hrSamples = new List<HrSamples>();
        List<GPSSamples> locationSamples = new List<GPSSamples>();
        List<AudioSamples> audioSamples = new List<AudioSamples>();



        BluetoothAdapter adapter;
        BluetoothDevice phone;
        BluetoothSocket phoneSocketString = null;
        BluetoothSocket phoneSocketAudio = null;
        const string MY_PHONE_ADDRESS = "B8:3B:CC:3F:9A:03";

        //BluetoothService btService;
        //BluetoothAudioService btAudioService;

        private AudioRecorderService recorder;
        //AudioPlayer player = new AudioPlayer();
        //AudioPlayer player = new AudioPlayer();
        // player;
        //AudioPlayer player;



        protected override async void OnCreate(Bundle? savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

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

            //timer = Xamarin.Forms.DependencyService.Get<IAdvancedTimer>();
            //timer.InitTimer(1000, TimerElapsed, true);
            timerIntent = new Intent(this, typeof(TimerService));
            btAudioIntent = new Intent(this, typeof(BluetoothAudioService));
            btStringIntent = new Intent(this, typeof(BluetoothService));
            var filter = new IntentFilter(TimerService.BROADCAST_ACTION);
            RegisterReceiver(new TimerReceiver(this), filter);

            adapter = BluetoothAdapter.DefaultAdapter;
            phone = null;

            //fileName = Application.Context.GetExternalFilesDir(null).ToString();
            //fileName = Application.Context.GetExternalFilesDir(null).ToString();

            //fileName += "/audiorecordtest.3gp";
            TryToGetPermissions();

            checkBluetoothConnection();
            //blSocketAudio();
            blSocketString();

            //if (checkBluetoothConnection())
            // blSocket();
            //StartClient();

            recorder = new AudioRecorderService()
            {
                StopRecordingAfterTimeout = true,
                TotalAudioTimeout = TimeSpan.FromSeconds(2) 
            };
            //player = new MediaPlayer();
            //player.SetDataSource(@"Resources\raw\sound.mp3");
            //player = MediaPlayer.Create(this, Resource.Raw.sound);
            //AudioPlayer player;
            //player.Prepare();
            //Android.Net.Uri uri = new Android.Net.Uri(@"Resources/raw/sound.mp3")
            //Uri soundPath = new Uri(@"Resources/raw/sound.mp3");
            //player = MediaPlayer.Create(this, soundPath)

            //player = new AudioPlayer();






            ButtonStop_Click(null, null);


        }


        #region Buttons
        private void ButtonStart_Click(object? sender, EventArgs e)
        {
            if (checkSensor())
                mSensorManager.RegisterListener(this, mHeartRateSensor, SensorDelay.Normal);

            buttonStart.Enabled = false;
            buttonStart.Visibility = Android.Views.ViewStates.Invisible;
            buttonStop.Enabled = true;
            buttonStop.Visibility = Android.Views.ViewStates.Visible;
            buttonPause.Enabled = true;
            buttonPause.Visibility = Android.Views.ViewStates.Visible;




            //timer.StartTimer();
            StartService(timerIntent);


        }

        private void ButtonStop_Click(object? sender, EventArgs e)
        {
            //timer.StopTimer();
            StopService(timerIntent);

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
            Thread.Sleep(3000);
            SendMessage("stop");

        }

        private void ButtonPause_Click(object? sender, EventArgs e)
        {
            //timer.PauseTimer();


            if (buttonPause.Text == "PAUSE")
            {
                buttonPause.Text = "START";
                try
                {
                    //startRecording();
                }
                catch (Java.Lang.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error in play audio " + ex.Message);
                }



            }
            else
            {
                buttonPause.Text = "PAUSE";

                //SendRecord();

                //PlayAudio();

            }



        }

        #endregion


        #region Timer
        public void TimerElapsed()
        {

            s++;


            if (s == 59)
            {
                m++;
                s = 0;

                Data data = new Data()
                {
                    HRSamples = hrSamples,
                    GPSSamples = locationSamples,
                    AudioSamples = audioSamples
                };


                string toWrite = JsonConvert.SerializeObject(data);
                System.Diagnostics.Debug.WriteLine(toWrite.Length);
                SendMessage(toWrite + "key");
                //btService.SendMessage(toWrite);
                //Xamarin.Forms.DependencyService.Get<IFileService>().CreateFile(toWrite);

                //string filePath = Xamarin.Forms.DependencyService.Get<FileService>().GetRootPath();
                //string fileName = Xamarin.Forms.DependencyService.Get<FileService>().Filename;
                // string printData = File.ReadAllText(filePath + "/" + fileName);
                //System.Diagnostics.Debug.WriteLine(printData);
                //if (phoneSocket.IsConnected)
                //    sendByBT(toWrite);
                //else
                //    System.Diagnostics.Debug.WriteLine("I have no connection");

                hrSamples.Clear();
                locationSamples.Clear();
                audioSamples.Clear();
            }
            else if (s == 25 || s == 50)
            {
                startRecording();
            }
            if (m == 59)
            {
                h++;
                m = 0;
            }

            RunOnUiThread(() =>
            {
                textTime.Text = String.Format("{0:00}:{1:00}:{2:00}", h, m, s);
            });


            readLocation();
            var date = DateTime.Now.ToString("s") + "+00:00";
            HrSamples hrSample = new HrSamples(date, hr);
            GPSSamples locationSample = new GPSSamples(date, latitude, longitude);

            hrSamples.Add(hrSample);
            locationSamples.Add(locationSample);






            System.Diagnostics.Debug.WriteLine(String.Format("{0:00}:{1:00}:{2:00}", h, m, s));



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
                    System.Diagnostics.Debug.WriteLine("There are no devices");
                    Toast.MakeText(Application.Context, "There are no devices", ToastLength.Long).Show();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Bluetooth adapter is not enabled");
                Toast.MakeText(Application.Context, "Bluetooth adapter is not enabled", ToastLength.Long).Show();
            }

        }

        private void blSocketString()
        {
            if (phone == null)
            {
                System.Diagnostics.Debug.WriteLine("Phone is not avaible");
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
                System.Diagnostics.Debug.WriteLine("phone socket error " + ex.Message.ToString());
                Toast.MakeText(Application.Context, "phone socket error " + ex.Message.ToString(), ToastLength.Long).Show();
            }




        }

        //private void blSocketAudio()
        //{
        //    if (phone == null)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Phone is not avaible");
        //        Toast.MakeText(Application.Context, "Phone is not avaible", ToastLength.Long).Show();
        //        return;
        //    }

        //    phoneSocketAudio = phone.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805F9B34FC"));
        //    try
        //    {
        //        phoneSocketAudio.Connect();
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("phone socket error " + ex.Message.ToString());
        //        Toast.MakeText(Application.Context, "phone socket error " + ex.Message.ToString(), ToastLength.Long).Show();
        //    }




        //}

        public void SendMessage(string message)
        {

            //checkBluetoothConnection();
            //blSocket();

            /*try
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
                phoneSocket.OutputStream.WriteAsync(bytes);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Problem " + e.Message.ToString());
                Toast.MakeText(Application.Context, "Problem " + e.Message.ToString(), ToastLength.Short).Show();
            }*/
            if (phoneSocketString != null && phoneSocketString.IsConnected)
            {
                try
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
                    phoneSocketString.OutputStream.WriteAsync(bytes);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Problem " + e.Message.ToString());
                    Toast.MakeText(Application.Context, "Problem " + e.Message.ToString(), ToastLength.Short).Show();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Problem with phoneSocket");
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

        void TryToGetPermissions()
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                GetPermissionsAsync();
                return;
            }


        }
        private int requestLocationId = 0;

        Dictionary<int, string> PermissionsGroup = new Dictionary<int, string>()
        {
            {0,Manifest.Permission.BodySensors },
            {1,Manifest.Permission.WriteExternalStorage },
            {2,Manifest.Permission.ReadExternalStorage },
            {3,Manifest.Permission.AccessBackgroundLocation },
            {4,Manifest.Permission.AccessCoarseLocation },
            {5,Manifest.Permission.AccessFineLocation },
            {6,Manifest.Permission.RecordAudio },
            {7,Manifest.Permission.CaptureAudioOutput },

        };



        readonly string[] PermissionsGroupLocation =
            {
            Manifest.Permission.BodySensors, Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.ReadExternalStorage, Manifest.Permission.AccessBackgroundLocation,
            Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation, Manifest.Permission.RecordAudio,
            Manifest.Permission.Internet, Manifest.Permission.WakeLock
        };
        void GetPermissionsAsync()
        {
            foreach (var permission in PermissionsGroupLocation)
            {

                if (CheckSelfPermission(permission) == (int)Android.Content.PM.Permission.Granted)
                {
                    //TODO change the message to show the permissions name
                    Toast.MakeText(this, "Special permissions granted1", ToastLength.Short).Show();

                    System.Diagnostics.Debug.WriteLine("\n\n\n\n\n\n\n\n\n\n\nSpecial permissions granted1" + permission);
                }


                if (ShouldShowRequestPermissionRationale(permission))
                {
                    //set alert for executing the task
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Permissions Needed");
                    alert.SetMessage("The application need special permissions to continue");
                    alert.SetPositiveButton("Request Permissions", (senderAlert, args) =>
                    {
                        RequestPermissions(PermissionsGroupLocation, requestLocationId);
                    });

                    alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                    {
                        Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                    });

                    Dialog dialog = alert.Create();
                    dialog.Show();


                }


                RequestPermissions(PermissionsGroupLocation, requestLocationId);
                requestLocationId++;
            }

        }
        /* public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
         {
             switch (requestCode)
             {
                 case requestLocationId:
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
         }*/
        

        #endregion


        #region recordSound

        private async Task startRecording()
        {
            try
            {
                if (!recorder.IsRecording)
                {
                    Toast.MakeText(this, "speak", ToastLength.Short).Show();
                    await recorder.StartRecording();
                    System.Threading.Thread.Sleep(3500);
                    recorder.StopRecording();
                    System.Diagnostics.Debug.WriteLine(recorder.GetAudioFilePath());
                    byte[] audioBytes = await File.ReadAllBytesAsync(recorder.GetAudioFilePath());
                    var sample = Convert.ToBase64String(audioBytes);
                    //player.Play(recorder.GetAudioFilePath());
                    System.Diagnostics.Debug.Write(sample.Length);

                    var date = DateTime.Now.ToString("s") + "+00:00";
                    AudioSamples audioSample = new AudioSamples(date, sample);

                    audioSamples.Add(audioSample);
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Can't make record " + e.Message);
            }
        }

        /*async Task SendRecord(string fileName)
        {
            if (recorder.FilePath != null) 
            {
                
               // var fileName = recorder.GetAudioFilePath();
               try
                {

                    System.Diagnostics.Debug.WriteLine(fileName);
                    byte[] bytes = File.ReadAllBytes(fileName);
                    //var audioSample = Convert.ToBase64String(bytes);

                    // Wyslij dane przez gniazdo Bluetooth
                    phoneSocketAudio.OutputStream.Write(bytes, 0, bytes.Length);

                    //phoneSocket.Close();
                }
                catch (IOException e)
                {
                    System.Diagnostics.Debug.WriteLine("Can't send record " + e.Message);
                    Toast.MakeText(this, "Can't send record " + e.Message, ToastLength. Long).Show();
                }
               
            }
        }
        */


        void PlayAudio()
        {

            //player.Play()


        }

        #endregion


    }
}