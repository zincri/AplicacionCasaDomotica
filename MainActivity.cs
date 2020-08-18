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

//using FanControllerBluetooth;
using System.Linq;
using AlertDialog = Android.App.AlertDialog;


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
        ToggleButton tgConnect;
        TextView Result;
        //String a enviar
        private Java.Lang.String dataToSend;
        //Variables para el manejo del bluetooth Adaptador y Socket
        private BluetoothAdapter mBluetoothAdapter = null;
        private BluetoothSocket btSocket = null;
        //Streams de lectura I/O
        private Stream outStream = null;
        private Stream inStream = null;
        //MAC Address del dispositivo Bluetooth
        private static string address = "00:18:E4:40:00:06";
        //Id Unico de comunicacion
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        #endregion

        #region Prueba
        BluetoothConnection myConnection = new BluetoothConnection();
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

         

            #region Prueba
            // Get our button from the layout resource,
            // and attach an event to it
            Button buttonConnect = FindViewById<Button>(Resource.Id.button1);
            Button buttonDisconnect = FindViewById<Button>(Resource.Id.button2);

            SeekBar fanDrehzahl = FindViewById<SeekBar>(Resource.Id.seekBar1);

            TextView connected = FindViewById<TextView>(Resource.Id.textView1);



            BluetoothSocket _socket = null;




            buttonConnect.Click += delegate
            {


                myConnection = new BluetoothConnection();
                myConnection.getAdapter();
                myConnection.thisAdapter.StartDiscovery();

                try
                {
                    myConnection.getDevice();
                    myConnection.thisDevice.SetPairingConfirmation(false);

                    myConnection.thisDevice.SetPairingConfirmation(true);
                    myConnection.thisDevice.CreateBond();

                }
                catch (Exception deviceEX)
                {
                }
                myConnection.thisAdapter.CancelDiscovery();

                try
                { _socket = myConnection.thisDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")); } //the UUID of HC-05 and HC-06 is the same
                catch (Exception ex)
                {
                    AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                    AlertDialog alert = dialog.Create();
                    alert.SetTitle("Error");
                    alert.SetMessage("Please go to settings and connect with the bluetooth module at first.");
                    alert.SetButton("OK", (c, ev) =>
                    {
                        // Ok button click task: alert goes away  
                    });
                    alert.Show();
                }

                myConnection.thisSocket = _socket;

                try
                {
                    myConnection.thisSocket.Connect();

                    connected.Text = "Connected to the Arduino!";

                    buttonDisconnect.Enabled = true;
                    buttonConnect.Enabled = false;

                }
                catch (Exception CloseEX)
                { }
            };

            buttonDisconnect.Click += delegate
            {

                try
                {
                    buttonConnect.Enabled = true;

                    myConnection.thisDevice.Dispose();

                    myConnection.thisSocket.OutputStream.WriteByte(200);
                    myConnection.thisSocket.OutputStream.Close();

                    myConnection.thisSocket.Close();

                    myConnection = new BluetoothConnection();
                    _socket = null;

                    connected.Text = "Not connected to the Arduino!";
                }
                catch { }
            };



            fanDrehzahl.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    try
                    {
                        
                        /*if(e.Progress <=62 )
                            dataToSend = new Java.Lang.String("b");
                        else
                            dataToSend = new Java.Lang.String("a");*/
                        dataToSend = new Java.Lang.String("A");

                        writeData(dataToSend);
                        //writeData(dataToSend);
                        //Java.Lang.String message = dataToSend;
                        //byte[] msgBuffer = message.GetBytes();
                        //Console.WriteLine("PROGRESO: " + msgBuffer);
                        //myConnection.thisSocket.OutputStream.Write(msgBuffer, 0, msgBuffer.Length);
                        //System.Threading.Thread.Sleep(10);
                    }
                    catch { }
                }
            };

       
        #endregion
    }

        private void writeDataModificated(Java.Lang.String data)
        {
            //Extraemos el stream de salida
            try
            {
                outStream = myConnection.thisSocket.OutputStream;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Error al enviar" + e.Message);
            }

            //creamos el string que enviaremos
            Java.Lang.String message = data;

            //lo convertimos en bytes
            byte[] msgBuffer = message.GetBytes();

            try
            {
                //Escribimos en el buffer el arreglo que acabamos de generar
                //outStream.Write(msgBuffer, 0, msgBuffer.Length);
                outStream.WriteByte(2); 
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Error al enviar" + e.Message);
            }
        }

        //Metodo de envio de datos la bluetooth
        private void writeData(Java.Lang.String data)
        {
            //Extraemos el stream de salida
            try
            {
                outStream = btSocket.OutputStream;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Error al enviar" + e.Message);
            }

            //creamos el string que enviaremos
            Java.Lang.String message = data;

            //lo convertimos en bytes
            byte[] msgBuffer = message.GetBytes();

            try
            {
                //Escribimos en el buffer el arreglo que acabamos de generar
                outStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Error al enviar" + e.Message);
            }
        }


        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            #region CodigoGrabar
            if (requestCode == voice) {
                if (resultCode == Android.App.Result.Ok) {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput =  matches[0];

                        switch (textInput)
                        {
                            case "Apagar luz": 
                                texto.Text = "A";
                                break;
                            case "Encender luz":
                                texto.Text = "B";
                                break;
                            case "Encender ventilador":
                                texto.Text = "C";
                                break;
                            case "encender ventilador":
                                texto.Text = "C";
                                break;
                            case "Apagar ventilador":
                                texto.Text = "D";
                                break;
                            case "Abrir puerta":
                                texto.Text = "E";
                                break;
                            case "Cerrar puerta":
                                texto.Text = "F";
                                break;
                            case "Encender alarma":
                                texto.Text = "G";
                                break;
                            case "Apagar alarma":
                                texto.Text = "H";
                                break;
                            default:
                                texto.Text = "No encontrado";
                                break;
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

    #region Prueba

    public class BluetoothConnection
    {

        public void getAdapter() { this.thisAdapter = BluetoothAdapter.DefaultAdapter; }


        //change the bd.name according to the name of your bluetooth module
        public void getDevice() { this.thisDevice = (from bd in this.thisAdapter.BondedDevices where bd.Name == "SLAVE_ZINCRI_" select bd).FirstOrDefault(); }

        public BluetoothAdapter thisAdapter { get; set; }
        public BluetoothDevice thisDevice { get; set; }

        public BluetoothSocket thisSocket { get; set; }
    }
    #endregion
}