using System.Threading.Tasks;
using Foundation.Features.Ucp.Models;

namespace Foundation.Features.Ucp;

public interface IUcpCheckoutService
{
    Task<UcpCheckoutSessionResponse> CreateSessionAsync(CreateCheckoutSessionRequest request);
    Task<UcpCheckoutSessionResponse> GetSessionAsync(string sessionId);
    Task<UcpCheckoutSessionResponse> UpdateSessionAsync(string sessionId, UpdateCheckoutSessionRequest request);
    Task<UcpCheckoutSessionResponse> CompleteSessionAsync(string sessionId, CompleteCheckoutSessionRequest request);
    Task<UcpCheckoutSessionResponse> CancelSessionAsync(string sessionId);
}