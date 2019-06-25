#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="Helper.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    #region Using Directives

    using System;

    #endregion

    /// <summary>
    /// A set of helper methods.
    /// </summary>
    /// <remarks>I understand that "helper" classes are bad practice, but this isn't exactly a
    /// commercial-grade app.</remarks>
    public static class Helper
    {
        #region Methods

        /// <summary>
        /// Gets the machine's architecture.
        /// </summary>
        /// <returns>Returns a <see cref="MachineArch"/> representing the machine architecture.</returns>
        public static MachineArch GetMachineArch() =>
            Environment.Is64BitOperatingSystem ? MachineArch.x64 : MachineArch.x86;

        #endregion
    }
}
