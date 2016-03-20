using System;

namespace NetworkCommon.Entities
{
    [Serializable]
    public class MessageData
    {
        /// <summary>
        ///     16 bytes message guid
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     client-server Commands
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        ///     Chat message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        ///     User status (offline, online)
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        ///     Message sent time
        /// </summary>
        public DateTime MessageTime { get; set; }

        public bool Error { get; set; }
    }
}