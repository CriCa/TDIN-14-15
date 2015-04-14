using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Messaging;

namespace Client.Helpers
{
    public abstract class NotificationMessenger
    {
        public static void sendNotification(object sender, NotificationType type, string message)
        {
            Messenger.Default.Send<NotificationMessage<NotificationType>>(new NotificationMessage<NotificationType>(sender, type, message));
        }
    }

    public enum NotifType
    {
       LOGIN, LOGOUT
    };

    public class NotificationType
    {
        NotifType type;
        object parameters;

        public NotifType Type { get { return type; } }

        public List<object> Parameters { get { return parameters as List<object>; } }

        public NotificationType(NotifType type, object parameters)
        {
            this.type = type;
            this.parameters = parameters;
        }
    }
}
