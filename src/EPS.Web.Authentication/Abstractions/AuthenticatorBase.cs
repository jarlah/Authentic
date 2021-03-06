using System;
using System.Web;
using Common.Logging;
using EPS.Web.Authentication.Configuration;

namespace EPS.Web.Authentication.Abstractions
{
    /// <summary>   
    /// A base class for implementing custom http context inspection handlers as defined by <see cref="T:
    /// EPS.Web.Authentication.Abstractions.IAuthenticator{T}"/>. 
    /// </summary>
    /// <remarks>   ebrown, 1/3/2011. </remarks>
    public abstract class AuthenticatorBase<T> : 
        IAuthenticator<T> where T :
        class, IAuthenticatorConfiguration
    {
        private static readonly ILog log = LogManager.GetCurrentClassLogger();        
        private readonly T _config;
        
        private AuthenticatorBase()
        { }

        /// <summary>   Constructor that must be called by implementors to properly pass configuration information. </summary>
        /// <remarks>   ebrown, 1/3/2011. </remarks>
        /// <param name="config">   The configuration. </param>
        protected AuthenticatorBase(T config)
        {
            this._config = config;
        }

        /// <summary>   Gets the log4net log instance. </summary>
        /// <value> The log. </value>
        protected ILog Log
        {
            get { return log; }
        }

        /// <summary>   Gets the human-friendly name of the inspector. </summary>
        /// <value> The name. </value>
        public string Name
        {
            get { return Configuration.Name; }
        }

        /*
        HttpContextInspectingAuthenticatorConfigurationElement IHttpContextInspectingAuthenticator<T>.Configuration
        {
            get { return config; }
        }
        */

        /// <summary>   Gets the base configuration instance -- intended to be used by infrastructure. </summary>
        /// <value> The configuration. </value>
        IAuthenticatorConfiguration IAuthenticator.Configuration
        {
            get { return _config; }
        }

        /// <summary>   Gets the specific configuration instance as specified in the generic class definition. </summary>
        /// <value> The configuration. </value>
        public T Configuration
        {
            get { return _config; }
        }

        /// <summary>   Authenticates based on a given HttpContextBase. </summary>
        /// <param name="context">  The context. </param>
        /// <returns>   A result instance that indicates where the authentication failed, and if successful returns an IPrincipal instance. </returns>
        public abstract AuthenticationResult Authenticate(HttpContextBase context);
    }
}
