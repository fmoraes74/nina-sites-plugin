using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile;
using NINA.Profile.Interfaces;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Settings = FMoraes.NINA.SitesPlugin.Properties.Settings;
using System.Windows.Input;
using System.IO;

namespace FMoraes.NINA.SitesPlugin {
    /// <summary>
    /// This class exports the IPluginManifest interface and will be used for the general plugin information and options
    /// The base class "PluginBase" will populate all the necessary Manifest Meta Data out of the AssemblyInfo attributes. Please fill these accoringly
    /// 
    /// An instance of this class will be created and set as datacontext on the plugin options tab in N.I.N.A. to be able to configure global plugin settings
    /// The user interface for the settings will be defined by a DataTemplate with the key having the naming convention "<MyPlugin.Name>_Options" where MyPlugin.Name corresponds to the AssemblyTitle - In this template example it is found in the Options.xaml
    /// </summary>
    [Export(typeof(IPluginManifest))]
    public class SitesPlugin : PluginBase, INotifyPropertyChanged {
        private IPluginOptionsAccessor pluginSettings;
        private IProfileService profileService;

        private ObserveAllCollection<SiteInfo> _sites;
        private SiteInfo _selectedSite;
        public SiteInfo SelectedSite {
            get {
                return _selectedSite;
            }
            set {
                _selectedSite = value;
                RaisePropertyChanged();
            }
        }

        public ObserveAllCollection<SiteInfo> Sites {
            get {
                return _sites;
            }
            set {
                _sites = value;
                RaisePropertyChanged();
            }
        }

        public ICommand AddSiteCommand { get; private set; }
        public ICommand RemoveSiteCommand { get; private set; }
        public ICommand SetSiteCommand { get; private set; }
        public ICommand CloneSiteCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [ImportingConstructor]
        public SitesPlugin(IProfileService profileService) {
            if (Settings.Default.UpdateSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Settings.Default);
            }
            _sites = new ObserveAllCollection<SiteInfo>();
            this.profileService = profileService;

            if (Settings.Default.Sites == null) {
                Settings.Default.Sites = new SiteInfo[0];
                CoreUtil.SaveSettings(Settings.Default);
            } else if (Settings.Default.Sites.Length > 0) {
                var sites = Settings.Default.Sites;
                Sites.Clear();
                foreach (SiteInfo site in sites) {
                    Sites.Add(site);
                    site.PropertyChanged += SiteChanged;
                }
                UpdateActiveSite();
            }
            // This helper class can be used to store plugin settings that are dependent on the current profile
            this.pluginSettings = new PluginOptionsAccessor(profileService, Guid.Parse(this.Identifier));
            this.AddSiteCommand = new RelayCommand(AddSite);
            this.RemoveSiteCommand = new RelayCommand(RemoveSite);
            this.SetSiteCommand = new RelayCommand(SetSite);
            this.CloneSiteCommand = new RelayCommand(CloneSite);

            // React on a changed profile
            profileService.ProfileChanged += ProfileService_ProfileChanged;
            UpdateActiveSite();
        }

        private void AddSite(object arg) {
            var site = new SiteInfo("new site", 0.0, 0.0, 0.0);
            site.PropertyChanged += SiteChanged;
            Sites.Add(site);
            SelectedSite = site;
            //UpdateActiveSite();
            Settings.Default.Sites = Sites.ToArray();
            CoreUtil.SaveSettings(Settings.Default);
        }

        private void RemoveSite(object arg) {
            if (SelectedSite == null && Sites.Count > 0) {
                SelectedSite = Sites.Last();
            }
            SelectedSite.PropertyChanged -= SiteChanged;
            Sites.Remove(SelectedSite);
            if (Sites.Count > 0) {
                SelectedSite = Sites.Last();
            }
            Settings.Default.Sites = Sites.ToArray();
            CoreUtil.SaveSettings(Settings.Default);
        }

        private void SetSite(object arg) {
            if (SelectedSite == null && Sites.Count > 0) {
                SelectedSite = Sites.Last();
            }
            profileService.ChangeLatitude(SelectedSite.Latitude);
            profileService.ChangeLongitude(SelectedSite.Longitude);
            profileService.ChangeElevation(SelectedSite.Elevation);

            UpdateActiveSite();
        }

        private void CloneSite(object arg) {
            var astro = profileService.ActiveProfile.AstrometrySettings;
            var site = new SiteInfo("Current Location", astro.Latitude, astro.Longitude, astro.Elevation);
            site.PropertyChanged += SiteChanged;
            Sites.Add(site);
            SelectedSite = site;
            Settings.Default.Sites = Sites.ToArray();
            CoreUtil.SaveSettings(Settings.Default);
        }

        private void SiteChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == "IsCurrent") return;
            Settings.Default.Sites = Sites.ToArray();
            CoreUtil.SaveSettings(Settings.Default);
            UpdateActiveSite();
        }

        private void UpdateActiveSite() {
            var astro = profileService.ActiveProfile.AstrometrySettings;
            foreach (var site in Sites) {
                site.IsCurrent = (site.Latitude == astro.Latitude && site.Longitude == astro.Longitude && site.Elevation == astro.Elevation);
            }
        }

        private string SerializeSites() {
            var sites = Sites.ToArray();
            var ser = new XmlSerializer(sites.GetType());
            var writer = new StringWriter();
            ser.Serialize(writer, sites);
            Logger.Debug(writer.ToString());
            return writer.ToString();
        }

        public override Task Teardown() {
            // Make sure to unregister an event when the object is no longer in use. Otherwise garbage collection will be prevented.
            profileService.ProfileChanged -= ProfileService_ProfileChanged;
            return base.Teardown();
        }

        private void ProfileService_ProfileChanged(object sender, EventArgs e) {
            // Rase the event that this profile specific value has been changed due to the profile switch
            RaisePropertyChanged(nameof(ProfileSpecificNotificationMessage));
        }

        public string ProfileSpecificNotificationMessage {
            get {
                return pluginSettings.GetValueString(nameof(ProfileSpecificNotificationMessage), string.Empty);
            }
            set {
                pluginSettings.SetValueString(nameof(ProfileSpecificNotificationMessage), value);
                RaisePropertyChanged();
            }
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
