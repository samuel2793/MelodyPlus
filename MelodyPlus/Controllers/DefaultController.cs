using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using static MelodyPlus.Constants;

namespace AuthServer.Controllers
{

    [ApiController]
    public class DefaultController : ControllerBase
    {
        public const string CLIENT_CALLBACK_URL = "http://localhost:4002/auth";        
        public static readonly string AUTH_HEADER = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{CLIENT_ID}:{CLIENT_SECRET}"));
        public static readonly Uri SPOTIFY_ACCOUNTS_ENDPOINT = new Uri("https://accounts.spotify.com");
        private static readonly HttpClient client = new HttpClient();
        static DefaultController()
        {
            client.BaseAddress = SPOTIFY_ACCOUNTS_ENDPOINT;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", AUTH_HEADER);
        }
        [HttpGet]
        [Route("/")]
        public ActionResult Index()
        {
            var queryString = Request.QueryString;
            queryString = queryString.Add("client_id", CLIENT_ID);
            queryString = queryString.Add("redirect_uri", CLIENT_CALLBACK_URL);
            return Redirect($"{SPOTIFY_ACCOUNTS_ENDPOINT}authorize/{queryString.ToUriComponent()}");
        }
        private static byte[] GetKey()
        {
            return KeyDerivation.Pbkdf2(ENCRYPTION_SECRET, Encoding.UTF8.GetBytes(SALT).Take(16).ToArray(),prf: KeyDerivationPrf.HMACSHA512,iterationCount:10000,numBytesRequested:256/8);
        }
        private static byte[] GetIV()
        {
            return KeyDerivation.Pbkdf2(ENCRYPTION_SECRET, Encoding.UTF8.GetBytes(SALT).Take(16).ToArray(), prf: KeyDerivationPrf.HMACSHA512, iterationCount: 10000, numBytesRequested: 128/ 8);
        }
        [HttpPost]
        [Route("/")]
        public async Task<ActionResult<TokenData>> Swap([FromForm]TokenModel model)
        {
            switch (model.grant_type)
            {
                case "authorization_code":
                    {
                        var content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            ["grant_type"] = model.grant_type,
                            ["code"] = model.code,
                            ["redirect_uri"] = model.redirect_uri
                        }
                        );
                        var response = await client.PostAsync("/api/token", content);
                        if (response.IsSuccessStatusCode)
                        {
                            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenData>(await response.Content.ReadAsStringAsync());
                            RijndaelManaged rmCrypto = new RijndaelManaged();
                            rmCrypto.KeySize = 256;
                            byte[] clearBytes = Encoding.Unicode.GetBytes(data.refresh_token);
                            using (MemoryStream ms = new MemoryStream())
                            {
                                using (CryptoStream cs = new CryptoStream(ms, rmCrypto.CreateEncryptor(GetKey(),GetIV()), CryptoStreamMode.Write))
                                {
                                    cs.Write(clearBytes, 0, clearBytes.Length);
                                    cs.Close();
                                }
                                data.refresh_token = Convert.ToBase64String(ms.ToArray());
                            }
                            //MemoryStream stream = new MemoryStream();
                            //CryptoStream cryptStream = new CryptoStream(stream, rmCrypto.CreateEncryptor(GetKey(), GetIV()), CryptoStreamMode.Write);
                            //StreamWriter sWriter = new StreamWriter(cryptStream);
                            //sWriter.Write(data.refresh_token);
                            //sWriter.Flush();
                            //data.refresh_token = Convert.ToBase64String(stream.ToArray());
                            return data;
                        }
                        else
                        {
                            return StatusCode((int)response.StatusCode);
                        }
                    }
                case "refresh_token":
                    {
                        RijndaelManaged rmCrypto = new RijndaelManaged();
                        rmCrypto.KeySize = 256;
                        byte[] cipherBytes = Convert.FromBase64String(model.refresh_token);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, rmCrypto.CreateDecryptor(GetKey(),GetIV()), CryptoStreamMode.Write))
                            {
                                cs.Write(cipherBytes, 0, cipherBytes.Length);
                                cs.Close();
                            }
                            model.refresh_token = Encoding.Unicode.GetString(ms.ToArray());
                        }
                        var content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            ["grant_type"] = model.grant_type,
                            ["refresh_token"] = model.refresh_token
                        }
                       );
                        var response = await client.PostAsync("/api/token", content);
                        if (response.IsSuccessStatusCode)
                        {
                            return Newtonsoft.Json.JsonConvert.DeserializeObject<TokenData>(await response.Content.ReadAsStringAsync());
                        }
                        else
                        {
                            return StatusCode((int)response.StatusCode);
                        }

                    }
                default:
                    return BadRequest();
            }

        }
        public class TokenModel
        {
            
            public string grant_type { get; set; }
            
            public string refresh_token { get; set; }
            
            public string code { get; set; }
            
            public string redirect_uri => CLIENT_CALLBACK_URL;
        }

        public class TokenData
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
        }
    }

}