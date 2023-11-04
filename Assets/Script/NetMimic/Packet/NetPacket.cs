using System.Collections.Generic;
using UnityEditor.Rendering;

namespace NetMimic
{
    public enum PacketType : byte
    {
        JoinPacket,
        EntitiesData,
        LeavePacket
    };

    public class NetworkMessage
    {
        public byte Type;
        public byte ClientId;

        protected NetworkMessage() { }

        public NetworkMessage(PacketType type, byte clientId) : this((byte)type, clientId) { }

        public NetworkMessage(byte type, byte clientId)
        {
            this.Type = type;
            this.ClientId = clientId;
        }

        public NetworkMessage(byte[] data)
        {
            DeSerialize(data);
        }

        public byte[] GetSerializeData()
        {
            return Serialize().GetBuffer();
        }
        protected virtual ByteSerializer Serialize()
        {
            ByteSerializer bs = new ByteSerializer();
            bs.WriteByte(Type);
            bs.WriteByte(ClientId);
            return bs;
        }

        public virtual ByteSerializer DeSerialize(byte[] data )
        {
            ByteSerializer bs = new ByteSerializer();
            bs.LoadBuffer(data);
            Type = bs.ReadByte();
            ClientId = bs.ReadByte();
            return bs;
        }
    }
}