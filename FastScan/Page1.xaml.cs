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

using Windows.Devices.Enumeration;
using Windows.Devices.PointOfService;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FastScan
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page1 : Page
    {
        BarcodeScanner _BarcodeScanner=null;
        ClaimedBarcodeScanner _ClaimedBarcodeScanner=null;
        static bool _autoScan = false;

        public Page1()
        {
            this.InitializeComponent();
            Class1.doLog("Page1 : InitializeComponent 1");
        }
        #region NavigationHelper
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Class1.doLog("Page1 : OnNavigatedTo");
            if (e.Parameter is Boolean)
                _autoScan = (bool)e.Parameter;
            await resetScanner();
            textBox_Scan.Text = "";
            Class1.doLog("Page1 : OnNavigatedTo-StartScanner()");

            if (await StartScanner())
                Class1.doLog("StartScanner() OK");
            else
                Class1.doLog("StartScanner() failed");
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Invoked when this page is no longer displayed.
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Class1.doLog("Page1 : OnNavigatedFrom");

            await resetScanner();
            
            base.OnNavigatedFrom(e);
        }

        async System.Threading.Tasks.Task resetScanner()
        {
            if (_ClaimedBarcodeScanner != null)
            {
                if (_autoScan)
                {
                    await _ClaimedBarcodeScanner.StopSoftwareTriggerAsync();
                }
                _ClaimedBarcodeScanner.DataReceived -= _ClaimedBarcodeScanner_DataReceived;
                _ClaimedBarcodeScanner.ReleaseDeviceRequested -= _ClaimedBarcodeScanner_ReleaseDeviceRequested;
                _ClaimedBarcodeScanner.Dispose();
                _ClaimedBarcodeScanner = null;
            }
            _BarcodeScanner = null;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //UI code here
                textBox_Scan.Background = new SolidColorBrush(Windows.UI.Colors.White);
                textBox_Scan.Text = "";
                textBox_Time.Text = "";
                //play sound at end of Release
            });
            App.mySoundEffects.Play(AppSoundEffects.SoundEfxEnum.ComputerError);
        }
        #endregion
        /// <summary>
        /// Each time a barcode is read this routing gets the type of barcode read and the barcode data.
        /// It then calls the GUI update method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _ClaimedBarcodeScanner_DataReceived(ClaimedBarcodeScanner sender, BarcodeScannerDataReceivedEventArgs args)
        {
            Class1.doLog("_ClaimedBarcodeScanner_DataReceived: " + args.Report.ScanData.ToString());
            string label;
            UInt32 symCode = args.Report.ScanDataType; // the symbology of the scanned data            
            using (var datareader = Windows.Storage.Streams.DataReader.FromBuffer(args.Report.ScanDataLabel))
            {
                label = datareader.ReadString(args.Report.ScanDataLabel.Length);
            }
            UpdateUI_scandata(BarcodeSymbologies.GetName(symCode), label);
        }

        /// <summary>
        /// Makes sure the application retains the claim on the _BarcodeScanner if another application trys to claim it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private  void _ClaimedBarcodeScanner_ReleaseDeviceRequested(object sender, ClaimedBarcodeScanner e)
        {
           Class1.doLog("Event ReleaseDeviceRequested received.");
//           await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
//            {
                // always retain the device
                //e.RetainDevice();
//            });
        }
        /// <summary>
        /// This method updates any object on the user interface
        /// </summary>
        /// <param name="symCode"></param>
        /// <param name="data"></param>
        private async void UpdateUI_scandata(string symCode, string data)
        {
            // The following is essentailly what we would call a delegate in a WIN6.1 or 6.5 application
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                textBox_Scan.Text = symCode + ", " + data;
                textBox_Time.Text = System.DateTime.Now.ToString();
                App.mySoundEffects.Play(AppSoundEffects.SoundEfxEnum.SUCCESS);
                //if (Audio_Player.Position == new System.TimeSpan(0))
                //{
                //    Audio_Player.Play();
                //}
                //else
                //{
                //    Audio_Player.Pause();
                //    Audio_Player.Position = new System.TimeSpan(0);
                //    Audio_Player.Play();
                //}
            });
            System.Diagnostics.Debug.WriteLine(data);
        }
        /// <summary>
        /// Instantiates the _BarcodeScanner and sets up the active symbologies
        /// </summary>
        private async System.Threading.Tasks.Task<bool> StartScanner()
        {
            try
            {
                // Waits for the default _BarcodeScanner opjet to be created
                if (await CreateDefaultScannerObject())
                {
                    Class1.doLog("CreateDefaultScannerObject()...");
                    // Waits for the defualt _BarcodeScanner object to be claimed by the application
                    if (await ClaimScanner())
                    {
                        Class1.doLog("ClaimScanner()...");

                        // enable the _BarcodeScanner.
                        // Note: If the _BarcodeScanner is not enabled (i.e. EnableAsync not called), attaching the event handler will not be any useful because the API will not fire the event 
                        // if the _ClaimedBarcodeScanner has not beed Enabled
                        await _ClaimedBarcodeScanner.EnableAsync();
                        Class1.doLog("_ClaimedBarcodeScanner.EnableAsync() done");

                        // It is always a good idea to have a release device requested event handler. If this event is not handled, there are chances of another app can 
                        // claim ownsership of the barcode _BarcodeScanner.
                        _ClaimedBarcodeScanner.ReleaseDeviceRequested += _ClaimedBarcodeScanner_ReleaseDeviceRequested;

                        // after successfully claiming, attach the datareceived event handler.
                        _ClaimedBarcodeScanner.DataReceived += _ClaimedBarcodeScanner_DataReceived;

                        // Ask the API to decode the data by default. By setting this, API will decode the raw data from the barcode _BarcodeScanner and 
                        // send the ScanDataLabel and ScanDataType in the DataReceived event
                        _ClaimedBarcodeScanner.IsDecodeDataEnabled = true;

                        //// enable the _BarcodeScanner.
                        //// Note: If the _BarcodeScanner is not enabled (i.e. EnableAsync not called), attaching the event handler will not be any useful because the API will not fire the event 
                        //// if the _ClaimedBarcodeScanner has not beed Enabled
                        //await _ClaimedBarcodeScanner.EnableAsync();
                        //Class1.doLog("_ClaimedBarcodeScanner.EnableAsync() done");

                        // Set all active symbologies to false to start with a clean slate.
                        await _ClaimedBarcodeScanner.SetActiveSymbologiesAsync(new List<uint> { 0 });
                        List<uint> Symbologies = new List<uint>();
                        Symbologies.Add(BarcodeSymbologies.Upca);
                        Symbologies.Add(BarcodeSymbologies.UpcaAdd2);
                        Symbologies.Add(BarcodeSymbologies.UpcaAdd5);
                        Symbologies.Add(BarcodeSymbologies.Ean13);
                        Symbologies.Add(BarcodeSymbologies.Ean13Add2);
                        Symbologies.Add(BarcodeSymbologies.Ean13Add5);
                        Symbologies.Add(BarcodeSymbologies.Code128);
                        Symbologies.Add(BarcodeSymbologies.Gs1128);
                        Symbologies.Add(BarcodeSymbologies.DataMatrix);
                        Symbologies.Add(BarcodeSymbologies.Code39);
                        Symbologies.Add(BarcodeSymbologies.Code39Ex);
                        Symbologies.Add(BarcodeSymbologies.Pdf417);

                        await _ClaimedBarcodeScanner.SetActiveSymbologiesAsync(Symbologies);
                        Class1.doLog("_ClaimedBarcodeScanner.SetActiveSymbologiesAsync(Symbologies)");
                        Class1.doLog(" Ready to scan.");
                        if (_autoScan)
                            await _ClaimedBarcodeScanner.StartSoftwareTriggerAsync();
                    }
                }
            }catch(Exception ex)
            {
                await showMessage("StartScanner " + ex.Message);
            }
            if (_ClaimedBarcodeScanner != null)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    //UI code here
                    textBox_Scan.Background = new SolidColorBrush(Windows.UI.Colors.LightGreen);
                });
                return true;
            }
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    //UI code here
                    textBox_Scan.Background = new SolidColorBrush(Windows.UI.Colors.LightPink);
                });
                return false;
            }
        }
        /// <summary>
        /// Creates the default bar code _BarcodeScanner
        /// </summary>
        /// <returns>true if bar code _BarcodeScanner is returned</returns>
        private async System.Threading.Tasks.Task<bool> CreateDefaultScannerObject()
        {
            try
            {
                if (_BarcodeScanner == null)
                {
                    _BarcodeScanner = await BarcodeScanner.FromIdAsync(@"\\?\ACPI#SCN00001#0#{c243ffbd-3afc-45e9-b3d3-2ba18bc7ebc5}\POSBarcodeScanner");
                    //DeviceInformationCollection deviceCollection = await DeviceInformation.FindAllAsync(BarcodeScanner.GetDeviceSelector());
                    //if (deviceCollection != null && deviceCollection.Count > 0)
                    //{
                    //    foreach(DeviceInformation di in deviceCollection)
                    //    {
                    //        Class1.doLog("found: " + di.Name +", id="+di.Id);
                    //        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    //            //UI code here
                    //            textBox_Scan.Text = di.Id;
                    //        });
                    //        if (di.Id.Contains("POSBarcodeScanner"))
                    //        {
                    //            _BarcodeScanner = await BarcodeScanner.FromIdAsync(di.Id);
                    //            break;
                    //        }
                    //   }
                    //}
                    //else
                    //{
                    //    Class1.doLog("no POSBarcodeScanner found.");
                    //}
                }
                else
                    await System.Threading.Tasks.Task.Delay(0);
            }catch(Exception ex)
            {
                await showMessage("CreateDefaultScannerObject " + ex.Message);
            }
            if (_BarcodeScanner == null)
            {
                Class1.doLog("no POSBarcodeScanner found.");
                return false;
            }
            else
            {
                Class1.doLog("POSBarcodeScanner found.");
                return true;
            }
        }

        async System.Threading.Tasks.Task showMessage(string msg)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(msg);
            await dialog.ShowAsync();            
        }
        /// <summary>
        /// This method claims the barcode _BarcodeScanner 
        /// </summary>
        /// <returns>true if claim is successful. Otherwise returns false</returns>
        private async System.Threading.Tasks.Task<bool> ClaimScanner()
        {
            if (_ClaimedBarcodeScanner == null)
            {
                // claim the barcode _BarcodeScanner
                _ClaimedBarcodeScanner = await _BarcodeScanner.ClaimScannerAsync();

                // enable the claimed barcode _BarcodeScanner
                if (_ClaimedBarcodeScanner == null)
                {
                    Class1.doLog("Claim barcode _BarcodeScanner failed.");
                    return false;
                }
            }
            return true;
        }

        private void button_PagechangeToMain_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private async void button_Scan_Click(object sender, RoutedEventArgs e)
        {
            if (_ClaimedBarcodeScanner != null)
                await _ClaimedBarcodeScanner.StartSoftwareTriggerAsync();
        }

        private void button_Pagechange_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Page2), MainPage.bEnableAutoScan);
        }
    }
}
