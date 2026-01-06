using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;

[assembly: Preserve]
namespace Adinmo
{
    public class AdinmoNativeWrapper
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        [DllImport("__Internal")]
        [Preserve] 
        private static extern string GetUserAgentJS();
        public static string GetUA()
        {
            string userAgent = GetUserAgentJS();
            return userAgent;
        }
#endif
#if !UNITY_EDITOR && UNITY_ANDROID
        [DllImport("AdinmoAndroidWebPlugin")]
        [RuntimeInitializeOnLoadMethod]
        public static extern System.IntPtr updateTexture();
#endif
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        [Preserve]
        public static extern System.IntPtr AllocWebWrapper(System.IntPtr nativeTexturePointer, System.IntPtr webBitmap,int textureWidth, int textureHeight, int renderWidth, int renderHeight, string backgroundColour, int borderMode);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void ReleaseWebWrapper(System.IntPtr webWrapper);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void LoadUrl(System.IntPtr webWrapper, string url);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void LoadHtml(System.IntPtr webWrapper, string html);

        [DllImport("__Internal")]
        [Preserve]
        public static extern bool GetFrame(System.IntPtr webWrapper);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void RequestUserAgent(string gameObjectName, string callbackMethod);

        [DllImport("__Internal")]
        [Preserve]
        public static extern bool CalculateHash(System.IntPtr webWrapper);

        [DllImport("__Internal")]
        [Preserve]
        public static extern bool GetNavigationComplete(System.IntPtr webWrapper);

        [DllImport("__Internal")]
        [Preserve]
        public static extern bool UpToDate(System.IntPtr webWrapper);

        [DllImport("__Internal")]
        [Preserve]
        public static extern bool GetContentHasLoaded(System.IntPtr webWrapper);

        [DllImport("__Internal")]
        [Preserve]
        public static extern System.IntPtr AllocWebBitmap(string guid, int width, int height, string adinmoManagerName);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void ReleaseWebBitmap(System.IntPtr webWrapper);

        [DllImport("__Internal")]
        [Preserve]
        public static extern bool CopyWebpage(System.IntPtr webBitmap);

        [DllImport("__Internal")]
        [Preserve]
        public static extern System.IntPtr AllocAudioWrapper();

        [DllImport("__Internal")]
        [Preserve]
        public static extern void DownloadAudio(System.IntPtr ptr,string url,float timeout);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void PlayAudio(System.IntPtr ptr);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void StopAudio(System.IntPtr ptr);

        [DllImport("__Internal")]
        [Preserve]
        public static extern bool IsPlayingAudio(System.IntPtr ptr);

        [DllImport("__Internal")]
        [Preserve]
        public static extern float TimeAudio(System.IntPtr ptr);

        [DllImport("__Internal")]
        [Preserve]
        public static extern int GetStateAudio(System.IntPtr ptr);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void ReleaseAudioWrapper(System.IntPtr ptr);

        [DllImport("__Internal")]
        [Preserve]
        public static extern int GetCurrentVolume();

        [DllImport("__Internal")]
        [Preserve]
        public static extern void GetConsentString(byte[] byteArray);

        [DllImport ("__Internal")]
	    [Preserve]
        public static extern void StartListeningForMuteChanges();

	    [DllImport ("__Internal")]
        [Preserve]
	    public static extern void StopListeningForMuteChanges();

	    [DllImport ("__Internal")]
	    [Preserve]
        public static extern bool IsMuted();

        [DllImport("__Internal")]
        [Preserve]
        public static extern void LoadWebpageOnOverlay(string url, string adinmoManagerName, string domains);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void ReleaseWebOverlay();

        [DllImport("__Internal")]
        [Preserve]
        public static extern System.IntPtr LoadWebpageOnMagnifier(string url, string adinmoManagerName, float xpos, float ypos, float width, float height, float contentWidth, float contentHeight, int screenWidth, int screenHeight, string installerDomains);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void ReleaseWebMagnifier(System.IntPtr webMagnifier);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void OnTouch(System.IntPtr webWrapper,float x, float y, int touchPhase);



        [DllImport("__Internal")]
        [Preserve]
        public static extern bool CreateSKOverlay(string appID, string adinmoManagerName);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void RemoveSKOverlay();

        [DllImport("__Internal")]
        [Preserve]
        public static extern void OpenURL(string adinmoManagerName,string url);



#if ADINMO_UMP_IOS
        [DllImport("__Internal")]
        [Preserve]
        public static extern void loadUMP(string adinmoManagerName, string adModTestDeviceId, bool reset);

        [DllImport("__Internal")]
        [Preserve]
        public static extern void resetUMP();

#endif
#endif
    }
}