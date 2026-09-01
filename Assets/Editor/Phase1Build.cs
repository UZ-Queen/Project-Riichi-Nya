#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 1단계 Windows 플레이어 빌드와 검증 보고서 생성을 담당한다.
/// </summary>
public static class Phase1Build
{
    /// <summary>
    /// 고정된 씬으로 Windows 플레이어를 빌드하고 보고서를 기록한 뒤 배치 Editor를 종료한다.
    /// </summary>
    public static void BuildWindowsPlayer()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        if (string.IsNullOrEmpty(projectRoot))
        {
            throw new BuildFailedException("Unity project root could not be resolved.");
        }

        string reportPath = Path.Combine(projectRoot, "Temp", "phase1", "build-report.txt");
        string outputPath = Path.Combine(projectRoot, "Builds", "phase1", "RiichiNya.exe");
        string durableReportPath = Path.Combine(Path.GetDirectoryName(outputPath), "build-report.txt");

        Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

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
        using (FileStream stream = new FileStream(durableReportPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            writer.Write(reportText);
            writer.Flush();
            stream.Flush(true);
        }

        File.Copy(durableReportPath, reportPath, true);
        Debug.Log($"Phase1 build report written: {durableReportPath}");

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new BuildFailedException(reportText);
        }

        EditorApplication.Exit(0);
    }
}

#endif
