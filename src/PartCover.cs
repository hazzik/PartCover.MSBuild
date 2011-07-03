using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PartCover.MSBuild
{
    /// <summary>
    /// Execute the PartCover coverage.
    /// </summary>
    /// <remarks>
    /// <![CDATA[
    ///  <PartCover ToolPath="$(LibDirectory)\PartCover\"
    ///			    Target="$(LibDirectory)\NUnit\nunit-console.exe"
    ///			    TargetArgs="%(TestAssemblies.FullPath) /xml=%(TestAssemblies.Filename).xml /labels /nologo /noshadow"
    ///             WorkingDirectory="$(MSBuildProjectDirectory)"
    ///			    Output="partcover.xml"
    ///             Include="[AssemblyName.*]*"
    ///             Exclude="[*.Test]*"
    ///	/>
    /// ]]>
    /// </remarks>
    public class PartCover : ToolTask
    {
        private const string DefaultApplicationName = "PartCover.exe";

        protected override string ToolName
        {
            get { return DefaultApplicationName; }
        }

        /// <summary>
        /// The application to execute to get the coverage results.
        /// Generally this will be your unit testing exe.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// The arguments to pass to the <see cref="Target"/> executable
        /// </summary>
        public string TargetArgs { get; set; }

        public string WorkingDirectory { get; set; }

        /// <summary>
        /// The file where partcover will write its output.
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// The assembly expressions to include in the coverage.
        /// </summary>
        /// <example>
        /// [AssemblyName.*]*
        /// </example>
        public ITaskItem[] Include { get; set; }

        /// <summary>
        /// The assembly expressions to exclude from the coverage.
        /// </summary>
        /// <example>
        /// Exclude="[*.Test]*"
        /// </example>
        public ITaskItem[] Exclude { get; set; }

        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(ToolPath, ToolName);
        }

        protected override string GenerateCommandLineCommands()
        {
            var builder = new StringBuilder();
            AppendIfPresent(builder, "--target", Target);
            AppendIfPresent(builder, "--target-work-dir", WorkingDirectory);
            AppendIfPresent(builder, "--target-args", QuoteIfNeeded(TargetArgs));
            AppendIfPresent(builder, "--output", Output);

            AppendMultipleItemsTo(builder, "--include", Include);
            AppendMultipleItemsTo(builder, "--exclude", Exclude);

            builder.Append("--register");

            Log.LogCommandLine(builder.ToString());

            return builder.ToString();
        }

        protected override void LogEventsFromTextOutput(String singleLine, MessageImportance messageImportance)
        {
            base.LogEventsFromTextOutput(singleLine, MessageImportance.High);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>
        /// Argument specifies arguments for target process. If target argument contains spaces 
        /// - quote <argument/>. If you want specify quote (") in <arguments/>, then precede it 
        /// by slash (\)
        /// </remarks>
        private static string QuoteIfNeeded(string args)
        {
            if (String.IsNullOrWhiteSpace(args) || args.Contains(" ") == false)
                return args;

            if (args.Contains("\""))
                args = args.Replace("\"", "\\\"");

            return String.Format("\"{0}\"", args);
        }

        private static void AppendIfPresent(StringBuilder builder, string cmdArg, string value)
        {
            if (String.IsNullOrWhiteSpace(value) == false)
                builder.AppendFormat("{0} {1} ", cmdArg, value);
        }

        private static void AppendMultipleItemsTo(StringBuilder builder, string cmdArg, IEnumerable<ITaskItem> items)
        {
            if (null == items) return;

            foreach (ITaskItem item in items)
                builder.AppendFormat("{0} {1} ", cmdArg, item.ItemSpec);
        }
    }
}