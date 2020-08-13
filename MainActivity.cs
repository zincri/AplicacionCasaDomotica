using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Content;
using Android.Speech;//Libreria para grabar.
using Android.Widget;

using System;
using Android.Views;
using System.IO;
using Java.Util;
using Android.Bluetooth;
using System.Threading.Tasks;

namespace AplicacionCasaDomotica
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        #region  VariablesGrabar
        private bool isRecording;
        private readonly int voice = 10;
        private TextView texto;
        private Button boton_grabar;
        #endregion

        #region VariablesBluetooth
        //Creamos las variables necesarios para trabajar
        //Widgets
        //ToggleButton tgConnect;
        //TextView Result;
        //String a enviar
        //private Java.Lang.String dataToSend;
        //Variables para el manejo del bluetooth Adaptador y Socket
        //private BluetoothAdapter mBluetoothAdapter = null;
        //private BluetoothSocket btSocket = null;
        //Streams de lectura I/O
        //private Stream outStream = null;
        //private Stream inStream = null;
        //MAC Address del dispositivo Bluetooth
        //private static string address = "00:13:01:07:01:59";
        //Id Unico de comunicacion
        //private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            #region CodigoGrabar
            isRecording = false;
            boton_grabar = FindViewById<Button>(Resource.Id.btn_grabar);
            texto = FindViewById<TextView>(Resource.Id.tv_texto);

            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
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
                    isRecording = true;
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
            isRecording = false;
            #endregion

            #region CodigoBluetooth
            //tgConnect = FindViewById<ToggleButton>(Resource.Id.toggleButton1);
            //Result = FindViewById<TextView>(Resource.Id.textView1);
            //Asignacion de evento del toggle button
            //tgConnect.CheckedChange += tgConnect_HandleCheckedChange;
            //Verificamos la disponibilidad del sensor Bluetooth en el dispositivo
            //CheckBt();
            #endregion
        }

        private void tgConnect_HandleCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            #region CodigoGrabar
            if (requestCode == voice) {
                if (resultCode == Result.Ok) {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput =  matches[0];

                        if (textInput == "Hola")
                        {
                            texto.Text = "H";
                        }
                        else {
                            texto.Text = textInput;
                        }

                        
                        
                    }
                    else {
                        texto.Text = "No se reconoce";
                    }
                }
            }
            #endregion
            base.OnActivityResult(requestCode, resultCode, data);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}