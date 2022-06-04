﻿using CafeApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static CafeApi.Models.Enum.UserEnum;

namespace CafeApi
{
    public class Method
    {
        static ApiContext _context = new ApiContext();

        private static string secret = JsonSerializer.Serialize(new { secret = Encrypt("MySecretKey") });

        public static string profilePath = Path.GetFullPath(Environment.CurrentDirectory + "/Resources/Profiles/");
        public static string thumbMenuPath = Path.GetFullPath(Environment.CurrentDirectory + "/Resources/Thumbnail/");
        public static string imgMenuPath = Path.GetFullPath(Environment.CurrentDirectory + "/Resources/ImageMenu/");

        private static string Sha(string s)
        {
            using (var t = SHA256.Create())
            {
                return string.Concat(t.ComputeHash(Encoding.UTF8.GetBytes(s)).Select(x => x.ToString("x2")));
            }
        }

        public static bool validNumber(string s, bool st = false)
        {
            if (st)
            {
                return double.TryParse(s, out double t);
            }
            else
            {
                return long.TryParse(s, out long t);
            }
        }

        public static string Encrypt(string s) => Sha(Sha(s));

        public static object error(ModelStateDictionary st) => new { errors = st.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).FirstOrDefault() };

        public static string Encode(User user, bool st)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);
            var descriptor = new SecurityTokenDescriptor();
            if (st)
            {
                descriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("slt", Encrypt(user.Id.ToString())) }),
                    Expires = DateTime.UtcNow.AddDays(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
            }
            else
            {
                descriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("slt", Encrypt(user.Id.ToString())) }),
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
            }
            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        public static Valid Decode(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secret);
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                }, out var result);
                var jwt = (JwtSecurityToken)result;
                var st = _context.Users.ToList();
                foreach (var i in st)
                {
                    if (Encrypt(i.Id.ToString()) == jwt.Claims.FirstOrDefault(s => s.Type == "slt").Value)
                    {
                        return new Valid { Id = i.Id, Role = (UserRole)i.Role, IsValid = true };
                    }
                }
                return new Valid { IsValid = false };
            }
            catch
            {
                return new Valid { IsValid = false };
            }
        }
    }

    public class Valid
    {
        public long? Id { get; set; }
        public UserRole? Role { get; set; }
        public bool IsValid { get; set; }
    }
}
