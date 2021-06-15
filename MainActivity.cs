using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Graphics;
using Android.Widget;
using Plugin.Media;
using System;
using System.IO;
using Plugin.CurrentActivity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Android.Runtime;

namespace AzureStorage
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        string Archivo;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            SupportActionBar.Hide();
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            var Imagen = FindViewById<ImageView>(Resource.Id.imagen);
            var btnAlmacenar = FindViewById<Button>(Resource.Id.btnGuardar);

            btnAlmacenar.click += async delegate
            {
                try
                {
                    var CuentadeAlmacenamiento = CloudStorageAccount.Parse
                    ("DefaultEndpointsProtocol=https;AccountName=programacionmoviles;AccountKey=K4HLGMkMGB87LlncsykIQe5QO85Ges6DDZ1wjK8M7EFpZeR+k+7fKLm3uy3th+R6mvmYeDa6pf2sn62Q3dZkWg==;EndpointSuffix=core.windows.net");
                    var ClienteBlob = CuentadeAlmacenamiento.CreateCloudBlobClient();
                    var Carpeta = ClienteBlob.GetContainerReference('enrique');
                    var resourceBlob = Carpeta.GetBlockBlobReference(txtNombre.text + ".jpg");
                    await resourceBlob.UploadFromFileAsync(Archivo.ToString());
                    Toast.MakeText(this, "Imagen Almacenada en Blob de Azure", ToastLenght.Long).Show();

                    var TablaNoSQL = CuentadeAlmacenamiento.CreateCloudTableClient();
                    var Coleccion = TablaNoSQL.GetTableReference("Registro");
                    await Coleccion.CreateIfNotExistsAsync();
                    var clientes = new Clientes("Clientes", txtNombre.Text);
                    clientes.Correo = txtCorreo.Text;
                    clientes.Saldo = double.Parse(txtSaldo.Text);
                    clientes.Edad = int.Parse(txtEdad.Text);
                    clientes.Domicilio = txtDomicilio.text;
                    clientes.ImagenBlob = txtNombre.text + ".jpg";
                    var Store = TableOperation.Insert(clientes);
                    await Coleccion.ExecuteAsync(Store);
                    Toast.MakeText(this, "Datos Guardados en Tabla NoSQL en Azure", ToastLength.Long).Show();
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                }
            };
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public class Clientes : TableEntity
        {
            public Clientes(string Categoria, string Nombre)
            {
                PartitionKey = Categoria;
                RowKey = Nombre;
            }
            public string Correo { get; set; }
            public string Domicilio { get; set; }
            public int Edad { get; set; }
            public double Saldo { get; set; }
            public string ImagenBlob { get; set; }

        }

    }
}