using CatCore;
using CatCore.Models.Twitch.PubSub.Responses.VideoPlayback;
using CatCore.Services.Multiplexer;
using CatCore.Services.Twitch.Interfaces;
using CatCore.Models.Twitch.Helix.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

namespace ViewerCountDisplay.Models
{
    public class CatCoreManager : IInitializable, IDisposable
    {
        public CatCoreInstance _catInstance { get; private set; }
        public event Action<StreamUp> OnStreamUP;
        public event Action<StreamDown> OnStreamDown;
        public event Action<ViewCountUpdate> OnViewCountUpdated;
        private string _ownUserID;
        private ChatServiceMultiplexer _chatServiceMultiplexer;
        private ITwitchService _twitchPratformService;
        private ITwitchHelixApiService _twitchHelixApiService;
        private ITwitchPubSubServiceManager _twitchPubSubServiceManager;
        private ITwitchUserStateTrackerService _twitchUserStateTrackerService;
        private ITwitchChannelManagementService _twitchChannelManagementService;
        private bool disposedValue;

        public void Initialize()
        {
            this._catInstance = CatCoreInstance.Create();
            this._chatServiceMultiplexer = this._catInstance.RunAllServices();
            this._twitchPratformService = this._chatServiceMultiplexer.GetTwitchPlatformService();
            this._twitchHelixApiService = this._twitchPratformService.GetHelixApiService();
            this._twitchPubSubServiceManager = this._twitchPratformService.GetPubSubService();
            this._twitchPubSubServiceManager.OnViewCountUpdated += this.OnTwitchPubSubServiceManager_OnViewCountUpdated;
            this._twitchPubSubServiceManager.OnStreamUp += this.OnTwitchPubSubServiceManager_OnStreamUp;
            this._twitchPubSubServiceManager.OnStreamDown += this.OnTwitchPubSubServiceManager_OnStreamDown;
            this._twitchUserStateTrackerService = this._twitchPratformService.GetUserStateTrackerService();
            this._twitchChannelManagementService = this._twitchPratformService.GetChannelManagementService();
        }

        public void OnTwitchPubSubServiceManager_OnStreamUp(string channelId, StreamUp streamUP)
        {
            if (channelId != this.GetOwnUserID())
                return;
            this.OnStreamUP?.Invoke(streamUP);
        }
        public void OnTwitchPubSubServiceManager_OnStreamDown(string channelId, StreamDown streamDown)
        {
            if (channelId != this.GetOwnUserID())
                return;
            this.OnStreamDown?.Invoke(streamDown);
        }

        public void OnTwitchPubSubServiceManager_OnViewCountUpdated(string channelId, ViewCountUpdate viewerCount)
        {
            if (channelId != this.GetOwnUserID())
                return;
            this.OnViewCountUpdated?.Invoke(viewerCount);
        }

        public string GetOwnUserID()
        {
            if (this._ownUserID != null)
                return this._ownUserID;
            var ownChannel = this._twitchChannelManagementService.GetOwnChannel();
            if (ownChannel == null)
                return null;
            this._ownUserID = ownChannel.Id;
            return ownChannel.Id;
        }

        public string GetChannelIDToUserID(string channelID)
        {
            var userState = this._twitchUserStateTrackerService.GetUserState(channelID);
            if (userState == null)
                return null;
            return userState.UserId;
        }

        public async Task<Stream?> GetStream(string userID)
        {
            if (userID == null)
                return null;
            var stream = await this._twitchHelixApiService.GetStreams(new string[] { userID });
            if (stream == null)
                return null;
            if (!stream.HasValue)
                return null;
            var streamData = stream.Value.Data.FirstOrDefault();
            return streamData;
        }

        public async Task<string> GetUserNameToID(string userName)
        {
            var userInfo = await this._twitchHelixApiService.FetchUserInfo(null, new string[] { userName });
            if (userInfo == null)
                return null;
            if (!userInfo.HasValue)
                return null;
            var userData = userInfo.Value.Data.FirstOrDefault();
            return userData.UserId;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    this._twitchPubSubServiceManager.OnViewCountUpdated -= this.OnTwitchPubSubServiceManager_OnViewCountUpdated;
                    this._twitchPubSubServiceManager.OnStreamUp -= this.OnTwitchPubSubServiceManager_OnStreamUp;
                    this._twitchPubSubServiceManager.OnStreamDown -= this.OnTwitchPubSubServiceManager_OnStreamDown;
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~CatCoreManager()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
