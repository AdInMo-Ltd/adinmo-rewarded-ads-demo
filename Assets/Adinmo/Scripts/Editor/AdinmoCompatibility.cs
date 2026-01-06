using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace Adinmo
{
    public class AdinmoCompatibility : MonoBehaviour
    {
        private static string PURCHASE_SYMBOL_V4 = "ADINMO_UNITY_STORE_V4";
        private static readonly string PURCHASE_SYMBOL_V5 = "ADINMO_UNITY_STORE_V5";
        // Prevent dead-stripping
        public void KeepStrippedSymbols()
        {
            UnityWebRequest r = new();
#if UNITY_2017_2_OR_NEWER
            r.SendWebRequest();
#else
            r.Send();
#endif
        }


        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string path)
        {

            string destination;
            if (Application.platform == RuntimePlatform.WindowsEditor) //If on windows
                destination = Path.GetDirectoryName(path) + @"\" + PlayerSettings.productName + "_Data" + @"\Adinmo\Plugins\Cef";
            else //If on mac
                destination = Path.GetDirectoryName(path) + @"/" + PlayerSettings.productName + "_Data" + @"/Adinmo/Plugins/Cef";

            if (target == BuildTarget.StandaloneWindows64) //Deploy Cef for Windows 64
            {
                //If a adinmo plugins folder doesnt exist
                if (!Directory.Exists(destination))
                    Directory.CreateDirectory(destination);

                string character = @"\";
                if (Application.platform != RuntimePlatform.WindowsEditor)
                    character = "/";

                string[] files;
                if (Application.platform == RuntimePlatform.WindowsEditor) //If on Windows
                    files = Directory.GetFiles(Application.dataPath + @"\Adinmo\Plugins\Cef Windows x64");
                else //If on mac
                    files = Directory.GetFiles(Application.dataPath + @"/Adinmo/Plugins/Cef Windows x64");

                foreach (string file in files)
                {
                    if (!file.Contains("meta") && !file.Contains("debug"))
                    {

                        if (!File.Exists(destination + @"\" + Path.GetFileName(file)))
                        {
                            if (Application.platform == RuntimePlatform.WindowsEditor)
                                FileUtil.CopyFileOrDirectory(file, destination + character + Path.GetFileName(file));
                            else
                                FileUtil.CopyFileOrDirectory(file, destination + character + Path.GetFileName(file));
                        }
                    }
                }

                if (Application.platform == RuntimePlatform.WindowsEditor)
                    Directory.CreateDirectory(destination + @"\locales");
                else
                    Directory.CreateDirectory(destination + @"/locales");

                if (Application.platform == RuntimePlatform.WindowsEditor) //If on Windows
                    files = Directory.GetFiles(Application.dataPath + @"\Adinmo\Plugins\Cef Windows x64\locales");
                else //If on mac
                    files = Directory.GetFiles(Application.dataPath + @"/Adinmo/Plugins/Cef Windows x64/locales");

                foreach (string file in files)
                {
                    if (!file.Contains("meta"))
                    {
                        if (Application.platform == RuntimePlatform.WindowsEditor)
                        {
                            if (!File.Exists(destination + @"\locales\" + Path.GetFileName(file)))
                                FileUtil.CopyFileOrDirectory(file, destination + @"\locales\" + Path.GetFileName(file));
                        }
                        else
                        {
                            if (!File.Exists(destination + @"/locales/" + Path.GetFileName(file)))
                                FileUtil.CopyFileOrDirectory(file, destination + @"/locales/" + Path.GetFileName(file));
                        }
                    }
                }
            }
            else if (target != BuildTarget.Android && target != BuildTarget.iOS && target != BuildTarget.WebGL) //If not on Android or iOS which dont need any additional deployment
                Debug.LogError("AdInMo does not support this build target:" + target + ".");



            if (target == BuildTarget.iOS)
            {
                AddPListValues(path);
            }

        }


        // Implement a function to read and write values to the plist file:
        static void AddPListValues(string pathToXcode)
        {
#if UNITY_IOS
        
            string iosCode = AdinmoEditorCommon.GetiOSCode();
            if (!string.IsNullOrEmpty(iosCode))
            {
                // Retrieve the plist file from the Xcode project directory:
                string plistPath = pathToXcode + "/Info.plist";
                PlistDocument plistObj = new PlistDocument();


                // Read the values from the plist file:
                plistObj.ReadFromString(File.ReadAllText(plistPath));

                // Set values from the root object:
                PlistElementDict plistRoot = plistObj.root;




                // Set the description key-value in the plist:
                plistRoot.SetString("GADApplicationIdentifier", iosCode);

                // Save changes to the plist:
                File.WriteAllText(plistPath, plistObj.WriteToString());
            }

#endif
        }


        [InitializeOnLoadMethod]
        private static void SetupUnityStore()
        {
            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
                Thread.Sleep(100);

            if (listRequest.Error != null)
            {
                Debug.Log("Error: " + listRequest.Error.message);
                return;
            }

            var packages = listRequest.Result;
            string version=null;
            foreach (var package in packages)
            {
                if (package.source == PackageSource.Registry)
                {
                    if (package.name == "com.unity.purchasing" )
                    {
                        int versionNo = GetVersionNo(package.version);
                        if (versionNo >= 5000000)
                        {
                            version = PURCHASE_SYMBOL_V5;
                        }
                        else if (versionNo >= 4000000)
                        {
                            version = PURCHASE_SYMBOL_V4;
                        }
                            
                    }
                }
            }
            NamedBuildTarget bt = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string symbols = PlayerSettings.GetScriptingDefineSymbols(bt);
            
            if (symbols.Contains(PURCHASE_SYMBOL_V4) && (version is null || version == PURCHASE_SYMBOL_V5))
            {
                RemoveDefine(PURCHASE_SYMBOL_V4);
            }
            if (symbols.Contains(PURCHASE_SYMBOL_V5) && (version is null || version == PURCHASE_SYMBOL_V4))
            {
                RemoveDefine(PURCHASE_SYMBOL_V5);
            }
            if(version !=null && !symbols.Contains(version))
                AddDefine(version);
        }

        static int GetVersionNo(string version)
        {
            string[] split=version.Split('.');

            int res = 0;
            if (split.Length > 0)
            {
                res = int.Parse(split[0]) * 1000000;
            }
            if (split.Length > 1)
            {
                res += int.Parse(split[1]) * 1000;
            }
            if (split.Length > 2)
            {
                res += int.Parse(split[2]);
            }
            return res;

        }

        public static void AddDefine(string def)
        {
            NamedBuildTarget bt = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string symbols = PlayerSettings.GetScriptingDefineSymbols(bt);

            StringBuilder sb = new();
            sb.Append(symbols);

            if (sb.Length == 0) sb.Append(def + ";");
            else sb.Append($"{(sb[^1] == ';' ? "" : ";")}{def}");

            PlayerSettings.SetScriptingDefineSymbols(bt, sb.ToString());
        }
        public static void RemoveDefine(string def)
        {
            NamedBuildTarget bt = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string symbols = PlayerSettings.GetScriptingDefineSymbols(bt);
            PlayerSettings.SetScriptingDefineSymbols(bt, symbols.Replace(def, ""));
        }
    }


    class AdinmoPreProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_IOS
            string iosCode = AdinmoEditorCommon.GetiOSCode();
            if(!string.IsNullOrEmpty(iosCode))
            {
                string symbols=PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
                if (string.IsNullOrEmpty(symbols))
                {
                    symbols = "ADINMO_UMP_IOS";
                }
                else if (!symbols.Contains("ADINMO_UMP_IOS"))
                {
                    symbols+=";ADINMO_UMP_IOS";
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, symbols);
            }
            else
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
                
                if(!string.IsNullOrEmpty(symbols) && symbols.Contains("ADINMO_UMP_IOS"))
                {
                    symbols = symbols.Replace(";ADINMO_UMP_IOS", "");
                    symbols = symbols.Replace("ADINMO_UMP_IOS", "");

                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, symbols);
            }
#endif
        }
    }
}

