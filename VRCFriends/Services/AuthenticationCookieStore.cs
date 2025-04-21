using System.Diagnostics;
using System.IO;
using System.Text.Json;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Models;
using VRChat.API.Client;

namespace VRCFriends.Services
{
    public class AuthenticationCookieStore : IAuthenticationCookieStore
    {
        private readonly string _storePath;
        private readonly string _defaultStorePath;

        private const string _storeFileName = "vrcfriends_data";
        private const string _storeFileNameExtension = ".json";

        private readonly object _lockObject = new() { };

        public AuthenticationCookieStore(string? storePath = "")
        {
            _defaultStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vrcfriends");

            if (string.IsNullOrWhiteSpace(storePath))
                storePath = _defaultStorePath;

            _storePath = storePath;
        }

        public bool LoadAuthenticationCookies(IReadableConfiguration configuration)
        {
            StreamReader? streamReader = null;

            if (!File.Exists(Path.Combine(_storePath, string.Concat(_storeFileName, _storeFileNameExtension))))
                return false;

            lock (_lockObject)
            {
                try
                {
                    streamReader = new StreamReader(Path.Combine(_storePath, string.Concat(_storeFileName, _storeFileNameExtension)));
                    string cookieDataFile = streamReader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(cookieDataFile))
                        return false;

                    var authCookieData = JsonSerializer.Deserialize<AuthenticationData>(cookieDataFile);

                    if (authCookieData is not null && authCookieData.AuthCookie is not null && authCookieData.TwoFactorCookie is not null)
                    {
                        // Cookies can be set by editing the config at the start of your program like this:
                        configuration.DefaultHeaders.Add("Cookie", $"auth={authCookieData.AuthCookie.Value}; twoFactorAuth={authCookieData.TwoFactorCookie.Value}");

                        ApiClient.CookieContainer.Add(authCookieData.AuthCookie);
                        ApiClient.CookieContainer.Add(authCookieData.TwoFactorCookie);

                        return true;
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex);
                }
                finally
                {
                    streamReader?.Dispose();
                }
            }

            return false;
        }

        public void SaveAuthenticationCookies()
        {
            StreamWriter? streamWriter = null;

            string tempFileName = Path.Combine(_storePath, string.Concat(_storeFileName, "_new", _storeFileNameExtension));
            string mainFileName = Path.Combine(_storePath, string.Concat(_storeFileName, _storeFileNameExtension));

            lock (_lockObject)
            {
                try
                {
                    if (!Directory.Exists(_storePath))
                        Directory.CreateDirectory(_storePath);

                    var configurationCookies = ApiClient.CookieContainer.GetAllCookies();
                    var authenticationData = new AuthenticationData
                    {
                        AuthCookie = configurationCookies.FirstOrDefault(cookie => cookie.Name == "auth"),
                        TwoFactorCookie = configurationCookies.FirstOrDefault(cookie => cookie.Name == "twoFactorAuth")
                    };

                    var serializedData = JsonSerializer.Serialize(authenticationData);

                    if (string.IsNullOrWhiteSpace(serializedData))
                        return;

                    streamWriter = new StreamWriter(Path.Combine(_storePath, string.Concat(_storeFileName, "_new", _storeFileNameExtension)), false);
                    streamWriter.Write(serializedData);
                    streamWriter.Flush();
                    streamWriter.Close();

                    File.Move(tempFileName, mainFileName, true);

                }
                catch (Exception ex)
                {
                    if (File.Exists(tempFileName))
                        File.Delete(tempFileName);

                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex);
                }
                finally
                {
                    streamWriter?.Dispose();
                }
            }
        }
    }
}
