using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.Res;
using Android.Locations;
using Android.Media;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidAppWear;
using AndroidAppWear.Services;
using Java.Util;

namespace AndroidAppWear
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {

        private TextView textBL;
        private Button listenButton;
        private BluetoothAdapter adapter;
        private BluetoothServerSocket ServerSocketString;

        private BluetoothSocket watchSocketString = null;
        private string toFileWrite;
        private const int RequestLocationId = 0;

        private HTTPService httpservice;
        private FileService fileservice;

        private int it = 0;

        const string MY_WATCH_ADRESS = "64:5D:F4:66:EF:33";
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            textBL = FindViewById<TextView>(Resource.Id.bl_text);
            textBL.Text = "press button to connect with watch";

            listenButton = FindViewById<Button>(Resource.Id.buttonListen);
            listenButton.Click += ListenButton_Click;


            httpservice = new HTTPService();
            fileservice = new FileService();

            TryToGetPermissions();


            httpservice = new HTTPService();
        }

        private async void ListenButton_Click(object? sender, EventArgs e)
        {
            checkBluetoothConnectionString();
            blSocketString();

            if (watchSocketString != null)
            {
                textBL.Text = "Connected with watch";
                listenButton.Enabled = false;
                await ReadFromBt();
            }
        }


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
            Manifest.Permission.Internet, Manifest.Permission.Bluetooth, Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.WriteExternalStorage, Manifest.Permission.ReadExternalStorage
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

        #region Bluetooth

        private void checkBluetoothConnectionString()
        {
            adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter.IsEnabled)
            {
                BluetoothServerSocket tmp = null;
                try
                {
                    tmp = adapter.ListenUsingInsecureRfcommWithServiceRecord("MyAppServer", UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));
                }
                catch (Exception e)
                {
                    Toast.MakeText(this, "Phone is not paired", ToastLength.Long).Show();
                    textBL.Text = "press button to connect with watch";
                    listenButton.Enabled = true;
                }
                ServerSocketString = tmp;
            }
            else
            {

                Toast.MakeText(this, "Turn on Bluetooth", ToastLength.Long).Show();
            }
        }



        private async void blSocketString()
        {
            if (!adapter.IsEnabled)
            {
                return;
            }

            else if (ServerSocketString == null)
            {
                return;
            }

            try
            {
                
                watchSocketString = ServerSocketString.Accept();
                System.Diagnostics.Debug.WriteLine("Phone is not avaible \n\n\n\n\n");
            }
            catch (Exception e)
            {
                textBL.Text = "press button to connect with watch";
                listenButton.Enabled = true;
                watchSocketString = null;
            }
        }

        private async Task ReadFromBt()
        {
            var bytes = new byte[2000000];
            while (true)
            {
                var numBytesRead = await watchSocketString.InputStream.ReadAsync(bytes, 0, bytes.Length);
                string receivedString = System.Text.Encoding.UTF8.GetString(bytes, 0, numBytesRead);
                toFileWrite += receivedString;

                if (receivedString.EndsWith("key"))
                {
                    try
                    {
                        it++;
                        System.Diagnostics.Debug.WriteLine("for file " + toFileWrite);
                        System.Diagnostics.Debug.WriteLine("for file " + toFileWrite.Length);
                        toFileWrite = toFileWrite.Remove(toFileWrite.Length - 3);
                        var fileName = "jsonFile" + it + ".json";
                        fileservice.CreateFile(toFileWrite, fileName);
                        toFileWrite = null;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("cant write to file " + e.Message);
                    }
                }
                if (receivedString == "stop")
                {
                    System.Diagnostics.Debug.WriteLine("stopped");
                    toFileWrite = null;
                    await httpservice.sendHTTPPost(it);
                    ServerSocketString.Close();
                    watchSocketString.Close();
                    adapter = null;
                    ServerSocketString = null;
                    watchSocketString = null;
                    textBL.Text = "press button to connect with watch";
                    listenButton.Enabled = true;
                    if (watchSocketString == null || !watchSocketString.IsConnected)
                        return;

                }

            }


        }





        #endregion
    }


}
