using System.ComponentModel;
using System.Runtime.CompilerServices;
using TKUtils;

namespace DiskDriveInfo
{
    public class UserSettings : SettingsManager<UserSettings>, INotifyPropertyChanged
    {
        #region Constructor
        public UserSettings()
        {
            // Set defaults
            GridZoom = 1;
            IncludeNotReady = false;
            KeepOnTop = false;
            ShadeAltRows = true;
            Use1024 = true;
            WindowLeft = 100;
            WindowTop = 100;
        }
        #endregion Constructor

        #region Properties
        public bool IncludeNotReady
        {
            get => includeNotReady;
            set
            {
                includeNotReady = value;
                OnPropertyChanged();
            }
        }

        public bool ShadeAltRows
        {
            get => shadeAltRows;
            set
            {
                shadeAltRows = value;
                OnPropertyChanged();
            }
        }

        public bool KeepOnTop
        {
            get => keepOnTop;
            set
            {
                keepOnTop = value;
                OnPropertyChanged();
            }
        }

        public bool Use1024
        {
            get { return use1024; }
            set
            {
                use1024 = value;
                OnPropertyChanged();
            }
        }

        public double GridZoom
        {
            get
            {
                if (gridZoom <= 0)
                {
                    gridZoom = 1;
                }
                return gridZoom;
            }
            set
            {
                gridZoom = value;
                OnPropertyChanged();
            }
        }

        public double WindowLeft
        {
            get
            {
                if (windowLeft < 0)
                {
                    windowLeft = 100;
                }
                return windowLeft;
            }
            set => windowLeft = value;
        }

        public double WindowTop
        {
            get
            {
                if (windowTop < 0)
                {
                    windowTop = 100;
                }
                return windowTop;
            }
            set => windowTop = value;
        }
        #endregion Properties

        #region Private backing fields
        private bool includeNotReady;
        private bool shadeAltRows;
        private bool keepOnTop;
        private bool use1024;
        private double gridZoom;
        private double windowLeft;
        private double windowTop;
        #endregion Private backing fields

        #region Handle property change event
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Handle property change event
    }
}
