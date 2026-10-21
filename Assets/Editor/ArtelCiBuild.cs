using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.Rendering;

public static class ArtelCiBuild
{
    public static void BuildLinux()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        if (scenes.Length == 0)
            throw new BuildFailedException("No enabled scenes in build settings.");

        const string directory = "build/StandaloneLinux64";
        Directory.CreateDirectory(directory);
        const BuildTarget target = BuildTarget.StandaloneLinux64;
        var useDefaultGraphics = PlayerSettings.GetUseDefaultGraphicsAPIs(target);
        var graphicsApis = PlayerSettings.GetGraphicsAPIs(target);
        try
        {
            // Xvfb 와 Mesa 에서 화면을 캡처할 수 있도록 QA 빌드에 OpenGLCore 를 지정한다.
            PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
            PlayerSettings.SetGraphicsAPIs(target, new[] { GraphicsDeviceType.OpenGLCore });
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = directory + "/WordVenture.x86_64",
                target = target,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("ARTEL Linux build failed: " + report.summary.result);
        }
        finally
        {
            PlayerSettings.SetGraphicsAPIs(target, graphicsApis);
            PlayerSettings.SetUseDefaultGraphicsAPIs(target, useDefaultGraphics);
        }
    }
}
