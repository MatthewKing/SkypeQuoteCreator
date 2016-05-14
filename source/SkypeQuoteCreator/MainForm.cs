namespace SkypeQuoteCreator
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Deployment.Application;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using Settings = Properties.Settings;

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
            if (Settings.Default.NameHistory == null)
            {
                Settings.Default.NameHistory = new StringCollection();
                Settings.Default.Save();
            }

            if (String.IsNullOrEmpty(Settings.Default.UserId))
            {
                Settings.Default.UserId = Guid.NewGuid().ToString();
                Settings.Default.Save();
            }

            this.UseCurrentDate();
            this.UseCachedNames();
            this.CheckForUpdates();

            Analytics.TrackScreenView("Main", Settings.Default.UserId);
        }

        /// <summary>
        /// Checks for any ClickOnce updates.
        /// </summary>
        private void CheckForUpdates()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment deployment = ApplicationDeployment.CurrentDeployment;
                deployment.CheckForUpdateCompleted += this.CheckForUpdateCompleted;
                deployment.UpdateProgressChanged += this.UpdateProgressChanged;
                deployment.UpdateCompleted += this.UpdateCompleted;

                try
                {
                    deployment.CheckForUpdateAsync();
                }
                catch (Exception ex)
                {
                    if (ex is InvalidOperationException ||
                        ex is InvalidDeploymentException ||
                        ex is DeploymentDownloadException)
                    {
                        // Swallow.
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the CheckForUpdateCompleted event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A CheckForUpdateCompletedEventArgs that contains the event data.</param>
        private void CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                if (e.UpdateAvailable)
                {
                    this.uxUpdate.Text = "Update available.";
                    this.uxUpdate.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the UpdateProgressChanged event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A DeploymentProgressChangedEventArgs that contains the event data.</param>
        private void UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            this.uxUpdate.Text = String.Format(
                "{0:D}K out of {1:D}K downloaded - {2:D}% complete",
                e.BytesCompleted / 1024,
                e.BytesTotal / 1024,
                e.ProgressPercentage);
        }

        /// <summary>
        /// Handles the UpdateCompleted event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An AsyncCompletedEventArgs that contains the event data.</param>
        private void UpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.uxUpdate.Text = "Update will be applied next time program runs.";
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
            this.SaveCurrentNameToCache();
            this.UseCachedNames();
            this.SaveToClipboard();
        }

        /// <summary>
        /// Handles the LinkClicked event for uxUpdate.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A LinkLabelLinkClickedEventArgs that contains the event data.</param>
        private void uxUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.uxUpdate.Enabled = false;
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment deployment = ApplicationDeployment.CurrentDeployment;
                deployment.UpdateAsync();
            }
        }

        /// <summary>
        /// Saves the current name to the name cache.
        /// </summary>
        private void SaveCurrentNameToCache()
        {
            string name = this.uxName.Text;

            if (!Settings.Default.NameHistory.Contains(name))
            {
                Settings.Default.NameHistory.Add(name);
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Populates the items in uxName with the saved names.
        /// </summary>
        private void UseCachedNames()
        {
            this.uxName.Items.Clear();

            foreach (string name in Settings.Default.NameHistory)
            {
                this.uxName.Items.Add(name);
            }
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
                "<quote author=\"{0}\" authorname=\"{0}\" timestamp=\"{1}\">{2}</quote>",
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
