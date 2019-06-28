#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="DownloadForm.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    #region Using Directives

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Properties;
    using Version = SemVer.Version;

    #endregion

    /// <summary>
    /// The main window of the application.
    /// </summary>
    public partial class DownloadForm : Form
    {
        #region Fields

        private          string          setupTempFile = "";
        private readonly string[]        commandLineArgs;
        private readonly WebClient       client = new WebClient();
        private readonly Updater         updater;
        private readonly VirtualParadise virtualParadise;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadForm"/> class.
        /// </summary>
        public DownloadForm(string[] args)
        {
            this.InitializeComponent();

            // Store CLI args as DI to pass to VP
            this.commandLineArgs = args;
            this.virtualParadise = VirtualParadise.GetCurrent();
            this.updater         = new Updater(this.virtualParadise);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when <see cref="buttonCancel"/> is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.s</param>
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
                    this.updater.Cancel();
                    this.labelDownloading.Text = Resources.DownloadCancelled;
                    this.progressBar.Value     = 0;

                    Application.DoEvents();

                    // We've cancelled the update but we should still confirm VP launch
                    result = MessageBox.Show(String.Format(Resources.Launch, this.virtualParadise.Version),
                                             Resources.LaunchTitle,
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        this.virtualParadise.Launch(this.commandLineArgs);
                    }
                }
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// Checks for a Virtual Paradise update.
        /// </summary>
        /// <param name="channel">The update channel to use.</param>
        /// <returns>Returns <see langword="true"/> if the there is an updated and the user accepted, <see langword="false"/> otherwise.</returns>
        private async Task<Version> CheckForUpdates(UpdateChannel channel)
        {
            return await this.updater.FetchLatest(channel);
        }

        /// <summary>
        /// Called when the form first loads.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private async void DownloadForm_Load(object sender, EventArgs e)
        {
            await this.updater.LoadDefaultConfiguration();

            this.Show();
            this.Run();
        }

        /// <summary>
        /// Performs the update routine.
        /// </summary>
        private async void Run()
        {
            this.labelDownloading.Text = String.Format(Resources.VpExeCheck, VirtualParadise.ExeFilename);
            this.progressBar.Style     = ProgressBarStyle.Marquee;

            if (this.virtualParadise == null)
            {
                this.progressBar.Style = ProgressBarStyle.Continuous;
                this.progressBar.Value = 0;

                MessageBox.Show(String.Format(Resources.VpExeNotFound, VirtualParadise.ExeFilename),
                                Resources.Error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                Environment.Exit(0);
                return;
            }

            UpdateChannel channel = (int)this.updater.Config["stable_only"] == 1
                ? UpdateChannel.Stable
                : UpdateChannel.PreRelease;

            this.labelDownloading.Text = Resources.UpdateCheck;
            Version currentVersion = this.virtualParadise.Version;
            Version latestVersion  = await this.CheckForUpdates(channel);

            if (currentVersion < latestVersion)
            {
                DialogResult result = MessageBox.Show(
                    String.Format(Resources.UpdatePrompt, currentVersion, latestVersion),
                    Resources.UpdateAvailable,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    // The update was denied but the user still intended to launch Virtual Paradise
                    this.virtualParadise.Launch(this.commandLineArgs);
                    Environment.Exit(0);
                    return;
                }
            }
            else
            {
                // Everything is up to date!
                this.labelDownloading.Text = Resources.UpToDate;
                this.progressBar.Style     = ProgressBarStyle.Continuous;
                this.progressBar.Value     = this.progressBar.Maximum;
                this.buttonCancel.Text     = Resources.Close;

                this.virtualParadise.Launch(this.commandLineArgs);

                Environment.Exit(0);
                return;
            }

            this.labelDownloading.Text = Resources.DownloadLinkFetch;
            Uri downloadUri;

            try
            {
                downloadUri = await this.updater.FetchDownloadLink(channel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(Resources.DownloadLinkFetchError, ex.Message),
                                Resources.Error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                DialogResult result =
                    MessageBox.Show(String.Format(Resources.Launch, this.virtualParadise.Version),
                                    Resources.LaunchTitle,
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    this.virtualParadise.Launch(this.commandLineArgs);
                }

                Environment.Exit(0);
                return;
            }

            this.updater.WebClientProgressChanged += this.WebClientProgressChanged;

            try
            {
                // Download the update
                await this.updater.Download(downloadUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(Resources.DownloadUpdateError, ex.Message),
                                Resources.Error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                DialogResult result =
                    MessageBox.Show(String.Format(Resources.Launch, this.virtualParadise.Version),
                                    Resources.LaunchTitle,
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    this.virtualParadise.Launch(this.commandLineArgs);
                }

                Environment.Exit(0);
                return;
            }

            this.progressBar.Style     = ProgressBarStyle.Marquee;
            this.labelDownloading.Text = Resources.WaitingForSetup;

            try
            {
                // Launch the update setup
                await this.updater.Launch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(Resources.LaunchSetupError, ex.Message),
                                Resources.Error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                DialogResult result =
                    MessageBox.Show(String.Format(Resources.Launch, this.virtualParadise.Version),
                                    Resources.LaunchTitle,
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    this.virtualParadise.Launch(this.commandLineArgs);
                }

                Environment.Exit(0);
                return;
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// Called when the internal <see cref="WebClient"/> updates its progress.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void WebClientProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Update progress for user
            this.progressBar.Style     = ProgressBarStyle.Continuous;
            this.progressBar.Value     = e.ProgressPercentage;
            this.labelDownloading.Text = String.Format(Resources.DownloadingUpdate, e.ProgressPercentage);
        }

        #endregion
    }
}
