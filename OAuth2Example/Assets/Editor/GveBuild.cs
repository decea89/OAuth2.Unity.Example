using UnityEngine;
 using System.Collections;
 using UnityEditor.Callbacks;
 using UnityEditor;
 using UnityEditor.iOS.Xcode;
 using System;
 using System.IO;
 using System.Linq;
//  using M.Common;
 using System.Collections.Generic;
 
 public class GveBuild {
 
     private const string NSCameraUsageDescription = "NSCameraUsageDescription";
	 private const string NSPhotoLibraryUsageDescription = "NSPhotoLibraryUsageDescription"; 
 
     [PostProcessBuildAttribute(1)]
     public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
         Debug.Log("GveBuild.OnPostprocessBuild " + target + " " + pathToBuiltProject);
         switch (target) {
             case BuildTarget.iOS:
                 string plistFilePath = pathToBuiltProject + Path.DirectorySeparatorChar + "Info.plist";
                 // 10/5/16 The code that uses Xcode is flagged as missing in editor, but it compiles when we return to Unity and works
                 // when build is run.
                 // Q: why doesn't this work? A: 2nd link says it is fixed in 5.4.2, we'll see
                 // https://issuetracker.unity3d.com/issues/ios-monodevelop-unityeditor-dot-ios-dot-xcode-namespace-isnt-recognised-in-monodevelop
                 // https://issuetracker.unity3d.com/issues/unity-does-not-include-unityeditor-dot-ios-dot-xcode-in-project-file
                 
                 // Read the existing plist file
                 try {
                     PlistDocument plist = new PlistDocument();
                     plist.ReadFromFile(plistFilePath);
                     Debug.Log("GveBuild.OnPostprocessBuild successfully read " + plistFilePath + ": " + ValueToString(plist.root));
 
                     // Add our modifications
                     if (plist.root.values.ContainsKey(NSCameraUsageDescription)) {
                         Debug.LogError("GveBuild.OnPostprocessBuild key already set? Do not overwrite: " + NSCameraUsageDescription + " = " + ValueToString(plist.root.values[NSCameraUsageDescription]));
                     } else {
                         plist.root.values[NSCameraUsageDescription] = new PlistElementString("Camera is not used by our application (API reference is due to Unity libraries)");
                         Debug.Log("GveBuild.OnPostprocessBuild added NSCameraUsageDescription = " + ValueToString(plist.root.values[NSCameraUsageDescription]));
                     }

					 if (plist.root.values.ContainsKey(NSPhotoLibraryUsageDescription)) {
                         Debug.LogError("GveBuild.OnPostprocessBuild key already set? Do not overwrite: " + NSPhotoLibraryUsageDescription + " = " + ValueToString(plist.root.values[NSPhotoLibraryUsageDescription]));
                     } else {
                         plist.root.values[NSPhotoLibraryUsageDescription] = new PlistElementString("PhotoLibrary is not used by our application (API reference is due to Unity libraries)");
                         Debug.Log("GveBuild.OnPostprocessBuild added NSPhotoLibraryUsageDescription = " + ValueToString(plist.root.values[NSPhotoLibraryUsageDescription]));
                     }
 
                     // Write the modified file
                     string plistPathNew = plistFilePath + ".new";
                     plist.WriteToFile(plistPathNew);
 
                     // Replace the original file. Note subsequent build steps modify the plist file further (e.g. currently facebook entries are added after this script runs)
                     if (true) {
                         // Delete the old file
                         File.Delete(plistFilePath);
                     } else {
                         // Keep the old file for diff while testing
                         string plistPathOld = plistFilePath + ".orig";
                         File.Move(plistFilePath, plistPathOld);
                     }
                     File.Move(plistPathNew, plistFilePath);
                     Debug.Log("GveBuild.OnPostprocessBuild successfully updated " + plistFilePath);
                 } catch (Exception e) {
                     Debug.LogError("GveBuild.OnPostprocessBuild PList error " + e);
                 }
                 break;
 
             default:
                 // nada
                 break;
         }
     }
 
     /// <summary>
     /// Utility to get value of a PlistElement as string
     /// </summary>
     private static string ValueToString(PlistElement element) {
         if (element is PlistElementString) {
             return ((PlistElementString)element).value;
         } else if (element is PlistElementInteger) {
             return ((PlistElementInteger)element).value.ToString();
         } else if (element is PlistElementBoolean) {
             return ((PlistElementBoolean)element).value.ToString();
        //  } else if (element is PlistElementDict) {
        //      Dictionary<string, PlistElement> values;
        //      return ((PlistElementDict)element).values.Select(x => x.Key + " : " + ValueToString(x.Value)).Join();
         } else {
             return element.ToString();
         }
     }
 }
