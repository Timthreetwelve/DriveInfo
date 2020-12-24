// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using Directives
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        public MainWindow()
        {
            UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);

            InitializeComponent();

            ReadSettings();
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            GetInfoFromDrive();
        }

        #region Get drive information
        public void GetInfoFromDrive()
        {
            dataGrid1.ItemsSource = DriveInformation.DriveInfoList;
            DriveInformation.DriveInfoList.Clear();
            Stopwatch procWatch = Stopwatch.StartNew();
            foreach (DriveInfo drive in GetLogicalDrives())
            {
                sb1.Content = $"Processing {drive}";
                try
                {
                    if (drive != null)
                    {
                        GetDriveDetails(drive);
                    }
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Error processing drive {drive}\n{ex.Message}",
                        "DriveInfo Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            procWatch.Stop();
            if (dataGrid1.Items.Count != 1)
            {
                sb1.Content = $"Displayed {dataGrid1.Items.Count} drives in {procWatch.Elapsed.TotalMilliseconds:N2} ms";
            }
            else
            {
                sb1.Content = $"Displayed {dataGrid1.Items.Count} drive in {procWatch.Elapsed.TotalMilliseconds:N2} ms";
            }
        }
        #endregion Get drive information

        #region Get details for an individual drive
        private void GetDriveDetails(DriveInfo d)
        {
            // GBPref can be 1000 or 1024 based on user preference
            int GBPref;
            if (UserSettings.Setting.Use1024)
            {
                GBPref = 1024;
                colFree.Header = "Free (GiB)";
                colSize.Header = "Size (GiB)";
            }
            else
            {
                GBPref = 1000;
                colFree.Header = "Free (GB)";
                colSize.Header = "Size (GB)";
            }

            Stopwatch watch = Stopwatch.StartNew();
            if (d.IsReady)
            {
                // DriveInfo reports sizes as long integers.  Since dividing integers
                // won't result in a decimal value, cast them as double to determine
                // the percent used.  Formatted by StringFormat=P in the XAML.
                double percentFree = (double)d.AvailableFreeSpace / d.TotalSize;

                // Formatted in XAML.
                double totSize = d.TotalSize / Math.Pow(GBPref, 3);

                // Formatted in XAML.
                double gbFree = d.AvailableFreeSpace / Math.Pow(GBPref, 3);

                // Add the information for each drive to the driveInfo list
                DriveInformation di = new DriveInformation
                {
                    Name = d.Name,
                    DriveType = d.DriveType.ToString(),
                    Format = d.DriveFormat,
                    Label = d.VolumeLabel,
                    TotalSize = totSize,
                    GBFree = gbFree,
                    PercentFree = percentFree
                };
                DriveInformation.DriveInfoList.Add(di);
            }
            // If drive is not ready
            else if (UserSettings.Setting.IncludeNotReady)
            {
                DriveInformation di = new DriveInformation
                {
                    Name = d.Name,
                    DriveType = d.DriveType.ToString(),
                    Label = "Not Ready"
                };
                DriveInformation.DriveInfoList.Add(di);
            }
            watch.Stop();
            Debug.WriteLine($"Processed drive {d} in {watch.Elapsed.TotalMilliseconds} ms.");
        }
        #endregion

        #region Get array of logical drives
        private DriveInfo[] GetLogicalDrives()
        {
            try
            {
                return DriveInfo.GetDrives();
            }
            catch (IOException ex)
            {
                _ = MessageBox.Show($"I/O error\n{ex.Message}",
                                    "DriveInfo Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                Environment.Exit(1);
            }
            catch (UnauthorizedAccessException ex)
            {
                _ = MessageBox.Show($"Security error\n{ex.Message}",
                                    "DriveInfo Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                Environment.Exit(2);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Unknown error\n{ex.Message}",
                                    "DriveInfo Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                Environment.Exit(3);
            }
            return null;
        }
        #endregion Get array of logical drives

        #region Read settings
        private void ReadSettings()
        {
            Debug.WriteLine($"{AppInfo.AppName} {AppInfo.TitleVersion} is starting up");
            Debug.WriteLine($"Settings file is {UserSettings.GetSettingsFilename()}");

            // Settings change event
            UserSettings.Setting.PropertyChanged += UserSettingChanged;

            // Use either GB or GiB for space measurements
            if (UserSettings.Setting.Use1024)
            {
                colFree.Header = "Free (GiB)";
                colSize.Header = "Size (GiB)";
                mnuGiB.IsChecked = true;
            }
            else
            {
                colFree.Header = "Free (GB)";
                colSize.Header = "Size (GB)";
                mnuGB.IsChecked = true;
            }

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
        #endregion Keyboard Events

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
        #endregion Mouse Events

        #region Window Events
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Save settings
            _ = UserSettings.ListSettings();
            UserSettings.SaveSettings();
        }
        #endregion Window Events

        #region Menu Events

        // File menu
        private void CmCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            CopytoClipBoard();
        }
        private void CmExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //View menu
        private void CmRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetInfoFromDrive();
            dataGrid1.Items.Refresh();
        }
        private void MnuGB_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.Use1024 = false;
            mnuGiB.IsChecked = false;
        }
        private void MnuGiB_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.Use1024 = true;
            mnuGB.IsChecked = false;
        }
        private void CmColors_Checked(object sender, RoutedEventArgs e)
        {
            AltRowShadingOn();
        }
        private void CmColors_Unchecked(object sender, RoutedEventArgs e)
        {
            AltRowShadingOff();
        }
        private void GridSmaller_Click(object sender, RoutedEventArgs e)
        {
            GridSmaller();
        }
        private void GridLarger_Click(object sender, RoutedEventArgs e)
        {
            GridLarger();
        }

        // Help Menu
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

                case "Use1024":
                    GetInfoFromDrive();
                    dataGrid1.Items.Refresh();
                    break;

                case "IncludeNotReady":
                    cmRefresh.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    break;
            }
            Debug.WriteLine($"***Setting change: {e.PropertyName} New Value: {newValue}");
        }
        #endregion Setting change

        #region Window Title
        public void WindowTitleVersion()
        {
            Title = $"{AppInfo.AppName} - {AppInfo.TitleVersion}";
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

        #region Grid Size
        private void GridSmaller()
        {
            double curZoom = UserSettings.Setting.GridZoom;
            if (curZoom > 0.9)
            {
                curZoom -= .05;
                UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
            }
            dataGrid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }
        private void GridLarger()
        {
            double curZoom = UserSettings.Setting.GridZoom;
            if (curZoom < 1.3)
            {
                curZoom += .05;
                UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
            }
            dataGrid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }
        #endregion Grid Size
    }
}
