using System;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using HMUI;
using ViewerCountDisplay.HarmonyPatches;
using BeatSaberMarkupLanguage.FloatingScreen;
using CatCore.Models.Twitch.PubSub.Responses.VideoPlayback;

namespace ViewerCountDisplay.Models
{
    public class ViewerCountDisplayController : IInitializable, ITickable, IDisposable
    {
        private CatCoreManager _catCoreManager;
        public static readonly Vector2 CanvasSize = new Vector2(30, 30);
        public static readonly Vector3 CanvasScale = new Vector3(1f, 1f, 1f);
        public static readonly Vector3 CanvasPosition = new Vector3(80f, -55f, 0);
        public static readonly Vector3 CanvasRotation = new Vector3(0, 0, 0);
        public static readonly int StreamUpCheckInterval = 10;
        public CurvedTextMeshPro _viewerCountTextMesh;
        public CurvedTextMeshPro _onStreamTimeTextMesh;
        private bool disposedValue;
        private bool _init;
        public float _currentCycleTime = 0f;
        public uint _viewCount;
        public DateTimeOffset _startTime;
        public bool _onStream;
        public bool _onStreamUP;
        public int _onStreamUpCheckInterval = StreamUpCheckInterval;

        public ViewerCountDisplayController(CatCoreManager catCoreManager)
        {
            this._catCoreManager = catCoreManager;
        }
        public void Initialize()
        {
            ChatDisplaySetupScreensPatch.OnSetupScreens += this.OnSetupScreens;
            this._catCoreManager.OnStreamUP += this.OnStreamUP;
            this._catCoreManager.OnStreamDown += this.OnStreamDown;
            this._catCoreManager.OnViewCountUpdated += this.OnViewCountUpdated;
        }

        public void Tick()
        {
            if (this._currentCycleTime < 1f)
            {
                this._currentCycleTime += Time.deltaTime;
                return;
            }
            this._currentCycleTime = 0f;
            if (!this._init)
            {
                if (this._catCoreManager.GetOwnUserID() != null)
                {
                    this._init = true;
                    _ = this.GetStreamData();
                }
            }
            if (!this._init || this._viewerCountTextMesh == null)
                return;
            if (this._onStreamUP)
            {
                if (this._onStreamUpCheckInterval <= 0)
                    _ = this.GetStreamData();
                this._onStreamUpCheckInterval--;
            }
            if (this._onStream)
            {
                this._viewerCountTextMesh.text = this._viewCount.ToString();
                this._onStreamTimeTextMesh.text = (DateTime.Now - this._startTime).ToString(@"hh\:mm\:ss");
            }
            else
            {
                this._viewerCountTextMesh.text = "";
                this._onStreamTimeTextMesh.text = "";
            }
        }

        public void OnSetupScreens(FloatingScreen chatScreen)
        {
            this.NewCanvasCreate("ViewerCountDisplay", chatScreen.transform);
        }

        public void OnStreamUP(StreamUp streamUp)
        {
            this._onStreamUP = true;
        }

        public void OnStreamDown(StreamDown streamDown)
        {
            this._onStream = false;
        }

        public void OnViewCountUpdated(ViewCountUpdate viewerCount)
        {
            this._viewCount = viewerCount.Viewers;
            this.ViewCountDisplayUpdate();
        }

        public async Task GetStreamData()
        {
            this._onStreamUpCheckInterval = StreamUpCheckInterval;
            var stream = await this._catCoreManager.GetStream(this._catCoreManager.GetOwnUserID());
            if (stream == null || !stream.HasValue || stream.Value.UserId != this._catCoreManager.GetOwnUserID())
            {
                this._onStream = false;
                return;
            }
            this._onStream = true;
            this._viewCount = stream.Value.ViewerCount;
            this._startTime = stream.Value.StartedAt;
            this._onStreamUP = false;
        }

        public void ViewCountDisplayUpdate()
        {
            if (this._viewerCountTextMesh == null)
                return;
            this._viewerCountTextMesh.text = this._viewCount.ToString();
        }

        public GameObject NewCanvasCreate(string canvasName, Transform parent)
        {
            var rootObject = new GameObject(canvasName, typeof(Canvas), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            rootObject.transform.parent = parent;
            var sizeFitter = rootObject.GetComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var canvas = rootObject.GetComponent<Canvas>();
            canvas.sortingOrder = 3;
            canvas.renderMode = RenderMode.WorldSpace;
            var rectTransform = canvas.transform as RectTransform;
            rectTransform.sizeDelta = CanvasSize;
            rootObject.transform.localPosition = CanvasPosition;
            rootObject.transform.localEulerAngles = CanvasRotation; ;
            rootObject.transform.localScale = CanvasScale;
            this._viewerCountTextMesh = this.CreateText(canvas.transform as RectTransform, string.Empty, new Vector2(10, 31));
            rectTransform = this._viewerCountTextMesh.transform as RectTransform;
            rectTransform.SetParent(canvas.transform, false);
            rectTransform.anchoredPosition = Vector2.zero;
            this._viewerCountTextMesh.fontSize = 12;
            this._viewerCountTextMesh.color = Color.white;
            this._viewerCountTextMesh.text = "-";
            this._onStreamTimeTextMesh = this.CreateText(canvas.transform as RectTransform, string.Empty, new Vector2(10, 31));
            rectTransform = this._onStreamTimeTextMesh.transform as RectTransform;
            rectTransform.SetParent(canvas.transform, false);
            rectTransform.anchoredPosition = Vector2.zero;
            this._onStreamTimeTextMesh.fontSize = 8;
            this._onStreamTimeTextMesh.color = Color.white;
            this._onStreamTimeTextMesh.text = "-";
            return rootObject;
        }

        private CurvedTextMeshPro CreateText(RectTransform parent, string text, Vector2 anchoredPosition)
        {
            return this.CreateText(parent, text, anchoredPosition, new Vector2(0, 0));
        }
        private CurvedTextMeshPro CreateText(RectTransform parent, string text, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var gameObj = new GameObject("CustomUIText");
            gameObj.SetActive(false);

            var textMesh = gameObj.AddComponent<CurvedTextMeshPro>();
            textMesh.rectTransform.SetParent(parent, false);
            textMesh.text = text;
            textMesh.fontSize = 4;
            textMesh.overrideColorTags = true;
            textMesh.color = Color.white;
            textMesh.rectTransform.anchorMin = new Vector2(0f, 0f);
            textMesh.rectTransform.anchorMax = new Vector2(0f, 0f);
            textMesh.rectTransform.sizeDelta = sizeDelta;
            textMesh.rectTransform.anchoredPosition = anchoredPosition;

            gameObj.SetActive(true);
            return textMesh;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    ChatDisplaySetupScreensPatch.OnSetupScreens -= this.OnSetupScreens;
                    this._catCoreManager.OnStreamUP -= this.OnStreamUP;
                    this._catCoreManager.OnStreamDown -= this.OnStreamDown;
                    this._catCoreManager.OnViewCountUpdated -= this.OnViewCountUpdated;
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~ViewerCountDisplayController()
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
