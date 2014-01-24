namespace SkypeQuoteCreator
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using System.Windows.Forms;

    /// <summary>
    /// Simple wrapper for Google Universal Analytics. 
    /// </summary>
    internal static class Analytics
    {
        /// <summary>
        /// Google analytics address.
        /// </summary>
        private const string address = "http://www.google-analytics.com/collect";

        /// <summary>
        /// Google analytics tracking ID.
        /// </summary>
        private const string trackingId = "UA-47159349-3";

        /// <summary>
        /// Tracks the screen view.
        /// </summary>
        /// <param name="viewName">View name.</param>
        /// <param name="clientId">Client ID.</param>
        public static void TrackScreenView(string viewName, string clientId)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.UploadValuesAsync(new Uri(address), new NameValueCollection()
                    {
                        { "v", "1" },
                        { "tid", trackingId },
                        { "an", Application.ProductName },
                        { "av", Application.ProductVersion },
                        { "t", "appview" },
                        { "cd", viewName },
                        { "cid", clientId },
                    });
                }
                catch
                {
                    // Swallow all exceptions.
                }
            }
        }
    }
}
