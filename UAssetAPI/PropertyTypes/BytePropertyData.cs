﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace UAssetAPI.PropertyTypes
{
    public enum BytePropertyType
    {
        Byte,
        FName,
    }

    /// <summary>
    /// Describes a byte or an enumeration value.
    /// </summary>
    public class BytePropertyData : PropertyData
    {
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public BytePropertyType ByteType;

        [JsonProperty]
        public FName EnumType;
        [JsonProperty]
        public byte Value;
        [JsonProperty]
        public FName EnumValue;

        public bool ShouldSerializeValue()
        {
            return ByteType == BytePropertyType.Byte;
        }

        public bool ShouldSerializeEnumValue()
        {
            return ByteType == BytePropertyType.FName;
        }

        public BytePropertyData(FName name) : base(name)
        {

        }

        public BytePropertyData()
        {

        }

        private static readonly FName CurrentPropertyType = new FName("ByteProperty");
        public override FName PropertyType { get { return CurrentPropertyType; } }

        public override void Read(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2 = 0)
        {
            ReadCustom(reader, includeHeader, leng1, leng2, true);
        }

        private void ReadCustom(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2, bool canRepeat)
        {
            if (includeHeader)
            {
                EnumType = reader.ReadFName();
                PropertyGuid = reader.ReadPropertyGuid();
            }

            switch (leng1)
            {
                case 1:
                    ByteType = BytePropertyType.Byte;
                    Value = reader.ReadByte();
                    break;
                case 0:// Should be only seen in maps
                    int nameMapPointer = reader.ReadInt32();
                    reader.BaseStream.Position -= sizeof(int);
                    //maybe also check if there is Enum name in the NameMap
                    if (reader.Asset.GetNameReference(nameMapPointer).ToString().Contains("::"))
                    {
                        ByteType = BytePropertyType.FName;
                        EnumValue = reader.ReadFName();
                        break;
                    } 
                    else
                    {
                        ByteType = BytePropertyType.Byte;
                        Value = reader.ReadByte();
                        break;
                    }
                case 8:
                    ByteType = BytePropertyType.FName;
                    EnumValue = reader.ReadFName();
                    break;
                default:
                    if (canRepeat)
                    {
                        ReadCustom(reader, false, leng2, 0, false);
                        return;
                    }
                    throw new FormatException("Invalid length " + leng1 + " for ByteProperty");
            }
        }

        public override int Write(AssetBinaryWriter writer, bool includeHeader)
        {
            if (includeHeader)
            {
                writer.Write(EnumType);
                writer.WritePropertyGuid(PropertyGuid);
            }

            switch (ByteType)
            {
                case BytePropertyType.Byte:
                    writer.Write((byte)Value);
                    return 1;
                case BytePropertyType.FName:
                    writer.Write(EnumValue);
                    return 8;
                default:
                    throw new FormatException("Invalid BytePropertyType " + ByteType);
            }
        }

        public FName GetEnumBase()
        {
            return EnumType;
        }

        public FName GetEnumFull()
        {
            return EnumValue;
        }

        public override string ToString()
        {
            if (ByteType == BytePropertyType.Byte) return Convert.ToString(Value);
            //return GetEnumFull().Value;
            return Value.ToString();
        }

        public override void FromString(string[] d, UAsset asset)
        {
            EnumType = FName.FromString(d[0]);
            if (byte.TryParse(d[1], out byte res))
            {
                ByteType = BytePropertyType.Byte;
                Value = res;
            }
            else
            {
                ByteType = BytePropertyType.FName;
                EnumValue = FName.FromString(d[1]);
            }
        }
    }
}