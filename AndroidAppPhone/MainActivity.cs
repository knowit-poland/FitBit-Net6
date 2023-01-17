using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.Res;
using Android.Media;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidAppWear;
using AndroidAppWear.Interfaces;
using AndroidAppWear.Services;
using Java.IO;
using Java.Net;
using Java.Util;
using Java.Util.Logging;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace AndroidAppWear
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {

        private TextView textBL;
        private BluetoothAdapter adapter;
        private BluetoothServerSocket ServerSocketString;
        private BluetoothServerSocket ServerSocketAudio;

        private BluetoothSocket watchSocketString = null;
        private BluetoothSocket watchSocketAudio = null;
        private const string Message = "ThIsAAsAAparatoooor";
        string toFileWrite;

        int it = 0;

        const string MY_WATCH_ADRESS = "64:5D:F4:66:EF:33";
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            HTTPService https;

            textBL = FindViewById<TextView>(Resource.Id.bl_text);
            textBL.Text = "nowa wiadomość ";

            adapter = BluetoothAdapter.DefaultAdapter;





            TryToGetPermissions();


            https = new HTTPService();
            checkBluetoothConnectionString();
            blSocketString();

            if (watchSocketString != null)
            {
                await ReadFromBt();
            }
            //if (watchSocketAudio != null)
            //{
            // await readRecord();


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
            Manifest.Permission.WriteExternalStorage
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

        #region Bluetooth

        private void checkBluetoothConnectionString()
        {
            if (adapter.IsEnabled)
            {
                System.Diagnostics.Debug.WriteLine("Bluettoh is enabled\n\n\n\n\n");

                BluetoothServerSocket tmp = null;
                try
                {
                    tmp = adapter.ListenUsingInsecureRfcommWithServiceRecord("MyAppServer", UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error in listenig ");
                }
                ServerSocketString = tmp;
            }
            else
            {

                System.Diagnostics.Debug.WriteLine("Not Devices \n\n\n\n\n");
            }
        }

        private void checkBluetoothConnectionAudio()
        {
            if (adapter.IsEnabled)
            {
                System.Diagnostics.Debug.WriteLine("Bluettoh is enabled\n\n\n\n\n");

                BluetoothServerSocket tmp = null;
                try
                {
                    tmp = adapter.ListenUsingInsecureRfcommWithServiceRecord("MyAppServer", UUID.FromString("00001101-0000-1000-8000-00805F9B34FC"));

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error in listenig ");
                }
                ServerSocketAudio = tmp;
            }
            else
            {

                System.Diagnostics.Debug.WriteLine("Not Devices \n\n\n\n\n");
            }
        }

        private async void blSocketString()
        {

            if (ServerSocketString == null)
            {
                System.Diagnostics.Debug.WriteLine("Phone is not avaible \n\n\n\n\n");
                return;
            }

            try
            {
                watchSocketString = ServerSocketString.Accept();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("phone socket error " + e.Message.ToString());
            }




        }

        private async void blSocketAudio()
        {

            if (ServerSocketAudio == null)
            {
                System.Diagnostics.Debug.WriteLine("Phone is not avaible \n\n\n\n\n");
                return;
            }

            try
            {
                watchSocketAudio = ServerSocketAudio.Accept();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("phone socket error " + e.Message.ToString());
            }
        }



        private async Task ReadFromBt()
        {
            System.Diagnostics.Debug.WriteLine("wear soccet connected string");
            var bytes = new byte[2000000];
            while (watchSocketString.IsConnected)
            {
                var numBytesRead = await watchSocketString.InputStream.ReadAsync(bytes, 0, bytes.Length);
                //System.Diagnostics.Debug.WriteLine(numBytesRead);
                string receivedString = System.Text.Encoding.UTF8.GetString(bytes, 0, numBytesRead);
                System.Diagnostics.Debug.WriteLine(receivedString);
                toFileWrite += receivedString;

                if (receivedString.EndsWith("key"))
                {
                    System.Diagnostics.Debug.WriteLine("receiveed" + receivedString);


                    //watchSocketString.Close();
                    //textBL.Text = receivedString;

                    try
                    {

                        System.Diagnostics.Debug.WriteLine("for file " + toFileWrite);
                        toFileWrite = toFileWrite.Remove(toFileWrite.Length - 3);
                        Xamarin.Forms.DependencyService.Get<IFileService>().CreateFile(toFileWrite);
                        //System.Diagnostics.Debug.WriteLine("file");
                        //string filePath = Xamarin.Forms.DependencyService.Get<FileService>().GetRootPath();
                        //string fileName = Xamarin.Forms.DependencyService.Get<FileService>().Filename;
                        //string printData = (filePath + "/" + fileName);
                        toFileWrite = null;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("cant write to file " + e.Message);
                    }
                }
                if(receivedString == "stop")
                {
                    System.Diagnostics.Debug.WriteLine("stopped");
                }
            }
        }


        async Task readRecord()
        {
            System.Diagnostics.Debug.WriteLine("wear socet connected audio");
            while (watchSocketAudio.IsConnected)
            {

                //var bytes = new byte[1024];
                //var numBytesRead = await watchSocketString.InputStream.ReadAsync(bytes, 0, bytes.Length);
                string filePath = Xamarin.Forms.DependencyService.Get<FileService>().GetRootPath();
                string fileIt = "plik" + it++ + ".wav";
                string fileName = filePath + "/" + fileIt;
                var buffer = new byte[300];

                using (var mmInStream = watchSocketAudio.InputStream)
                using (var file = new FileOutputStream(fileName))
                {
                    int bytesRead = 0;
                    System.Diagnostics.Debug.WriteLine("Written audio");
                    while ((bytesRead = await mmInStream.ReadAsync(buffer)) > 0)
                    {
                        file.Write(buffer, 0, bytesRead);
                    }
                    System.Diagnostics.Debug.WriteLine("Written audio");


                }
                System.Diagnostics.Debug.WriteLine("Written audio");
            }
            System.Diagnostics.Debug.WriteLine("Written audio");


        }





        #endregion
    }


}
