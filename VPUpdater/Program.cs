#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="Program.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    #endregion

    /// <summary>
    /// Application entry class.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        [STAThread]
        private static async Task Main(string[] args)
        {
            args = args.Skip(1).ToArray();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (DownloadForm form = await DownloadForm.Build(args))
            {
                Application.Run(form);
            }
        }
    }
}
