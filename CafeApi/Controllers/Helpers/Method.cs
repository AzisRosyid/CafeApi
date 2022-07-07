using CafeApi.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using static CafeApi.Models.Enums.UserEnum;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using ImageMagick;
using static System.Net.Mime.MediaTypeNames;

namespace CafeApi.Controllers.Helpers
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

        public static string token(User user, bool st)
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

        public static Valid valid(string token)
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

        public File? saveImage(string path)
        {
            try
            {
                var info = new FileInfo(path);
                string copyPath = String.Empty;
                if (quality == ImageQuality.Medium) copyPath = $"{Method.imgBookPath}Medium/";
                if (quality == ImageQuality.Low) copyPath = $"{Method.imgBookPath}Low/";
                if (!Directory.Exists(copyPath)) Directory.CreateDirectory(copyPath);
                copyPath = $"{copyPath}{image}";
                if (!File.Exists(copyPath)) info.CopyTo(copyPath, true);
                else return File(File.ReadAllBytes(copyPath), "image/jpeg");
                var copy = new FileInfo(copyPath);
                ImageOptimizer opt = new ImageOptimizer();
                opt.LosslessCompress(copy);
                copy.Refresh();
                using (var img = new MagickImage(copy.FullName))
                {
                    if (quality == ImageQuality.Medium) img.Scale(img.Width / 2, 0);
                    else if (quality == ImageQuality.Low) img.Scale(img.Width / 3, 0);
                    img.Write(copy);
                }
                info = copy;
                return File(File.ReadAllBytes(info.FullName), "image/jpeg");
            }
            catch
            {
                return null;
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
