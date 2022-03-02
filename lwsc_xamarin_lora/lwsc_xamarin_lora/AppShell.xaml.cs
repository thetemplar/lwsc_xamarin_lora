using lwsc_xamarin_lora.ViewModels;
using lwsc_xamarin_lora.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace lwsc_xamarin_lora
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
