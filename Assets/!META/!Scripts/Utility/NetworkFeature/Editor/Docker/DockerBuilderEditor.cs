using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build.Profile;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class DockerBuilderUnity6 : EditorWindow
{
    [Header("Unity 6 Build Settings")]
    [SerializeField] private BuildProfile buildProfile;
    [SerializeField] private string buildFolder = "Builds/Server";

    [Header("Docker Image Settings")]
    [SerializeField] private string imageName = "meta-iubip";
    [SerializeField] private string imageTag = "latest";

    [Header("Dockerfile Parameters")]
    [SerializeField] private string baseImage = "ubuntu:22.04";
    [SerializeField] private string binaryName = "metaServer";
    [SerializeField] private string unityArgs = "-batchmode -nographics";

    private bool isProcessing = false;
    private CancellationTokenSource cts;
    private Process currentProcess;

    [MenuItem("Tools/Docker/Unity 6 Edgegap Builder")]
    public static void ShowWindow()
    {
        GetWindow<DockerBuilderUnity6>("Docker Builder (Unity 6)");
    }

    private void OnGUI()
    {
        // Блокируем настройки, если идет процесс
        GUI.enabled = !isProcessing;

        EditorGUILayout.Space();
        GUILayout.Label("1. Unity Build Settings", EditorStyles.boldLabel);
        buildProfile = (BuildProfile)EditorGUILayout.ObjectField("Build Profile", buildProfile, typeof(BuildProfile), false);
        buildFolder = EditorGUILayout.TextField("Output Folder", buildFolder);
        binaryName = EditorGUILayout.TextField("Binary Name", binaryName);

        EditorGUILayout.Space();
        GUILayout.Label("2. Dockerfile & Container Settings", EditorStyles.boldLabel);
        imageName = EditorGUILayout.TextField("Image Name", imageName);
        imageTag = EditorGUILayout.TextField("Image Tag", imageTag);
        baseImage = EditorGUILayout.TextField("Base OS Image", baseImage);
        unityArgs = EditorGUILayout.TextField("Unity CMD Args", unityArgs);

        EditorGUILayout.Space();

        if (buildProfile == null)
        {
            EditorGUILayout.HelpBox("Пожалуйста, выберите Build Profile.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("1. Build Linux Server", GUILayout.Height(35)))
        {
            ExecuteUnityBuild();
        }

        EditorGUILayout.Space();

        // Кнопка запуска Docker
        GUI.enabled = !isProcessing;
        if (GUILayout.Button("2. Update Dockerfile & Build Image", GUILayout.Height(35)))
        {
            _ = BuildDockerAsync();
        }

        // Кнопка ОТМЕНЫ (появляется только во время работы)
        GUI.enabled = true;
        if (isProcessing)
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("CANCEL BUILD", GUILayout.Height(30)))
            {
                CancelOperation();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.HelpBox("Docker Build в процессе... Прогресс в консоли.", MessageType.Info);
        }

        EditorGUILayout.LabelField("Dockerfile Location:", GetDockerfileFullPath(), EditorStyles.miniLabel);
    }

    private async Task BuildDockerAsync()
    {
        isProcessing = true;
        cts = new CancellationTokenSource();

        try
        {
            string path = GetDockerfileFullPath();
            GenerateDockerfile(path);

            string fullImageName = $"{imageName}:{imageTag}".ToLower();
            string normalizedBuildPath = buildFolder.Replace("\\", "/");

            // Используем buildx и --load для современности
            string args = $"buildx build --load -f \"{path}\" " +
                          $"--build-arg SERVER_BUILD_PATH=\"{normalizedBuildPath}\" " +
                          $"-t {fullImageName} .";

            UnityEngine.Debug.Log($"[Docker] Starting Build: docker {args}");

            bool success = await RunProcessAsync("docker", args, cts.Token);

            if (success)
                UnityEngine.Debug.Log("[Docker] Build Completed Successfully!");
            else
                UnityEngine.Debug.LogWarning("[Docker] Build failed or was cancelled.");
        }
        catch (System.OperationCanceledException)
        {
            UnityEngine.Debug.LogWarning("[Docker] Build Cancelled by user.");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"[Docker] Exception: {e.Message}");
        }
        finally
        {
            isProcessing = false;
            cts?.Dispose();
            cts = null;
            Repaint();
        }
    }

    private void CancelOperation()
    {
        if (cts != null) cts.Cancel();

        if (currentProcess != null && !currentProcess.HasExited)
        {
            UnityEngine.Debug.Log("[Docker] Killing process...");
            currentProcess.Kill(); // true = убить дерево процессов
        }
    }

    private async Task<bool> RunProcessAsync(string filename, string args, CancellationToken token)
    {
        var tcs = new TaskCompletionSource<bool>();

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = filename,
            Arguments = args,
            WorkingDirectory = Directory.GetCurrentDirectory(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        currentProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };

        // Обычные логи
        currentProcess.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null) UnityEngine.Debug.Log($"<color=white>[Docker]: {e.Data}</color>");
        };

        // ОШИБКИ теперь будут красными для наглядности
        currentProcess.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null) UnityEngine.Debug.LogError($"[Docker Error]: {e.Data}");
        };

        currentProcess.Exited += (s, e) =>
        {
            if (currentProcess != null)
                tcs.TrySetResult(currentProcess.ExitCode == 0);
        };

        try
        {
            currentProcess.Start();
            currentProcess.BeginOutputReadLine();
            currentProcess.BeginErrorReadLine();
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"[System Error]: Failed to start process: {ex.Message}");
            return false;
        }

        using (token.Register(() => tcs.TrySetCanceled()))
        {
            try
            {
                return await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }
    }
    private void GenerateDockerfile(string path)
    {
        string content = $@"FROM {baseImage}
ARG DEBIAN_FRONTEND=noninteractive
ARG SERVER_BUILD_PATH={buildFolder}
COPY ${{SERVER_BUILD_PATH}} /root/build/
WORKDIR /root/
RUN chmod +x /root/build/{binaryName}
RUN apt-get update && \
    apt-get install -y ca-certificates && \
    apt-get clean && \
    update-ca-certificates
CMD [ ""/root/build/{binaryName}"", ""{unityArgs}"", ""$UNITY_COMMANDLINE_ARGS""]";

        File.WriteAllText(path, content);
    }

    private void ExecuteUnityBuild()
    {
        string locationPath = Path.Combine(buildFolder, binaryName);
        BuildPlayerWithProfileOptions options = new BuildPlayerWithProfileOptions()
        {
            buildProfile = buildProfile,
            locationPathName = locationPath,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
            UnityEngine.Debug.Log($"[Build] Success! Path: {locationPath}");
        else
            UnityEngine.Debug.LogError("[Build] Failed!");
    }

    private string GetDockerfileFullPath()
    {
        MonoScript ms = MonoScript.FromScriptableObject(this);
        string scriptPath = AssetDatabase.GetAssetPath(ms);
        string directory = Path.GetDirectoryName(scriptPath);
        return Path.GetFullPath(Path.Combine(directory, "Dockerfile"));
    }

    private void OnDisable()
    {
        CancelOperation(); // Страховка при закрытии окна
    }
}
