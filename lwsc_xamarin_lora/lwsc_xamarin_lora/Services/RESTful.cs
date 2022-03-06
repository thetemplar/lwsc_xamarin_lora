using lwsc_xamarin_lora.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xamarin.Essentials;
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

        class FireRes
        {
            public string result;
        }

        static public bool Fire(Machine item)
        {
            var status = RESTful.Query("/fire?id=" + item.MachineID + "&f_id=" + item.FunctionID, RESTful.RESTType.POST, out string res, true);
            if (status != HttpStatusCode.OK)
                return false;

            var parsedJson = JsonConvert.DeserializeObject<FireRes>(res);

            if (parsedJson.result == "success")
            {
                if (App.ShowInformation)
                    DependencyService.Get<IMessage>().ShortAlert(App.IpAddress + ": " + res);

                try
                {
                    // Use default vibration length
                    Vibration.Vibrate();

                    // Or use specified time
                    var duration = TimeSpan.FromSeconds(0.3);
                    Vibration.Vibrate(duration);
                }
                catch (FeatureNotSupportedException ex)
                {
                    DependencyService.Get<IMessage>().ShortAlert("Vibration not supported on device");
                }
                catch (Exception ex)
                {
                    DependencyService.Get<IMessage>().ShortAlert("Vibration: Other error has occurred.");
                }
            }
            else
            {
                if (App.ShowInformation)
                    DependencyService.Get<IMessage>().ShortAlert(App.IpAddress + ": " + res);
                return false;
            }

            return true;
        }

        static public HttpStatusCode UploadFile(string url, string path, out string result)
        {
            IPEndPoint remoteEP;
            IPAddress ipAddress;
            if (App.IpAddress.Length > 0)
            {
                ipAddress = IPAddress.Parse(App.IpAddress);
                remoteEP = new IPEndPoint(ipAddress, 80);
            }
            else
            {
                if (_dnsCache == null)
                {
                    var resolvedIp = Dns.GetHostEntry("lwsc.ddns.net");
                    _dnsCache = resolvedIp.AddressList[0];
                }
                ipAddress = _dnsCache;
                remoteEP = new IPEndPoint(_dnsCache, 8280);
            }

            using (WebClient client = new WebClient())
            {
                var resultBytes = client.UploadFile("http://" + remoteEP + "/upload", path);
                result = Encoding.ASCII.GetString(resultBytes, 0, resultBytes.Length);
            }

            return HttpStatusCode.OK;
        }

        static public HttpStatusCode Query(string url, RESTType type, out string result, bool fast = false)
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
                    var resolvedIp = Dns.GetHostEntry("lwsc.ddns.net");
                    _dnsCache = resolvedIp.AddressList[0];
                }
                ipAddress = _dnsCache;
                remoteEP = new IPEndPoint(_dnsCache, 8280);
            }

            if (fast && App.Experimental)
            {
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

                byte[] msg = Encoding.ASCII.GetBytes(method + " " + url + " HTTP/1.1\r\n\r\n");

                int bytesSent = sender.Send(msg);

                int bytesRec = sender.Receive(bytes);
                string s = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine("Echoed test = {0}", s);

                var headerCode = (HttpStatusCode)int.Parse(s.Split(' ')[1]);

                if (headerCode != HttpStatusCode.OK)
                    return headerCode;

                bytesRec = sender.Receive(bytes);
                result = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            else
            {
                var request = (HttpWebRequest)WebRequest.Create("http://" + remoteEP.ToString() + url);
                request.Timeout = 500;

                switch (type)
                {
                    case RESTType.GET:
                        request.Method = "GET";
                        break;
                    case RESTType.POST:
                        request.Method = "POST";
                        break;
                    case RESTType.DELETE:
                        request.Method = "DELETE";
                        break;
                    default:
                        throw new Exception();
                }
                var content = string.Empty;

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    using (var stream = response.GetResponseStream())
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            content = sr.ReadToEnd();
                        }
                    }

                    result = content;
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        response = (HttpWebResponse)e.Response;
                        return response.StatusCode;
                    }
                    else if (e.Status == WebExceptionStatus.Timeout)
                    {
                        return HttpStatusCode.GatewayTimeout;
                    }
                    else
                    {
                        return HttpStatusCode.Conflict;
                    }
                }
                catch (Exception ex)
                {
                    return HttpStatusCode.GatewayTimeout;
                }
            }

            return HttpStatusCode.OK;
        }
    }
}
