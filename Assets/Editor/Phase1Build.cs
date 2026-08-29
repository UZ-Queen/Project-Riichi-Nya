#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public static class Phase1Build
{
    public static void BuildWindowsPlayer()
    {
        const string reportPath = "Temp/phase1/build-report.txt";
        const string outputPath = "Builds/phase1/RiichiNya.exe";

        Directory.CreateDirectory("Temp/phase1");
        Directory.CreateDirectory("Builds/phase1");

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.StrictMode
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        string reportText =
            $"Result: {report.summary.result}\n" +
            $"Output: {report.summary.outputPath}\n" +
            $"Errors: {report.summary.totalErrors}\n" +
            $"Warnings: {report.summary.totalWarnings}\n" +
            $"Size: {report.summary.totalSize}\n" +
            $"Duration: {report.summary.totalTime}";
        File.WriteAllText(reportPath, reportText);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new BuildFailedException(reportText);
        }
    }
}

#endif
