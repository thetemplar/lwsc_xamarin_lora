using lwsc_remote.Models;
using lwsc_remote.Services;
using lwsc_remote.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lwsc_remote.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PadPage : ContentPage
    {
        ItemsViewModel _viewModel;
        private Button _buttonToChange;

        public PadPage()
        {
            BindingContext = _viewModel = new ItemsViewModel();
            _viewModel.LoadItemsCommand.Execute(_viewModel);
            InitializeComponent();

            string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pad-tmp");
            if (File.Exists(p))
            {
                var res = File.ReadAllText(p);
                var entries = res.Split(';');

                ButtonGrid.RowDefinitions.Clear();
                ButtonGrid.ColumnDefinitions.Clear();
                ButtonGrid.Children.Clear();
                ButtonGrid.VerticalOptions = LayoutOptions.FillAndExpand;
                ButtonGrid.HorizontalOptions = LayoutOptions.FillAndExpand;

                var w = int.Parse(entries[0]);
                var h = int.Parse(entries[1]);

                for (int i = 0; i < w; i++)
                    ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                for (int i = 0; i < h; i++)
                    ButtonGrid.RowDefinitions.Add(new RowDefinition() { });

                int c = 2;
                for (int i = 0; i < h; i++)
                    for (int j = 0; j < w; j++)
                    {
                        var bt = new Button() { Text = entries[c], VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
                        var lp = new LongPressBehavior();
                        lp.ShortPressed += MyButton_Pressed;
                        lp.LongPressed += MyButton_LongPressed;
                        lp.TagText = entries[c];
                        lp.Button = bt;
                        bt.Behaviors.Add(lp);
                        ButtonGrid.Children.Add(bt, j, i);
                        c++;
                    }
            }
        }

        public void MyButton_Pressed(object sender, EventArgs e)
        {
            var t = ((LongPressBehavior)sender).TagText;
            Machine item = _viewModel.Items.FirstOrDefault(x => x.Name == t);
            if(item == null)
            {
                DependencyService.Get<IMessage>().ShortAlert("Not found.");
                return;
            }

            item.WasSelected = true;
            var status = RESTful.Query("/fire?id=" + item.MachineID + "&f_id=" + item.FunctionID, RESTful.RESTType.POST, out string res);

            var parsedJson = JsonConvert.DeserializeObject<FireRes>(res);

            if (parsedJson.result == "success")
            {
                if(App.ShowInformation)
                    DependencyService.Get<IMessage>().ShortAlert(App.IpAddress + ": " + res);


                if (((LongPressBehavior)sender).Button != null)
                    ((LongPressBehavior)sender).Button.TextColor = Color.White;

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
                if(App.ShowInformation)
                    DependencyService.Get<IMessage>().ShortAlert(App.IpAddress + ": " + res);

                if (((LongPressBehavior)sender).Button != null)
                    ((LongPressBehavior)sender).Button.TextColor = Color.Red;
                //return false;
            }

            //return true;
        }

        public void MyButton_LongPressed(object sender, EventArgs e)
        {
            overlay_machine.IsVisible = true;
            _buttonToChange = ((LongPressBehavior)sender).Button;
            if (_buttonToChange == null)
                _buttonToChange = FindByName(((LongPressBehavior)sender).TagText) as Button;
        }

        private void OnMachineCancelButtonClicked(object sender, EventArgs e)
        {
            overlay_machine.IsVisible = false;
        }

        private void OnMachineChangeButtonClicked(object sender, EventArgs e)
        {
            if (overlay_picker.SelectedIndex != -1)
            {
                var bt = _buttonToChange;
                bt.Text = overlay_picker.Items[overlay_picker.SelectedIndex];
                var b = (LongPressBehavior)bt.Behaviors[0];
                b.TagText = overlay_picker.Items[overlay_picker.SelectedIndex];


                overlay_picker.SelectedIndex = -1;


                string s = "";
                s += ButtonGrid.ColumnDefinitions.Count() + ";";
                s += ButtonGrid.RowDefinitions.Count() + ";";

                for (int i = 0; i < ButtonGrid.Children.Count(); i++)
                    s += ((Button)ButtonGrid.Children[i]).Text + ";";
                string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pad-tmp");
                File.WriteAllText(p, s);
            }
            overlay_machine.IsVisible = false;
        }

        private void OnNewPadCancelButtonClicked(object sender, EventArgs e)
        {
            overlay_newpad.IsVisible = false;
        }

        private void OnNewPadNewButtonClicked(object sender, EventArgs e)
        {
            if (NewPadWidth.Text.Count() == 0 || NewPadHeight.Text.Count() == 0)
            {
                overlay_newpad.IsVisible = true;
                return;
            }
            ButtonGrid.RowDefinitions.Clear();
            ButtonGrid.ColumnDefinitions.Clear();
            ButtonGrid.Children.Clear();
            ButtonGrid.VerticalOptions = LayoutOptions.FillAndExpand;
            ButtonGrid.HorizontalOptions = LayoutOptions.FillAndExpand;

            var w = int.Parse(NewPadWidth.Text);
            var h = int.Parse(NewPadHeight.Text);

            for (int i = 0; i < w; i++)
                ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            for (int i = 0; i < h; i++)
                ButtonGrid.RowDefinitions.Add(new RowDefinition() {});

            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    var bt = new Button() { Text = "Change me!", VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
                    var lp = new LongPressBehavior();
                    lp.ShortPressed += MyButton_Pressed;
                    lp.LongPressed += MyButton_LongPressed;
                    lp.TagText = "bt_" + j + "_" + i;
                    lp.Button = bt;
                    bt.Behaviors.Add(lp);
                    ButtonGrid.Children.Add(bt, j, i);
                }
            overlay_newpad.IsVisible = false;
        }

        private void OnNewPadClicked(object sender, EventArgs e)
        {
            overlay_newpad.IsVisible = true;
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            overlay_save.IsVisible = true;
        }

        private void OnLoadClicked(object sender, EventArgs e)
        {
            var status = RESTful.Query("/file_list", RESTful.RESTType.GET, out string res);
            var files = JsonConvert.DeserializeObject<List<string>>(res);
            overlay_load_picker.Items.Clear();
            if (files != null && files.Count > 1)
                foreach (var file in files.Where(x => x.StartsWith("pad_")))
                    overlay_load_picker.Items.Add(file.Substring(4));
            overlay_load.IsVisible = true;
        }

        private void OnSaveCancelButtonClicked(object sender, EventArgs e)
        {
            overlay_save.IsVisible = false;
        }

        private void OnSaveButtonClicked(object sender, EventArgs e)
        {
            overlay_save.IsVisible = false;
            string s = "";
            s += ButtonGrid.ColumnDefinitions.Count() + ";";
            s += ButtonGrid.RowDefinitions.Count() + ";";

            for (int i = 0; i < ButtonGrid.Children.Count(); i++)
                s += ((Button)ButtonGrid.Children[i]).Text + ";";
            string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pad_" + SaveName.Text);
            File.WriteAllText(p, s);



            IPEndPoint remoteEP;
            IPAddress ipAddress;
            if (App.IpAddress.Length > 0)
            {
                ipAddress = IPAddress.Parse(App.IpAddress);
                remoteEP = new IPEndPoint(ipAddress, 80);
            }
            else
            {
                var resolvedIp = Dns.GetHostEntry("lwsc.ddns.net");
                var _dnsCache = resolvedIp.AddressList[0];
                remoteEP = new IPEndPoint(_dnsCache, 8280);
            }


            using (WebClient client = new WebClient())
            {
                client.UploadFile("http://" + remoteEP + "/upload", p);
            }
        }

        private void OnLoadCancelButtonClicked(object sender, EventArgs e)
        {
            overlay_load.IsVisible = false;
        }

        private void OnLoadButtonClicked(object sender, EventArgs e)
        {
            if (overlay_load_picker.SelectedIndex == -1)
            {
                overlay_load.IsVisible = false;
                return;
            }
            var status = RESTful.Query("/file?filename=pad_"+ overlay_load_picker.Items[overlay_load_picker.SelectedIndex], RESTful.RESTType.GET, out string res);

            var entries = res.Split(';');

            ButtonGrid.RowDefinitions.Clear();
            ButtonGrid.ColumnDefinitions.Clear();
            ButtonGrid.Children.Clear();
            ButtonGrid.VerticalOptions = LayoutOptions.FillAndExpand;
            ButtonGrid.HorizontalOptions = LayoutOptions.FillAndExpand;

            var w = int.Parse(entries[0]);
            var h = int.Parse(entries[1]);

            for (int i = 0; i < w; i++)
                ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            for (int i = 0; i < h; i++)
                ButtonGrid.RowDefinitions.Add(new RowDefinition() { });

            int c = 2;
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    var bt = new Button() { Text = entries[c], VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
                    var lp = new LongPressBehavior();
                    lp.ShortPressed += MyButton_Pressed;
                    lp.LongPressed += MyButton_LongPressed;
                    lp.TagText = entries[c];
                    lp.Button = bt;
                    bt.Behaviors.Add(lp);
                    ButtonGrid.Children.Add(bt, j, i);
                    c++;
                }

            overlay_load.IsVisible = false;
        }
    }
}