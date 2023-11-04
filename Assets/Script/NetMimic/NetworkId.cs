using System;

namespace NetMimic
{
    public readonly struct NetworkId : IEquatable<NetworkId>
    {
        public int Value { get; }

        public NetworkId(int fullid)
        {
            Value = fullid;
        }

        public NetworkId(int id, byte clientId)
        {
            if (id > 0xFFFFFF || id < 0)
            {
                throw new ArgumentOutOfRangeException($"NetworkId: ID value {id} is out of range.");
            }

            Value = (int)(id << 8 | clientId);
        }

        public byte ClientId => (byte)(Value & 0xFF);

        public static bool ContainsClientId(byte clientId, int value)
        {
            return clientId == (byte)(value & 0xFF);
        }

        public override bool Equals(object obj)
        {
            throw new NotSupportedException();
        }

        public bool Equals(NetworkId id)
        {
            return Value == id.Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(NetworkId left, NetworkId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NetworkId left, NetworkId right)
        {
            return !(left == right);
        }
    }
}