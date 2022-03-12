using lwsc_xamarin_lora.Services;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lwsc_xamarin_lora.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            if (!RESTful.IsInGPSRange(out double dist))
            {
                DependencyService.Get<IMessage>().ShortAlert("GPS: Nicht auf dem Gelände!");
            }
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
            if (App.IpAddress.Length > 0)
                DependencyService.Get<IMessage>().ShortAlert("IP: " + App.IpAddress);
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
    }
}