using EnhancedStreamChat.Chat;
using BeatSaberMarkupLanguage.FloatingScreen;
using HarmonyLib;
using System;

namespace ViewerCountDisplay.HarmonyPatches
{
    [HarmonyPatch(typeof(ChatDisplay)), HarmonyPatch("SetupScreens", MethodType.Normal)]
    public class ChatDisplaySetupScreensPatch
    {
        public static event Action<FloatingScreen> OnSetupScreens;
        static void Postfix(FloatingScreen ____chatScreen)
        {
            OnSetupScreens?.Invoke(____chatScreen);
        }
    }
}
