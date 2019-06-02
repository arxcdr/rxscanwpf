using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WIA;
using System.Diagnostics;

namespace RXscanWPF
{
    class WIAScanner
    {
        const string wiaFormatBMP = "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}";

        // Path where scanned images will be stored for conversion
        static string scannedDocumentsPath;

        // Color mode
        static Utils.ColorMode clrMode;

        // Set color mode
        public WIAScanner(Utils.ColorMode clr)
        {
            clrMode = clr;
        }

        class WIA_DPS_DOCUMENT_HANDLING_SELECT
        {
            public const uint FEEDER = 0x00000001;
            public const uint FLATBED = 0x00000002;
        }
        class WIA_DPS_DOCUMENT_HANDLING_STATUS
        {
            public const uint FEED_READY = 0x00000001;
        }
        class WIA_PROPERTIES
        {
            public const uint WIA_RESERVED_FOR_NEW_PROPS = 1024;
            public const uint WIA_DIP_FIRST = 2;
            public const uint WIA_DPA_FIRST = WIA_DIP_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            public const uint WIA_DPC_FIRST = WIA_DPA_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            //
            // Scanner only device properties (DPS)
            //
            public const uint WIA_DPS_FIRST = WIA_DPC_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            public const uint WIA_DPS_DOCUMENT_HANDLING_STATUS = WIA_DPS_FIRST + 13;
            public const uint WIA_DPS_DOCUMENT_HANDLING_SELECT = WIA_DPS_FIRST + 14;
        }
        /// <summary>
        /// Use scanner to scan an image (with user selecting the scanner from a dialog).
        /// </summary>
        /// <returns>Scanned images.</returns>
        public static List<Image> Scan()
        {
            if(CreateTempDirectory())
            {
                WIA.ICommonDialog dialog = new WIA.CommonDialog();
                WIA.Device device = dialog.ShowSelectDevice
                    (WIA.WiaDeviceType.UnspecifiedDeviceType, true, false);
                if (device != null)
                {
                    return Scan(device.DeviceID);
                }
                else
                {
                    throw new Exception("You must select a device for scanning.");
                }
            }
            else
            {
                return null;
            }          
        }
        /// <summary>
        /// Use scanner to scan an image (scanner is selected by its unique id).
        /// </summary>
        /// <param name="scannerName"></param>
        /// <returns>Scanned images.</returns>
        public static List<Image> Scan(string scannerId)
        {
            List<Image> images = new List<Image>();
            bool hasMorePages = true;
            while (hasMorePages)
            {
                // select the correct scanner using the provided scannerId parameter
                WIA.DeviceManager manager = new WIA.DeviceManager();
                WIA.Device device = null;
                foreach (WIA.DeviceInfo info in manager.DeviceInfos)
                {
                    if (info.DeviceID == scannerId)
                    {
                        // connect to scanner
                        device = info.Connect();
                        break;
                    }
                }
                // device was not found
                if (device == null)
                {
                    // enumerate available devices
                    string availableDevices = "";
                    foreach (WIA.DeviceInfo info in manager.DeviceInfos)
                    {
                        availableDevices += info.DeviceID + "\n";
                    }
                    // show error with available devices
                    throw new Exception("The device with provided ID could not be found." +
                        "Available Devices:\n" + availableDevices);
                }

                // Set color intent to greyscale if user requested it
                if (clrMode == Utils.ColorMode.Greyscale)
                {
                    Property prop = device.Items[1].Properties.get_Item("6146");
                    prop.set_Value(2); //4 is Black-white,gray is 2, color 1 (Color Intent)
                }

                WIA.Item item = device.Items[1] as WIA.Item;

                try
                {
                    // scan image
                    WIA.ICommonDialog wiaCommonDialog = new WIA.CommonDialog();
                    WIA.ImageFile image = (WIA.ImageFile)wiaCommonDialog.ShowTransfer(item, wiaFormatBMP, false);
                    // save to temp file
                    string fileName = Utils.GetNextFileName(scannedDocumentsPath + "\\NumérisationSansTitre.bmp");
                    //File.Delete(fileName);
                    image.SaveFile(fileName);
                    image = null;
                    // add file to output list
                    images.Add(Image.FromFile(fileName));
                }
                catch (Exception exc)
                {
                    throw exc;
                }
                finally
                {
                    item = null;
                    //determine if there are any more pages waiting
                    WIA.Property documentHandlingSelect = null;
                    WIA.Property documentHandlingStatus = null;
                    foreach (WIA.Property prop in device.Properties)
                    {
                        if (prop.PropertyID == WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_SELECT)
                            documentHandlingSelect = prop;
                        if (prop.PropertyID == WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_STATUS)
                            documentHandlingStatus = prop;
                    }
                    // assume there are no more pages
                    hasMorePages = false;
                    // may not exist on flatbed scanner but required for feeder
                    if (documentHandlingSelect != null)
                    {
                        // check for document feeder
                        if ((Convert.ToUInt32(documentHandlingSelect.get_Value()) &
                        WIA_DPS_DOCUMENT_HANDLING_SELECT.FEEDER) != 0)
                        {
                            hasMorePages = ((Convert.ToUInt32(documentHandlingStatus.get_Value()) &
                            WIA_DPS_DOCUMENT_HANDLING_STATUS.FEED_READY) != 0);
                        }
                    }
                }
            }
            return images;
        }
        /// <summary>
        /// Gets the list of available WIA devices.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDevices()
        {
            List<string> devices = new List<string>();
            WIA.DeviceManager manager = new WIA.DeviceManager();
            foreach (WIA.DeviceInfo info in manager.DeviceInfos)
            {
                devices.Add(info.DeviceID);
            }
            return devices;
        }

        /// <summary>
        /// Creates a temporary directory in MyDocuments to store scanned docs until conversion.
        /// </summary>
        private static bool CreateTempDirectory()
        {
            try
            {
                scannedDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\RXscan";
                Directory.CreateDirectory(scannedDocumentsPath);
                return true;
            }
            catch (Exception e)
            {
                scannedDocumentsPath = null;
                MessageBox.Show("Error: " + e.Message);
                return false;
            }
        }
    }
}
