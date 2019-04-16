namespace Microsoft.Dynamics.CrmRestClient
{
	using System;

	[Serializable]
	public class LockableException : Exception
	{
		public LockableException(string message) : base(message)
		{
			//
		}

		public LockableException(string message, Exception innerException) : base(message, innerException)
		{
			//
		}
	}
}
