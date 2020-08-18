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
            tgConnect = FindViewById<ToggleButton>(Resource.Id.toggleButton1);
            Result = FindViewById<TextView>(Resource.Id.textView1);
            //Asignacion de evento del toggle button
            tgConnect.CheckedChange += tgConnect_HandleCheckedChange;
            //Verificamos la disponibilidad del sensor Bluetooth en el dispositivo
            CheckBt();
            #endregion
        }

        #region MetodosBluetooth

        private void CheckBt()
        {
            //asignamos el sensor bluetooth con el que vamos a trabajar
            mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            //Verificamos que este habilitado
            if (!mBluetoothAdapter.Enable())
            {
                Toast.MakeText(this, "Bluetooth Desactivado",
                    ToastLength.Short).Show();
            }
            //verificamos que no sea nulo el sensor
            if (mBluetoothAdapter == null)
            {
                Toast.MakeText(this,
                    "Bluetooth No Existe o esta Ocupado", ToastLength.Short)
                    .Show();
            }
        }
        //Evento de cambio de estado del toggle button
        void tgConnect_HandleCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                //si se activa el toggle button se incial el metodo de conexion
                Connect();
            }
            else
            {
                //en caso de desactivar el toggle button se desconecta del arduino
                if (btSocket.IsConnected)
                {
                    try
                    {
                        btSocket.Close();
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        //Evento de conexion al Bluetooth
        public void Connect()
        {
            //Iniciamos la conexion con el arduino
            BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);
            System.Console.WriteLine("Conexion en curso" + device);

            //Indicamos al adaptador que ya no sea visible
            mBluetoothAdapter.CancelDiscovery();
            try
            {
                //Inicamos el socket de comunicacion con el arduino
                btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                //Conectamos el socket
                btSocket.Connect();
                System.Console.WriteLine("Conexion Correcta");
            }
            catch (System.Exception e)
            {
                //en caso de generarnos error cerramos el socket
                Console.WriteLine(e.Message);
                try
                {
                    btSocket.Close();
                }
                catch (System.Exception)
                {
                    System.Console.WriteLine("Imposible Conectar");
                }
                System.Console.WriteLine("Socket Creado");
            }
            //Una vez conectados al bluetooth mandamos llamar el metodo que generara el hilo
            //que recibira los datos del arduino
            beginListenForData();
            //NOTA envio la letra e ya que el sketch esta configurado para funcionar cuando
            //recibe esta letra.
            dataToSend = new Java.Lang.String("A");
            writeData(dataToSend);
        }
        //Evento para inicializar el hilo que escuchara las peticiones del bluetooth
        public void beginListenForData()
        {
            //Extraemos el stream de entrada
            try
            {
                inStream = btSocket.InputStream;
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            //Creamos un hilo que estara corriendo en background el cual verificara si hay algun dato
            //por parte del arduino
            Task.Factory.StartNew(() => {
                //declaramos el buffer donde guardaremos la lectura
                byte[] buffer = new byte[1024];
                //declaramos el numero de bytes recibidos
                int bytes;
                while (true)
                {
                    try
                    {
                        //leemos el buffer de entrada y asignamos la cantidad de bytes entrantes
                        bytes = inStream.Read(buffer, 0, buffer.Length);
                        //Verificamos que los bytes contengan informacion
                        if (bytes > 0)
                        {
                            //Corremos en la interfaz principal
                            RunOnUiThread(() => {
                                //Convertimos el valor de la informacion llegada a string
                                string valor = System.Text.Encoding.ASCII.GetString(buffer);
                                //Agregamos a nuestro label la informacion llegada
                                Result.Text = Result.Text + "\n" + valor;
                            });
                        }
                    }
                    catch (Java.IO.IOException)
                    {
                        //En caso de error limpiamos nuestra label y cortamos el hilo de comunicacion
                        RunOnUiThread(() => {
                            Result.Text = string.Empty;
                        });
                        break;
                    }
                }
            });
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
        #endregion

        private void TgConnect_HandleCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            throw new NotImplementedException();
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
}