using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Friends;
using VRCFriends.Business.Models;
using VRChat.API.Client;
using VRChat.API.Model;
using File = System.IO.File;
using VRChat.API.Api;
using System.Threading.Tasks;

namespace VRCFriends.Business.Factories
{
    public class LimitedUserDtoFactory : ILimitedUserDtoFactory
    {
        private readonly string _storePath;
        private readonly IInstanceDtoFactory _instanceDtoFactory;
        private readonly IInstancesApi _instancesApi;

        public LimitedUserDtoFactory(IStateMediator stateMediator, IInstanceDtoFactory instanceDtoFactory, IInstancesApi instancesApi, string storePath = "")
        {
            _instanceDtoFactory = instanceDtoFactory;
            _instancesApi = instancesApi;

            if (string.IsNullOrWhiteSpace(storePath))
                storePath = Path.Combine(stateMediator.AppDataPath, "images");

            _storePath = storePath;
        }

        public LimitedUserDto ConvertToDto(LimitedUser user) => ConvertToDtoAsync(user).Result;

        public async Task<LimitedUserDto> ConvertToDtoAsync(LimitedUser user)
        {
            var avatarUrl = user.ProfilePicOverride;

            if (string.IsNullOrWhiteSpace(avatarUrl))
                avatarUrl = user.CurrentAvatarImageUrl;

            return new LimitedUserDto()
            {
                Id = user.Id,
                CurrentAvatarImage = await GetAvatarImageAsync(avatarUrl),
                DisplayName = user.DisplayName,
                Location = await GetUserLocation(user),
                OnlineStatus = GetUserOnlineStatus(user),
                Status = user.Status
            };
        }

        private async Task<Bitmap> GetAvatarImageAsync(string currentAvatarImageUrl)
        {
            HttpClient httpClient = null;
            Stream webStream = null;

            if (string.IsNullOrWhiteSpace(currentAvatarImageUrl))
                return null;

            try
            {
                if (!Directory.Exists(_storePath))
                    Directory.CreateDirectory(_storePath);

                string cachedFilename = GetCachedFilename(currentAvatarImageUrl);

                if (File.Exists(cachedFilename))
                    return new Bitmap(cachedFilename);

                var configuration = (Configuration)GlobalConfiguration.Instance;

                httpClient = new HttpClient();

                foreach (var header in configuration.DefaultHeaders)
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);

                httpClient.DefaultRequestHeaders.Add("User-Agent", configuration.UserAgent);

                webStream = await httpClient.GetStreamAsync(currentAvatarImageUrl);

                var bitmap = new Bitmap(webStream);
                bitmap?.Save(cachedFilename, ImageFormat.Png);

                webStream.Flush();
                webStream.Close();

                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex);
            }
            finally
            {
                webStream?.Dispose();
                httpClient?.Dispose();
            }

            return null;
        }

        private string GetCachedFilename(string currentAvatarImageUrl)
        {
            using (var hashAlgorithm = MD5.Create())
            {
                var hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(currentAvatarImageUrl));
                var checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return Path.Combine(_storePath, string.Concat(checksum, ".png"));
            }
        }

        private async Task<InstanceDto> GetUserLocation(LimitedUser user)
        {
            if (GetUserOnlineStatus(user) == OnlineStatusEnum.IsOnline)
            {
                if (user.Location.Equals("private", StringComparison.CurrentCultureIgnoreCase))
                    return _instanceDtoFactory.Create("In a Private World");
                else
                {
                    var worldInstanceData = user.Location.Split('~');

                    if (worldInstanceData.Length > 0)
                    {
                        if (worldInstanceData[0].Equals("traveling", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return _instanceDtoFactory.Create("Travelling Between Worlds");
                        }
                        else if (worldInstanceData[0].StartsWith("wrld_"))
                        {
                            var instanceWorldId = worldInstanceData[0].Split(':')[0];
                            var instanceData = user.Location.Substring(instanceWorldId.Length + 1);

                            var worldInstance = await _instancesApi.GetInstanceAsync(instanceWorldId, instanceData);

                            if (worldInstance != null)
                            {
                                return _instanceDtoFactory.Create(worldInstance.World.Name, worldInstance.Type, worldInstance.Region);
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static OnlineStatusEnum GetUserOnlineStatus(LimitedUser user)
        {
            if (user.Status != UserStatus.Offline && !user.Location.Equals("offline", StringComparison.CurrentCultureIgnoreCase))
                return OnlineStatusEnum.IsOnline;

            if (user.Status != UserStatus.Offline && user.Location.Equals("offline", StringComparison.CurrentCultureIgnoreCase))
                return OnlineStatusEnum.IsOnAnotherPlatform;

            return OnlineStatusEnum.IsOffline;
        }
    }
}
