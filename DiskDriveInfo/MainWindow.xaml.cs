// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Collections.ObjectModel;
using TKUtils;

namespace DiskDriveInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool altRows;

        //! Main Window
        public MainWindow()
        {
            InitializeComponent();

            ReadSettings();

        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            GetDriveInfo();
        }

        //! Get information on all drives that are in 'ready' status
        #region Get drive information
        public void GetDriveInfo()
        {
            // Define the driveInfo list to hold the drive information
            var driveInfo = new ObservableCollection<DriveInformation>();

            // Populate the data grid with drive information obtained by GetDriveInfo()
            dataGrid1.ItemsSource = driveInfo;

            // Get drive information
            DriveInfo[] drives = DriveInfo.GetDrives();

            // Use Try/Catch since it's possible to get exceptions
            try
            {
                // Loop through all drives
                foreach (DriveInfo d in drives)
                {
                    // It's not possible to get info from drives that are not in ready status
                    if (d.IsReady)
                    {
                        // DriveInfo reports sizes as long integers.  Since dividing integers
                        // won't result in a decimal value, cast them as double to determine
                        // the percent used.  Formatted by StringFormat=P in the XAML.
                        double percentFree = (double)d.AvailableFreeSpace / (double)d.TotalSize;

                        double gbFree = d.AvailableFreeSpace / Math.Pow(1000, 3);

                        // Divide the size in bytes by 1000^3 to get GB.
                        // Formatted by StringFormat={}{0:N1} in XAML.
                        double totSize = d.TotalSize / Math.Pow(1000, 3);



                        // Add the information for each drive to the driveInfo list
                        driveInfo.Add(new DriveInformation(d.Name,
                                                           d.DriveType.ToString(),
                                                           d.DriveFormat,
                                                           d.VolumeLabel,
                                                           totSize,
                                                           gbFree,
                                                           percentFree));
                    }
                }
            }
            // Catch any exceptions. We don't really do anything but at least the app won't crash
            catch (Exception e)
            {
                Debug.WriteLine($"Error - {e}");
            }

        }
        #endregion

        //! Read configuration file
        #region Read config
        private void ReadSettings()
        {
            // Settings upgrade
            if (Properties.Settings.Default.SettingsUpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.SettingsUpgradeRequired = false;
                Properties.Settings.Default.Save();
                // CleanupPrevSettings must be called AFTER settings Upgrade and Save
                CleanUp.CleanupPrevSettings();
            }

            // Alternate row shading
            if (Properties.Settings.Default.ShadeAltRows)
            {
                cmColors.IsChecked = true;
                altRows = true;
            }
            else
            {
                cmColors.IsChecked = false;
                altRows = false;
            }


            WindowTitleVersion();
        }
        #endregion Read config

        //! Add version number to the window title
        #region Window Title
        public void WindowTitleVersion()
        {
            // Get the assembly version
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            // Remove the release (last) node
            string titleVer = version.ToString().Remove(version.ToString().LastIndexOf("."));

            // Set the windows title
            Title = "DriveInfo - " + titleVer;
        }
        #endregion

        //! Handle keypresses
        #region Keypresses
        /// <summary>
        /// Update the window when F5 is pressed
        /// Color alternating rows when F7 is pressed, revert with F8
        /// Press Escape to exit
        /// </summary>
        private void Window_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                GetDriveInfo();
                dataGrid1.Items.Refresh();
            }
            if (e.Key == Key.F7)
            {
                if (altRows)
                {
                    AltRowShadingOff();
                    altRows = false;
                    cmColors.IsChecked = false;
                }
                else
                {
                    AltRowShadingOn();
                    altRows = true;
                    cmColors.IsChecked = true;
                }
            }

            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
        }
        #endregion

        //! Alternate row shading
        #region Alternate row shading
        private void AltRowShadingOff()
        {
            dataGrid1.AlternationCount = 0;
            dataGrid1.RowBackground = new SolidColorBrush(Colors.White);
            dataGrid1.AlternatingRowBackground = new SolidColorBrush(Colors.White);
            dataGrid1.Items.Refresh();
            Properties.Settings.Default.ShadeAltRows = false;
            altRows = false;
        }

        private void AltRowShadingOn()
        {
            dataGrid1.AlternationCount = 1;
            dataGrid1.RowBackground = new SolidColorBrush(Colors.White);
            dataGrid1.AlternatingRowBackground = new SolidColorBrush(Colors.WhiteSmoke);
            dataGrid1.Items.Refresh();
            Properties.Settings.Default.ShadeAltRows = true;
            altRows = true;
        }
        #endregion

        //! Save the window position at shutdown so that it will start in the same location
        #region Windows Closing
        // Triggered by  Closing="Window_Closing"  in the MainWindow.xaml
        // WindowTop and WindowLeft are saved in user.config
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // save the property settings
            Properties.Settings.Default.Save();
        }
        #endregion

        //! Context menu
        #region Context menu
        private void CmRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetDriveInfo();
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
        #endregion

        //! Copy to clipboard
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

            // Exclude the header row
            dataGrid1.ClipboardCopyMode = DataGridClipboardCopyMode.ExcludeHeader;
        }


        #endregion

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
            TextFileViewer.ViewTextFile(Path.Combine(AppInfo.AppDirectory,"ReadMe.txt"));
        }
    }
}
