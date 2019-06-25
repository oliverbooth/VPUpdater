#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="MachineArch.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    #region Using Directives

    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    #endregion

    /// <summary>
    /// An enumeration of compatible machine architectures.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum MachineArch
    {
        /// <summary>
        /// x86 instruction set.
        /// </summary>
        [Description("x86 instruction set.")]
        x86 = 0,

        /// <summary>
        /// x86-64 instruction set.
        /// </summary>
        [Description("x64 instruction set.")]
        x64 = 1
    }
}
