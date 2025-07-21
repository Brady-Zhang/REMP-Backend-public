using RealEstate.Domain;

namespace RealEstate.Service.JwtService
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(User user);
    }
}
