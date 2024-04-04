using Gabbro_Secret_Manager.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class JweService
    {
        private readonly JweSettings _settings;
        private readonly SymmetricSecurityKey _encryptionKey;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly TokenValidationParameters _validationParameters;

        public JweService(IOptions<JweSettings> options)
        {
            _settings = options.Value;
            _encryptionKey = new(Convert.FromBase64String(_settings.EncryptionKey));
            _signingKey = new(Convert.FromBase64String(_settings.SigningKey));
            _validationParameters = new TokenValidationParameters
            {
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Audience,
                IssuerSigningKey = _signingKey,
                TokenDecryptionKey = _encryptionKey,
                ValidateLifetime = false
            };
        }

        public string CreateToken(Dictionary<string, string> claims)
        {
            var claimsList = claims.Select(claim => new Claim(claim.Key, claim.Value));

            var signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            var encryptingCredentials = new EncryptingCredentials(_encryptionKey, SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                SigningCredentials = signingCredentials,
                EncryptingCredentials = encryptingCredentials,
                Subject = new ClaimsIdentity(claimsList)
            };

            return new JsonWebTokenHandler
                {
                    SetDefaultTimesOnTokenCreation = false
                }.CreateToken(tokenDescriptor);
        }

        public async Task<bool> IsValid(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var result = await tokenHandler.ValidateTokenAsync(token, _validationParameters);
            return result.IsValid;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetClaims(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var result = await tokenHandler.ValidateTokenAsync(token, _validationParameters);
            return result.Claims.ToDictionary(kvp => kvp.Key, kvp => (string)kvp.Value);
        }
    }
}
