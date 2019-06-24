#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="VirtualParadise.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AngleSharp;
    using AngleSharp.Dom;
    using SemVer = SemVer.Version;

    /// <summary>
    /// Virtual Paradise helper class.
    /// </summary>
    public class VirtualParadise
    {
        #region

        /// <summary>
        /// The human-readable system architecture string.
        /// </summary>
        public static readonly string Arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";

        /// <summary>
        /// The Virtual Paradise executable filename.
        /// </summary>
        public static readonly string Exe = "VirtualParadise.exe";

        /// <summary>
        /// The assumed path (including filename) of the Virtual Paradise executable.
        /// </summary>
        public static readonly string ExePath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + Exe;

        /// <summary>
        /// The root URI of Virtual Paradise.
        /// </summary>
        public static readonly string Uri = "https://virtualparadise.org";

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current Virtual Paradise version.
        /// </summary>
        /// <returns>Returns a semantic-version compliant version.</returns>
        public static SemVer GetCurrentVersion()
        {
            if (IsInVpPath())
            {
                // Virtual Paradise does not use the Semantic Version standard,
                // but we can use System.Version as a middle-man, and build a SemVer-complaint
                // version from its properties.
                // Edwin pls. https://semver.org/ - you can thank me later.
                string  fileVersion = FileVersionInfo.GetVersionInfo(ExePath).FileVersion;
                Version version     = Version.Parse(fileVersion);
                return new SemVer(version.Major, version.Minor, version.Build - 1);
            }

            return new SemVer("0.0.0", true);
        }

        /// <summary>
        /// Gets the latest download link.
        /// </summary>
        /// <remarks></remarks>
        public static async Task<string> GetDownloadLink()
        {
            IConfiguration   config   = Configuration.Default.WithDefaultLoader();
            string           address  = $"{Uri}/Download";
            IBrowsingContext context  = BrowsingContext.New(config);
            IDocument        document = await context.OpenAsync(address);

            string                    selector = @".download a.btn";
            IHtmlCollection<IElement> cells    = document.QuerySelectorAll(selector);

            IElement a =
                cells.FirstOrDefault(c =>
                                         Regex.Match(c.GetAttribute("href"),
                                                     $"windows_{Regex.Escape(Arch)}")
                                              .Success);

            string href = a?.GetAttribute("href") ?? "";
            return href;
        }

        /// <summary>
        /// Gets the current Virtual Paradise version.
        /// </summary>
        /// <returns>Returns a semantic-version compliant version.</returns>
        public static async Task<SemVer> GetLatestVersion()
        {
            using (WebClient client = new WebClient())
            {
                string version = await client.DownloadStringTaskAsync($"{Uri}/version.txt");
                return new SemVer(version);
            }
        }

        /// <summary>
        /// Determines if we're currently in the Virtual Paradise path.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if <see cref="ExePath"/> exists, <see langword="false"/> otherwise.</returns>
        public static bool IsInVpPath() =>
            File.Exists(ExePath);

        /// <summary>
        /// Launches Virtual Paradise.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Launch(params string[] args)
        {
            if (IsInVpPath())
            {
                Process.Start(ExePath, String.Join(" ", args));
            }
        }

        #endregion
    }
}
