using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
    using UnityEditor.iOS.Xcode.Custom;
    using UnityEditor.iOS.Xcode.Custom.Extensions;
#endif

[InitializeOnLoad]
public class ProjectFileHook
{
    [PostProcessBuildAttribute]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (BuildTarget.WSAPlayer == target)
        {
            string csProjDir = searchFolder(path + "/GeneratedProjects", "Assembly-CSharp");
            if (null == csProjDir)
            {
                Debug.Log("ERROR! can't find CS project dir");
                return;
            }
            string csProjFile = searchFile(csProjDir, ".csproj");
            if (null == csProjFile)
            {
                Debug.Log("ERROR! can't find CS project file");
                return;
            }
            addMacroToVSProject(csProjFile, "ENABLE_ADDEALS_UWP;");

            bool hasAddAdDeals = false;
            string nugetCfg = csProjDir + "/packages.config";
            if (System.IO.File.Exists(nugetCfg))
            { // add addealssdk to packages.config if packages.config exist
                addAdDealsSDKWithXml(nugetCfg);
                hasAddAdDeals = true;
            }
            nugetCfg = csProjDir + "/project.json";
            if (System.IO.File.Exists(nugetCfg))
            { // add addealssdk to packages.config if project.json exist
                addAdDealsSDKWithJson(nugetCfg);
                hasAddAdDeals = true;
            }
            if (!hasAddAdDeals)
            { // both packages.config and project.json does not exist, create default config
#if UNITY_5
                addAdDealsSDKWithXml(csProjDir + "/packages.config");
#else
                addAdDealsSDKWithJson(csProjDir + "/project.json");
#endif
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

        string framework = Path.Combine(defaultLocationInProj, exampleFrameworkName);
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
                || line.Contains("compile(name: 'AdDealsWrapper', ext:'aar')"))
            {
                continue;
            }
            lines.Add(line);
        }
        File.WriteAllLines(path, lines.ToArray());
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


