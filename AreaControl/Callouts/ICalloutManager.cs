using System;
using System.Collections.Generic;

namespace AreaControl.Callouts
{
    public interface ICalloutManager
    {
        /// <summary>
        /// Get the callouts of this plugin.
        /// </summary>
        IList<Type> Callouts { get; }
    }
}