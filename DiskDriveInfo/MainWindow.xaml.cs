// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using Directives
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TKUtils;
#endregion

namespace DiskDriveInfo
{
    public partial class MainWindow : Window
    {
        string[] drives;
        DriveInfo info;

        public MainWindow()
        {
            UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);

            InitializeComponent();

            ReadSettings();
        }

        #region Get drive information
        public async Task GetDriveInf()
        {
            var driveInfo = new ObservableCollection<DriveInformation>();

            // Get the drives from the environment
            try
            {
                drives = Environment.GetLogicalDrives();
            }
            catch (IOException ex)
            {
                _ = MessageBox.Show($"I/O error\ne{ex.Message}",
                                    "DriveInfo Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
            }
            catch (SecurityException ex)
            {
                _ = MessageBox.Show($"Security error\ne{ex.Message}",
                                    "DriveInfo Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
            }

            foreach (string d in drives)
            {
                string msg = $"Processing {d}";
                Debug.WriteLine(msg);
                sb1.Content = msg;

                if (d != null)
                {
                    try
                    {
                        info = new DriveInfo(d);
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show($"Error getting drive information for {d}\ne{ex.Message}",
                                            "DriveInfo Error",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error);
                    }
                    dataGrid1.ItemsSource = driveInfo;

                    await Task.Run(() =>
                    {
                        if (info.IsReady)
                        {
                            // DriveInfo reports sizes as long integers.  Since dividing integers
                            // won't result in a decimal value, cast them as double to determine
                            // the percent used.  Formatted by StringFormat=P in the XAML.
                            double percentFree = (double)info.AvailableFreeSpace / info.TotalSize;

                            // Divide the size in bytes by 1000^3 to get GB.
                            // Formatted by StringFormat={}{0:N1} in XAML.
                            double totSize = info.TotalSize / Math.Pow(1000, 3);

                            double gbFree = info.AvailableFreeSpace / Math.Pow(1000, 3);

                            // modify driveInfo on UI thread
                            App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                // Add the information for each drive to the driveInfo list
                                driveInfo.Add(new DriveInformation(info.Name,
                                                                       info.DriveType.ToString(),
                                                                       info.DriveFormat,
                                                                       info.VolumeLabel,
                                                                       totSize,
                                                                       gbFree,
                                                                       percentFree));
                            });
                        }
                        else
                        {
                            if (UserSettings.Setting.IncludeNotReady)
                            {
                                App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                driveInfo.Add(new DriveInformation(info.Name, "Not Ready", "", "", null, null, null));
                            });
                            }
                        }
                    });
                }
                sb1.Content = "Processing complete";
            }
        }
        #endregion

        #region Read settings
        private void ReadSettings()
        {
            Debug.WriteLine($"Settings file is {UserSettings.GetSettingsFilename()}");

            // Settings change event
            UserSettings.Setting.PropertyChanged += UserSettingChanged;

            // Alternate row shading
            if (UserSettings.Setting.ShadeAltRows)
            {
                AltRowShadingOn();
            }

            // Set data grid zoom level
            double curZoom = UserSettings.Setting.GridZoom;
            dataGrid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);

            // Put version in window title
            WindowTitleVersion();
        }
        #endregion Read settings

        #region Keyboard Events
        private void Window_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                mnuAbout.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            }

            if (e.Key == Key.F5)
            {
                cmRefresh.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            }

            if (e.Key == Key.F7)
            {
                UserSettings.Setting.ShadeAltRows = !UserSettings.Setting.ShadeAltRows;
            }

            if (e.Key == Key.C && (e.KeyboardDevice.Modifiers == ModifierKeys.Control))
            {
                CopytoClipBoard();
            }
        }
        #endregion

        #region Mouse Events
        private void DataGrid1_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Delta > 0)
            {
                GridLarger();
            }
            else if (e.Delta < 0)
            {
                GridSmaller();
            }
        }
        #endregion

        #region Window Events
        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            await GetDriveInf();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Save settings
            _ = UserSettings.ListSettings();
            UserSettings.SaveSettings();
        }
        #endregion Window Events

        #region Menu Events
        private async void CmRefresh_Click(object sender, RoutedEventArgs e)
        {
            await GetDriveInf();
            dataGrid1.Items.Refresh();
        }

        private void CmExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CmColors_Checked(object sender, RoutedEventArgs e)
        {
            AltRowShadingOn();
        }

        private void CmColors_Unchecked(object sender, RoutedEventArgs e)
        {
            AltRowShadingOff();
        }

        private void CmCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            CopytoClipBoard();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About about = new About
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            _ = about.ShowDialog();
        }

        private void ReadMe_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(Path.Combine(AppInfo.AppDirectory, "ReadMe.txt"));
        }
        #endregion Menu Events

        #region Helper Methods

        #region Window Title
        public void WindowTitleVersion()
        {
            // Get the assembly version
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            // Remove the release (last) node
            string titleVer = version.ToString().Remove(version.ToString().LastIndexOf("."));

            // Set the windows title
            Title = $"DriveInfo - {titleVer}";
        }
        #endregion Window Title

        #region Alternate row shading
        private void AltRowShadingOff()
        {
            dataGrid1.AlternationCount = 0;
            dataGrid1.RowBackground = new SolidColorBrush(Colors.White);
            dataGrid1.AlternatingRowBackground = new SolidColorBrush(Colors.White);
            dataGrid1.Items.Refresh();
        }

        private void AltRowShadingOn()
        {
            dataGrid1.AlternationCount = 2;
            dataGrid1.RowBackground = new SolidColorBrush(Colors.White);
            dataGrid1.AlternatingRowBackground = new SolidColorBrush(Colors.WhiteSmoke);
            dataGrid1.Items.Refresh();
        }
        #endregion Alternate row shading

        #region Copy to clipboard
        private void CopytoClipBoard()
        {
            // Clear the clipboard
            Clipboard.Clear();

            // Include the header row
            dataGrid1.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            // Select all the cells
            dataGrid1.SelectAllCells();

            // Execute the copy
            ApplicationCommands.Copy.Execute(null, dataGrid1);

            // Unselect the cells
            dataGrid1.UnselectAllCells();
        }
        #endregion Copy to clipboard

        #region Setting change
        private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
            var newValue = prop?.GetValue(sender, null);
            switch (e.PropertyName)
            {
                case "ShadeAltRows":
                    if ((bool)newValue)
                    {
                        AltRowShadingOn();
                    }
                    else
                    {
                        AltRowShadingOff();
                    }
                    break;

                case "KeepOnTop":
                    Topmost = (bool)newValue;
                    break;

                case "IncludeNotReady":
                    cmRefresh.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    break;
            }
            Debug.WriteLine($"***Setting change: {e.PropertyName} New Value: {newValue}");
        }
        #endregion Setting change

        #region Grid Size
        private void GridSmaller()
        {
            //double curZoom = Properties.Settings.Default.GridZoom;
            double curZoom = UserSettings.Setting.GridZoom;
            if (curZoom > 0.9)
            {
                curZoom -= .05;
                //Properties.Settings.Default.GridZoom = curZoom;
                UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
            }

            dataGrid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }

        private void GridLarger()
        {
            //double curZoom = Properties.Settings.Default.GridZoom;
            double curZoom = UserSettings.Setting.GridZoom;
            if (curZoom < 1.3)
            {
                curZoom += .05;
                //Properties.Settings.Default.GridZoom = curZoom;
                UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
            }

            dataGrid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }

        private void GridSmaller_Click(object sender, RoutedEventArgs e)
        {
            GridSmaller();
        }

        private void GridLarger_Click(object sender, RoutedEventArgs e)
        {
            GridLarger();
        }
        #endregion Grid Size

        #endregion Helper Methods
    }
}
