using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using WIA;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace RXscanWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Default local folder path (MyDocuments) to save scanned files
        String localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Temporary File Path
        String tempFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\RXscan";

        // Default folder path to display in the UI
        public String dPath
        {
            get { return localFolderPath; }
            set
            {
                localFolderPath = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("dPath");
            }
        }

        // UI language
        public String UIloc { get; set; }

        // UI Button Text Properties
        public String UI_bwsb { get; set; }
        public String UI_clsb { get; set; }
        public String UI_shfb { get; set; }
        public String UI_chfb { get; set; }

        // List of temporary images to convert
        List<System.Drawing.Image> scannedImages;

        SaveToJPEG jpegConverter;
        WIAScanner wiaScanner;
        
        public static MainWindow Current;

        public MainWindow()
        {
            InitializeComponent();

            // Set UI
            DataContext = this;
            SetFrenchUI();            
        }

        // FILE SYSTEM CODE

        // Initiate document scanning in greyscale
        private void BWScanButton_Click(object sender, RoutedEventArgs e)
        {
            DisableUI();

            // Create WIAScanner object with save path
            wiaScanner = new WIAScanner(Utils.ColorMode.Greyscale);

            scannedImages = WIAScanner.Scan();
            //System.Threading.Thread.Sleep(1000);

            if (scannedImages != null)
            {
                foreach (Bitmap img in scannedImages)
                {
                    Debug.WriteLine("Image: " + img.ToString() + img.GetType());
                    jpegConverter = new SaveToJPEG(img, dPath);
                    jpegConverter.Convert();
                }
            }

            // Clean up temp files & enable UI
            DeleteTempFiles();
            EnableUI();
        }

        // Initiate document scanning in color
        private void CLRScanButton_Click(object sender, RoutedEventArgs e)
        {
            DisableUI();

            // Create WIAScanner object with save path
            wiaScanner = new WIAScanner(Utils.ColorMode.Color);
        
            scannedImages = WIAScanner.Scan();
            //System.Threading.Thread.Sleep(1000);

            if (scannedImages != null)
            {
                foreach (Bitmap img in scannedImages)
                {
                    Debug.WriteLine("Image: " + img.ToString() + img.GetType());
                    jpegConverter = new SaveToJPEG(img, dPath);
                    jpegConverter.Convert();
                }
            }

            // Clean up temp files & enable UI
            DeleteTempFiles();
            EnableUI();
        }

        private void ShowFilesButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Opening local folder...");

            // Open local folder
            Process.Start(@dPath);
        }

        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Browsing folders...");

            // Open dialog to let user change local folder
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog()
            {
                EnsurePathExists = true,
                EnsureFileExists = false,
                AllowNonFileSystemItems = false,               
            };
            folderDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            folderDialog.IsFolderPicker = true;
            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                dPath = folderDialog.FileName;                
                Debug.WriteLine("New local folder path: " + dPath);
            }
        }

        // UI CODE

        // Switch to English UI if not already selected
        private void LocaleENButton_Click(object sender, RoutedEventArgs e)
        {
            if (UIloc != "English")
            {
                SetEnglishUI();
            }
            else
            {

            }
            OnPropertyChanged("UI_chfb");
            OnPropertyChanged("UI_bwsb");
            OnPropertyChanged("UI_clsb");
            OnPropertyChanged("UI_shfb");
        }

        private void SetEnglishUI()
        {
            UI_bwsb = "BLACK & WHITE";
            UI_clsb = "COLOR";
            UI_chfb = "SELECT FOLDER";
            UI_shfb = "VIEW SCANNED DOCUMENTS";
            StatusBlock.Text = "Ready to scan.";
            localeENButton.Foreground = new SolidColorBrush(Colors.LightGray);
            localeFRButton.Foreground = new SolidColorBrush(Colors.White);
            UIloc = "English";
        }

        // Switch to French UI if not already selected
        private void LocaleFRButton_Click(object sender, RoutedEventArgs e)
        {
            if (UIloc != "French")
            {
                SetFrenchUI();           
            }
            else
            {

            }
            OnPropertyChanged("UI_chfb");
            OnPropertyChanged("UI_bwsb");
            OnPropertyChanged("UI_clsb");
            OnPropertyChanged("UI_shfb");
        }

        private void SetFrenchUI()
        {
            UI_bwsb = "NOIR ET BLANC";
            UI_clsb = "COULEUR";
            UI_chfb = "CHANGER LE DOSSIER";
            UI_shfb = "AFFICHER LES FICHIERS NUMÉRISÉS";
            StatusBlock.Text = "Prêt pour la numérisation.";
            localeENButton.Foreground = new SolidColorBrush(Colors.White);
            localeFRButton.Foreground = new SolidColorBrush(Colors.LightGray);
            UIloc = "French";            
        }

        //private void UpdateDPath()
        //{
        //    dPath = localFolderPath;
        //    OnPropertyChanged(dPath);
        //}

        // Disable buttons while a document is being scanned to prevent errors
        private void DisableUI()
        {
            BWScanButton.IsEnabled = false;
            CLRScanButton.IsEnabled = false;
            FolderButton.IsEnabled = false;
            ShowFilesButton.IsEnabled = false;

            if(UIloc == "French")
            {
                StatusBlock.Text = "Numérisation en cours, veuillez patienter...";
            }
            else
            {
                StatusBlock.Text = "Please wait while your document is being scanned...";
            }            
        }

        // Enable the UI when the app is ready
        private void EnableUI()
        {
            BWScanButton.IsEnabled = true;
            CLRScanButton.IsEnabled = true;
            FolderButton.IsEnabled = true;
            ShowFilesButton.IsEnabled = true;

            if (UIloc == "French")
            {
                StatusBlock.Text = "Prêt pour la numérisation.";
            }
            else
            {
                StatusBlock.Text = "Ready.";
            }            
        }

        /// <summary>
        /// Deletes temporary image files from the RXscan directory in MyDocuments after conversion.
        /// </summary>
        private void DeleteTempFiles()
        {
            System.IO.DirectoryInfo tempDir = new DirectoryInfo(tempFilePath);
            foreach (FileInfo file in tempDir.EnumerateFiles())
            {
                // Ensure only image files are deleted
                if(file.Extension == ".bmp")
                {
                    // Check if file is in use
                    if (!IsFileLocked(file))
                    {
                        try
                        {
                            file.Delete();
                            Debug.WriteLine("Deleted: " + file.Name);
                        }
                        catch(Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }
                }
            }
        }


        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChangedEvent = PropertyChanged;
            if (propertyChangedEvent != null)
            {
                propertyChangedEvent(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
