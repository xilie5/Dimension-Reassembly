using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CompoundBox.Editor
{
    public static class CommandLineBuild
    {
        private const string DefaultOutputPath = "Builds/Windows/CompoundBox.exe";

        [MenuItem("Tools/Compound Box/Build Windows64")]
        public static void BuildWindows64()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before building.");
            }

            var outputPath = Environment.GetEnvironmentVariable("COMPOUND_BOX_BUILD_PATH");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = DefaultOutputPath;
            }

            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/SampleScene.unity" },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CleanBuildCache
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"Compound Box build failed: {report.summary.result}, " +
                    $"{report.summary.totalErrors} error(s).");
            }

            Debug.Log(
                $"Compound Box build succeeded: {outputPath}, " +
                $"{report.summary.totalSize / 1024f / 1024f:0.00} MB.");
        }
    }
}
