using System.Threading.Tasks;

namespace Microsoft.Dynamics.CrmRestClient
{
	public interface IAuthenticator
	{
		Task<string> GetToken(string resourceId, bool forceRefresh);

		bool IsTokenExpired(string resourceId);
	}
}
