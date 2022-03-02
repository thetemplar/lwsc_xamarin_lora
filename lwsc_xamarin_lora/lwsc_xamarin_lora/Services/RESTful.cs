using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xamarin.Forms;

namespace lwsc_xamarin_lora.Services
{
    internal class RESTful
    {
        static IPAddress _dnsCache;
        public enum RESTType
        {
            GET,
            POST,
            DELETE
        }

        static public HttpStatusCode Query(string url, RESTType type, out string result)
        {
            byte[] bytes = new byte[10240];
            result = "";
            IPEndPoint remoteEP;
            IPAddress ipAddress;
            if (App.IpAddress.Length > 0)
            {
                ipAddress = IPAddress.Parse(App.IpAddress);
                remoteEP = new IPEndPoint(ipAddress, 80);
            }
            else
            {
                if(_dnsCache == null)
                {
                    //Uri myUri = new Uri("http://lwsc.ddns.net:8280/");
                    //_dnsCache = Dns.GetHostAddresses(myUri.Host)[0];
                    var resolvedIp = Dns.GetHostEntry("lwsc.ddns.net");
                    _dnsCache = resolvedIp.AddressList[0];
                }
                ipAddress = _dnsCache;
                remoteEP = new IPEndPoint(_dnsCache, 8280);
            }

            //var request = (HttpWebRequest)WebRequest.Create("http://" + defaultUrl + url);
            //request.Timeout = 1000;

            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);

            string method;
            switch (type)
            {
                case RESTType.GET:
                    method = "GET";
                    break;
                case RESTType.POST:
                    method = "POST";
                    break;
                case RESTType.DELETE:
                    method = "DELETE";
                    break;
                default:
                    throw new Exception();
            }

            byte[] msg = Encoding.ASCII.GetBytes(method + " " + url + " HTTP/1.0\r\n\r\n");

            int bytesSent = sender.Send(msg);

            int bytesRec = sender.Receive(bytes);
            string s = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            Console.WriteLine("Echoed test = {0}", s);

            var headerCode = (HttpStatusCode)int.Parse(s.Split(' ')[1]);

            if (headerCode != HttpStatusCode.OK)
                return headerCode;

            bytesRec = sender.Receive(bytes);
            result = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            Console.WriteLine("Echoed test = {0}", result);

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();

            return HttpStatusCode.OK;
        }
    }
}
