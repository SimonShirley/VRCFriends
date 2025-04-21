using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Friends;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;
using File = System.IO.File;

namespace VRCFriends.Business.Models
{
    public class FriendsModel : IFriendsModel
    {
        private readonly IFriendsApi _friendsApi;
        private readonly IInstancesApi _instancesApi;
        private readonly IStateMediator _stateMediator;

        private readonly string _storePath;

        public FriendsModel(IFriendsApi friendsApi, IStateMediator stateMediator, IInstancesApi instancesApi)
        {
            _friendsApi = friendsApi;
            _stateMediator = stateMediator;

            _storePath = Path.Combine(_stateMediator.AppDataPath, "images");
            _instancesApi = instancesApi;
        }

        public DateTime LastRefresh { get; private set; }

        public void RefreshFriendsList()
        {
            try
            {
                UpdateFriendsList(false);
                UpdateFriendsList(true);
            }
            catch (ApiException apiEx)
            {
                Debug.WriteLine(apiEx.Message);
                Debug.WriteLine(apiEx.ErrorCode);
                Debug.WriteLine(apiEx.ErrorContent);
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void UpdateFriendsList(bool offline)
        {
            IList<LimitedUserDto> friendDtoList = new List<LimitedUserDto>();

            var friendsList = _friendsApi.GetFriends(offline: offline);

            foreach (LimitedUser friend in friendsList)
                friendDtoList.Add(ConvertApiLimitedUserToDtoUser(friend));

            LastRefresh = DateTime.Now;

            _stateMediator.OnFriendsListUpdated(friendDtoList);
        }

        public int GetOnlineFriendsCount()
        {
            return _stateMediator.FriendsDictionary.Count(friend => friend.Value.OnlineStatus == OnlineStatusEnum.IsOnline);
        }

        private LimitedUserDto ConvertApiLimitedUserToDtoUser(LimitedUser user)
        {
            var avatarUrl = user.ProfilePicOverride;

            if (string.IsNullOrWhiteSpace(avatarUrl))
                avatarUrl = user.CurrentAvatarImageUrl;

            LimitedUserDto friend = new LimitedUserDto()
            {
                Id = user.Id,
                CurrentAvatarImage = GetAvatarImage(avatarUrl),
                DisplayName = user.DisplayName,
                Location = GetUserLocation(user),
                OnlineStatus = GetUserOnlineStatus(user),
                Status = user.Status
            };

            return friend;
        }

        private Bitmap GetAvatarImage(string currentAvatarImageUrl)
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

                webStream = httpClient.GetStreamAsync(currentAvatarImageUrl).Result;

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
                return Path.Combine(_stateMediator.AppDataPath, "images", string.Concat(checksum, ".png"));
            }
        }

        private InstanceDto GetUserLocation(LimitedUser user)
        {
            if (GetUserOnlineStatus(user) == OnlineStatusEnum.IsOnline)
            {
                if (user.Location.Equals("private", StringComparison.CurrentCultureIgnoreCase))
                    return new InstanceDto { WorldName = "In a Private World" };
                else
                {
                    var worldInstanceData = user.Location.Split('~');

                    if (worldInstanceData.Length > 0)
                    {
                        if (worldInstanceData[0].Equals("traveling", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return new InstanceDto { WorldName = "Travelling Between Worlds" };
                        }
                        else if (worldInstanceData[0].StartsWith("wrld_"))
                        {
                            var instanceWorldId = worldInstanceData[0].Split(':')[0];
                            var instanceData = user.Location.Substring(instanceWorldId.Length + 1);

                            var worldInstance = _instancesApi.GetInstance(instanceWorldId, instanceData);

                            if (worldInstance != null)
                            {
                                return new InstanceDto
                                {
                                    WorldName = worldInstance.World.Name,
                                    Region = worldInstance.Region,
                                    InstanceType = worldInstance.Type
                                };
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
