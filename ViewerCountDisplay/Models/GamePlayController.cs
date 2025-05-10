using System;
using Zenject;

namespace ViewerCountDisplay.Models
{
    public class GamePlayController : IInitializable, IDisposable
    {
        private GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private ViewerCountDisplayController _viewerCountDisplayController;
        private bool disposedValue;

        public GamePlayController(GameplayCoreSceneSetupData gameplayCoreSceneSetupData, ViewerCountDisplayController viewerCountDisplayController)
        {
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this._viewerCountDisplayController = viewerCountDisplayController;
        }

        public void Initialize()
        {
            var level = this._gameplayCoreSceneSetupData.difficultyBeatmap.level;
            this._viewerCountDisplayController._songName = level.songName;
            this._viewerCountDisplayController._levelAuthorName = level.levelAuthorName;
            this._viewerCountDisplayController._onPlayStart = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    this._viewerCountDisplayController._onPlayEnd = true;
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~GamePlayController()
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
