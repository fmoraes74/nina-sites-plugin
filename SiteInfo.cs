using System.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

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
        }

        public SiteInfo(string name, double lat, double lon, double ele)
        {
            _current = false;
            _name = name;
            _latitude = lat;
            _longitude = lon;
            _elevation = ele;
        }

        private bool _current;
        private string _name;
        private double _latitude;
        private double _longitude;
        private double _elevation;

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

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
