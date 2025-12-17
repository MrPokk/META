using System;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using VContainer;
using VContainer.Unity;

public class EntryPointClient : IStartable, IDisposable
{
    private readonly NetworkConfig _networkConfig;
    private readonly NetworkManager _networkManager;
    private IEnumerable<IProviderHandler> _providers;

    [Inject]
    public EntryPointClient(
        NetworkConfig clientConfig,
        NetworkManager networkManager,
        IEnumerable<IProviderHandler> providers)
    {
        _networkConfig = clientConfig;
        _networkManager = networkManager;
        _providers = providers;
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок запуска клиента
    // Критично для отслеживания проблем инициализации
    public void Start()
    {
        try
        {
            LoggerUtility.Info("[Client] Injecting client...");
            _networkConfig.Configure(_networkManager);
            SceneLoader.LoadScene(SceneTypes.Menu);
        }
        catch (Exception ex)
        {
            // Логируем критическую ошибку запуска клиента
            LoggerUtility.Critical($"Failed to start client: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок настройки подключения клиента
    // Критично для отслеживания проблем сетевого подключения
    public void SetupConnection()
    {
        try
        {
            _networkManager.StartClient();
            SetupProvider();
            OnSubscribeClient();
            OnClientStart();
        }
        catch (Exception ex)
        {
            // Логируем критическую ошибку настройки подключения
            LoggerUtility.Critical($"Failed to setup client connection: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок настройки провайдеров
    // Включает проверку на null провайдеры в коллекции
    private void SetupProvider()
    {
        try
        {
            foreach (var provider in _providers)
            {
                // Проверка на null провайдер
                if (provider == null)
                {
                    LoggerUtility.Warning("Null provider found in providers collection");
                    continue;
                }
                provider.HandlersClient();
            }
        }
        catch (Exception ex)
        {
            // Логируем ошибку настройки провайдеров
            LoggerUtility.Error($"Error in provider setup: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок в обработчике старта клиента
    // Защищает от падения при ошибках в ECS системах
    private void OnClientStart()
    {
        try
        {
            LoggerUtility.Info("[Client] Client started!");
            EcsSystems.Run<IClientStart>(system => system.Start());
        }
        catch (Exception ex)
        {
            // Логируем ошибку в обработчике старта, но не прерываем выполнение
            LoggerUtility.Error($"Error in OnClientStart: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок в обработчике подключения клиента
    // Защищает от падения при ошибках в ECS системах при подключении
    private void OnClientConnected()
    {
        try
        {
            LoggerUtility.Info("[Client] Client connected successfully!");
            EcsSystems.Run<IClientConnected>(system => system.Connect());
        }
        catch (Exception ex)
        {
            // Логируем ошибку в обработчике подключения
            LoggerUtility.Error($"Error in OnClientConnected: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок в обработчике отключения клиента
    // Защищает от падения при ошибках в ECS системах при отключении
    private void OnClientDisconnected()
    {
        try
        {
            LoggerUtility.Info("[Client] Connection disconnected!");
            EcsSystems.Run<IClientDisconnected>(system => system.Disconnect());
        }
        catch (Exception ex)
        {
            // Логируем ошибку в обработчике отключения
            LoggerUtility.Error($"Error in OnClientDisconnected: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок в обработчике сетевых ошибок клиента
    // Включает детальную информацию об ошибке транспорта
    private void OnClientError(TransportError error, string arg2)
    {
        try
        {
            // Логируем детальную информацию об ошибке транспорта
            LoggerUtility.Error($"[Client] Connection failed or disconnected! Error: {error}, Details: {arg2}");
            EcsSystems.Run<IClientError>(system => system.OnError());
        }
        catch (Exception ex)
        {
            // Логируем ошибку в самом обработчике ошибок
            LoggerUtility.Error($"Error in OnClientError handler: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void OnSubscribeClient()
    {
        LoggerUtility.Info("[Client] Subscribing to events...");
        NetworkClient.OnConnectedEvent += OnClientConnected;
        NetworkClient.OnErrorEvent += OnClientError;
        NetworkClient.OnDisconnectedEvent += OnClientDisconnected;
    }

    private void OnUnsubscribeClient()
    {
        LoggerUtility.Info("[Client] Unsubscribing from events...");
        NetworkClient.OnConnectedEvent -= OnClientConnected;
        NetworkClient.OnErrorEvent -= OnClientError;
        NetworkClient.OnDisconnectedEvent -= OnClientDisconnected;
    }

    public void Dispose()
    {
        if (NetworkClient.active)
        {
            OnUnsubscribeClient();
        }
    }

}
