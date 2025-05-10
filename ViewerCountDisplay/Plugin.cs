using ViewerCountDisplay.Installers;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using System.Reflection;

namespace ViewerCountDisplay
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        private Harmony _harmony;
        public const string HARMONY_ID = "com.github.rynan4818.ViewerCountDisplay";
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        /// <summary>
        /// IPAによってプラグインが最初にロードされたときに呼び出される（ゲームが開始されたとき、またはプラグインが無効な状態で開始された場合は有効化されたときのいずれか）
        /// [Init]コンストラクタを使用するメソッドや、InitWithConfigなどの通常のメソッドの前に呼び出されるメソッド
        /// [Init]は1つのコンストラクタにのみ使用してください
        /// </summary>
        public void Init(IPALogger logger, Config conf, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            Log.Info("ViewerCountDisplay initialized.");
            this._harmony = new Harmony(HARMONY_ID);

            //BSIPAのConfigを使用する場合はコメントを外します
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");

            //使用するZenjectのインストーラーのコメントを外します
            zenjector.Install<ViewerCountDisplayAppInstaller>(Location.App);
            //zenjector.Install<ViewerCountDisplayMenuInstaller>(Location.Menu);
            zenjector.Install<ViewerCountDisplayPlayerInstaller>(Location.Player);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
            this._harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
            this._harmony.UnpatchSelf();
        }
    }
}
