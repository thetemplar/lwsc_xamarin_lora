using System;
using System.ComponentModel;
using lwsc_remote.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lwsc_remote.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void ShowInformation(object sender, EventArgs e)
        {
            App.ShowInformation = !App.ShowInformation;
            if (App.ShowInformation)
                btInformation.Text = "Hide Information";
            else
                btInformation.Text = "Show Information";
        }

        private void ShowIP(object sender, EventArgs e)
        {
            DependencyService.Get<IMessage>().ShortAlert("IP: " + App.IpAddress);
        }
    }
}