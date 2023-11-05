using NetMimic;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class NetworkAttributes
    {
        public List<KeyValuePair<string, float>> floatAttributes = new List<KeyValuePair<string, float>>();
        public List<KeyValuePair<string, int>> intAttributes = new List<KeyValuePair<string, int>>();
        public List<KeyValuePair<string, string>> stringAttributes = new List<KeyValuePair<string, string>>();
    }

    public class EntityData
    {
        public List<int> Entity = new List<int>();
        public List<string> Asset = new List<string>();
        public List<Vector3> Positions = new List<Vector3>();
        public List<Quaternion> Rotations = new List<Quaternion>();
        public List<int> AnimState = new List<int>();
        public List<float> AnimTime = new List<float>();
        public List<NetworkAttributes> NetworkAttributes = new List<NetworkAttributes>();
    }

    public class EntityDataMessage : NetworkMessage
    {
        public EntityData EntityData = new EntityData();

        public EntityDataMessage(byte clientId) : base(PacketType.EntitiesData, clientId)
        {
        }

        public EntityDataMessage(byte[] data)
        {
            DeSerialize(data);
        }

        public override ByteSerializer DeSerialize(byte[] data)
        {
            ByteSerializer bs = base.DeSerialize(data);

            uint Entitylength = bs.ReadUInt();
            for (uint i = 0; i < Entitylength; i++)
            {
                EntityData.Entity.Add(bs.ReadInt());
                EntityData.Asset.Add(bs.ReadString());
                EntityData.Positions.Add(new Vector3(bs.ReadFloat(), bs.ReadFloat(), bs.ReadFloat()));
                EntityData.Rotations.Add(new Quaternion(bs.ReadFloat(), bs.ReadFloat(), bs.ReadFloat(), bs.ReadFloat()));

                NetworkAttributes netAttributes = new NetworkAttributes();
                EntityData.NetworkAttributes.Add(netAttributes);

                byte floatAttrinutes = bs.ReadByte();
                for( byte f = 0; f < floatAttrinutes; f++)
                {
                    netAttributes.floatAttributes.Add( new KeyValuePair<string, float>( bs.ReadString(),bs.ReadFloat()));
                }

                byte intAttrinutes = bs.ReadByte();
                for (byte f = 0; f < intAttrinutes; f++)
                {
                    netAttributes.intAttributes.Add(new KeyValuePair<string, int>(bs.ReadString(), bs.ReadInt()));
                }

                byte stringAttrinutes = bs.ReadByte();
                for (byte f = 0; f < stringAttrinutes; f++)
                {
                    netAttributes.stringAttributes.Add(new KeyValuePair<string, string>(bs.ReadString(), bs.ReadString()));
                }
            }

            return bs;
        }

        protected override ByteSerializer Serialize()
        {
            ByteSerializer bs = base.Serialize();

            bs.WriteUInt((uint)EntityData.Entity.Count);
            for (int i = 0; i < EntityData.Entity.Count; i++)
            {
                bs.WriteInt(EntityData.Entity[i]);

                bs.WriteString(EntityData.Asset[i]);

                bs.WriteFloat(EntityData.Positions[i].x);
                bs.WriteFloat(EntityData.Positions[i].y);
                bs.WriteFloat(EntityData.Positions[i].z);

                bs.WriteFloat(EntityData.Rotations[i].x);
                bs.WriteFloat(EntityData.Rotations[i].y);
                bs.WriteFloat(EntityData.Rotations[i].z);
                bs.WriteFloat(EntityData.Rotations[i].w);

                NetworkAttributes na = EntityData.NetworkAttributes[i];

                bs.WriteByte((byte)na.floatAttributes.Count);
                for ( int f = 0; f < na.floatAttributes.Count; f++ )
                {
                    bs.WriteString(na.floatAttributes[f].Key);
                    bs.WriteFloat(na.floatAttributes[f].Value);
                }

                bs.WriteByte((byte)na.intAttributes.Count);
                for (int ii = 0; ii < na.intAttributes.Count; ii++)
                {
                    bs.WriteString(na.intAttributes[ii].Key);
                    bs.WriteInt(na.intAttributes[ii].Value);
                }

                bs.WriteByte((byte)na.stringAttributes.Count);
                for (int s = 0; s < na.stringAttributes.Count; s++)
                {
                    bs.WriteString(na.stringAttributes[s].Key);
                    bs.WriteString(na.stringAttributes[s].Value);
                }
            }

            return bs;
        }

        public void AddEntry(int id, NetEntity entity, Attributes attributes )
        {
           EntityData.Entity.Add(id);
           EntityData.Asset.Add(entity.GetAdress);
           EntityData.Positions.Add(entity.transform.position);
           EntityData.Rotations.Add(entity.transform.rotation);
            
            NetworkAttributes netAttributes = new NetworkAttributes();
            EntityData.NetworkAttributes.Add(netAttributes);

            if (attributes != null)
            {
                for (int i = 0; i < attributes.NetworkAttributes.Count; i++)
                {
                    AttributeValue value = attributes.Get(attributes.NetworkAttributes[i]);
                    switch (value.type)
                    {
                        case AttributeValue.ValueType.Int:
                            netAttributes.intAttributes.Add(new KeyValuePair<string, int>(attributes.NetworkAttributes[i], value.intValue));
                            break;
                        case AttributeValue.ValueType.Float:
                            netAttributes.floatAttributes.Add(new KeyValuePair<string, float>(attributes.NetworkAttributes[i], value.floatValue));
                            break;
                        case AttributeValue.ValueType.String:
                            netAttributes.stringAttributes.Add(new KeyValuePair<string, string>(attributes.NetworkAttributes[i], value.stringValue));
                            break;
                    }
                }
            }
        }

        public void Clear()
        {
            EntityData.Entity.Clear();
            EntityData.Asset.Clear();
            EntityData.Positions.Clear();
            EntityData.Rotations.Clear();
            EntityData.NetworkAttributes.Clear();
        }
    }
}