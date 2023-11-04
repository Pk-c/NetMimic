using NetMimic;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class EntityData
    {
        public List<int> Entity = new List<int>();
        public List<string> Asset = new List<string>();
        public List<Vector3> Positions = new List<Vector3>();
        public List<Quaternion> Rotations = new List<Quaternion>();
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
            }

            return bs;
        }

        public void AddEntry(int id, NetEntity entity)
        {
            EntityData.Entity.Add(id);
            EntityData.Asset.Add(entity.Address);
            EntityData.Positions.Add(entity.transform.position);
            EntityData.Rotations.Add(entity.transform.rotation);
        }

        public void Clear()
        {
            EntityData.Entity.Clear();
            EntityData.Asset.Clear();
            EntityData.Positions.Clear();
            EntityData.Rotations.Clear();
        }
    }
}