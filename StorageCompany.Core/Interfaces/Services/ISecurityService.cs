using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface ISecurityService
{
        public string HashPassword(string password);
        
        public void VerifyPasswordOrThrow(string password, string hashedPassword);
        
        public string GenerateSalt();
        
        public string GenerateJwt(JwtClaims claims);
       
        public AuthResponse Login(AuthLoginRequest dto);
        
        public AuthResponse Register(AuthRegisterRequest dto);
        
        public JwtClaims VerifyJwtOrThrow(string jwt);
}