using api.Models;
using api.Models.Jwts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Extensions;

namespace api.Services.Auths
{
    public interface ITokenService
    {
        string GenerateToken(User user, bool isAccessToken);
        ClaimsPrincipal ValidateToken(string token, CancellationToken ct = default);
        bool DeleteToken(string token, CancellationToken ct = default);
        //Task UpdateRefreshTokenAsync(object id, string newRefreshToken);
    }
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
            var jwtKey = EnvironmentVariables.JwtKey;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }
        //<summary>
        /// Creates a JWT token for the given user.
        /// <param name="user">The user for whom the token is being created.</param>
        /// <param name="isAccessToken">Indicates whether the token is an access token or a refresh token.</param>
        /// <returns>The generated JWT token as a string.</returns>
        public string GenerateToken(User user, bool isAccessToken)
        {

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            };

            DateTime expires;

            if (isAccessToken)
            {
                var accessToken = new AccessToken
                (
                   user.Id.ToString(),
                   DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                   user.Role.Name
                );

                expires = accessToken.ExpirationTime;
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, accessToken.TokenId));
                claims.Add(new Claim(ClaimTypes.Role, accessToken.RoleName));
            }
            else
            {
                var refreshToken = new RefreshToken
                (
                     user.Id.ToString(),
                     DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays)
                );
                expires = refreshToken.ExpirationTime;
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, refreshToken.TokenId));
            }



            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = creds,
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public bool DeleteToken(string token, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public ClaimsPrincipal ValidateToken(string token, CancellationToken ct = default)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Hàm này sẽ ném ra lỗi nếu Token hết hạn hoặc chữ ký sai
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                // Log lại nếu cần biết là token đã hết hạn
                return null;
            }
            catch
            {
                return null; // Các lỗi kỹ thuật khác (sai chữ ký, sai định dạng)
            }
        }
    }
}
