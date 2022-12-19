namespace AndroidAppPhone
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {

        TextView textBL;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            textBL = FindViewById<TextView>(Resource.Id.bl_text);
            textBL.Text = "heja";
        }
    }
}