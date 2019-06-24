#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="DownloadForm.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Properties;
    using Version = SemVer.Version;

    /// <summary>
    /// The main window of the application.
    /// </summary>
    public partial class DownloadForm : Form
    {
        #region Fields

        private          string    setupTempFile = "";
        private readonly string[]  commandLineArgs;
        private          WebClient client = new WebClient();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadForm"/> class.
        /// </summary>
        public DownloadForm(string[] args)
        {
            // Store CLI args as DI to pass to VP
            this.commandLineArgs = args;
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private async void DownloadForm_Load(object sender, EventArgs e)
        {
            this.Show();

            if (!VirtualParadise.IsInVpPath())
            {
                // VirtualParadise.exe not found, we cannot do anything
                DialogResult result = MessageBox.Show(Resources.VpExeNotFound,
                                                      Resources.Error,
                                                      MessageBoxButtons.OK,
                                                      MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                {
                    VirtualParadise.Launch(this.commandLineArgs);
                    Environment.Exit(0);
                    return;
                }
            }

            this.labelDownloading.Text = Resources.UpdateCheck;

            bool update = await CheckForUpdates();

            if (!update)
            {
                // No update is available - the client is up to date

                this.labelDownloading.Text = Resources.UpToDate;
                this.progressBar.Style     = ProgressBarStyle.Continuous;
                this.progressBar.Value     = this.progressBar.Maximum;
                this.buttonCancel.Text     = Resources.Close;

                // Launch as normal
                VirtualParadise.Launch(this.commandLineArgs);
                Environment.Exit(0);
                return;
            }

            string downloadLink = await VirtualParadise.GetDownloadLink();
            if (String.IsNullOrEmpty(downloadLink))
            {
                // There was an error parsing the HTML or fetching the download page
                DialogResult result = MessageBox.Show(Resources.DownloadLinkFetchFail,
                                                      Resources.Error,
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    VirtualParadise.Launch(this.commandLineArgs);
                }

                Environment.Exit(0);
                return;
            }

            this.client.DownloadFileCompleted += this.LaunchSetup;
            this.client.DownloadProgressChanged += (o, args) =>
                                                   {
                                                       // Update progress for user
                                                       this.progressBar.Style = ProgressBarStyle.Continuous;
                                                       this.progressBar.Value = args.ProgressPercentage;
                                                       this.labelDownloading.Text =
                                                           String.Format(Resources.DownloadingUpdate,
                                                                         args.ProgressPercentage);
                                                   };

            this.setupTempFile = Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetFileName(downloadLink);
            try
            {
                // Download the setup file to TEMP
                await this.client.DownloadFileTaskAsync(downloadLink, this.setupTempFile);
            }
            catch
            {
                // ignored
            }
        }

        private void LaunchSetup(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }

            this.progressBar.Style = ProgressBarStyle.Marquee;

            if (String.IsNullOrEmpty(this.setupTempFile) || !File.Exists(this.setupTempFile))
            {
                // The setup file wasn't downloaded properly
                DialogResult result = MessageBox.Show(Resources.LaunchSetupError,
                                                      Resources.Error,
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // Launch VP anyway
                    VirtualParadise.Launch(this.commandLineArgs);
                }

                Environment.Exit(0);
                return;
            }

            // Launch setup and wait for its completion
            this.labelDownloading.Text = Resources.WaitingForSetup;
            Process process = new Process {StartInfo = new ProcessStartInfo(this.setupTempFile)};
            process.Start();
            process.WaitForExit();

            // TODO do any clean-up here

            Environment.Exit(0);
        }

        /// <summary>
        /// Checks for a Virtual Paradise update.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the there is an updated and the user accepted, <see langword="false"/> otherwise.</returns>
        private static async Task<bool> CheckForUpdates()
        {
            Version current = VirtualParadise.GetCurrentVersion();
            Version latest  = await VirtualParadise.GetLatestVersion();

            if (current >= latest) // thank you SemVer
            {
                return false;
            }

            DialogResult result = MessageBox.Show(String.Format(Resources.UpdatePrompt, current, latest),
                                                  Resources.UpdateAvailable,
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        #endregion

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            if (this.buttonCancel.Text != Resources.Close)
            {
                DialogResult result = MessageBox.Show(Resources.ConfirmCancel,
                                                      Resources.ConfirmCancelTitle,
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    this.client.CancelAsync();
                    this.labelDownloading.Text = Resources.DownloadCancelled;
                    this.progressBar.Value     = 0;

                    Application.DoEvents();

                    // We've cancelled the update but we should still confirm VP launch
                    result = MessageBox.Show(String.Format(Resources.Launch, VirtualParadise.GetCurrentVersion()),
                                             Resources.LaunchTitle,
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        VirtualParadise.Launch(this.commandLineArgs);
                    }
                }
            }

            Environment.Exit(0);
        }
    }
}
