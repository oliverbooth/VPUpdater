#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="Program.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    /// Application entry class.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            args = args.Skip(1).ToArray();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DownloadForm(args));
        }
    }
}
