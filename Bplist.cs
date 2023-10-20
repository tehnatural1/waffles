
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace BplistConverter
{
    public class BplistConverter
    {
        public class BplistUID
        {
            public int Value { get; }

            public BplistUID(int value)
            {
                Value = value;
            }
        }

        public class BplistError : Exception
        {
            public BplistError(string message) : base(message)
            {
            }
        }

        private static object _objectConverter = null;

        public static void SetObjectConverter(Func<object, object> function)
        {
            _objectConverter = function;
        }

        private static object DecodeObject(BinaryReader reader, long offset, byte collectionOffsetSize, List<long> offsetTable)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte typeByte = reader.ReadByte();

            if (typeByte == 0x00) // Null
            {
                return null;
            }
            else if (typeByte == 0x08) // False
            {
                return false;
            }
            else if (typeByte == 0x09) // True
            {
                return true;
            }
            else if (typeByte == 0x0F) // Fill
            {
                throw new BplistError("Fill type not currently supported at offset " + reader.BaseStream.Position);
            }
            else if ((typeByte & 0xF0) == 0x10) // Int
            {
                int intLength = 1 << (typeByte & 0x0F);
                byte[] intBytes = reader.ReadBytes(intLength);
                return DecodeMultibyteInt(intBytes);
            }
            else if ((typeByte & 0xF0) == 0x20) // Float
            {
                int floatLength = 1 << (typeByte & 0x0F);
                byte[] floatBytes = reader.ReadBytes(floatLength);
                return DecodeFloat(floatBytes);
            }
            else if ((typeByte & 0xFF) == 0x33) // Date
            {
                byte[] dateBytes = reader.ReadBytes(8);
                double dateValue = DecodeFloat(dateBytes);
                try
                {
                    return new DateTime(2001, 1, 1).Add(TimeSpan.FromSeconds(dateValue));
                }
                catch (OverflowException)
                {
                    return DateTime.MinValue;
                }
            }
            else if ((typeByte & 0xF0) == 0x40) // Data
            {
                int dataLength;
                if ((typeByte & 0x0F) != 0x0F)
                {
                    dataLength = typeByte & 0x0F;
                }
                else
                {
                    byte intTypeByte = reader.ReadByte();
                    if ((intTypeByte & 0xF0) != 0x10)
                    {
                        throw new BplistError("Long Data field definition not followed by int type at offset " + reader.BaseStream.Position);
                    }
                    int intLength = 1 << (intTypeByte & 0x0F);
                    byte[] intBytes = reader.ReadBytes(intLength);
                    dataLength = DecodeMultibyteInt(intBytes, false);
                }
                return reader.ReadBytes(dataLength);
            }
            else if ((typeByte & 0xF0) == 0x50) // ASCII
            {
                int asciiLength;
                if ((typeByte & 0x0F) != 0x0F)
                {
                    asciiLength = typeByte & 0x0F;
                }
                else
                {
                    byte intTypeByte = reader.ReadByte();
                    if ((intTypeByte & 0xF0) != 0x10)
                    {
                        throw new BplistError("Long ASCII field definition not followed by int type at offset " + reader.BaseStream.Position);
                    }
                    int intLength = 1 << (intTypeByte & 0x0F);
                    byte[] intBytes = reader.ReadBytes(intLength);
                    asciiLength = DecodeMultibyteInt(intBytes, false);
                }
                byte[] asciiBytes = reader.ReadBytes(asciiLength);
                return Encoding.ASCII.GetString(asciiBytes);
            }
            else if ((typeByte & 0xF0) == 0x60) // UTF-16
            {
                int utf16Length;
                if ((typeByte & 0x0F) != 0x0F)
                {
                    utf16Length = (typeByte & 0x0F) * 2;
                }
                else
                {
                    byte intTypeByte = reader.ReadByte();
                    if ((intTypeByte & 0xF0) != 0x10)
                    {
                        throw new BplistError("Long UTF-16 field definition not followed by int type at offset " + reader.BaseStream.Position);
                    }
                    int intLength = 1 << (intTypeByte & 0x0F);
                    byte[] intBytes = reader.ReadBytes(intLength);
                    utf16Length = DecodeMultibyteInt(intBytes, false) * 2;
                }
                byte[] utf16Bytes = reader.ReadBytes(utf16Length);
                return Encoding.BigEndianUnicode.GetString(utf16Bytes);
            }
            else if ((typeByte & 0xF0) == 0x80) // UID
            {
                int uidLength = (typeByte & 0x0F) + 1;
                byte[] uidBytes = reader.ReadBytes(uidLength);
                return new BplistUID(DecodeMultibyteInt(uidBytes, false));
            }
            else if ((typeByte & 0xF0) == 0xA0) // Array
            {
                int arrayCount;
                if ((typeByte & 0x0F) != 0x0F)
                {
                    arrayCount = typeByte & 0x0F;
                }
                else
                {
                    byte intTypeByte = reader.ReadByte();
                    if ((intTypeByte & 0xF0) != 0x10)
                    {
                        throw new BplistError("Long Array field definition not followed by int type at offset " + reader.BaseStream.Position);
                    }
                    int intLength = 1 << (intTypeByte & 0x0F);
                    byte[] intBytes = reader.ReadBytes(intLength);
                    arrayCount = DecodeMultibyteInt(intBytes, false);
                }
                List<long> arrayRefs = new List<long>();
                for (int i = 0; i < arrayCount; i++)
                {
                    arrayRefs.Add(DecodeMultibyteInt(reader.ReadBytes(collectionOffsetSize), false));
                }
                return arrayRefs.Select(objRef => DecodeObject(reader, offsetTable[(int)objRef], collectionOffsetSize, offsetTable)).ToList();
            }
            else if ((typeByte & 0xF0) == 0xC0) // Set
            {
                int setCount;
                if ((typeByte & 0x0F) != 0x0F)
                {
                    setCount = typeByte & 0x0F;
                }
                else
                {
                    byte intTypeByte = reader.ReadByte();
                    if ((intTypeByte & 0xF0) != 0x10)
 {
                        throw new BplistError("Long Set field definition not followed by int type at offset " + reader.BaseStream.Position);
                    }
                    int intLength = 1 << (intTypeByte & 0x0F);
                    byte[] intBytes = reader.ReadBytes(intLength);
                    setCount = DecodeMultibyteInt(intBytes, false);
                }
                List<long> setRefs = new List<long>();
                for (int i = 0; i < setCount; i++)
                {
                    setRefs.Add(DecodeMultibyteInt(reader.ReadBytes(collectionOffsetSize), false));
                }
                return setRefs.Select(objRef => DecodeObject(reader, offsetTable[(int)objRef], collectionOffsetSize, offsetTable)).ToList();
            }
            else if ((typeByte & 0xF0) == 0xD0) // Dict
            {
                int dictCount;
                if ((typeByte & 0x0F) != 0x0F)
                {
                    dictCount = typeByte & 0x0F;
                }
                else
                {
                    byte intTypeByte = reader.ReadByte();
                    if ((intTypeByte & 0xF0) != 0x10)
                    {
                        throw new BplistError("Long Dict field definition not followed by int type at offset " + reader.BaseStream.Position);
                    }
                    int intLength = 1 << (intTypeByte & 0x0F);
                    byte[] intBytes = reader.ReadBytes(intLength);
                    dictCount = DecodeMultibyteInt(intBytes, false);
                }
                List<long> keyRefs = new List<long>();
                for (int i = 0; i < dictCount; i++)
                {
                    keyRefs.Add(DecodeMultibyteInt(reader.ReadBytes(collectionOffsetSize), false));
                }
                List<long> valueRefs = new List<long>();
                for (int i = 0; i < dictCount; i++)
                {
                    valueRefs.Add(DecodeMultibyteInt(reader.ReadBytes(collectionOffsetSize), false));
                }

                Dictionary<object, object> dictResult = new Dictionary<object, object>();
                for (int i = 0; i < dictCount; i++)
                {
                    object key = DecodeObject(reader, offsetTable[(int)keyRefs[i]], collectionOffsetSize, offsetTable);
                    object val = DecodeObject(reader, offsetTable[(int)valueRefs[i]], collectionOffsetSize, offsetTable);
                    dictResult[key] = val;
                }
                return dictResult;
            }
            else
            {
                return null;
            }
        }

        private static int DecodeMultibyteInt(byte[] bytes, bool signed = true)
        {
            int length = bytes.Length;
            if (length == 1)
            {
                return signed ? (sbyte)bytes[0] : bytes[0];
            }
            else if (length == 2)
            {
                return BitConverter.ToInt16(bytes, 0);
            }
            else if (length == 4)
            {
                return BitConverter.ToInt32(bytes, 0);
            }
            else if (length == 8)
            {
                return (int)BitConverter.ToInt64(bytes, 0);
            }
            else if (length == 16)
            {
                long result = BitConverter.ToInt64(bytes, 0);
                if ((bytes[0] & 0x80) != 0 && signed)
                {
                    result -= 0x10000000000000000;
                }
                return (int)result;
            }
            else
            {
                throw new BplistError("Cannot decode multibyte int of length " + length);
            }
        }

        private static double DecodeFloat(byte[] bytes, bool signed = true)
        {
            int length = bytes.Length;
            if (length == 4)
            {
                return BitConverter.ToSingle(bytes, 0);
            }
            else if (length == 8)
            {
                return BitConverter.ToDouble(bytes, 0);
            }
            else
            {
                throw new BplistError("Cannot decode float of length " + length);
            }
        }

        public static object Load(Stream stream)
        {
            if (stream.Length < 8)
            {
                throw new BplistError("Bad file header");
            }

            BinaryReader reader = new BinaryReader(stream);

            // Check magic number
            if (Encoding.ASCII.GetString(reader.ReadBytes(8)) != "bplist00")
            {
                throw new BplistError("Bad file header");
            }

            // Read trailer
            stream.Seek(-32, SeekOrigin.End);
            byte offsetIntSize = reader.ReadByte();
            byte collectionOffsetSize = reader.ReadByte();
            ulong objectCount = reader.ReadUInt64();
            ulong topLevelObjectIndex = reader.ReadUInt64();
            ulong offsetTableOffset = reader.ReadUInt64();

            // Read offset table
            stream.Seek((long)offsetTableOffset, SeekOrigin.Begin);
            List<long> offsetTable = new List<long>();
            for (int i = 0; i < objectCount; i++)
            {
                offsetTable.Add(DecodeMultibyteInt(reader.ReadBytes(offsetIntSize), false));
            }

            return DecodeObject(reader, offsetTable[(int)topLevelObjectIndex], collectionOffsetSize, offsetTable);
        }
    }
}
