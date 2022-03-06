using lwsc_xamarin_lora.Models;
using lwsc_xamarin_lora.Services;
using lwsc_xamarin_lora.ViewModels;
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

namespace lwsc_xamarin_lora.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PadPage : ContentPage
    {
        ItemsViewModel _viewModel;
        private Button _buttonToChange;
        bool changeSize = false;

        public PadPage()
        {
            BindingContext = _viewModel = new ItemsViewModel();
            _viewModel.LoadItemsCommand.Execute(_viewModel);
            _viewModel.Items.Insert(0, new Machine() { Name = " ", MachineID = "-1", FunctionID = "-1" });

            InitializeComponent();

            string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pad-tmp");
            if (File.Exists(p))
            {
                var res = File.ReadAllText(p);
                string[] entries = res.Split(';');
                LoadGrid(int.Parse(entries[0]), int.Parse(entries[1]), entries.Skip(2));
            }
        }

        private void LoadGrid(int w, int h, IEnumerable<string> entries)
        {
            ButtonGrid.RowDefinitions.Clear();
            ButtonGrid.ColumnDefinitions.Clear();
            ButtonGrid.Children.Clear();
            ButtonGrid.VerticalOptions = LayoutOptions.FillAndExpand;
            ButtonGrid.HorizontalOptions = LayoutOptions.FillAndExpand;

            for (int i = 0; i < w; i++)
                ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            for (int i = 0; i < h; i++)
                ButtonGrid.RowDefinitions.Add(new RowDefinition() { });

            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    string name = (entries == null) ? " " : entries.ElementAt(i * w + j);

                    var bt = new Button() { Text = name, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
                    var lp = new LongPressBehavior();
                    lp.ShortPressed += MyButton_Pressed;
                    lp.LongPressed += MyButton_LongPressed;
                    lp.TagText = name;
                    lp.Button = bt;
                    bt.Behaviors.Add(lp);
                    ButtonGrid.Children.Add(bt, j, i);
                }
        }

        public void MyButton_Pressed(object sender, EventArgs e)
        {
            var t = ((LongPressBehavior)sender).TagText;
            Machine item = _viewModel.Items.FirstOrDefault(x => x.Name == t);
            if (item == null)
            {
                DependencyService.Get<IMessage>().ShortAlert("Not found.");
                return;
            }
            if (item.Name == " " || item.MachineID == "-1" || item.FunctionID == "-1")
            {
                return;
            }

            if (RESTful.Fire(item))
            {
                if (((LongPressBehavior)sender).Button != null)
                    ((LongPressBehavior)sender).Button.TextColor = Color.White;
            }
            else
            {
                if (((LongPressBehavior)sender).Button != null)
                    ((LongPressBehavior)sender).Button.TextColor = Color.Salmon;
            }
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
            string m = " ";
            if (overlay_picker.SelectedIndex != -1)
            {
                m = overlay_picker.Items[overlay_picker.SelectedIndex];
            }

            var bt = _buttonToChange;
            bt.Text = m;
            var b = (LongPressBehavior)bt.Behaviors[0];
            b.TagText = m;

            overlay_picker.SelectedIndex = -1;


            string s = "";
            s += ButtonGrid.ColumnDefinitions.Count() + ";";
            s += ButtonGrid.RowDefinitions.Count() + ";";

            for (int i = 0; i < ButtonGrid.Children.Count(); i++)
                s += ((Button)ButtonGrid.Children[i]).Text + ";";
            string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pad-tmp");
            File.WriteAllText(p, s);
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
            LoadGrid(int.Parse(NewPadWidth.Text), int.Parse(NewPadHeight.Text), null);
            overlay_newpad.IsVisible = false;
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
            string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pad_" + SaveName.Text.Replace("[", "").Replace("]", "").Replace(",", ".").Replace("\\", "").Replace("/", ""));
            File.WriteAllText(p, s);

            RESTful.UploadFile("/upload", p, out string _);
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

            LoadGrid(int.Parse(entries[0]), int.Parse(entries[1]), entries.Skip(2));

            overlay_load.IsVisible = false;
        }

        private void ToolbarItem_New(object sender, EventArgs e)
        {
            changeSize = false;
            overlay_newpad.IsVisible = true;
        }

        private void ToolbarItem_Save(object sender, EventArgs e)
        {
            overlay_save.IsVisible = true;
        }

        private void ToolbarItem_Load(object sender, EventArgs e)
        {
            var status = RESTful.Query("/file_list", RESTful.RESTType.GET, out string res);
            if(res.StartsWith("[") && res.EndsWith("]"))
            {
                string[] files = res.Replace("[", "").Replace("]", "").Split(',');
                overlay_load_picker.Items.Clear();
                if (files != null && files.Count() > 1)
                    foreach (var file in files.Where(x => x.StartsWith("pad_")))
                        overlay_load_picker.Items.Add(file.Substring(4));
            }
            overlay_load.IsVisible = true;
        }
    }
}