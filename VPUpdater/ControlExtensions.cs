#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="ControlExtensions.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    #region Using Directives

    using System.Windows.Forms;

    #endregion

    /// <summary>
    /// Extension methods for <see cref="Control"/>.
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// Thread-safe method invocation. Calls <see cref="Control.Invoke"/> if <see cref="Control.InvokeRequired"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="control">The control from which to invoke.</param>
        /// <param name="action">The action to invoke.</param>
        public static void InvokeIfRequired(this Control control, MethodInvoker action)
        {
            if (control?.InvokeRequired ?? false)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
