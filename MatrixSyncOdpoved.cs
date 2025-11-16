using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MatrixUWP
{
    public class MatrixSyncOdpoved
    {
        [JsonProperty("next_batch")]
        public string NextBatch { get; set; }

        [JsonProperty("rooms")]
        public Rooms Rooms { get; set; }
    }

    public class Rooms
    {
        [JsonProperty("join")]
        public Dictionary<string, JoinedRoom> Join { get; set; }
    }

    public class JoinedRoom
    {
        [JsonProperty("timeline")]
        public Timeline Timeline { get; set; }

        [JsonProperty("state")]
        public State State { get; set; }

        // Add other properties like ephemeral, account_data, etc. if needed
    }

    public class State
    {
        [JsonProperty("events")]
        public List<StateEvent> Events { get; set; }
    }

    public class StateEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; }

        [JsonProperty("state_key")]
        public string StateKey { get; set; } // Used to identify what the state is about

        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonProperty("content")]
        public Dictionary<string, object> Content { get; set; }

        [JsonProperty("origin_server_ts")]
        public long OriginServerTs { get; set; }
    }

    public class Timeline
    {
        [JsonProperty("events")]
        public List<Event> Events { get; set; }

        [JsonProperty("limited")]
        public bool Limited { get; set; }

        [JsonProperty("prev_batch")]
        public string PrevBatch { get; set; }
    }

    public class Event
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; }

        [JsonProperty("content")]
        public MessageContent Content { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonProperty("origin_server_ts")]
        public long OriginServerTs { get; set; }
    }

    public class MessageContent
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("msgtype")]
        public string MsgType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }











    public class MatrixSeznamChatu_JedenChat
    {
        public string IdChatu { get; set; }
        public string NazevChatu { get; set; }
        public string PosledniZprava { get; set; }
        public DateTime DateTimePosledniZpravy { get; set; }
        public long UnixoveSekundyPosledniZpravy { get; set; }
        public BitmapImage ObrazekChatu { get; set; }
    }
}
