using System;
using System.ComponentModel;

namespace lwsc_xamarin_lora.Models
{
    public class Machine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public string MachineID { get; set; }
        public string FunctionID { get; set; }

        private bool wasSelected = false;
        public bool WasSelected {
            get => wasSelected;
            set
            {
                if (wasSelected != value)
                {
                    wasSelected = value;
                    OnPropertyChanged(nameof(WasSelected));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}