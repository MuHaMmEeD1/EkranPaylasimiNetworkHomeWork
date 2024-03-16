using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System;

namespace ClientEkranPaylasimi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Mutex mutex = new Mutex(false ,"Global873");
        UdpClient Client;
        IPEndPoint? remoutEP;
        bool Started = false;

        public MainWindow()
        {
            InitializeComponent();
            remoutEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27001);
            Client = new UdpClient();

            Client.Send(new byte[1], remoutEP);


            _ = Task.Run(() => {

                byte[] bytes = new byte[ushort.MaxValue - 30]; ;


                while (true)
                {
                    bytes = new byte[ushort.MaxValue - 30];
                    List<byte> EsasBytesList = new List<byte>();

                    while (true)
                    {
                        IPEndPoint? rEP = null;
                        bytes = Client.Receive(ref rEP);

                        int bytesReceived = bytes.Length;
                        EsasBytesList.AddRange(bytes.Take(bytesReceived));

                        if (bytesReceived < ushort.MaxValue - 30)
                        {
                            break;
                        }
                    }

                    byte[] EsasBytes = EsasBytesList.ToArray();



                    Dispatcher.Invoke(() => {
                        BitmapImage bitmap = new BitmapImage();

                        using (MemoryStream ms = new MemoryStream(EsasBytes))
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                        }

                        EsasImage.Source = bitmap;
                    });
                }




            });
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (!Started) {
                if (mutex.WaitOne(1000))
                {
                    if (!Started)
                    {
                        Started = true;
                        StartScreenMetod();
                    }
                }
                else
                {
                    MessageBox.Show("Hal Hazirda Istifadededi");
                }
            }
            else
            {
                MessageBox.Show("Hal Hazirda Istifadededi");
            }

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (Started)
            {
                Started = false;
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("Onsuzda islemir");
            }
        }



        async Task StartScreenMetod()
        {
            _ = Task.Run(() => {

                while (Started)
                {
                    Bitmap bitmap = ScreenEkran();
                    byte[] bytes = ImegiBytaCevir(bitmap);
                    var bytesList = bytes.Chunk(ushort.MaxValue - 30);

                    foreach (var by in bytesList)
                    {
                        Client.Send(by, remoutEP);
                    }

                }

            });

        }


        static Bitmap ScreenEkran() // +
        {

            Rectangle bounds = new Rectangle(0, 0, 1920, 1080);

            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(screenshot))
            {
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            return screenshot;

        }

        static byte[] ImegiBytaCevir(Bitmap image) // +
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }


    }
}