using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;
using System;
using MSPlayground.Core.Utils.Editor;

/// <summary>
/// Menu items to facilitate builds. The static functions can be called in command prompts.
/// Our flavour of the CreateStudio_Jenkins shared library calls BuildHololens in the UWP build pipeline.
/// This pipeline can build clean addressables player content as well.
/// 
/// Support build platforms:
///		- Hololens
///			- Handles the 2 step process of the Unity build to generate the Visual Studio project
///           and then the Visual Studio build to build the UWP app
///     - PC Standalone
/// </summary>
public static class CommandLine
{
    static string identityName = PlayerSettings.WSA.packageName;
    static string packageDisplayName = PlayerSettings.productName;
    static string certificatePublisher = PlayerSettings.WSA.certificateIssuer;
    static string oldVersionMajor = PlayerSettings.WSA.packageVersion.Major.ToString();
    static string oldVersionMinor = PlayerSettings.WSA.packageVersion.Minor.ToString();
    static string oldVersionBuild = PlayerSettings.WSA.packageVersion.Build.ToString();
    static string oldVersionRev = PlayerSettings.WSA.packageVersion.Revision.ToString();
    static string hololensBuildLocation = "Build/Hololens";
    static string standaloneBuildLocation = "Build/Standalone/";

    static string dateStamp = string.Format("{0}{1}", DateTime.UtcNow.Date.Month, DateTime.UtcNow.Date.Day);

    static string DefaultBundleVersionString =
        oldVersionMajor + "." + oldVersionMinor + "." + getBuildNum() + "." + oldVersionRev;

    static string OverrideVersionString = null;

    static string BundleVersionString
    {
        get
        {
            if (OverrideVersionString != null)
            {
                return OverrideVersionString;
            }

            return DefaultBundleVersionString;
        }
    }

    static string hololensBuildPath = Path.Combine(Application.dataPath, "../" + hololensBuildLocation);
    static string hololensLocalVersionPath = Path.Combine(Application.dataPath, "../hololensBuildVersion.txt");
    static string appxManifestPath = Path.Combine(hololensBuildPath, packageDisplayName + "/Package.appxmanifest");

    static string packDispVer
    {
        get => packageDisplayName + "_" + BundleVersionString;
    }

    // default for release
    static string appxDir = Path.Combine(hololensBuildPath, packageDisplayName + "/AppPackages/");

    static string appxPath
    {
        get => Path.Combine(hololensBuildPath, packageDisplayName + "/AppPackages/" + packDispVer + "_Test/");
    }

    static string appxPathMaster
    {
        get => Path.Combine(hololensBuildPath,
            packageDisplayName + "/AppPackages/" + packDispVer + "_Master_Test/");
    }

    static string appxDependenciesPath
    {
        get => Path.Combine(appxPath, $"Dependencies/{getUWPArchitecture()}");
    }

    static string appxDependenciesPathMaster
    {
        get => Path.Combine(appxPathMaster, $"Dependencies/{getUWPArchitecture()}");
    }

    // Check these locations for the first valid exeMS build path
    private static string[] exeMSBuildPaths = new[]
    {
        // This MSBuild executable path was obtained from the runBokkenBuild.groovy script of the createStudio_jenkins shared library
        "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\MSBuild\\Current\\Bin\\MSBuild.exe",
        "C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Msbuild\\Current\\Bin\\MSBuild.exe"
    };

    // values packed into a Text file and used in Jenkins build
    static string AppNameString = packageDisplayName;

    static string BundleShortVersionString = oldVersionMajor + "." + oldVersionMinor;
    static string AppNameStringTitle = "AppNameString";
    static string BundleVersionStringTitle = "BundleVersionString";
    static string BundleShortVersionStringTitle = "BundleShortVersionString";
    static string InfoTXTfileForJenkins = "ForJenkins.txt";

    // Use timestamp to generate the version string. Changing the version stops user data from being overwritten when
    // deploying to hololens.
    [MenuItem("Build/Hololens Auto Version Increment", false, 1)]
    public static void MenuBuildHololensTimestamped()
    {
        // Use the number of seconds so far today as the timestamp. There's a ~0.0001% chance of clashing with a
        // previous build but using more than 5 digits causes the build folder creation to fail.

        int timeStamp = (int) (DateTime.Now - DateTime.Today).TotalSeconds >> 1;

        int version = 1;

        try
        {
            Debug.Log("Version file path is " + hololensLocalVersionPath);
            string versionFile = File.ReadAllText(hololensLocalVersionPath);
            Debug.Log("Version file is " + versionFile);
            version = Int32.Parse(versionFile);
        }
        catch
        {
            Debug.Log("No version file found, starting with version 1");
        }

        try
        {
            // Store the next version in the file
            File.WriteAllText(hololensLocalVersionPath, (version + 1).ToString());
        }
        catch
        {
            Debug.LogWarning("Unable to write the local version file. Version increment will not work.");
        }

        OverrideVersionString = oldVersionMajor + "." + oldVersionMinor + "." + version + "." + oldVersionRev;

        BuildHololens();

        OverrideVersionString = null;
    }

    /// <summary>
    /// This function builds both addressables and hololens.
    /// It is for use in Editor, since the Jenkins pipeline requires the addressables and
    /// app build to be 2 separate steps to avoid race conditions.
    /// </summary>
    [MenuItem("Build/Hololens + Addressables", false, 1)]
    public static void MenuBuildHololensAndAddressables()
    {
        OverrideVersionString = null;
        string zipPath = Path.Combine(hololensBuildPath, "../Zip/" + packageDisplayName);
        BuildHololens(zipPath, true);
    }

    /// <summary>
    /// This function only builds hololens
    /// </summary>
    [MenuItem("Build/Hololens Only", false, 1)]
    public static void MenuBuildHololens()
    {
        OverrideVersionString = null;
        BuildHololens();
    }
    
    /// <summary>
    /// Standard build path.
    /// This function is called by the Jenkins library and only builds hololens
    /// (without addressables)
    /// </summary>
    public static void BuildHololens()
    {
        string zipPath = Path.Combine(hololensBuildPath, "../Zip/" + packageDisplayName);
        BuildHololens(zipPath);
    }

    public static void BuildHololens(string buildZipPath, bool buildAddressables = false)
    {
        Debug.Log($"===== MenuBuildHololens =====");

        Debug.Log($"Using version string: {OverrideVersionString}");

        try
        {
#if UNITY_2019_2_OR_NEWER
            List<string> listScenes = new List<string>();
            foreach (EditorBuildSettingsScene editorBuildScene in EditorBuildSettings.scenes)
            {
                if (editorBuildScene.enabled)
                {
                    listScenes.Add(editorBuildScene.path);
                }
            }

            EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;
            EditorUserBuildSettings.wsaSubtarget = WSASubtarget.HoloLens;
            EditorUserBuildSettings.wsaMinUWPSDK = "10.0.10240.0";
            EditorUserBuildSettings.wsaUWPSDK = "Latest installed";

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = listScenes.ToArray();
            buildPlayerOptions.locationPathName = hololensBuildLocation;
            buildPlayerOptions.target = BuildTarget.WSAPlayer;
            buildPlayerOptions.options = BuildOptions.None;

            WriteFileForJenkins();

            //regenerate the appxmanifest file if building on top of previous build
            if (File.Exists(appxManifestPath))
            {
                File.Delete(appxManifestPath);
            }
            //////////// Addressables
            Debug.Log($"Building Addressables: {buildAddressables}");
            if (buildAddressables)
            {
                // Addressables use the active build target so we have to switch
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
                Debug.Log($"[CommandLine] Active build target to build addressables for: {EditorUserBuildSettings.activeBuildTarget.ToString("G")}");
                
                // Force clean build of Addressables content
                BuildAddressables();
            }
            ////////////

            BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log($"BuildReport = {buildReport.summary.result.ToString()}");

            if (File.Exists(appxManifestPath))
            {
                Debug.Log("VS Project Built here: " + hololensBuildPath);
                PostBuildHololensNew(buildZipPath);
            }
            else
            {
                Debug.LogError("Build failed");
            }
#else
		Debug.LogError("Not implemented");
#endif
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat($"MenuBuildHololens exception\n{ex.Message}");
        }
    }


    /// <summary>
    /// Build PC standalone with addressables
    /// </summary>
    [MenuItem("Build/PC Standalone", false, 1)]
    public static void MenuBuildStandalone()
    {
        BuildStandalone(true);
    }
    
    
    public static void BuildStandalone(bool buildAddressables = false)
    {
        // Force clean build of Addressables content
        BuildAddressablesProcessor.PreExport();

        PlayerSettings.bundleVersion = BundleVersionString;

        try
        {
            List<string> listScenes = new List<string>();
            foreach (EditorBuildSettingsScene editorBuildScene in EditorBuildSettings.scenes)
            {
                if (editorBuildScene.enabled)
                {
                    listScenes.Add(editorBuildScene.path);
                }
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = listScenes.ToArray();
            buildPlayerOptions.locationPathName = standaloneBuildLocation + PlayerSettings.productName + ".exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;

            // temporarily hardcode this to non-develop build
            // this flag will be passed in from jenkins with ticket https://projectarcher.atlassian.net/browse/AR-1942
            //buildPlayerOptions.options = BuildOptions.Development;

            WriteFileForJenkins();
            
            //////////// Addressables
            Debug.Log($"Building Addressables: {buildAddressables}");
            if (buildAddressables)
            {
                // Addressables use the active build target so we have to switch
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                Debug.Log($"[CommandLine] Active build target to build addressables for: {EditorUserBuildSettings.activeBuildTarget.ToString("G")}");
                
                // Force clean build of Addressables content
                BuildAddressables();
            }
            ////////////

            BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log($"BuildReport = {buildReport.summary.result.ToString()}");
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat($"MenuBuildStandalone exception\n{ex.Message}");
        }
    }

    private static void WriteFileForJenkins()
    {
        System.IO.StreamWriter file = new System.IO.StreamWriter(InfoTXTfileForJenkins);
        file.WriteLine(AppNameStringTitle + "=" + AppNameString);
        file.WriteLine(BundleVersionStringTitle + "=" + BundleVersionString);
        file.WriteLine(BundleShortVersionStringTitle + "=" + BundleShortVersionString);
        file.Close();
    }

    // for projects generated by UNITY_2019
    private static void PostBuildHololensNew(string zipPath = null)
    {
        string identityVersionInit = "  <Identity Name=\"" + identityName + "\" Publisher=\"CN=" +
                                     certificatePublisher + "\" Version=\"" + oldVersionMajor + "." +
                                     oldVersionMinor + "." + oldVersionBuild + "." + oldVersionRev + "\" />";
        string identityVersionJenkins = "  <Identity Name=\"" + identityName + "\" Publisher=\"CN=" +
                                        certificatePublisher + "\" Version=\"" + BundleVersionString + "\" />";

        string platformConditionAnyCPU = "    <Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform>";
        string platformCondition =
            $"    <Platform Condition=\" '$(Platform)' == '' \">{getUWPArchitecture()}</Platform>";

        string[] vsProjPaths =
        {
            Path.Combine(hololensBuildPath, packageDisplayName + "/" + packageDisplayName + ".vcxproj"),
            Path.Combine(hololensBuildPath, "Il2CppOutputProject" + "/" + "Il2CppOutputProject.vcxproj"),
        };

        updateVSProjectFiles(appxManifestPath, identityVersionInit, identityVersionJenkins, false);

        foreach (string vsProjPath in vsProjPaths)
        {
            updateVSProjectFiles(vsProjPath, platformConditionAnyCPU, platformCondition, true);
        }

        Debug.Log("Manifest edited");

        CmdMSRestore();
        CmdMSBuild();
        moveAppx(zipPath);

#if UNITY_EDITOR
        EditorUtility.DisplayDialog("Hololens Build Done", appxPath, "OK");
#endif
        Debug.Log("Hololens Build Done: " + appxPath);
    }

    public static void CopyNugetPackages()
    {
        Debug.Log("============================== CopyNugetPackages ==============================");
        Debug.LogFormat("BuildPath = " + hololensBuildPath);
        Debug.LogFormat("Current = " + Directory.GetCurrentDirectory());
        CopyFilesRecursively(Path.Combine(Directory.GetCurrentDirectory(), "NugetPackages/packages"),
            Path.Combine(hololensBuildPath, "packages"));
    }

    public static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }

    public static void CmdMSRestore()
    {
        string configType = getVSConfigType();
        string uwpArchitecture = getUWPArchitecture();
        string argMSBuild = "\"" + hololensBuildPath + "\\" + packageDisplayName + ".sln\"" +
                            $" /t:restore /p:Platform={uwpArchitecture};Configuration=" + configType;
        ExecProcess("MSRESTORE", GetValidMSBuildExe(), argMSBuild);
    }

    public static void CmdMSBuild()
    {
        string configType = getVSConfigType();
        string uwpArchitecture = getUWPArchitecture();
        string argMSBuild = "\"" + hololensBuildPath + "\\" + packageDisplayName + ".sln\"" +
                            $" /fl /p:Platform={uwpArchitecture};AppxBundlePlatforms=\"{uwpArchitecture}\";UapAppxPackageBuildMode=SideloadOnly;AppxBundle=Always;Configuration=" +
                            configType + ";AppxPackageDir=\"" + appxDir + "\"";
        ExecProcess("MSBUILD", GetValidMSBuildExe(), argMSBuild);
    }

    /// <summary>
    /// Force clean build of Addressables content
    /// </summary>
    /// <remarks>
    /// This is called in the Jenkins pipeline as an individual bokken process step from the actual app build.
    /// Addressables use the active build target, so make sure to set your desired target before using this!
    /// </remarks>
    [MenuItem("Build/Addressables Only (Localization)", false, 2)]
    public static void BuildAddressables()
    {
        BuildAddressablesProcessor.PreExport();
    }

    private static void updateVSProjectFiles(string path, string oldValue, string newValue, bool exact)
    {
        if (File.Exists(path))
        {
            Debug.Log("updateVSProjectFiles - " + path + " - " + oldValue + " - " + newValue);
        }
        else
        {
            Debug.LogWarning("Warning - Could Not Update VSProject at Path: " + path);
            return;
        }

        string[] csprojLines = File.ReadAllLines(path);
        string trimmedOldValue = oldValue.Trim().ToLower();
        string trimmedCSProjFileLine = "";
        FileStream fs = File.Create(path);


        using (StreamWriter sw = new StreamWriter(fs))
        {
            trimmedCSProjFileLine = "";

            if (exact)
            {
                for (int i = 0; i < csprojLines.Length; i++)
                {
                    trimmedCSProjFileLine = csprojLines[i].Trim().ToLower();

                    if (trimmedCSProjFileLine == trimmedOldValue)
                    {
                        sw.WriteLine(newValue);
                    }
                    else
                    {
                        sw.WriteLine(csprojLines[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < csprojLines.Length; i++)
                {
                    trimmedCSProjFileLine = csprojLines[i].Trim().ToLower();

                    if (trimmedCSProjFileLine.Contains(trimmedOldValue))
                    {
                        sw.WriteLine(newValue);
                    }
                    else
                    {
                        sw.WriteLine(csprojLines[i]);
                    }
                }
            }
        }
    }

    private static string getVSConfigType()
    {
        return GetArgumentValue("-configType=", "Release");
    }

    private static string getUWPArchitecture()
    {
        string architecture = GetArgumentValue("-uwpArchitecture=", "ARM64");
        if (architecture != "ARM" && architecture != "ARM64")
        {
            Debug.LogError($"Invalid architecture {architecture}");
        }

        return architecture;
    }

    private static string getBuildNum()
    {
        return GetArgumentValue("-buildNum=", "0");
    }

    private static string GetArgumentValue(string argumentType, string defaultValue)
    {
        string returnValue = defaultValue;
        foreach (string CLArg in System.Environment.GetCommandLineArgs())
        {
            bool bFound = CLArg.Contains(argumentType);
            if (bFound)
            {
                returnValue = CLArg.Split('=')[1];
                Debug.Log("Found argument: " + argumentType + " is " + returnValue);
            }
        }

        return returnValue;
    }

    private static void moveAppx(string zipPath = null)
    {
        if (zipPath == null)
        {
            zipPath = Path.Combine(hololensBuildPath, "../Zip/" + packageDisplayName);
        }

        Debug.Log($"moveAppx zipPath = {zipPath}");

        // Clean Zip Path If Exists
        if (Directory.Exists(zipPath))
        {
            Directory.Delete(zipPath, true);
        }

        // Create New Zip Path
        Directory.CreateDirectory(zipPath + "/Dependencies");

        string configType = getVSConfigType();
        string uwpArchitecture = getUWPArchitecture();

        switch (configType)
        {
            case "Master":
                // special case for master
                MoveFile(Path.Combine(appxPathMaster, packDispVer + $"{uwpArchitecture}_Master.appxbundle"),
                    Path.Combine(zipPath, packDispVer + $"_{uwpArchitecture}_Master.appxbundle"));
                MoveFile(Path.Combine(appxDependenciesPathMaster, $"Microsoft.VCLibs.{uwpArchitecture}.14.00.appx"),
                    Path.Combine(zipPath, $"Dependencies/Microsoft.VCLibs.{uwpArchitecture}.14.00.appx"));
                break;
            case "Release":
            // release is the default
            default:
                MoveFile(Path.Combine(appxPath, packDispVer + $"_{uwpArchitecture}.appxbundle"),
                    Path.Combine(zipPath, packDispVer + $"_{uwpArchitecture}.appxbundle"));
                MoveFile(Path.Combine(appxPath, packDispVer + $"_{uwpArchitecture}.appxsym"),
                    Path.Combine(zipPath, packDispVer + $"_{uwpArchitecture}.appxsym"));
                MoveFile(Path.Combine(appxPath, packDispVer + $"_{uwpArchitecture}.cer"),
                    Path.Combine(zipPath, packDispVer + $"_{uwpArchitecture}.cer"));
                MoveFile(Path.Combine(appxPath, "Add-AppDevPackage.ps1"),
                    Path.Combine(zipPath, "Add-AppDevPackage.ps1"));
                MoveFile(Path.Combine(appxDependenciesPath, $"Microsoft.VCLibs.{uwpArchitecture}.14.00.appx"),
                    Path.Combine(zipPath, $"Dependencies/Microsoft.VCLibs.{uwpArchitecture}.14.00.appx"));
                break;
        }
    }

    static void MoveFile(string src, string dest)
    {
        Debug.Log($"MoveFile: {src} -> {dest}");

        if (File.Exists(src))
        {
            File.Move(src, dest);
        }
        else
        {
            Debug.LogError($"Src {src} does not exist");
            throw (new System.Exception($"MoveFile {src} Failed"));
        }
    }

    static void ExecProcess(string logHeader, string fileName, string arguments)
    {
        var proc = new System.Diagnostics.Process()
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        string output = "";
        Debug.Log($"{logHeader} Start {fileName} / {arguments}");
        proc.Start();
        while (!proc.StandardOutput.EndOfStream)
        {
            output += proc.StandardOutput.ReadLine() + "\n";
        }

        Debug.Log($"{logHeader} Done ==> \n {output}");
    }

    static string GetValidMSBuildExe()
    {
        foreach (var fileName in exeMSBuildPaths)
        {
            if (File.Exists(fileName))
            {
                return fileName;
            }
        }

        Debug.LogError($"No MSBuild.exe found at the specified paths.");
        return null;
    }
}