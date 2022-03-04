using lwsc_remote.Services;
using lwsc_remote.Views;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lwsc_remote
{
    public partial class App : Application
    {

        readonly UdpClient _udpClient = new UdpClient()
        {
            ExclusiveAddressUse = false,
            EnableBroadcast = true
        };
        public static string IpAddress = "";
        public static bool ShowInformation = false;
        

        public App()
        {
            InitializeComponent();

            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 5556));
            _udpClient.BeginReceive(OnUdpDataReceived, _udpClient);

            /*
            Task.Run(() =>
            {
                bool waiting = true;
                while (waiting)
                {
                    var recvBuffer = udpClient.Receive(ref from);
                    var val = Encoding.UTF8.GetString(recvBuffer);
                    var m = Regex.Match(val, @"WIFIBRIDGE ((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)) (ETH|WIFI)");
                    if (m.Success)
                    {
                        DependencyService.Get<IMessage>().LongAlert(val);
                        IpAddress = m.Groups[1].Value;
                        waiting = false;
                    }
                }
            });*/

            //DependencyService.Get<IMessage>().LongAlert("Hello");
            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
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
                IpAddress = m.Groups[1].Value;
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
