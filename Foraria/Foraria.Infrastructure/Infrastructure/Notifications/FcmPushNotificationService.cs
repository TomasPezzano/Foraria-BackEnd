using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Foraria.Infrastructure.Notifications;

public interface IFcmPushNotificationService
{
    Task<bool> SendPushNotificationAsync(
        string fcmToken,
        string title,
        string body,
        Dictionary<string, string>? data = null);

    Task<Dictionary<string, bool>> SendBatchPushNotificationsAsync(
        List<(string fcmToken, string title, string body, Dictionary<string, string>? data)> notifications);
}

public class FcmPushNotificationService : IFcmPushNotificationService
{
    private readonly ILogger<FcmPushNotificationService> _logger;
    private readonly FirebaseApp _firebaseApp;

    public FcmPushNotificationService(
        IConfiguration configuration,
        ILogger<FcmPushNotificationService> logger)
    {
        _logger = logger;


        var credentialPath = configuration["Firebase:CredentialPath"];

        if (string.IsNullOrEmpty(credentialPath) || !File.Exists(credentialPath))
        {
            throw new FileNotFoundException(
                "No se encontró el archivo de credenciales de Firebase. " +
                "Descarga el archivo desde Firebase Console y configura la ruta en appsettings.json");
        }

        _firebaseApp = FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(credentialPath)
        });

        _logger.LogInformation("Firebase Cloud Messaging inicializado correctamente");
    }

    public async Task<bool> SendPushNotificationAsync(
        string fcmToken,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        try
        {
            if (string.IsNullOrEmpty(fcmToken))
            {
                _logger.LogWarning("Intento de enviar notificación con FCM token vacío");
                return false;
            }

            var message = new Message()
            {
                Token = fcmToken,
                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>(),
                
                Android = new AndroidConfig()
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification()
                    {
                        ChannelId = "foraria_default",
                        DefaultSound = true,
                        DefaultVibrateTimings = true
                    }
                },
                
                Apns = new ApnsConfig()
                {
                    Aps = new Aps()
                    {
                        Alert = new ApsAlert()
                        {
                            Title = title,
                            Body = body
                        },
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

            _logger.LogInformation(
                "Notificación push enviada exitosamente. Response: {Response}",
                response);

            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            
            _logger.LogError(ex,
                "Error de Firebase al enviar notificación push. ErrorCode: {ErrorCode}",
                ex.MessagingErrorCode);
           
            if (ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument ||
                ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                _logger.LogWarning("Token FCM inválido o no registrado: {Token}", fcmToken);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al enviar notificación push");
            return false;
        }
    }

    public async Task<Dictionary<string, bool>> SendBatchPushNotificationsAsync(
        List<(string fcmToken, string title, string body, Dictionary<string, string>? data)> notifications)
    {
        var results = new Dictionary<string, bool>();

        try
        {
            
            const int batchSize = 500;
            var batches = notifications
                .Select((notif, index) => new { notif, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.notif).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                var messages = batch.Select(n => new Message()
                {
                    Token = n.fcmToken,
                    Notification = new FirebaseAdmin.Messaging.Notification()
                    {
                        Title = n.title,
                        Body = n.body
                    },
                    Data = n.data ?? new Dictionary<string, string>(),
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            ChannelId = "foraria_default",
                            DefaultSound = true
                        }
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Sound = "default",
                            Badge = 1
                        }
                    }
                }).ToList();

                var response = await FirebaseMessaging.DefaultInstance.SendAllAsync(messages);

                _logger.LogInformation(
                    "Batch enviado: {SuccessCount}/{TotalCount} exitosos",
                    response.SuccessCount,
                    batch.Count);

                
                for (int i = 0; i < batch.Count; i++)
                {
                    var token = batch[i].fcmToken;
                    results[token] = response.Responses[i].IsSuccess;

                    if (!response.Responses[i].IsSuccess)
                    {
                        _logger.LogWarning(
                            "Fallo al enviar a token {Token}. Error: {Error}",
                            token,
                            response.Responses[i].Exception?.Message);
                    }
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en envío batch de notificaciones");
          
            foreach (var notif in notifications)
            {
                if (!results.ContainsKey(notif.fcmToken))
                {
                    results[notif.fcmToken] = false;
                }
            }

            return results;
        }
    }
}