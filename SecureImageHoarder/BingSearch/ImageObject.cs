#region

using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

#endregion

namespace SecureImageHoarder.BingSearch
{
    /// <summary>
    ///     Storage for image bitmap data and title.
    /// </summary>
    public class ImageObject
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="title">title of image</param>
        /// <param name="image">bitmap data</param>
        public ImageObject(string title, Bitmap image)
        {
            Title = FilterTitle(title);
            Image = image;
        }

        public string Title { get; private set; }
        public Bitmap Image { get; private set; }

        /// <summary>
        ///     Filter title for invalid and unwanted characters.
        /// </summary>
        /// <param name="input">dirty input</param>
        /// <returns>clean output</returns>
        private string FilterTitle(string input)
        {
            string title = input;
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            title = r.Replace(title, "");
            return title;
        }
    }
}