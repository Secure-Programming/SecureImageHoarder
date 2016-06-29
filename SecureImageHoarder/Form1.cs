#region

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Bing;
using Cryptology;
using SecureImageHoarder.BingSearch;

#endregion

namespace SecureImageHoarder
{
    /// <summary>
    ///     ****************************************************
    ///     * Secure Image Hoarder - Alpha Test Version! (WIP) *
    ///     ****************************************************
    ///     Searches images on the world wide web using Bing search, and downloads the results automatically.
    ///     Uses encrypted storage.
    ///     TODO: Better threading using threadpools, proper encryption implementation, database storage and many tweaks and fixes!
    /// </summary>
    public partial class MainWindow : Form
    {
        //Fields
        private const string StartupText =
            "Welcome to Secure Image Hoarder!\r\n \r\nWith Secure Image Hoarder you can mass download many images,\r\nand store them safely!\r\nType in your search query and press Hoard! to start the hoarding!\r\n";

        private const string AccountKey = "rp9kRcserYNdEbUPRSY4ss7N8oBqIBB9qkao/5x5Omg";

        /// <summary>
        ///     Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Path.GetFileName(Assembly.GetEntryAssembly().Location));
            AcceptButton = searchButton;
            logBox.SelectedText = StartupText;
        }

        /// <summary>
        ///     Searches Bing using official Microsoft Bing C# API.
        /// </summary>
        /// <param name="query">Search query</param>
        private void BingSearch(string query)
        {
            // new container object
            var bingContainer = new BingSearchContainer(new Uri("https://api.datamarket.azure.com/Bing/Search/"));

            // the next line configures the bingContainer to use your credentials.
            bingContainer.Credentials = new NetworkCredential(AccountKey, AccountKey);

            // now we can build the query
            var imageQuery = bingContainer.Image(query, null, null, null, null, null, null);

            LogWrite("\r\nStarting search...");

            // Run the search
            var imageResults = imageQuery.Execute();

            LogWriteLine("Done.\r\n");

            LogWriteLine("Starting download:\r\n");

            // Go through list and download everything
            foreach (var result in imageResults)
            {
                Thread thread =
                    new Thread(() => SaveImage(new ImageObject(result.Title, DownloadBitmap(result.MediaUrl))));
                thread.Start();
            }

            // LogWriteLine("\r\nDone!\r\n");
        }

        /// <summary>
        ///     Save the downloaded image to an actual file, uses url title as filename (still messy).
        /// </summary>
        /// <param name="image">ImageObject</param>
        private void SaveImage(ImageObject image)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Images"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Images");
            }
            string filename = AppDomain.CurrentDomain.BaseDirectory + @"Images\" + image.Title + ".jpg";
            if (image.Image != null)
            {
                try
                {
                    image.Image.Save(filename, ImageFormat.Jpeg);
                    AESCrypt.EncryptFile(filename, filename + ".enc", "yolo");
                    LogWriteLine($"Fetched: {image.Title}");
                }
                catch
                {
                    LogWriteLine($"Store error: {image.Title}", Color.Red);
                }
            }
            else
            {
                LogWriteLine($"Download error: {image.Title}", Color.Red);
            }
        }

        /// <summary>
        ///     Write the logBox with enter (newline).
        /// </summary>
        /// <param name="input">text to display</param>
        private void LogWriteLine(string input)
        {
            LogWriter(input, Color.Black);
        }

        /// <summary>
        ///     Write the logBox with enter (newline).
        /// </summary>
        /// <param name="input">text to display</param>
        /// <param name="color">text color</param>
        private void LogWriteLine(string input, Color color)
        {
            LogWriter(input + "\r\n", color);
        }

        /// <summary>
        ///     Write the logBox (without newline).
        /// </summary>
        /// <param name="input">text to display</param>
        private void LogWrite(string input)
        {
            LogWriter(input, Color.Black);
        }

        /// <summary>
        ///     Write the logBox (without newline).
        /// </summary>
        /// <param name="input">tetx to display</param>
        /// <param name="color">text color</param>
        private void LogWrite(string input, Color color)
        {
            LogWriter(input, color);
        }

        /// <summary>
        ///     Actually outputs data to logBox with threading support.
        /// </summary>
        /// <param name="input">text to display</param>
        /// <param name="color">text color</param>
        private void LogWriter(string input, Color color)
        {
            Action colorChange = () => logBox.SelectionColor = color;
            Invoke(colorChange);
            Action action = () => logBox.SelectedText += $"{input}\r\n";
            Invoke(action);
            Action autoScroll = () => logBox.ScrollToCaret();
            Invoke(autoScroll);
        }

        /// <summary>
        ///     Download an image from the internet and stores it in a Bitmap object.
        /// </summary>
        /// <param name="url">image location</param>
        /// <returns></returns>
        private Bitmap DownloadBitmap(string url)
        {
            try
            {
                WebRequest request =
                    WebRequest.Create(
                        url);
                WebResponse response = request.GetResponse();
                Stream responseStream =
                    response.GetResponseStream();
                return new Bitmap(responseStream);
            }
            catch (WebException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Invokes search if user presses Hoard! button.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void searchButton_Click(object sender, EventArgs e)
        {
            if (searchBox.Text.Length > 0)
            {
                BingSearch(searchBox.Text);
            }
            else
            {
                MessageBox.Show("Please enter a search query before attempting to hoard the interwebz!",
                    "Invalid input!",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}