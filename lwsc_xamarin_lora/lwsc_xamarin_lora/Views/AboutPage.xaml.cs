using lwsc_xamarin_lora.Services;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lwsc_xamarin_lora.Views
{
    class CheckUser
    {
        public string result;
        public string rights;
    }
    
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            Task.Run(
                async () => {
                    var r = await RESTful.IsInGPSRangeAsyncTimeout(3000);
                    if (r == RESTful.GPSStatus.NOTAVAILIBLE)
                        DependencyService.Get<IMessage>().ShortAlert("GPS: Nicht verfügbar!");
                    if (r == RESTful.GPSStatus.OUTOFRANGE_NEAR)
                        DependencyService.Get<IMessage>().ShortAlert("GPS: Nicht auf dem Gelände! (<5km)");
                    if (r == RESTful.GPSStatus.OUTOFRANGE_FAR)
                        DependencyService.Get<IMessage>().ShortAlert("GPS: Nicht auf dem Gelände! (>5km)");
                }).Wait();

            eUsername.Text = Preferences.Get("Username", "User");
            ePassword.Text = Preferences.Get("Password", "lwsc");
        }

        private void ShowInformation(object sender, EventArgs e)
        {
            App.ShowInformation = !App.ShowInformation;
            if (App.ShowInformation)
            {
                btInformation.Text = "Auslöse-Information ausblenden";
                DependencyService.Get<IMessage>().ShortAlert("Ab jetzt werden Auslöse-Infos ausgeblendet.");
            }
            else
            {
                btInformation.Text = "Auslöse-Informationen anzeigen";
                DependencyService.Get<IMessage>().ShortAlert("Ab jetzt werden Auslöse-Infos gezeigt.");
            }
        }

        private void ShowIP(object sender, EventArgs e)
        {
            if (App.RemoteEP.Address.ToString().Length > 0)
                DependencyService.Get<IMessage>().ShortAlert("IP: " + App.RemoteEP.Address.ToString());
            else
                DependencyService.Get<IMessage>().ShortAlert("IP: <Internet>");
        }

        private void EnableExperimental(object sender, EventArgs e)
        {
            App.Experimental = !App.Experimental;
            if (App.Experimental)
            {
                btExperimental.Text = "Experimental deaktivieren";
                DependencyService.Get<IMessage>().ShortAlert("Experimental: Aktiviert (schneller)");
            }
            else
            {
                btExperimental.Text = "Experimental aktivieren";
                DependencyService.Get<IMessage>().ShortAlert("Experimental: Deaktiviert (sicherer)");
            }
        }

        private void TestGPS(object sender, EventArgs e)
        {
            DependencyService.Get<IMessage>().ShortAlert("GPS " + App.GPSStatus + Environment.NewLine + "Admin " + App.AdminStatus + Environment.NewLine + "WIFI " + App.WIFIStatus);
        }

        private void Login(object sender, EventArgs e)
        {
            var status = RESTful.Query("/check_user?username=" + eUsername.Text + "&password=" + ePassword.Text + "", RESTful.RESTType.GET, out string res, everywhere: true);
            if (status == HttpStatusCode.Unauthorized)
            {
                DependencyService.Get<IMessage>().ShortAlert("Unauthorized.");
                return;
            }
            if (status != HttpStatusCode.OK)
            {
                DependencyService.Get<IMessage>().ShortAlert("Error.");
                return;
            }

            var parsedJson = JsonConvert.DeserializeObject<CheckUser>(res);

            if (parsedJson.result == "no auth")
                DependencyService.Get<IMessage>().ShortAlert("User nicht gefunden.");
            else if (parsedJson.result == "wrong password")
                DependencyService.Get<IMessage>().ShortAlert("Falsches Password.");
            else if (parsedJson.result == "success")
            {
                App.Username = eUsername.Text;
                App.Password = ePassword.Text;
                Preferences.Set("Username", eUsername.Text);
                Preferences.Set("Password", ePassword.Text);
                DependencyService.Get<IMessage>().ShortAlert("Hallo " + App.Username + "! (" + parsedJson.rights + ")");
                if(parsedJson.rights == "Admin")
                {
                    App.AdminStatus = true;
                }
            }
            else
                DependencyService.Get<IMessage>().ShortAlert(res);

        }

        private void ForceIP(object sender, EventArgs e)
        {
            if (!eIP.Text.Contains(":"))
                eIP.Text += ":80";
            var ss = eIP.Text.Split(':');
            var ipAddress = IPAddress.Parse(ss[0]);
            App.RemoteEP = new IPEndPoint(ipAddress, int.Parse(ss[1]));
            DependencyService.Get<IMessage>().ShortAlert("IP ist gesetzt!");

            App.WIFIStatus = true;
        }
    }
}