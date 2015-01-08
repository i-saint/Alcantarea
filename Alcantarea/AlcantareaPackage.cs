using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MsBuild = Microsoft.Build;
using System.Threading;
using System.Threading.Tasks;


namespace primitive.Alcantarea
{
    public enum UIContext
    {
        None      = 0x00,
        Solution  = 0x01,
        Debugging = 0x02,
        AlcStart  = 0x04,
        AlcDebugging = 0x06,
    };

    public class UIContextHandler : IVsSelectionEvents
    {
        public uint m_cookie;
        AlcantareaPackage m_host;
        uint m_cookie_solution;
        uint m_cookie_debugging;
        uint m_cookie_design;

        public UIContextHandler(AlcantareaPackage host)
        {
            m_host = host;
            IVsMonitorSelection service = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (service != null)
            {
                service.GetCmdUIContextCookie(VSConstants.UICONTEXT.SolutionExists_guid, out m_cookie_solution);
                service.GetCmdUIContextCookie(VSConstants.UICONTEXT.Debugging_guid, out m_cookie_debugging);
                service.GetCmdUIContextCookie(VSConstants.UICONTEXT.DesignMode_guid, out m_cookie_design);
                service.AdviseSelectionEvents(this, out m_cookie);
            }
        }

        public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            int uic = 0;
            if (dwCmdUICookie == m_cookie_solution)  { uic |= (int)UIContext.Solution; }
            if (dwCmdUICookie == m_cookie_debugging) { uic |= (int)UIContext.Debugging; }
            m_host.onUIContext(uic, fActive);
            return VSConstants.S_OK;
        }
        public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return VSConstants.S_OK;
        }
        public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            return VSConstants.S_OK;
        }
    }



    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidAlcantareaPkgString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    public sealed class AlcantareaPackage : Package
    {
        public class CommandData
        {
            public CommandID ID { get; set; }
            public EventHandler Handler { get; set; }
            public Func<int, bool> Condition { get; set; }

            public CommandData(CommandID id, EventHandler h, Func<int, bool> c)
            {
                ID = id;
                Handler = h;
                Condition = c;
            }
        }

        public static alcGlobalConfig GlobalConfig { get; set; }
        public static AlcantareaPackage Instance { get; set; }
        public String[] LastLoadedObjFiles;

        String m_addindir;
        private CommandData[] m_commands;
        UIContextHandler m_uichandler;
        int m_context;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public AlcantareaPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));

            Instance = this;
            m_addindir = Path.GetDirectoryName(typeof(AlcantareaPackage).Assembly.Location);
            try
            {
                AlcantareaHelper.Startup(m_addindir);
            }
            catch(Exception er)
            {
                ShowMessage("Alcantarea: Notice", er.Message);
            }

            Guid cmdset = GuidList.guidAlcantareaCmdSet;
            m_commands = new CommandData[] {
                new CommandData(new CommandID(cmdset, (int)PkgCmdIDList.alcStartDebugging),  onStartDebugging, (int c)=>{ return (c&(int)UIContext.Debugging)==0; } ),
                new CommandData(new CommandID(cmdset, (int)PkgCmdIDList.alcAttachToDebugee), onAttachToDebugee,(int c)=>{ return (c&(int)UIContext.AlcDebugging)==(int)UIContext.Debugging; } ),
                new CommandData(new CommandID(cmdset, (int)PkgCmdIDList.alcApplyChange),     onApplyChange,    (int c)=>{ return (c&(int)UIContext.AlcDebugging)==(int)UIContext.AlcDebugging; } ),
                new CommandData(new CommandID(cmdset, (int)PkgCmdIDList.alcSymbolFilter),    onSymbolFilter,   (int c)=>{ return (c&(int)UIContext.AlcDebugging)==(int)UIContext.AlcDebugging; } ),
                new CommandData(new CommandID(cmdset, (int)PkgCmdIDList.alcToggleSuspend),   onToggleSuspend,  (int c)=>{ return (c&(int)UIContext.AlcDebugging)==(int)UIContext.AlcDebugging; } ),
                new CommandData(new CommandID(cmdset, (int)PkgCmdIDList.alcReloadSymbols),   onLoadSymbols,    (int c)=>{ return (c&(int)UIContext.AlcDebugging)==(int)UIContext.AlcDebugging; } ),
                new CommandData(new CommandID(cmdset, (int)PkgCmdIDList.alcLoadObjFiles),    onLoadObjFiles,   (int c)=>{ return (c&(int)UIContext.AlcDebugging)==(int)UIContext.AlcDebugging; } ),
                new CommandData(new CommandID(cmdset, (int)PkgCmdIDList.alcReloadObjFiles),  onReloadObjFiles, (int c)=>{
                    return (c & (int)UIContext.Debugging) != 0 &&
                        AlcantareaPackage.Instance.LastLoadedObjFiles!=null &&
                        AlcantareaPackage.Instance.LastLoadedObjFiles.Length > 0;
                } ),
                new CommandData(new CommandID(cmdset, (int)PkgCmdIDList.alcOptions),         onOptions,        (int c)=>{ return true; } ),
            };

            m_uichandler = new UIContextHandler(this);
            GlobalConfig = new alcGlobalConfig();
        }


        public static void ShowMessage(String subject, String message)
        {
            MessageBox.Show(message, subject);
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                foreach (CommandData c in m_commands)
                {
                    MenuCommand menuItem = new MenuCommand(c.Handler, c.ID);
                    mcs.AddCommand(menuItem);
                }
            }
        }
        #endregion

        /// <summary>
        /// </summary>
        private void onStartDebugging(object sender, EventArgs e)
        {
            m_context |= (int)UIContext.AlcStart;

            DTE dte = (DTE)GetService(typeof(DTE));

            IVsOutputWindowPane pane = (IVsOutputWindowPane)Package.GetGlobalService(typeof(SVsGeneralOutputWindowPane));

            VCProjectW vcproj;
            VCConfigurationW cfg;
            if (!ProjectUtil.getStartupProjectAndConfig(dte, out vcproj, out cfg))
            {
                return;
            }

            String exepath = cfg.Evaluate("$(TargetPath)");
            String workdir = cfg.Evaluate("$(LocalDebuggerWorkingDirectory)");
            String environment = cfg.Evaluate("$(LocalDebuggerEnvironment)");
            String platform = cfg.Evaluate("$(Platform)");
            String addindir = m_addindir;
            if (pane!=null)
            {
                pane.OutputString(exepath);
            }

            if (!File.Exists(exepath))
            {
                ShowMessage("Alcantarea: Error", "Executabe "+exepath+" not found");
                return;
            }

            // プロセスを suspend で起動して DynamicPatcher を inject
            uint pid = AlcantareaHelper.ExecuteSuspended(exepath, workdir, addindir, environment, platform);
            if (pid != 0)
            {
                // デバッガを attach
                VsDebugTargetInfo2 info = new VsDebugTargetInfo2();
                info.cbSize = (uint)Marshal.SizeOf(info);
                info.bstrExe = exepath;
                info.dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_AlreadyRunning;
                //info.dlo = (uint)_DEBUG_LAUNCH_OPERATION4.DLO_AttachToSuspendedLaunchProcess; // somehow this makes debugger not work
                info.dwProcessId = pid;
                info.LaunchFlags = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd | (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_DetachOnStop;

                Guid guidDbgEngine = VSConstants.DebugEnginesGuids.ManagedAndNative_guid;
                IntPtr pGuids = Marshal.AllocCoTaskMem(Marshal.SizeOf(guidDbgEngine));
                Marshal.StructureToPtr(guidDbgEngine, pGuids, false);
                info.pDebugEngines = pGuids;
                info.dwDebugEngineCount = 1;

                IVsDebugger2 idbg = (IVsDebugger2)Package.GetGlobalService(typeof(SVsShellDebugger));
                IntPtr ptr = Marshal.AllocCoTaskMem((int)info.cbSize);
                Marshal.StructureToPtr(info, ptr, false);
                int ret = idbg.LaunchDebugTargets2(1, ptr);

                // プロセスのスレッドを resume
                AlcantareaHelper.Resume(pid);
            }
        }

        /// <summary>
        /// </summary>
        private void onAttachToDebugee(object sender, EventArgs e)
        {
            DTE dte = (DTE)GetService(typeof(DTE));
            EnvDTE.Debugger debugger = dte.Debugger;
            if(debugger.DebuggedProcesses.Count>0) {
                EnvDTE.Process process = debugger.DebuggedProcesses.Item(1);
                if (AlcantareaHelper.InjectCoreDLL((uint)process.ProcessID, m_addindir))
                {
                    onUIContext((int)UIContext.AlcDebugging, 1);
                }
            }
        }

        private void RunBuild(String projectPath, String target, Dictionary<String, String> properties, Action<BuildLogger> handler)
        {
            BuildLogger logger = new BuildLogger();
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid generalPaneGuid = VSConstants.GUID_OutWindowDebugPane;
            IVsOutputWindowPane debugPane;
            outWindow.GetPane(ref generalPaneGuid, out debugPane);
            logger.OutputWindow = debugPane;

            logger.Verbosity = LoggerVerbosity.Detailed;
            MsBuild.Execution.BuildRequestData buildRequest = new MsBuild.Execution.BuildRequestData(projectPath, properties, null, new string[] { target }, null);
            MsBuild.Execution.BuildParameters buildParams = new MsBuild.Execution.BuildParameters()
            {
                DetailedSummary = false,
                Loggers = new List<ILogger>() { logger }
            };

            ThreadPool.QueueUserWorkItem((Object) =>
            {
                String selectedFiles = properties["SelectedFiles"];
                logger.Write(String.Format("Alcantarea info: compile started \"{0}\"\n", selectedFiles));
                try
                {
                    MsBuild.Execution.BuildResult result = MsBuild.Execution.BuildManager.DefaultBuildManager.Build(buildParams, buildRequest);
                    if (result.OverallResult == MsBuild.Execution.BuildResultCode.Success)
                    {
                        logger.Write(String.Format("Alcantarea info: compile succeeded \"{0}\"\n", selectedFiles));
                        handler(logger);
                    }
                    else
                    {
                        logger.Write(String.Format("Alcantarea info: compile failed \"{0}\"\n", selectedFiles));
                    }
                }
                catch(Exception er)
                {
                
                }
            });
        }

        /// <summary>
        /// </summary>
        private void onApplyChange(object sender, EventArgs e)
        {
            DTE dte = (DTE)GetService(typeof(DTE));
            VCDocumentInfo vcdoc = new VCDocumentInfo(dte);
            if (!vcdoc.IsValid()) { return; }

            dte.ActiveDocument.Save();

            List<String> cur_func = ProjectUtil.GetFunctionsInCurrentPosition(dte);
            VCFileW vcfile = vcdoc.VCFile;
            VCConfigurationW vcconf = vcdoc.VCConfiguration;

            Dictionary<String, String> properties = new Dictionary<String, String >();
            properties.Add("Configuration", vcconf.Evaluate("$(Configuration)"));
            properties.Add("Platform", vcconf.Evaluate("$(Platform)"));
            properties.Add("SelectedFiles", vcdoc.FileRelpath);
            properties.Add("BuildProjectReferences", "false");
            properties.Add("SolutionDir", vcconf.Evaluate("$(SolutionDir)"));
            properties.Add("SolutionExt", vcconf.Evaluate("$(SolutionExt)"));
            properties.Add("SolutionName", vcconf.Evaluate("$(SolutionName)"));
            properties.Add("SolutionFileName", vcconf.Evaluate("$(SolutionFileName)"));
            properties.Add("SolutionPath", vcconf.Evaluate("$(SolutionPath)"));

            RunBuild(vcdoc.ProjPath, vcfile.ItemType, properties, (BuildLogger logger) =>
            {
                if (vcfile.ItemType == "ClCompile")
                {
                    List<alcSymbolFilterColumn> filter = SymbolFilterUtil.GetDefaultFilter(vcdoc.ObjPath, true);
                    {
                        List<alcSymbolFilterColumn> loaded_filter = SymbolFilterUtil.LoadFromConfigFile(vcdoc.ObjFilterPath);
                        if (loaded_filter != null)
                        {
                            filter = SymbolFilterUtil.MergeFilter(filter, loaded_filter);
                        }
                    }

                    if (filter != null && cur_func.Count > 0)
                    {
                        bool changed = false;
                        foreach (alcSymbolFilterColumn c in filter)
                        {
                            foreach (String f in cur_func)
                            {
                                if (c.Name == f)
                                {
                                    c.FlagUpdate = true;
                                    changed = true;
                                }
                            }
                        }
                        if (changed)
                        {
                            SymbolFilterUtil.SaveConfig(vcdoc.ObjFilterPath, filter);
                        }
                    }
                    else
                    {
                        logger.Write(String.Format("Alcantarea warning: could not get cursor's current position function\n"));
                    }
                    AlcantareaHelper.RequestSetSymbolFilter(vcdoc.ObjPath, filter);
                    SendCommand("dpLoadBinary\n" + vcdoc.ObjPath + "\n\n");
                }
            });
        }


        /// <summary>
        /// </summary>
        private void onSymbolFilter(object sender, EventArgs e)
        {
            DTE dte = (DTE)GetService(typeof(DTE));

            VCDocumentInfo info = new VCDocumentInfo(dte);
            if (info.IsValid())
            {
                alcSymbolFilterForBinary(info.ObjPath);
            }
            else
            {
                // todo: show error message
            }
        }

        /// <summary>
        /// </summary>
        private void onToggleSuspend(object sender, EventArgs e)
        {
            SendCommand("dpToggleSuspend\n\n");
        }

        /// <summary>
        /// </summary>
        private void onLoadSymbols(object sender, EventArgs e)
        {
            SendCommand("dpLoadSymbols\n\n");
        }

        /// <summary>
        /// </summary>
        private void onLoadObjFiles(object sender, EventArgs e)
        {
            OpenFileDialog file_dlg = new OpenFileDialog();
            file_dlg.Multiselect = true;
            file_dlg.Filter = "Object Files (*.obj)|*.obj|All Files (*.*)|*.*";
            file_dlg.FilterIndex = 1;
            if (file_dlg.ShowDialog() == DialogResult.OK)
            {
                LastLoadedObjFiles = file_dlg.FileNames;
                foreach (String path in file_dlg.FileNames)
                {
                    alcLoadBinary(path);
                }
            }
        }

        /// <summary>
        /// </summary>
        public void onReloadObjFiles(object sender, EventArgs e)
        {
            foreach (String path in LastLoadedObjFiles)
            {
                alcLoadBinary(path, false);
            }
        }

        /// <summary>
        /// </summary>
        private void onOptions(object sender, EventArgs e)
        {
            OptionWindow window = new OptionWindow();
            window.AddinDir = m_addindir;
            window.Show();
        }


        /// <summary>
        /// </summary>
        public bool alcLoadBinary(String path_to_binary, bool symfilter = true)
        {
            if (symfilter)
            {
                alcSymbolFilterForBinary(path_to_binary);
            }
            SendCommand("dpLoadBinary\n" + path_to_binary + "\n\n");
            return true;
        }


        /// <summary>
        /// </summary>
        public void alcSymbolFilterForBinary(String path_to_obj)
        {
            SymbolFilterWindow sfwindow = new SymbolFilterWindow();
            sfwindow.LoadFilter(path_to_obj);
            sfwindow.ShowDialog();
        }


        private void onDebugBegin()
        {
        }

        private void onDebugEnd()
        {
        }


        public void onUIContext(int ctx, int active)
        {
            if (active != 0) {
                m_context |= ctx;
            }
            else {
                m_context &= ~ctx;
                if (ctx == (int)UIContext.Debugging)
                {
                    m_context &= (int)~UIContext.AlcStart;
                }
            }

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                foreach (CommandData c in m_commands)
                {
                    MenuCommand item = mcs.FindCommand(c.ID);
                    if (item != null)
                    {
                        item.Enabled = c.Condition(m_context);
                    }
                    
                }
            }
        }

        public static void SendCommand(String command)
        {
            if (!AlcantareaHelper.SendCommand(command))
            {
                ShowMessage("Alcantarea: Error",
@"Failed to send command to program. Possible reason:
- Program was not started by 'Alcantarea->Start'
- Failed to start TCP server
  The port was already used, blocked by firewall, etc.
- Server port and client port are different
  'Alcantarea->Option->TCP Connection' to reset"
                );
            }
        }
    }



}
