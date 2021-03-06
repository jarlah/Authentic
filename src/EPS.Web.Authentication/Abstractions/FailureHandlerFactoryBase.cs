using Common.Logging;
using EPS.Web.Authentication.Configuration;

namespace EPS.Web.Authentication.Abstractions
{
	/// <summary>   
	/// A base class that should be implemented by a configured failure handler factory.  This factory is responsible for creating an instance of the
	/// actual http context inspector following a standard protocol defined in the interface <see cref="T:EPS.Web.Authentication.Abstractions.IFailureHandlerFactory{T}"/>. 
	/// </summary>
	/// <remarks>   ebrown, 1/3/2011. </remarks>
	public abstract class FailureHandlerFactoryBase<T> :
		   IFailureHandlerFactory<T>
		   where T : class, IFailureHandlerConfiguration
	{
		private static readonly ILog log = LogManager.GetCurrentClassLogger();

		/// <summary>   Gets the log4net log instance. </summary>
		/// <value> The log. </value>
		protected ILog Log
		{
			get { return log; }
		}

		#region IFailureHandlerFactory<T> Members
		/// <summary>   Constructs an instance of a failure handler. </summary>
		/// <param name="config">   The configuration used to guide construction of the failure handler. </param>
		/// <returns>   An instance of a failure handler. </returns>
		public abstract IFailureHandler<T> Construct(T config);
		#endregion

		#region IFailureHandlerFactory Members
		/// <summary>   
		/// Constructs an instance of a failure handler, but returns non-generic interface.  Intended to be called by infrastructure
		/// -- don't use this as it may not contain customized configuration information. Delegates directly to the generic version. 
		/// </summary>
		/// <param name="config">   The configuration used to guide construction of the failure handler. </param>
		/// <returns>   An instance of a failure handler. </returns>
		public IFailureHandler Construct(IFailureHandlerConfiguration config)
		{
			return Construct((T)config);
		}
		#endregion
	}
}