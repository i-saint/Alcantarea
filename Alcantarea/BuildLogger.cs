using System;
using System.IO;
using System.Security;
using EnvDTE;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.Shell.Interop;


namespace primitive.Alcantarea
{
    public class BuildLogger : Logger
    {
        public IVsOutputWindowPane OutputWindow { get; set; }

        public override void Initialize(IEventSource eventSource)
        {
            eventSource.ProjectStarted += new ProjectStartedEventHandler(eventSource_ProjectStarted);
            eventSource.TaskStarted += new TaskStartedEventHandler(eventSource_TaskStarted);
            eventSource.MessageRaised += new BuildMessageEventHandler(eventSource_MessageRaised);
            eventSource.WarningRaised += new BuildWarningEventHandler(eventSource_WarningRaised);
            eventSource.ErrorRaised += new BuildErrorEventHandler(eventSource_ErrorRaised);
            eventSource.ProjectFinished += new ProjectFinishedEventHandler(eventSource_ProjectFinished);
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            Write(String.Format("{0}({1}): error {2}: {3}\n", e.File, e.LineNumber, e.Code, e.Message));
        }

        void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            Write(String.Format("{0}({1}): warning {2}: {3}\n", e.File, e.LineNumber, e.Code, e.Message));
        }

        void eventSource_MessageRaised(object sender, BuildMessageEventArgs e)
        {
            if ((e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                || (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal))
                || (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed))
                )
            {
                ;
            }
        }

        void eventSource_TaskStarted(object sender, TaskStartedEventArgs e)
        {
        }

        void eventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            //Write(String.Format("{0}\n", e.Message));
        }

        void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            //Write(String.Format("{0}\n\n", e.Message));
        }

        private void WriteLineWithSenderAndMessage(string line, BuildEventArgs e)
        {
            if (0 == String.Compare(e.SenderName, "MSBuild", true /*ignore case*/))
            {
                // Well, if the sender name is MSBuild, let's leave it out for prettiness
                //WriteLine(line, e);
            }
            else
            {
                //   WriteLine(e.SenderName + ": " + line, e);
            }
        }

        public void Write(string message)
        {
            if (OutputWindow != null)
            {
                OutputWindow.OutputString(message);
            }
        }
    }

}