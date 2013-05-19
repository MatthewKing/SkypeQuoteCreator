namespace SkypeQuoteCreator
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Main form for the application.
    /// </summary>
    internal sealed partial class MainForm : Form
    {
        /// <summary>
        /// Unix time epoch.
        /// </summary>
        private static readonly DateTime epoch =
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Initializes a new instance of the MainForm class.
        /// </summary>
        public MainForm()
        {
            this.InitializeComponent();
            this.Font = SystemFonts.MessageBoxFont;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        /// <summary>
        /// Handles the Load event for this form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains no event data.</param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.UseCurrentDate();
        }

        /// <summary>
        /// Handles the Click event for uxUseCurrentDate.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains no event data.</param>
        private void uxUseCurrentDate_Click(object sender, EventArgs e)
        {
            this.UseCurrentDate();
        }

        /// <summary>
        /// Handles the Click event for uxCopyToClipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains no event data.</param>
        private void uxCopyToClipboard_Click(object sender, EventArgs e)
        {
            this.SaveToClipboard();
        }

        /// <summary>
        /// Populates the Timestamp field with the current date.
        /// </summary>
        private void UseCurrentDate()
        {
            this.uxTimestamp.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Saves the specified message to the clipboard.
        /// </summary>
        private void SaveToClipboard()
        {
            // Clear the current clipboard.
            Clipboard.SetText(" ");

            DateTime dateTime;

            // If the DateTime is invalid, we'll just stop right here.
            if (!DateTime.TryParse(this.uxTimestamp.Text, out dateTime))
                return;

            string user = this.uxName.Text;
            string message = this.uxMessage.Text;

            string skypeMessageFragment = String.Format(
                "<quote author=\"{0}\" timestamp=\"{1}\">{2}</quote>",
                user,
                (dateTime.ToUniversalTime() - epoch).TotalSeconds,
                message);

            IDataObject dataObject = new DataObject();
            dataObject.SetData("System.String", message);
            dataObject.SetData("Text", message);
            dataObject.SetData("UnicodeText", message);
            dataObject.SetData("OEMText", message);

            dataObject.SetData("SkypeMessageFragment",
                new MemoryStream(Encoding.UTF8.GetBytes(skypeMessageFragment)));

            dataObject.SetData("Locale",
                new MemoryStream(BitConverter.GetBytes(CultureInfo.CurrentCulture.LCID)));

            Clipboard.SetDataObject(dataObject);
        }
    }
}
