using System;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using VContainer;
using VContainer.Unity;

public class EntryPointServer : IStartable, IDisposable
{
    private readonly NetworkConfig _networkConfig;
    private readonly NetworkManager _networkManager;
    private readonly IEnumerable<IProviderHandler> _providers;

    [Inject]
    public EntryPointServer(
        NetworkConfig networkConfig,
        NetworkManager networkManager,
        IEnumerable<IProviderHandler> providers)
    {
        _networkConfig = networkConfig;
        _networkManager = networkManager;
        _providers = providers;
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок запуска сервера
    // Критично для отслеживания проблем инициализации сервера
    public void Start()
    {
        try
        {
            LoggerUtility.Info("[Server] Injecting server...");
            _networkConfig.Configure(_networkManager);
            _networkManager.StartServer();
            SetupProvider();
            SubscribeServerEvents();
            OnServerStart();
        }
        catch (Exception ex)
        {
            // Логируем критическую ошибку запуска сервера
            LoggerUtility.Critical($"Failed to start server: {ex.Message}\n{ex.StackTrace}");
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
                provider.HandlersServer();
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
    // Добавлено логирование ошибок в обработчике старта сервера
    // Защищает от падения при ошибках в ECS системах
    private void OnServerStart()
    {
        try
        {
            LoggerUtility.Info("[Server] Server started!");
            EcsSystems.Run<IServerStart>(system => system.Start());
        }
        catch (Exception ex)
        {
            // Логируем ошибку в обработчике старта, но не прерываем выполнение
            LoggerUtility.Error($"Error in OnServerStart: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок в обработчике подключения клиента к серверу
    // Включает проверку на null клиента и логирование ID подключения
    private void OnServerConnected(NetworkConnectionToClient client)
    {
        try
        {
            // Проверка на null клиента
            if (client == null)
            {
                LoggerUtility.Warning("[Server] OnServerConnected called with null client");
                return;
            }
            LoggerUtility.Info($"[Server] Server to client connected! ConnectionId: {client.connectionId}");
            EcsSystems.Run<IServerConnected>(system => system.Connect(client));
        }
        catch (Exception ex)
        {
            // Логируем ошибку в обработчике подключения
            LoggerUtility.Error($"Error in OnServerConnected: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок в обработчике сетевых ошибок сервера
    // Включает детальную информацию об ошибке транспорта и ID подключения
    private void OnServerError(NetworkConnectionToClient client, TransportError error, string arg3)
    {
        try
        {
            // Логируем детальную информацию об ошибке транспорта
            LoggerUtility.Error($"[Server] Server error: {error}, ConnectionId: {client?.connectionId}, Details: {arg3}");
            EcsSystems.Run<IServerError>(system => system.OnError(client, error, arg3));
        }
        catch (Exception ex)
        {
            // Логируем ошибку в самом обработчике ошибок
            LoggerUtility.Error($"Error in OnServerError handler: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок в обработчике отключения клиента от сервера
    // Включает проверку на null клиента и логирование ID подключения
    private void OnServerDisconnected(NetworkConnectionToClient client)
    {
        try
        {
            // Проверка на null клиента
            if (client == null)
            {
                LoggerUtility.Warning("[Server] OnServerDisconnected called with null client");
                return;
            }
            LoggerUtility.Info($"[Server] Server disconnected! ConnectionId: {client.connectionId}");
            EcsSystems.Run<IServerDisconnected>(system => system.Disconnect(client));
        }
        catch (Exception ex)
        {
            // Логируем ошибку в обработчике отключения
            LoggerUtility.Error($"Error in OnServerDisconnected: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void SubscribeServerEvents()
    {
        LoggerUtility.Info("[Server] Subscribing to events...");
        NetworkServer.OnConnectedEvent += OnServerConnected;
        NetworkServer.OnDisconnectedEvent += OnServerDisconnected;
        NetworkServer.OnErrorEvent += OnServerError;
    }

    private void UnsubscribeServerEvents()
    {
        LoggerUtility.Info("[Server] Unsubscribing from events...");
        NetworkServer.OnConnectedEvent -= OnServerConnected;
        NetworkServer.OnDisconnectedEvent -= OnServerDisconnected;
        NetworkServer.OnErrorEvent -= OnServerError;
    }

    public void Dispose()
    {
        if (NetworkServer.active)
        {
            UnsubscribeServerEvents();
        }
    }
}
