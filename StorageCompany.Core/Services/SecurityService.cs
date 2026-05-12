using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Core.Services;

public class SecurityService() : ISecurityService
{
    public AuthResponse Login(AuthLoginRequest dto)
    {
        //var customer = 
        throw new NotImplementedException();
    }

    public AuthResponse Register(AuthRegisterRequest dto)
    {
        throw new NotImplementedException();
    }
    
    public string HashPassword(string password)
    {
        throw new NotImplementedException();
    }

    public void VerifyPasswordOrThrow(string password, string hashedPassword)
    {
        throw new NotImplementedException();
    }

    public string GenerateSalt()
    {
        throw new NotImplementedException();
    }

    public string GenerateJwt(JwtClaims claims)
    {
        throw new NotImplementedException();
    }
    
    public JwtClaims VerifyJwtOrThrow(string jwt)
    {
        throw new NotImplementedException();
    }
}