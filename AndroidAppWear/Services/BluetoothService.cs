using Android.Bluetooth;
using Android.Content;
using Android.OS;
using AndroidAppWear.Interfaces;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidAppWear.Services
{
    [Service(Label = "BluetoothService")]
    internal class BluetoothService : Service
    {

        BluetoothAdapter adapter;
        BluetoothDevice phone;
        BluetoothSocket phoneSocket = null;
        const string MY_PHONE_ADDRESS = "B8:3B:CC:3F:9A:03";

        
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

        private void blSocket()
        {
            if (phone == null)
            {
                System.Diagnostics.Debug.WriteLine("Phone is not avaible");
                Toast.MakeText(Application.Context, "Phone is not avaible", ToastLength.Long).Show();
                return;
            }

            phoneSocket = phone.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));
            try
            {
                phoneSocket.Connect();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("phone socket error " + ex.Message.ToString());
                Toast.MakeText(Application.Context, "phone socket error " + ex.Message.ToString(), ToastLength.Long).Show();
            }




        }

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
            if (phoneSocket != null && phoneSocket.IsConnected)
            {
                try
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
                    phoneSocket.OutputStream.WriteAsync(bytes);
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

        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }
    }
}
