using System;

namespace EPS.Web.Management
{
    /// <summary>   Credential cache add event. </summary>
    /// <remarks>   ebrown, 11/10/2010. </remarks>
    public class CredentialCacheAddEvent : CacheEvents
    {
        /// <summary>   Constructor. </summary>
        /// <remarks>   ebrown, 11/10/2010. </remarks>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="userName"> The username. </param>
        public CredentialCacheAddEvent(object sender, string userName)
            : base(Properties.ManagementStrings.CredentialIdentifierAddedFor + userName, sender, EventCodes.CacheAdd)
        { }
    }
}
