using lwsc_xamarin_lora.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace lwsc_xamarin_lora.Services
{
    public class RESTful
    {
        public enum GPSStatus
        {
            NOTAVAILIBLE,
            DENIED,
            OUTOFRANGE_NEAR,
            OUTOFRANGE_FAR,
            INRANGE
        }

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


        static public async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                DependencyService.Get<IMessage>().ShortAlert("Turn on GPS in the Settings.");
                return status;
            }

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                // Prompt the user with additional information as to why the permission is needed
                DependencyService.Get<IMessage>().ShortAlert("GPS Permission is needed.");
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            return status;
        }

        static public async Task<GPSStatus> IsInGPSRangeAsyncTimeout(int ms)
        {
            var gpsTask = IsInGPSRangeAsync();
            var r = await Task.WhenAny(gpsTask, Task.Delay(ms));
            if (r == gpsTask)
            {
                await gpsTask;
                return gpsTask.Result;
            }
            return GPSStatus.NOTAVAILIBLE;
        }

        static public async Task<GPSStatus> IsInGPSRangeAsync()
        {
            PermissionStatus s = PermissionStatus.Unknown;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                s = await CheckAndRequestLocationPermission();
            });
            if(s != PermissionStatus.Granted)
                return GPSStatus.NOTAVAILIBLE;

            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location == null)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5));
                    var cts = new CancellationTokenSource();

                    location = await Geolocation.GetLocationAsync(request);
                }

                if (location != null)
                {
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    Location lwsc = new Location(50.651667, 9.422954);
                    var dist = Location.CalculateDistance(location, lwsc, DistanceUnits.Kilometers);
                    if (dist <= 1.5)
                        return GPSStatus.INRANGE;
                    if (dist <= 5)
                        return GPSStatus.OUTOFRANGE_NEAR;
                    return GPSStatus.OUTOFRANGE_FAR;
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                DependencyService.Get<IMessage>().ShortAlert("GPS not supported.");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                DependencyService.Get<IMessage>().ShortAlert("GPS not enabled.");
            }
            catch (PermissionException pEx)
            {
                DependencyService.Get<IMessage>().ShortAlert("GPS no permission.");
            }
            catch (Exception ex)
            {
                DependencyService.Get<IMessage>().ShortAlert("GPS error.");
            }
            return GPSStatus.NOTAVAILIBLE;
        }


        static public bool Fire(Machine item)
        {
            var status = RESTful.Query("/fire?username=" + App.Username + "&password=" + App.Password + "&id=" + item.MachineID + "&f_id=" + item.FunctionID, RESTful.RESTType.POST, out string res, fast:true);

            if (status == HttpStatusCode.NonAuthoritativeInformation)
            {
                DependencyService.Get<IMessage>().ShortAlert("Nicht auf dem Gelände.");
                return false;
            }
            if (status == HttpStatusCode.Unauthorized)
            {
                DependencyService.Get<IMessage>().ShortAlert("Unauthorized.");
                return false;
            }
            if (status != HttpStatusCode.OK)
            {
                DependencyService.Get<IMessage>().ShortAlert("Error.");
                return false;
            }

            var parsedJson = JsonConvert.DeserializeObject<FireRes>(res);

            if (parsedJson.result == "success")
            {
                if (App.ShowInformation)
                    DependencyService.Get<IMessage>().ShortAlert(App.RemoteEP.Address + ": " + res);

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
                    DependencyService.Get<IMessage>().ShortAlert(App.RemoteEP.Address + ": " + res);
                return false;
            }

            return true;
        }

        static public HttpStatusCode UploadFile(string url, string path, out string result)
        {
            result = "";
            if (!App.WIFIStatus && !App.AdminStatus && App.GPSStatus != GPSStatus.INRANGE)
            {
                return HttpStatusCode.NonAuthoritativeInformation;
            }

            using (WebClient client = new WebClient())
            {
                try
                {
                    var resultBytes = client.UploadFile("http://" + App.RemoteEP + "/upload?username=" + App.Username + "&password=" + App.Password, path);
                    result = Encoding.ASCII.GetString(resultBytes, 0, resultBytes.Length);
                }
                catch (WebException ex)
                {
                    if (ex.Message.Contains("401")) return HttpStatusCode.Unauthorized;
                }
            }

            return HttpStatusCode.OK;
        }

        static public HttpStatusCode Query(string url, RESTType type, out string result, bool everywhere = false, bool fast = false)
        {
            byte[] bytes = new byte[10240];
            result = "";

            if (!everywhere && !App.WIFIStatus && !App.AdminStatus && App.GPSStatus != GPSStatus.INRANGE)
            {
                return HttpStatusCode.NonAuthoritativeInformation;
            }

            if (fast && App.Experimental)
            {
                Socket sender = new Socket(App.RemoteEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(App.RemoteEP);

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
                var request = (HttpWebRequest)WebRequest.Create("http://" + App.RemoteEP.ToString() + url);
                request.Timeout = 2000;

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
