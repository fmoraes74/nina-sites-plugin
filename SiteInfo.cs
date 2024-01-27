using System.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using NINA.Core.Utility;
using System.IO;
using System.Windows.Input;

namespace FMoraes.NINA.SitesPlugin
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class SiteInfo: INotifyPropertyChanged
    {
        public SiteInfo()
        {
            _current = false;
            _name = "";
            _latitude = 0.0;
            _longitude = 0.0;
            _elevation = 0.0;
            _horizonFilePath = string.Empty;
            HorizonCommand = new RelayCommand(OpenHorizonFilePathDiag);
        }

        public SiteInfo(string name, double lat, double lon, double ele, string hrz)
        {
            _current = false;
            _name = name;
            _latitude = lat;
            _longitude = lon;
            _elevation = ele;
            _horizonFilePath = hrz;
            HorizonCommand = new RelayCommand(OpenHorizonFilePathDiag);
        }

        private bool _current;
        private string _name;
        private double _latitude;
        private double _longitude;
        private double _elevation;

        [XmlIgnore]

        public ICommand HorizonCommand {
            get; private set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [XmlIgnore]
        public bool IsCurrent
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
                RaisePropertyChanged();
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public double Latitude
        {
            get
            {
                return _latitude;
            }

            set
            {
                _latitude = value;
                RaisePropertyChanged();
            }
        }
 
        public double Longitude
        {
            get
            {
                return _longitude;
            }

            set
            {
                _longitude = value;
                RaisePropertyChanged();
            }
        }

        public double Elevation
        {
            get
            {
                return _elevation;
            }

            set
            {
                _elevation = value;
                RaisePropertyChanged();
            }
        }

        private string _horizonFilePath;
        public string HorizonFilePath {
            get {
                return _horizonFilePath;
            }
            set {
                _horizonFilePath = value;
                RaisePropertyChanged();
            }
        }

        // borrowed from nina/NINA/ViewModel/OptionsVM.cs
        public void OpenHorizonFilePathDiag(object arg) {
            var dialog = GetFilteredFileDialog(string.Empty, string.Empty, "Horizon File|*.hrz;*.hzn;*.txt|MountWizzard4 Horizon File|*.hpts");
            if (dialog.ShowDialog() == true) {
                HorizonFilePath = dialog.FileName;
            }
        }

        public static Microsoft.Win32.OpenFileDialog GetFilteredFileDialog(string path, string filename, string filter) {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            if (File.Exists(path)) {
                dialog.InitialDirectory = Path.GetDirectoryName(path);
            }
            dialog.FileName = filename;
            dialog.Filter = filter;
            return dialog;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
