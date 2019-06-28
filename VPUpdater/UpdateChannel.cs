#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="UpdateChannel.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    #region Using Directives

    using System.ComponentModel;

    #endregion

    /// <summary>
    /// An enumeration of update channels.
    /// </summary>
    public enum UpdateChannel
    {
        /// <summary>
        /// Stable releases.
        /// </summary>
        [Description("Stable releases.")]
        Stable,

        /// <summary>
        /// Pre-releases.
        /// </summary>
        [Description("Pre-releases.")]
        PreRelease
    }
}
