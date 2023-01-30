using Android.Content;
using Android.Media;
using Android.OS;
using Android.Renderscripts;
using Plugin.AudioRecorder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Provider.MediaStore;

namespace AndroidAppWear.Services
{
    [Service(Label = "AudioService")]
    internal class AudioService : Service
    {
        private MediaPlayer player;
        private AudioRecorderService recorder;
        Toast toastStart;
        Toast toastStop;

        public AudioService()
        {
            player = MediaPlayer.Create(Application.Context, Resource.Raw.jak);
            recorder = new AudioRecorderService()
            {
                StopRecordingAfterTimeout = true,
                TotalAudioTimeout = TimeSpan.FromSeconds(3)
            };
            toastStart = Toast.MakeText(Application.Context, "Speak", ToastLength.Short);
            toastStop = Toast.MakeText(Application.Context, "Stop speak", ToastLength.Short);
            
        }
        private void PlaySound()
        {
            player.Start();
        }
        public string RecordSound()
        {
            string sample;
            PlaySound();
            while (player.IsPlaying) { }
            recorder.StartRecording();
            toastStart.Show();
            while (recorder.IsRecording) { }
            toastStop.Show();
            recorder.StopRecording();
            if (recorder.FilePath != null)
            {
                byte[] audioBytes = File.ReadAllBytes(recorder.GetAudioFilePath());
                sample = Convert.ToBase64String(audioBytes);
                var date = DateTime.Now.ToString("s") + "+00:00";
                System.Diagnostics.Debug.WriteLine(sample.Length);
            }
            else
            {
                sample = "";
            }

            

            return sample;

            

        }
        
        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }
    }
}
