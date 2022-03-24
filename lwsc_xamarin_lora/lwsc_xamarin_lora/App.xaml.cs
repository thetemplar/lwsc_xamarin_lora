using lwsc_xamarin_lora.Services;
using lwsc_xamarin_lora.Views;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lwsc_xamarin_lora
{
    public partial class App : Application
    {

        readonly UdpClient _udpClient = new UdpClient()
        {
            ExclusiveAddressUse = false,
            EnableBroadcast = true
        };

        public static IPEndPoint RemoteEP;

        public static bool ShowInformation = false;
        public static bool Experimental = false;

        public static DateTime LastGPSCheck;
        public static RESTful.GPSStatus LastGPSResult = RESTful.GPSStatus.NOTAVAILIBLE;

        public static string Username = "User";
        public static string Password = "lwsc";

        public App()
        {
            InitializeComponent();

            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 5556));
            _udpClient.BeginReceive(OnUdpDataReceived, _udpClient);

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();

            Task.Run(() =>
            {
                var resolvedIp = Dns.GetHostEntry("lwsc.ddns.net");
                var dnsCache = resolvedIp.AddressList[0];
                App.RemoteEP = new IPEndPoint(dnsCache, 8280);
            });

            Username = Preferences.Get("Username", "User");
            Password = Preferences.Get("Password", "lwsc");
        }

        private void OnUdpDataReceived(IAsyncResult result)
        {
            Debug.WriteLine($">>> in receive");

            var udpClient = result.AsyncState as UdpClient;
            if (udpClient == null)
                return;

            IPEndPoint remoteAddr = null;
            var recvBuffer = udpClient.EndReceive(result, ref remoteAddr);

            Debug.WriteLine($"MESSAGE FROM: {remoteAddr.Address}:{remoteAddr.Port}, MESSAGE SIZE: {recvBuffer?.Length ?? 0}");

            var val = Encoding.UTF8.GetString(recvBuffer);
            var m = Regex.Match(val, @"WIFIBRIDGE ((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)) (ETH|WIFI)");
            if (m.Success)
            {
                var ipAddress = IPAddress.Parse(m.Groups[1].Value);
                App.RemoteEP = new IPEndPoint(ipAddress, 80);
                DependencyService.Get<IMessage>().ShortAlert("WiFi Gateway connected!");
                App.LastGPSResult = RESTful.GPSStatus.WIFI;
                App.LastGPSCheck = DateTime.Now;
            } 
            else
            {
                udpClient.BeginReceive(OnUdpDataReceived, udpClient);
            }

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
