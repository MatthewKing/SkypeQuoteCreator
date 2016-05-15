using System;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace SkypeQuoteCreator
{
    /// <summary>
    /// Simple wrapper for keen.io Analytics.
    /// </summary>
    internal static class Analytics
    {
        private const string Resource = "https://api.keen.io/3.0/projects/{project_id}/events/{event_collection}?api_key={write_key}";
        private const string ProjectId = "53beb382709a39164a00000d";
        private const string WriteKey = "64e2b8a218379d6cfe838fc6999248a2406e68b17afa964a81559d3e63f8cd6e62867ea9c162a5144d477c3ffb2f9e731236fcc128f6e05f214cde257c1bfca2d37f2b6fc81dcfdb360e60cf8a22edea520e73afd7dfa4d32bffa4fc18f61ac4b46da363fd89ab15216839d95d7f4036";
        private const string PayloadFormat = "{'keen':{'addons':[{'name':'keen:ip_to_geo','input':{'ip':'ip_address'},'output':'ip_geo_info'}]},'ip_address':'${keen.ip}','app_name':'{app_name}','app_version':'{app_version}','user_id':'{user_id}','view_name':'{view_name}'}";

        /// <summary>
        /// Tracks the screen view.
        /// </summary>
        /// <param name="viewName">View name.</param>
        /// <param name="clientId">Client ID.</param>
        public static void TrackScreenView(string viewName, string clientId)
        {
            StringBuilder address = new StringBuilder(Resource);
            address.Replace("{project_id}", ProjectId);
            address.Replace("{event_collection}", "appviews");
            address.Replace("{write_key}", WriteKey);

            StringBuilder data = new StringBuilder(PayloadFormat);
            data.Replace("{app_name}", Application.ProductName);
            data.Replace("{app_version}", Application.ProductVersion);
            data.Replace("{user_id}", clientId);
            data.Replace("{view_name}", viewName);
            data.Replace("'", "\"");

            using (WebClient client = new WebClient())
            {
                client.Headers["Content-Type"] = "application/json";

                try
                {
                    client.UploadStringAsync(
                        address: new Uri(address.ToString()),
                        method: "POST",
                        data: data.ToString());
                }
                catch
                {
                    // Swallow all exceptions.
                }
            }
        }
    }
}
