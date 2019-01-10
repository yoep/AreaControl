using System;

namespace AreaControl.Menu
{
    public interface IResponseManager : IMenuComponent, IDisposable
    {
        /// <summary>
        /// Get the current response code.
        /// </summary>
        ResponseCode ResponseCode { get; }
        
        /// <summary>
        /// Get the response code audio for the current <see cref="ResponseCode"/>.
        /// </summary>
        string ResponseCodeAudio { get; }
    }
}