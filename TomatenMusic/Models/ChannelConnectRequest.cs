using DSharpPlus.Entities;
using Emzi0767.Utilities;
using Newtonsoft.Json;

namespace TomatenMusic_Api.Models
{
    public class ChannelConnectRequest
    {
        public ulong Channel_Id { get; set; }
        public ulong Guild_Id { get; set; }

    }
}
