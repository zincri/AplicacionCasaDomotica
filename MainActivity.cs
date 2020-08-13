using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Content;
using Android.Speech;
using Android.Widget;

namespace AplicacionCasaDomotica
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private bool isRecording;
        private readonly int voice = 10;
        private TextView texto;
        private Button boton_grabar;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            isRecording = false;
            boton_grabar = FindViewById<Button>(Resource.Id.btn_grabar);
            texto = FindViewById<TextView>(Resource.Id.tv_texto);

            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardaware.microphone")
            {
                var alert = new Android.App.AlertDialog.Builder(boton_grabar.Context);
                alert.SetTitle("No se detecta microfono para Grabar");
                alert.SetPositiveButton("Ok", (sender, e) =>
                {
                    texto.Text = "No hay microfono en el dispositivo";
                    boton_grabar.Enabled = false;
                    return;
                });
                alert.Show();
            }
            else
                boton_grabar.Click += delegate
                {
                    boton_grabar.Text = "Fin de la grabacion";
                    isRecording = !isRecording;
                    if (isRecording)
                    {
                        var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                        StartActivityForResult(voiceIntent, voice);
                    }

                };
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == voice) {
                if (resultCode == Result.Ok) {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = texto.Text + matches[0];

                        textInput = textInput.Substring(0, 500);
                        texto.Text = textInput;
                    }
                    else {
                        texto.Text = "No se reconoce";
                    }
                    boton_grabar.Text = "Seguir grabando";
                }
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}