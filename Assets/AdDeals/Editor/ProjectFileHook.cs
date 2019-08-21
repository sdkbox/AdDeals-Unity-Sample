using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
#if UNITY_5_6_OR_NEWER
using UnityEditor.Build;
#endif
using UnityEditor.Callbacks;
#if UNITY_IOS
    using UnityEditor.iOS.Xcode.Custom;
    using UnityEditor.iOS.Xcode.Custom.Extensions;
#endif

#if UNITY_5_6_OR_NEWER
#if UNITY_2018_1_OR_NEWER
class AdDealsCustomBuildProcessor : IPreprocessBuildWithReport
#else
class AdDealsCustomBuildProcessor : IPreprocessBuild
#endif
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
    {
        if (BuildTarget.WSAPlayer == report.summary.platform)
        {
            restoreSln(report.summary.outputPath);
            renameCsProject(report.summary.outputPath);
        }
    }
    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        if (BuildTarget.WSAPlayer == target)
        {
            restoreSln(path);
            renameCsProject(path);
        }
    }

    public void restoreSln(string path)
    {
        if (!Directory.Exists(Utils.PathCombine(path, "backup")))
        {
            return;
        }
        string slnFile = Utils.PathCombine(path, Application.productName + ".sln");
        if (File.Exists(slnFile))
        {
            File.Copy(slnFile, Utils.PathCombine(path, "backup", Application.productName + ".addeals.sln"), true);
        }
        string backupSln = Utils.PathCombine(path, "backup", Application.productName + ".sln");
        if (File.Exists(backupSln))
        {
            File.Copy(backupSln, slnFile, true);
        }
    }

    private void renameCsProject(string path)
    {
        string csFile = Utils.PathCombine(path, Application.productName, Application.productName + ".csproj");
        if (!File.Exists(csFile))
        {
            return;
        }
        string newCsFile = Utils.PathCombine(path, Application.productName, Application.productName + ".addeals.csproj");
        if (File.Exists(newCsFile))
        {
            File.Delete(newCsFile);
        }
        File.Move(csFile, newCsFile);
    }

}
#endif

[InitializeOnLoad]
public class ProjectFileHook
{
    [PostProcessBuildAttribute]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (BuildTarget.WSAPlayer == target)
        {
            //net/il2cpp_xaml/d3d_proj
            string projType = checkGenerateProjType(path);
            if (!projType.Contains("xaml"))
            {
                Debug.Log("ERROR! Must generate project with build type Xaml");
                return;
            }
            if (projType.Contains("proj"))
            {
                Debug.Log("ERROR! dont support debug project");
                return;
            }

            if (projType.Contains("il2cpp"))
            {
                if (fixSln(path))
                {
                    string addealsCsFile = Utils.PathCombine(path, Application.productName, Application.productName + ".addeals.csproj");
                    if (File.Exists(addealsCsFile))
                    {
                        // addeals cs file exist, so C# project files has copyed, just resotre this file
                        string csFile = Utils.PathCombine(path, Application.productName, Application.productName + ".csproj");
                        if (File.Exists(csFile))
                        {
                            File.Delete(csFile);
                        }
                        File.Move(addealsCsFile, csFile);
                    }
                    else
                    {
                        copyVSProjectFiles(Utils.PathCombine(Application.dataPath, "AdDeals", "Editor", "WSA", "UWPProjectTemplates.tar.gz"), path);
                    }
                }
            }
            else if (projType.Contains("net"))
            {
                string IL2CPPToDotNetBridgeFolder = Utils.PathCombine(path, "UUBridge");
                if (!Directory.Exists(IL2CPPToDotNetBridgeFolder))
                {
                    copyProjectFilesForNet(Utils.PathCombine(Application.dataPath, "AdDeals", "Editor", "WSA", "UWPProjectTemplates.tar.gz"), path);
                }

                patchCsProjForNetIf(path);
                patchMainPageForNetIf(path);

                string mainProj = Utils.PathCombine(path, Application.productName);
                bool hasAddAdDeals = false;
                string nugetCfg = mainProj + "/packages.config";
                if (System.IO.File.Exists(nugetCfg))
                { // add addealssdk to packages.config if packages.config exist
                    addAdDealsSDKWithXml(nugetCfg);
                    hasAddAdDeals = true;
                }
                nugetCfg = mainProj + "/project.json";
                if (System.IO.File.Exists(nugetCfg))
                { // add addealssdk to packages.config if project.json exist
                    addAdDealsSDKWithJson(nugetCfg);
                    hasAddAdDeals = true;
                }
                if (!hasAddAdDeals)
                { // both packages.config and project.json does not exist, create default config
#if UNITY_5
                    addAdDealsSDKWithXml(mainProj + "/packages.config");
#else
                    addAdDealsSDKWithJson(mainProj + "/project.json");
#endif
                }
            }
            else
            {
                Debug.Log("ERROR! unknow generate project type:" + projType);
            }
        } else if (BuildTarget.Android == target) {
            string buildGradle = path + "/" + Application.productName + "/build.gradle";
            fixBuildGradle(buildGradle);
        } else if (BuildTarget.iOS == target) {
            string pbxprojPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            fixPbxproject(pbxprojPath);
        }
    }

    private static void fixPbxproject(string path)
    {
#if UNITY_IOS

        UnityEditor.iOS.Xcode.PBXProject pbxProj = new UnityEditor.iOS.Xcode.PBXProject();
        pbxProj.ReadFromFile(path);

        string targetGuid = pbxProj.TargetGuidByName("Unity-iPhone");
        pbxProj.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");

        pbxProj.WriteToFile(path);

        AddDynamicFrameworksForUnity5(path);
#endif
    }

#if UNITY_IOS
    static void AddDynamicFrameworksForUnity5(string path)
    {
        UnityEditor.iOS.Xcode.Custom.PBXProject pbxProj = new UnityEditor.iOS.Xcode.Custom.PBXProject();
        pbxProj.ReadFromFile(path);

        string targetGuid = pbxProj.TargetGuidByName("Unity-iPhone");

        const string defaultLocationInProj = "Frameworks/AdDeals/Plugins/iOS";
        const string exampleFrameworkName = "AdDeals.framework";

        string framework = Utils.PathCombine(defaultLocationInProj, exampleFrameworkName);
        string fileGuid = pbxProj.AddFile(framework, "Frameworks/" + framework, PBXSourceTree.Sdk);
        PBXProjectExtensions.AddFileToEmbedFrameworks(pbxProj, targetGuid, fileGuid);
        pbxProj.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
        pbxProj.WriteToFile (path);
    }
#endif

    private static void fixBuildGradle(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            Debug.Log("ERROR! build.gradle not exist:" + path);
            return;
        }

        // remove follow lines:
        // compile(name: 'AdDeals', ext:'aar')
        // compile(name: 'AdDealsWrapper', ext:'aar')
        List<string> lines = new List<string>();
        foreach (string line in File.ReadAllLines(path))
        {
            if (line.Contains("compile(name: 'AdDeals', ext:'aar')")
                || line.Contains("compile(name: 'AdDealsWrapper', ext:'aar')")
                || line.Contains("implementation(name: 'AdDeals', ext:'aar')")
                || line.Contains("implementation(name: 'AdDealsWrapper', ext:'aar')")
                )
            {
                continue;
            }
            lines.Add(line);
        }
        File.WriteAllLines(path, lines.ToArray());
    }

    private static string checkGenerateProjType(string path)
    {
        string projType = "";
        if (null == path)
        {
            return projType;
        }
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (!dirInfo.Exists)
        {
            return projType;
        }

        if (File.Exists(Utils.PathCombine(path, Application.productName, Application.productName + ".csproj")))
        {
            projType += "net";
        }
        else if(Directory.Exists(Utils.PathCombine(path, "Il2CppOutputProject")))
        {
            projType += "il2cpp";
        }

        if (File.Exists(Utils.PathCombine(path, Application.productName, "App.xaml")))
        {
            projType += "_xaml";
        }
        else
        {
            projType += "_d3d";
        }

        foreach(DirectoryInfo folder in dirInfo.GetDirectories())
        {
            if ("GeneratedProjects" == folder.Name)
            {
                projType += "_proj";
                break;
            }
        }

        return projType;
    }

    private static bool fixSln(string path)
    {
        string slnFile = Utils.PathCombine(path, Application.productName + ".sln");
        if (!System.IO.File.Exists(slnFile))
        {
            Debug.Log("ERROR! can't find sln project file:" + slnFile);
            return false;
        }

        string[] readLines = File.ReadAllLines(slnFile);
        foreach (string line in readLines)
        {
            if (line.Contains("UUBridge"))
            {
                return true;
            }
        }

        string backupFolder = Utils.PathCombine(path, "backup");
        if (!Directory.Exists(backupFolder))
        {
            Directory.CreateDirectory(backupFolder);
        }
        //backup sln
        File.Copy(slnFile, Utils.PathCombine(path, "backup", Application.productName + ".sln"), true);

        int lastEndProjectPos = -1;
        List<string> lines = new List<string>();
        foreach (string line in readLines)
        {
            if (lastEndProjectPos >= 0)
            {
                lastEndProjectPos ++;
            }
            if (line.Contains(Application.productName + ".vcxproj"))
            {
                string s = string.Format(
                    "Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{0}\", \"{0}\\{0}.csproj\", \"{{2FABDF39-A3A3-4497-95BA-5AEE089EBF0F}}\"",
                    Application.productName);
                lines.Add(s);
                continue;
            }
            if (line.Equals("EndProject"))
            {
                lines.Add(line);
                lastEndProjectPos = 0;
                continue;
            }
            if (1 == lastEndProjectPos && line.Equals("Global"))
            {
                lines.Add("Project(\"{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}\") = \"UUBridge\", \"UUBridge\\UUBridge.vcxproj\", \"{e156c997-d189-438d-9293-d5865ba0991a}\"");
                lines.Add("EndProject");
                lines.Add(line);
                continue;
            }
            lines.Add(line);
        }
        File.WriteAllLines(slnFile, lines.ToArray());

        return true;
    }

    private static bool patchCsProjForNetIf(string path)
    {
        string csProjFile = Utils.PathCombine(path, Application.productName, Application.productName + ".csproj");
        if (!System.IO.File.Exists(csProjFile))
        {
            Debug.Log("ERROR! can't find csproj file:" + csProjFile);
            return false;
        }

        string csprojSource = File.ReadAllText(csProjFile);
        Match match1 = Regex.Match(csprojSource, @"\<AllowedReferenceRelatedFileExtensions>(.|\s)*<\/AllowedReferenceRelatedFileExtensions>");
        if (match1.Success)
        {
            csprojSource = csprojSource.Remove(match1.Index, match1.Length);
        }
        string[] readLines = Regex.Split(csprojSource, "\r\n|\r|\n");
        foreach (string line in readLines)
        {
            if (line.Contains("AdDealsWrapperUWPNative.cs"))
            {
                return true;
            }
        }

        string backupFolder = Utils.PathCombine(path, "backup");
        if (!Directory.Exists(backupFolder))
        {
            Directory.CreateDirectory(backupFolder);
        }
        //backup csproj
        File.Copy(csProjFile, Utils.PathCombine(path, "backup", Application.productName + ".csproj"), true);

        List<string> lines = new List<string>();
        foreach (string line in readLines)
        {
            if (line.Contains("App.xaml\""))
            {
                lines.Add("<Compile Include=\"AdDealsWrapperUWPNative.cs\" />");
                lines.Add(line);
                continue;
            }
            lines.Add(line);
        }
        File.WriteAllLines(csProjFile, lines.ToArray());

        return true;
    }

    private static bool patchMainPageForNetIf(string path)
    {
        string mainPage = Utils.PathCombine(path, Application.productName, "MainPage.xaml.cs");
        if (!System.IO.File.Exists(mainPage))
        {
            Debug.Log("ERROR! can't find ManiPage file:" + mainPage);
            return false;
        }
        string mainPageSource = File.ReadAllText(mainPage);
        string patch1 = "SetUWPBridge";
        string patchSource1 = @"
        UUBridge.BridgeBootstrapper.SetUWPBridge(new UWPBridge());
";
        int pos1 = -1;
        if (mainPageSource.Contains(patch1))
        {
            return true;
        }
        Match match1 = Regex.Match(mainPageSource, @"Window\.Current\.SizeChanged\s*\+=\s*onResizeHandler;\s+}");

        if (!match1.Success)
        {
            return false;
        }
        pos1 = match1.Index + match1.Length;

        int pos2 = mainPageSource.LastIndexOf('}');

        StringBuilder sb = new StringBuilder(mainPageSource.Length + 200);
        sb.Append(mainPageSource, 0, pos1);
        sb.Append(patchSource1);
        sb.Append(mainPageSource, pos1, pos2 - pos1);
        sb.Append(@"
    class UWPBridge : UUBridge.IUWPBridge
    {
        public void Send(string json)
        {
            AdDeals.AdDealsWrapperUWPNative.HandleEvent(json);
        }
    }

");
        sb.Append(mainPageSource, pos2, mainPageSource.Length - pos2);

        //backup mainPage
        string backupFolder = Utils.PathCombine(path, "backup");
        if (!Directory.Exists(backupFolder))
        {
            Directory.CreateDirectory(backupFolder);
        }
        File.Copy(mainPage, Utils.PathCombine(path, "backup", "MainPage.xaml.cs"), true);

        File.WriteAllText(mainPage, sb.ToString());

        return true;
    }

    private static void extractTarGz(string gzfile, string path)
    {
        string backupFolder = Utils.PathCombine(path, "backup");
        string backupVSFolder = Utils.PathCombine(backupFolder, "UWPProjectTemplates");
        if (Directory.Exists(backupVSFolder))
        {
            Directory.Delete(backupVSFolder, true);
        }
        Tar.ExtractTarGz(gzfile, backupFolder);
    }

    private static void copyUWPProjectTemplatesProject(string srcFolder, string dstPath)
    {
        var replaceDict = new Dictionary<string, string>();
        replaceDict.Add("__PH_ProductName__", Application.productName);
        replaceDict.Add("__PH_Namespace__", Application.productName.Replace(" ", "_"));
        replaceDict.Add("__PH_Version__", Application.version);
        replaceDict.Add("__PH_Company__", Application.companyName);

        foreach (string dirPath in Directory.GetDirectories(srcFolder, "*", SearchOption.AllDirectories))
        {
            string dstFolder = dirPath.Replace(srcFolder, dstPath);
            Debug.Log(dstFolder);
            dstFolder = applyReplace(dstFolder, replaceDict);
            if (!Directory.Exists(dstFolder))
            {
                Directory.CreateDirectory(dstFolder);
            }
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string sourceFile in Directory.GetFiles(srcFolder, "*.*", SearchOption.AllDirectories))
        {
            string dstFile = sourceFile.Replace(srcFolder, dstPath);
            dstFile = applyReplace(dstFile, replaceDict);
            string srcContent = File.ReadAllText(sourceFile);
            srcContent = applyReplace(srcContent, replaceDict);
            File.WriteAllText(dstFile, srcContent);
        }
    }

    private static void copyProjectFilesForNet(string gzfile, string path)
    {
        extractTarGz(gzfile, path);
        string backupFolder = Utils.PathCombine(path, "backup");
        string backupVSFolder = Utils.PathCombine(backupFolder, "UWPProjectTemplates");
        File.Copy(Utils.PathCombine(backupVSFolder, "__PH_ProductName__", "AdDealsWrapperUWPNative.cs"),
            Utils.PathCombine(path, Application.productName, "AdDealsWrapperUWPNative.cs"));
    }

    private static void copyVSProjectFiles(string gzfile, string path)
    {
        extractTarGz(gzfile, path);
        string backupFolder = Utils.PathCombine(path, "backup");
        string backupVSFolder = Utils.PathCombine(backupFolder, "UWPProjectTemplates");
        copyUWPProjectTemplatesProject(backupVSFolder, path);
    }

    private static string applyReplace(string content, Dictionary<string, string> replaceSymbols)
    {
        foreach (var k in replaceSymbols.Keys)
        {
            if (content.Contains(k))
            {
                content = content.Replace(k, replaceSymbols[k]);
            }
        }

        return content;
    }

    private static string searchFile(string path, string target)
    {
        if (null == path)
        {
            return null;
        }
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (!dirInfo.Exists)
        {
            return null;
        }
        foreach(FileInfo file in dirInfo.GetFiles())
        {
            if (file.Name.EndsWith(target))
            {
                return file.FullName;
            }
        }
        return null;
    }

    private static string searchFolder(string path, string target)
    {
        if (null == path)
        {
            return null;
        }
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (!dirInfo.Exists)
        {
            return null;
        }

        foreach(DirectoryInfo folder in dirInfo.GetDirectories())
        {
            if (target == folder.Name)
            {
                return folder.FullName;
            }
        }

        foreach(DirectoryInfo folder in dirInfo.GetDirectories())
        {
            string result = searchFolder(folder.FullName, target);
            if (null != result)
            {
                return result;
            }
        }
        return null;
    }

    private static void addMacroToVSProject(string projectPath, string macro)
    {
        if (!System.IO.File.Exists(projectPath))
        {
            Debug.Log("ERROR! Project file not exist:" + projectPath);
            return;
        }
        XmlDocument doc = new XmlDocument();
        doc.Load(projectPath);

        XmlNamespaceManager xnManager = new XmlNamespaceManager(doc.NameTable);
        xnManager.AddNamespace("t", "http://schemas.microsoft.com/developer/msbuild/2003");
        XmlNodeList nodeList = doc.SelectNodes("/t:Project/t:PropertyGroup/t:DefineConstants", xnManager);
        foreach (XmlNode item in nodeList)
        {
            if (!item.InnerText.Contains(macro))
            {
                item.InnerText += macro;
            }
        }
        doc.Save(projectPath);
    }

    private static void addAdDealsSDKWithXml(string projectJson)
    {
        if (!System.IO.File.Exists(projectJson))
        {
            string nugetCfgString ="<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
            +"<packages>\r\n"
            +"    <package id=\"AdDealsUniversalSDKW81\" version=\"4.6.0\" targetFramework=\"win81\" />\r\n"
            +"    <package id=\"Newtonsoft.Json\" version=\"9.0.1\" targetFramework=\"win81\" />\r\n"
            +"</packages>";
            System.IO.File.WriteAllText(projectJson, nugetCfgString);
            return;
        }

        XmlDocument doc = new XmlDocument();
        doc.Load(projectJson);

        bool hasAdDeals = false;
        bool hasNewtonsoftJson = false;
        XmlNode packagesNode = doc.SelectSingleNode("/packages");
        if (null == packagesNode)
        {
            packagesNode = doc.CreateNode(XmlNodeType.Element, "packages", null);
            if (null == doc.DocumentElement)
            {
                doc.AppendChild(packagesNode);
            }
            else
            {
                doc.DocumentElement.AppendChild(packagesNode);
            }
        }
        foreach (XmlNode item in packagesNode.ChildNodes)
        {
            foreach(XmlAttribute attr in item.Attributes)
            {
                if (attr.LocalName == "id" && attr.InnerText == "AdDealsUniversalSDKW81")
                {
                    hasAdDeals = true;
                }
                else if (attr.LocalName == "id" && attr.InnerText == "Newtonsoft.Json")
                {
                    hasNewtonsoftJson = true;
                }
            }
        }
        if (!hasAdDeals)
        {
            XmlNode packageNode = doc.CreateNode(XmlNodeType.Element, "package", null);

            XmlNode attr = doc.CreateNode(XmlNodeType.Attribute, "id", null);
            attr.Value = "AdDealsUniversalSDKW81";
            packageNode.Attributes.SetNamedItem(attr);
            attr = doc.CreateNode(XmlNodeType.Attribute, "version", null);
            attr.Value = "4.6.0";
            packageNode.Attributes.SetNamedItem(attr);
            attr = doc.CreateNode(XmlNodeType.Attribute, "targetFramework", null);
            attr.Value = "win81";
            packageNode.Attributes.SetNamedItem(attr);

            packagesNode.AppendChild(packageNode);
        }
        if (!hasNewtonsoftJson)
        {
            XmlNode packageNode = doc.CreateNode(XmlNodeType.Element, "package", null);

            XmlNode attr = doc.CreateNode(XmlNodeType.Attribute, "id", null);
            attr.Value = "Newtonsoft.Json";
            packageNode.Attributes.SetNamedItem(attr);
            attr = doc.CreateNode(XmlNodeType.Attribute, "version", null);
            attr.Value = "9.0.1";
            packageNode.Attributes.SetNamedItem(attr);
            attr = doc.CreateNode(XmlNodeType.Attribute, "targetFramework", null);
            attr.Value = "win81";
            packageNode.Attributes.SetNamedItem(attr);

            packagesNode.AppendChild(packageNode);
        }
        doc.Save(projectJson);
    }

    private static void addAdDealsSDKWithJson(string projectJson)
    {
        if (!System.IO.File.Exists(projectJson))
        {
            Debug.Log("ERROR! Project nuget json not exist:" + projectJson);
            return;
        }

        string jsonString = System.IO.File.ReadAllText(projectJson);
        JSONObject j = new JSONObject(jsonString);
        JSONObject dep = j.GetField("dependencies");
        if (null != dep && dep.HasField("AdDealsUniversalSDKW81"))
        {
            return;
        }
        if (null == dep)
        {
            dep = new JSONObject();
            j.AddField("dependencies", dep);
        }
        dep.AddField("AdDealsUniversalSDKW81", "4.6.0");
        string newJsonString = j.Print(true);
        System.IO.File.WriteAllText(projectJson, newJsonString);
    }

}


