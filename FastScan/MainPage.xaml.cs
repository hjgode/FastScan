using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FastScan
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region global vars
        string[] WedgeDisable = new string[]
            {"<?xml version=\"1.0\"?>",
                "<ConfigDoc flags=\"720\" name=\"Data Collection Profiles\">",
                "  <Section flags=\"000\" name=\"HONWedge\" id=\"WedgeConfig\">",
                "     <Key cmd=\"DEVICE\" list=\"Internal,USB\" name=\"Device Type\">Internal</Key>",
                "     <Key cmd=\"TYPE\" list=\"Incremental,Full\" name=\"ProfileType\">Incremental</Key>",
                "     <Key cmd=\"ENABLE_WEDGE\" list=\"true,false\" name=\"WedgeDisable\">false</Key>",
                "  </Section>",
                "</ConfigDoc>"
            };
        string[] WedgeEnable = new string[]
            {"<?xml version=\"1.0\"?>",
                "<ConfigDoc flags=\"720\" name=\"Data Collection Profiles\">",
                "  <Section flags=\"000\" name=\"HONWedge\" id=\"WedgeConfig\">",
                "     <Key cmd=\"DEVICE\" list=\"Internal,USB\" name=\"Device Type\">Internal</Key>",
                "     <Key cmd=\"TYPE\" list=\"Incremental,Full\" name=\"ProfileType\">Incremental</Key>",
                "     <Key cmd=\"ENABLE_WEDGE\" list=\"true,false\" name=\"WedgeDisable\">true</Key>",
                "  </Section>",
                "</ConfigDoc>"
            };
        #endregion
        public static bool bEnableAutoScan = false;
        static bool bEnableWedge = false;

        public MainPage()
        {
            this.InitializeComponent();
            enableWedge(bEnableWedge);
        }
        protected  override void OnNavigatedTo(NavigationEventArgs e)
        {
            checkBox_EnableAutoScan.IsChecked = bEnableAutoScan;
            checkBox_enableWedge.IsChecked = bEnableWedge;
        }
        protected  override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bEnableAutoScan = (checkBox_EnableAutoScan.IsChecked == true);
            bEnableWedge = (checkBox_enableWedge.IsChecked == true);
        }
        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox_enableWedge.IsChecked == true)
            {
                //enable wedge
                bEnableAutoScan = true;
            }
            else
            {
                //disable wedge
                bEnableAutoScan = false;
            }
            enableWedge(bEnableAutoScan);
        }
        async void enableWedge(bool bEnable)
        {
            try
            {
                //add <uap:Capability Name="documentsLibrary" /> to manifest
                //Note: You must add File Type Associations to your app manifest that declare specific file types that your app can access in this location.

                // Open/create the subfolder where the profile is stored.
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.KnownFolders.DocumentsLibrary;
                Windows.Storage.StorageFolder profilesFolder = await storageFolder.CreateFolderAsync("Profile", Windows.Storage.CreationCollisionOption.OpenIfExists);

                // Open/create the profile file.
                Windows.Storage.StorageFile profileFile = await profilesFolder.CreateFileAsync("HoneywellDecoderSettingsV2.Exm", Windows.Storage.CreationCollisionOption.ReplaceExisting);

                if (bEnable)
                {
                    // Write the profile to the file.
                    await Windows.Storage.FileIO.WriteLinesAsync(profileFile, WedgeEnable);
                }
                else
                {
                    await Windows.Storage.FileIO.WriteLinesAsync(profileFile, WedgeDisable);
                }
            }catch(Exception ex)
            {
                Class1.doLog("Exception in enableWedge: " + ex.Message);
            }
            // Delay to allow time for new profile(s) to be processed.
            await System.Threading.Tasks.Task.Delay(50);

        }

        private void button_View1_Click(object sender, RoutedEventArgs e)
        {
            bool bEnableAutoScan = false;
            if (this.checkBox_EnableAutoScan.IsChecked == true)
                bEnableAutoScan = true;

            this.Frame.Navigate(typeof(Page1), bEnableAutoScan);
        }

        private void button_View2_Click(object sender, RoutedEventArgs e)
        {
            bool bEnableAutoScan = false;
            if (this.checkBox_EnableAutoScan.IsChecked == true)
                bEnableAutoScan = true;

            this.Frame.Navigate(typeof(Page2), bEnableAutoScan);

        }
    }
}
