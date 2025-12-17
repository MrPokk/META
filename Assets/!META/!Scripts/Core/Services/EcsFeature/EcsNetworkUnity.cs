using System;
using BitterECS.Core;
using VContainer;
using System.Linq;
using BitterECS.Integration;


#if UNITY_EDITOR
using Unity.Multiplayer.Playmode;
#endif

public class EcsNetworkUnity : EcsUnityRoot
{
    [Inject]
    private NetworkConfig _networkConfig;

    protected override void Update()
    {
        base.Update();

#if UNITY_EDITOR
        RunHandlingInEditor();
#else
        RunHandlingInBuild();
#endif
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

#if UNITY_EDITOR
        FixedRunHandlingInEditor();
#else
        FixedRunHandlingInBuild();
#endif
    }

#if UNITY_EDITOR
    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок выполнения ECS систем в редакторе
    // Защищает от падения при ошибках в отдельных системах - каждая система обрабатывается отдельно
    private void RunHandlingInEditor()
    {
        try
        {
            var tags = CurrentPlayer.ReadOnlyTags();

            if (tags.Contains("Server"))
            {
                EcsSystems.Run<IServerConnectedRun>(system => 
                {
                    try
                    {
                        system.Run();
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку в конкретной ECS системе, но продолжаем выполнение остальных
                        LoggerUtility.Error($"Error in ECS system {system.GetType().Name}.Run(): {ex.Message}\n{ex.StackTrace}");
                    }
                });
            }
            else if (tags.Contains("Client"))
            {
                EcsSystems.Run<IClientConnectedRun>(system => 
                {
                    try
                    {
                        system.Run();
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку в конкретной ECS системе, но продолжаем выполнение остальных
                        LoggerUtility.Error($"Error in ECS system {system.GetType().Name}.Run(): {ex.Message}\n{ex.StackTrace}");
                    }
                });
            }
            else
            {
                RunHandlingInBuild();
            }
        }
        catch (Exception ex)
        {
            // Логируем общую ошибку в обработке редактора
            LoggerUtility.Error($"Error in RunHandlingInEditor: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок выполнения ECS систем в FixedUpdate редактора
    // Защищает от падения при ошибках в отдельных системах - каждая система обрабатывается отдельно
    private void FixedRunHandlingInEditor()
    {
        try
        {
            var tags = CurrentPlayer.ReadOnlyTags();

            if (tags.Contains("Server"))
            {
                EcsSystems.Run<IServerConnectedFixedRun>(system => 
                {
                    try
                    {
                        system.FixedRun();
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку в конкретной ECS системе, но продолжаем выполнение остальных
                        LoggerUtility.Error($"Error in ECS system {system.GetType().Name}.FixedRun(): {ex.Message}\n{ex.StackTrace}");
                    }
                });
            }
            else if (tags.Contains("Client"))
            {
                EcsSystems.Run<IClientConnectedFixedRun>(system => 
                {
                    try
                    {
                        system.FixedRun();
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку в конкретной ECS системе, но продолжаем выполнение остальных
                        LoggerUtility.Error($"Error in ECS system {system.GetType().Name}.FixedRun(): {ex.Message}\n{ex.StackTrace}");
                    }
                });
            }
            else
            {
                FixedRunHandlingInBuild();
            }
        }
        catch (Exception ex)
        {
            // Логируем общую ошибку в обработке FixedUpdate редактора
            LoggerUtility.Error($"Error in FixedRunHandlingInEditor: {ex.Message}\n{ex.StackTrace}");
        }
    }
#endif

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок выполнения ECS систем в билде
    // Защищает от падения при ошибках в отдельных системах - каждая система обрабатывается отдельно
    private void RunHandlingInBuild()
    {
        try
        {
            switch (_networkConfig.NetworkType)
            {
                case NetworkType.Server:
                    EcsSystems.Run<IServerConnectedRun>(system => 
                    {
                        try
                        {
                            system.Run();
                        }
                        catch (Exception ex)
                        {
                            // Логируем ошибку в конкретной ECS системе, но продолжаем выполнение остальных
                            LoggerUtility.Error($"Error in ECS system {system.GetType().Name}.Run(): {ex.Message}\n{ex.StackTrace}");
                        }
                    });
                    break;
                case NetworkType.Client:
                    EcsSystems.Run<IClientConnectedRun>(system => 
                    {
                        try
                        {
                            system.Run();
                        }
                        catch (Exception ex)
                        {
                            // Логируем ошибку в конкретной ECS системе, но продолжаем выполнение остальных
                            LoggerUtility.Error($"Error in ECS system {system.GetType().Name}.Run(): {ex.Message}\n{ex.StackTrace}");
                        }
                    });
                    break;
                default:
                    // Логируем ошибку неверного типа сети
                    LoggerUtility.Error($"Invalid network type: {_networkConfig.NetworkType}");
                    throw new Exception($"Invalid network type: {_networkConfig.NetworkType}");
            }
        }
        catch (Exception ex)
        {
            // Логируем критическую ошибку в обработке билда
            LoggerUtility.Critical($"Error in RunHandlingInBuild: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок выполнения ECS систем в FixedUpdate билда
    // Защищает от падения при ошибках в отдельных системах - каждая система обрабатывается отдельно
    private void FixedRunHandlingInBuild()
    {
        try
        {
            switch (_networkConfig.NetworkType)
            {
                case NetworkType.Server:
                    EcsSystems.Run<IServerConnectedFixedRun>(system => 
                    {
                        try
                        {
                            system.FixedRun();
                        }
                        catch (Exception ex)
                        {
                            // Логируем ошибку в конкретной ECS системе, но продолжаем выполнение остальных
                            LoggerUtility.Error($"Error in ECS system {system.GetType().Name}.FixedRun(): {ex.Message}\n{ex.StackTrace}");
                        }
                    });
                    break;
                case NetworkType.Client:
                    EcsSystems.Run<IClientConnectedFixedRun>(system => 
                    {
                        try
                        {
                            system.FixedRun();
                        }
                        catch (Exception ex)
                        {
                            // Логируем ошибку в конкретной ECS системе, но продолжаем выполнение остальных
                            LoggerUtility.Error($"Error in ECS system {system.GetType().Name}.FixedRun(): {ex.Message}\n{ex.StackTrace}");
                        }
                    });
                    break;
                default:
                    // Логируем ошибку неверного типа сети
                    LoggerUtility.Error($"Invalid network type: {_networkConfig.NetworkType}");
                    throw new Exception($"Invalid network type: {_networkConfig.NetworkType}");
            }
        }
        catch (Exception ex)
        {
            // Логируем критическую ошибку в обработке FixedUpdate билда
            LoggerUtility.Critical($"Error in FixedRunHandlingInBuild: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
