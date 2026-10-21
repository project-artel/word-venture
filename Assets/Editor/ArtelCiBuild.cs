using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public static class ArtelCiBuild
{
    public static void BuildWindows()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        if (scenes.Length == 0)
            throw new BuildFailedException("No enabled scenes in build settings.");

        const string directory = "build/StandaloneWindows64";
        Directory.CreateDirectory(directory);
        // SDK 의 QA 기능을 포함하되 실제 player 는 화면을 렌더링하는 일반 빌드로 실행한다.
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = directory + "/WordVenture.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("ARTEL Windows build failed: " + report.summary.result);
    }
}
