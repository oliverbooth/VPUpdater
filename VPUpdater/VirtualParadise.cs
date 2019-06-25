#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="VirtualParadise.cs" company="VPUpdater">
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
    using SemVer = SemVer.Version;
    using SysVer = System.Version;

    #endregion

    /// <summary>
    /// Virtual Paradise helper class.
    /// </summary>
    public class VirtualParadise
    {
        #region Constants

        /// <summary>
        /// Gets the exe filename of Virtual Paradise.
        /// </summary>
        public const string ExeFilename = @"VirtualParadise.exe";

        /// <summary>
        /// Gets the root hostname for Virtual Paradise.
        /// </summary>
        public const string Hostname = @"virtualparadise.org";

        /// <summary>
        /// Gets the URI of Virtual Paradise.
        /// </summary>
        public static readonly Uri Uri = new Uri($@"https://{Hostname}");

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualParadise"/> class.
        /// </summary>
        /// <param name="current">The file information for Virtual Paradise.</param>
        private VirtualParadise(FileInfo current)
        {
            // Virtual Paradise does not use the Semantic Version standard,
            // but we can use System.Version as a middle-man, and build a SemVer-complaint
            // version from its properties.
            // Edwin pls. https://semver.org/ - you can thank me later.
            string fileVersion = FileVersionInfo.GetVersionInfo(current.FullName).FileVersion;
            SysVer version     = SysVer.Parse(fileVersion);

            this.FileInfo = current;
            this.Version  = GetSemVerFromSystemVersion(version);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the file information for this version of Virtual Paradise.
        /// </summary>
        public FileInfo FileInfo { get; }

        /// <summary>
        /// Gets the version string of this Virtual Paradise.
        /// </summary>
        public SemVer Version { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current version of Virtual Paradise.
        /// </summary>
        /// <returns>Returns a new instance of <see cref="VirtualParadise"/>, or <see langword="null"/> on failure.</returns>
        public static VirtualParadise GetCurrent() =>
            GetCurrent(Environment.CurrentDirectory);

        /// <summary>
        /// Gets the current version of Virtual Paradise.
        /// </summary>
        /// <param name="path">Optional. The path of the Virtual Paradise install. Defaults to current directory.</param>
        /// <returns>Returns a new instance of <see cref="VirtualParadise"/>, or <see langword="null"/> on failure.</returns>
        public static VirtualParadise GetCurrent(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            if (!attributes.HasFlag(FileAttributes.Directory))
            {
                path = Path.GetDirectoryName(path);
            }

            if (!Directory.Exists(path))
            {
                return null;
            }

            string filename = path + Path.DirectorySeparatorChar + ExeFilename;
            return File.Exists(filename) ? new VirtualParadise(new FileInfo(filename)) : null;
        }

        /// <summary>
        /// Translates a <see cref="SysVer"/> into a semantic version compliant <see cref="SemVer"/>.
        /// </summary>
        /// <param name="version">The <see cref="SysVer"/> to translate.</param>
        /// <returns>Returns a <see cref="SemVer"/> representing the <see cref="SysVer"/>.</returns>
        private static SemVer GetSemVerFromSystemVersion(SysVer version) =>
            new SemVer(version.Major, version.Minor, version.Build);

        /// <summary>
        /// Launches Virtual Paradise.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public void Launch(params string[] args)
        {
            Process.Start(this.FileInfo.FullName, String.Join(" ", args));
        }

        #endregion
    }
}
