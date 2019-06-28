#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="Updater.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    #region Using Directives

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Syndication;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;
    using AngleSharp;
    using AngleSharp.Dom;
    using Properties;
    using SemVer = SemVer.Version;

    #endregion

    /// <summary>
    /// Represents an updater.
    /// </summary>
    public class Updater : IDisposable
    {
        #region Fields

        /// <summary>
        /// Regular expression for matching semantic version strings.
        /// </summary>
        private const string SemVerRegex =
            @"(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(-(0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(\.(0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*)?(\+[0-9a-zA-Z-]+(\.[0-9a-zA-Z-]+)*)?";

        /// <summary>
        /// The <see cref="VirtualParadise"/> instance.
        /// </summary>
        private readonly VirtualParadise virtualParadise;

        /// <summary>
        /// The web client instance.
        /// </summary>
        private WebClient webClient = new WebClient();

        /// <summary>
        /// The setup path.
        /// </summary>
        private string setupPath;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Updater"/> class.
        /// </summary>
        /// <param name="virtualParadise">The <see cref="VirtualParadise"/> instance.</param>
        public Updater(VirtualParadise virtualParadise)
        {
            this.virtualParadise = virtualParadise;
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when an internal <see cref="WebClient"/> download progress has changed.
        /// </summary>
        public DownloadProgressChangedEventHandler WebClientProgressChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Cancels any updates in progress.
        /// </summary>
        public void Cancel()
        {
            this.webClient?.CancelAsync();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.webClient?.Dispose();
        }

        /// <summary>
        /// Downloads the link to the latest
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<string> Download(Uri uri)
        {
            if (uri.Host != VirtualParadise.Hostname && !uri.Host.EndsWith('.' + VirtualParadise.Hostname))
            {
                throw new UriFormatException(String.Format(Resources.UriNotFromVp, VirtualParadise.Hostname));
            }

            string setupFilename = Path.GetFileName(uri.ToString());
            string tempFilename  = Path.GetTempPath() + Path.DirectorySeparatorChar + setupFilename;

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += this.WebClientProgressChanged;
                this.webClient                 =  client;
                await client.DownloadFileTaskAsync(uri, tempFilename);
            }

            if (!File.Exists(tempFilename))
            {
                throw new FileNotFoundException(String.Format(Resources.FileNotFound, setupFilename));
            }

            this.setupPath = tempFilename;
            return tempFilename;
        }

        /// <summary>
        /// Fetches the latest version of Virtual Paradise.
        /// </summary>
        /// <returns>Gets the version string of the latest stable Virtual Paradise.</returns>
        public async Task<SemVer> FetchLatest(UpdateChannel channel = UpdateChannel.Stable)
        {
            switch (channel)
            {
                case UpdateChannel.PreRelease:
                    string rssUri = new Uri(VirtualParadise.Uri, @"/edwin/feed").ToString();
                    using (XmlReader rssReader = XmlReader.Create(rssUri))
                    {
                        SyndicationFeed feed  = SyndicationFeed.Load(rssReader);
                        Match           match = null;
                        SyndicationItem item =
                            feed.Items.FirstOrDefault(i => Regex.Match(i.Title.Text, @"Virtual Paradise").Success &&
                                                           (match =
                                                               Regex.Match(
                                                                   i.Title.Text, SemVerRegex,
                                                                   RegexOptions.IgnoreCase))
                                                          .Success);

                        if (!(item is null || match is null))
                        {
                            // Return the version
                            return new SemVer(match.Value);
                        }

                        // Return the latest stable instead
                        return await this.FetchLatest();
                    }

                case UpdateChannel.Stable:
                default:
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadProgressChanged += this.WebClientProgressChanged;
                        this.webClient                 =  client;

                        Uri    uri           = new Uri(VirtualParadise.Uri, @"version.txt");
                        string versionString = await client.DownloadStringTaskAsync(uri);

                        return new SemVer(versionString);
                    }
            }
        }

        /// <summary>
        /// Fetches the Uri of the latest download from the Virtual Paradise download page.
        /// </summary>
        /// <param name="channel">The update channel to use.</param>
        /// <param name="item">If <paramref name="channel"/> is set to <see cref="UpdateChannel.PreRelease"/>, the method will parse
        /// this <see cref="SyndicationItem"/> for a valid download URI.</param>
        /// <returns>Returns a <see cref="Uri"/> containing the download link.</returns>
        public async Task<Uri> FetchDownloadLink(UpdateChannel   channel = UpdateChannel.Stable,
                                                 SyndicationItem item    = null)
        {
            IConfiguration   config          = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context         = BrowsingContext.New(config);
            Uri              downloadPageUri = new Uri(VirtualParadise.Uri, @"Download");

            using (IDocument document = await context.OpenAsync(downloadPageUri.ToString()))
            {
                const string selector   = @".download a.btn";
                string       systemArch = Helper.GetMachineArch().ToString();

                IHtmlCollection<IElement> cells = document.QuerySelectorAll(selector);
                IElement a =
                    cells.FirstOrDefault(c =>
                                             Regex.Match(c.GetAttribute("href"),
                                                         $"windows_{Regex.Escape(systemArch)}")
                                                  .Success);

                string href = a?.GetAttribute("href") ?? "";
                return new Uri(href);
            }
        }

        /// <summary>
        /// Launches the setup.
        /// </summary>
        public async Task Launch()
        {
            if (!File.Exists(this.setupPath))
            {
                throw new FileNotFoundException(
                    String.Format(Resources.FileNotFound, Path.GetFileName(this.setupPath)));
            }

            await Task.Run(() =>
                           {
                               Process process = new Process {StartInfo = {FileName = this.setupPath}};
                               process.Start();
                               process.WaitForExit();
                           });
        }

        #endregion
    }
}
