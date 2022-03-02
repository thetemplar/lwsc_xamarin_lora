using lwsc_xamarin_lora.Models;
using lwsc_xamarin_lora.Services;
using lwsc_xamarin_lora.ViewModels;
using lwsc_xamarin_lora.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lwsc_xamarin_lora.Views
{
    class FireRes
    {
        public string result;
    }
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel _viewModel;
        ItemsViewModel _viewModelFiltered;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ItemsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        
        private void functionList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Machine item = e.Item as Machine;
            item.WasSelected = true;
            var status = RESTful.Query("/fire?id=" + item.MachineID + "&f_id=" + item.FunctionID, RESTful.RESTType.POST, out string res);

            var parsedJson = JsonConvert.DeserializeObject<FireRes>(res);

            if (parsedJson.result == "success")
            {
                DependencyService.Get<IMessage>().ShortAlert(App.IpAddress + ": " + res);

                this.functionList.SelectedItem = null;
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
                    //return false;
                }
                catch (Exception ex)
                {
                    DependencyService.Get<IMessage>().ShortAlert("Vibration: Other error has occurred.");
                    //return false;
                }
            }
            else
            {
                DependencyService.Get<IMessage>().ShortAlert(App.IpAddress + ": " + res);
                //return false;
            }

            //return true;
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            if(Mode.Text == "Select Only")
            {
                _viewModelFiltered = new ItemsViewModel(_viewModel.Items.Where(x => x.WasSelected).ToList());
                BindingContext = _viewModelFiltered;
                Mode.Text = "Show All";
            } else
            {
                Mode.Text = "Select Only";
                BindingContext = _viewModel;

            }
        }
    }
}