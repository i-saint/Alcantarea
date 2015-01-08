using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using EnvDTE;


namespace primitive.Alcantarea
{
    // VCEngine namespace の中の class 群は VisualStudio のバージョンが違うと GUID が変わって別の class と認識されるため、
    // 以下の様な wrapper を用意しないと複数バージョン対応できない (ウンコー！)

    public class WrapperBase
    {
        public Object Object { get; set; }
        public Object getProperty(String name) { return ProjectUtil.getProperty(Object, name); }
        public Object callMember(String name, Object obj) { return ProjectUtil.callMember(Object, name, obj); }
    }

    public class VCProjectW : WrapperBase
    {
        public VCProjectW(Object o) { Object = o; }
        public String UniqueName { get { return (String)getProperty("UniqueName"); } }
        public IVCCollectionW Configurations { get { return new IVCCollectionW(getProperty("Configurations")); } }
        public String ProjectDirectory { get { return (String)getProperty("ProjectDirectory"); } }
        public String ProjectFile { get { return (String)getProperty("ProjectFile"); } }
        public String ItemName { get { return (String)getProperty("ItemName"); } }
    }

    public class VCConfigurationW : WrapperBase
    {
        public VCConfigurationW(Object o) { Object = o; }
        public String Name { get { return (String)getProperty("Name"); } }
        public String Evaluate(String str)
        {
            return (String)callMember("Evaluate", str);
        }
    }

    public class VCFileW : WrapperBase
    {
        public VCFileW(Object o) { Object = o; }
        public VCProjectW project { get { return new VCProjectW(getProperty("project")); } }
        public String Name { get { return (String)getProperty("Name"); } }
        public String FullPath { get { return (String)getProperty("FullPath"); } }
        public String RelativePath { get { return (String)getProperty("RelativePath"); } }
        public String ItemType { get { return (String)getProperty("ItemType"); } }
    }

    public class IVCCollectionW : WrapperBase
    {
        public IVCCollectionW(Object o) { Object = o; }
        public int Count { get { return (int)getProperty("Count"); } }
        public VCConfigurationW Item(int i)
        {
            return new VCConfigurationW(callMember("Item", i));
        }
    }



    public class VCDocumentInfo
    {
        public VCProjectW VCProject { get; set; }
        public VCConfigurationW VCConfiguration { get; set; }
        public VCFileW VCFile { get; set; }

        public VCDocumentInfo(DTE app)
        {
            VCProjectW vcproj;
            VCConfigurationW cfg;
            if (!ProjectUtil.getActiveProjectAndConfig(app, out vcproj, out cfg))
            {
                return;
            }

            Document doc = app.ActiveDocument;
            ProjectItem item = doc.ProjectItem;
            if (ProjectUtil.matchTypeName(item.Object, "VCProjectFileShim"))
            {
                VCFileW vcfile = new VCFileW(item.Object);
                this.VCProject = vcproj;
                this.VCConfiguration = cfg;
                this.VCFile = vcfile;
            }
        }

        public bool IsValid()
        {
            return this.VCProject != null && this.VCConfiguration != null && this.VCFile != null;
        }

        public string FilePath {
            get { return this.VCFile.FullPath; }
        }

        public string ObjPath {
            get
            {
                String path = this.VCConfiguration.Evaluate("$(IntDir)");
                if (!Path.IsPathRooted(path))
                {
                    path = this.VCConfiguration.Evaluate("$(ProjectDir)")+path;
                }
                path += Regex.Replace(this.VCFile.Name, "\\.(cpp|cc|c|cxx)", ".obj");
                return path;
            }
        }

        public string ObjFilterPath {
            get
            {
                return ObjPath + ".alc";
            }
        }

        public string FileRelpath {
            get { return this.VCFile.RelativePath; }
        }

        public string ProjPath
        {
            get { return this.VCConfiguration.Evaluate("$(ProjectPath)"); }
        }

        public string ProjName {
            get { return this.VCProject.ItemName;  }
        }
    }



    public class ProjectUtil
    {
        public static bool matchTypeName(Object obj, String name)
        {
            return obj != null && obj.GetType().Name == name;
        }

        public static Object getProperty(Object obj, String name)
        {
            return obj.GetType().InvokeMember(name, BindingFlags.GetProperty, null, obj, null);
        }

        public static Object callMember(Object obj, String name, Object arg)
        {
            return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, obj, new Object[] { arg });
        }


        private static bool getStartupProjectAndConfigR(EnvDTE.Project project, String startup, out VCProjectW o_proj, out VCConfigurationW o_conf)
        {
            o_proj = null;
            o_conf = null;
            if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    EnvDTE.Project child = item.SubProject;
                    if (child != null)
                    {
                        if (getStartupProjectAndConfigR(child, startup, out o_proj, out o_conf))
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (!matchTypeName(project.Object, "VCProjectShim")) { return false; }
                VCProjectW vcproj = new VCProjectW(project.Object);
                o_proj = vcproj;

                if (project.UniqueName == startup)
                {
                    String configname = project.ConfigurationManager.ActiveConfiguration.ConfigurationName + "|" + project.ConfigurationManager.ActiveConfiguration.PlatformName;
                    IVCCollectionW cfgs = vcproj.Configurations;
                    int num = cfgs.Count;
                    for (int i = 1; i <= num; ++i)
                    {
                        VCConfigurationW c = cfgs.Item(i);
                        if (c.Name == configname)
                        {
                            o_conf = c;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool getStartupProjectAndConfig(DTE app, out VCProjectW o_proj, out VCConfigurationW o_conf)
        {
            o_proj = null;
            o_conf = null;
            Solution sln = app.Solution;
            if (sln == null || sln.SolutionBuild.StartupProjects==null)
            {
                return false;
            }

            String startup = (String)((Array)sln.SolutionBuild.StartupProjects).GetValue(0);
            foreach (EnvDTE.Project project in sln.Projects)
            {
                if (getStartupProjectAndConfigR(project, startup, out o_proj, out o_conf))
                {
                    return true;
                }
            }
            return false;
        }


        public static bool getActiveProjectAndConfig(DTE app, out VCProjectW out_proj, out VCConfigurationW out_conf)
        {
            out_proj = null;
            out_conf = null;

            Document doc = app.ActiveDocument;
            if (doc == null) { return false; }

            ProjectItem item = doc.ProjectItem;
            if (!matchTypeName(item.Object, "VCProjectFileShim")) { return false; }

            EnvDTE.Project proj = item.ContainingProject;
            VCFileW vcfile = new VCFileW(item.Object);
            VCProjectW vcproj = vcfile.project;
            String configname = proj.ConfigurationManager.ActiveConfiguration.ConfigurationName + "|" + proj.ConfigurationManager.ActiveConfiguration.PlatformName;
            IVCCollectionW cfgs = vcproj.Configurations;
            int num = cfgs.Count;
            for (int i = 1; i <= num; ++i)
            {
                VCConfigurationW c = cfgs.Item(i);
                String name = (String)getProperty(c, "Name");
                if (name == configname)
                {
                    out_proj = vcproj;
                    out_conf = c;
                    return true;
                }
            }

            return false;
        }


        private static void GetFunctionsInCurrentPositionImpl(CodeElement element, List<String> functions)
        {
            if (element == null) { return;  }
            if (element.Kind == vsCMElement.vsCMElementFunction)
            {
                functions.Add(element.FullName);
            }
            else if (
                element.Kind == vsCMElement.vsCMElementNamespace ||
                element.Kind == vsCMElement.vsCMElementClass ||
                element.Kind == vsCMElement.vsCMElementStruct ||
                element.Kind == vsCMElement.vsCMElementUnion
                )
            {
                foreach (CodeElement child in element.Children)
                {
                    GetFunctionsInCurrentPositionImpl(child, functions);
                }
            }
        }

        public static List<String> GetFunctionsInCurrentPosition(DTE dte)
        {
            List<String> ret = new List<String>();
            if (dte.ActiveDocument != null) {
                try
                {
                    TextDocument tdoc = dte.ActiveDocument.Object() as TextDocument;
                    GetFunctionsInCurrentPositionImpl(tdoc.Selection.ActivePoint.get_CodeElement(vsCMElement.vsCMElementNamespace), ret);
                }
                catch (Exception)
                {
                }
            }
            return ret;
        }
    }
}