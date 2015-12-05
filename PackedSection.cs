using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Phobos.WoT
{
	public class PackedSection
	{
		public static readonly Int32 Packed_Header = 0x62a14e45;
		public const int MAX_LENGTH = 256;

		public class DataDescriptor
		{
			public readonly int address;
			public readonly int end;
			public readonly int type;

			public DataDescriptor(int end, int type, int address)
			{
				this.end = end;
				this.type = type;
				this.address = address;
			}

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder("[");
				sb.Append("0x");
				sb.Append(Convert.ToString(end, 16));
				sb.Append(", ");
				sb.Append("0x");
				sb.Append(Convert.ToString(type, 16));
				sb.Append("]@0x");
				sb.Append(Convert.ToString(address, 16));
				return sb.ToString();
			}
		}

		public class ElementDescriptor
		{
			public readonly int nameIndex;
			public readonly DataDescriptor dataDescriptor;

			public ElementDescriptor(int nameIndex, DataDescriptor dataDescriptor)
			{
				this.nameIndex = nameIndex;
				this.dataDescriptor = dataDescriptor;
			}

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder("[");
				sb.Append("0x");
				sb.Append(Convert.ToString(nameIndex, 16));
				sb.Append(":");
				sb.Append(dataDescriptor);
				return sb.ToString();
			}
		}

		public string readStringTillZero(BinaryReader reader)
		{
			char[] work = new char[MAX_LENGTH];

			int i = 0;

			char c = reader.ReadChar();
			while (c != Convert.ToChar(0x00))
			{
				work[i++] = c;
				c = reader.ReadChar();
			}
			return new string(work, 0, i);
		}

		public List<string> readDictionary(BinaryReader reader)
		{
			List<string> dictionary = new List<string>();
			int counter = 0;
			string text = readStringTillZero(reader);

			while (!(text.Length == 0))
			{
				dictionary.Add(text);
				text = readStringTillZero(reader);
				counter++;
			}
			return dictionary;
		}

		public int readLittleEndianShort(BinaryReader reader)
		{
			if (reader.BaseStream.Position + 2 > reader.BaseStream.Length)
			{
				Console.WriteLine(String.Format("Reading ({0}) past the end of the stream ({1})!", reader.BaseStream.Position, reader.BaseStream.Length));
				return -1;
			}

			int LittleEndianShort = reader.ReadInt16();
			return LittleEndianShort;
		}

		public int readLittleEndianInt(BinaryReader reader)
		{
			if (reader.BaseStream.Position + 4 > reader.BaseStream.Length)
			{
				Console.WriteLine("Reading past the end of the stream!");
				return -1;
			}

			int LittleEndianInt = reader.ReadInt32();
			return LittleEndianInt;
		}

		public DataDescriptor readDataDescriptor(BinaryReader reader)
		{
			int selfEndAndType = readLittleEndianInt(reader);
			return new DataDescriptor(selfEndAndType & 0x0fffffff, selfEndAndType >> 28, (int)reader.BaseStream.Position);
		}

		public ElementDescriptor[] readElementDescriptors(BinaryReader reader, int number)
		{
			ElementDescriptor[] elements = new ElementDescriptor[number];
			for (int i = 0; i < number; i++)
			{
				int nameIndex = readLittleEndianShort(reader);
				DataDescriptor dataDescriptor = readDataDescriptor(reader);
				if (dataDescriptor.type != -1) elements[i] = new ElementDescriptor(nameIndex, dataDescriptor);
			}
			return elements;
		}

		public string readString(BinaryReader reader, int lengthInBytes)
		{
			string rString = new string(reader.ReadChars(lengthInBytes), 0, lengthInBytes);

			return rString;
		}

		public string readNumber(BinaryReader reader, int lengthInBytes)
		{
			string Number = "";
			switch (lengthInBytes)
			{
				case 1:
					Number = Convert.ToString(reader.ReadSByte());
					break;
				case 2:
					Number = Convert.ToString(readLittleEndianShort(reader));
					break;
				case 4:
					Number = Convert.ToString(readLittleEndianInt(reader));
					break;
				default:
					Number = "0";
					break;
			}
			return Number;

		}

		public float readLittleEndianFloat(BinaryReader reader)
		{
			float LittleEndianFloat = reader.ReadSingle();
			return LittleEndianFloat;
		}

		public string readFloats(BinaryReader reader, int lengthInBytes)
		{
			int n = lengthInBytes / 4;

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < n; i++)
			{

				if (i != 0)
				{
					sb.Append(" ");
				}
				float rFloat = readLittleEndianFloat(reader);
				sb.Append(rFloat.ToString("0.000000"));
			}
			return sb.ToString();
		}


		public bool readBoolean(BinaryReader reader, int lengthInBytes)
		{
			bool @bool = lengthInBytes == 1;
			if (@bool)
			{
				if (reader.ReadSByte() != 1)
				{
					throw new System.ArgumentException("Boolean error");
				}
			}

			return @bool;
		}

		public string readBase64(BinaryReader reader, int lengthInBytes)
		{
			/*sbyte[] bytes = new sbyte[lengthInBytes];
			for (int i = 0; i < lengthInBytes; i++) bytes[i] = reader.ReadSByte();
			string s1 = byteArrayToBase64(bytes);*/

			byte[] data = new byte[lengthInBytes];
			for (int i = 0; i < lengthInBytes; i++) data[i] = reader.ReadByte();

			return Convert.ToBase64String(data);
		}

		public string readAndToHex(BinaryReader reader, int lengthInBytes)
		{
			sbyte[] bytes = new sbyte[lengthInBytes];
			for (int i = 0; i < lengthInBytes; i++)
			{
				bytes[i] = reader.ReadSByte();
			}
			StringBuilder sb = new StringBuilder("[ ");
			foreach (byte b in bytes)
			{
				sb.Append(Convert.ToString((b & 0xff), 16));
				sb.Append(" ");
			}
			sb.Append("]L:");
			sb.Append(lengthInBytes);

			return sb.ToString();
		}

		public int readData(BinaryReader reader, List<string> dictionary, XmlNode element, XmlDocument xDoc, int offset, DataDescriptor dataDescriptor)
		{
			int lengthInBytes = dataDescriptor.end - offset;
			if (dataDescriptor.type == 0x0)
			{
				// Element                
				readElement(reader, element, xDoc, dictionary);
			}
			else if (dataDescriptor.type == 0x1)
			{
				// String
				element.InnerText = readString(reader, lengthInBytes);

			}
			else if (dataDescriptor.type == 0x2)
			{
				// Integer number
				element.InnerText = readNumber(reader, lengthInBytes);
			}
			else if (dataDescriptor.type == 0x3)
			{
				// Floats
				string str = readFloats(reader, lengthInBytes);

				string[] strData = str.Split(' ');
				if (strData.Length == 12)
				{
					XmlNode row0 = xDoc.CreateElement("row0");
					XmlNode row1 = xDoc.CreateElement("row1");
					XmlNode row2 = xDoc.CreateElement("row2");
					XmlNode row3 = xDoc.CreateElement("row3");
					row0.InnerText = strData[0] + " " + strData[1] + " " + strData[2];
					row1.InnerText = strData[3] + " " + strData[4] + " " + strData[5];
					row2.InnerText = strData[6] + " " + strData[7] + " " + strData[8];
					row3.InnerText = strData[9] + " " + strData[10] + " " + strData[11];
					element.AppendChild(row0);
					element.AppendChild(row1);
					element.AppendChild(row2);
					element.AppendChild(row3);
				}
				else
				{
					element.InnerText = str;
				}
			}
			else if (dataDescriptor.type == 0x4)
			{
				// Boolean

				if (readBoolean(reader, lengthInBytes))
				{
					element.InnerText = "true";
				}
				else
				{
					element.InnerText = "false";
				}

			}
			else if (dataDescriptor.type == 0x5)
			{
				// Base64
				element.InnerText = readBase64(reader, lengthInBytes);
			}
			else
			{
				throw new System.ArgumentException("Unknown type of \"" + element.Name + ": " + dataDescriptor.ToString() + " " + readAndToHex(reader, lengthInBytes));
			}

			return dataDescriptor.end;
		}

		public void readElement(BinaryReader reader, XmlNode element, XmlDocument xDoc, List<string> dictionary)
		{
			int childrenNmber = readLittleEndianShort(reader);

			if (childrenNmber == 0)//"Continental_AOS-895-1"
			{
				Console.WriteLine("Parsing corrupted record");
				reader.BaseStream.Position -= 3;
				childrenNmber = readLittleEndianShort(reader);
			}

			DataDescriptor selfDataDescriptor = readDataDescriptor(reader);
			ElementDescriptor[] children = readElementDescriptors(reader, childrenNmber);

			int offset = readData(reader, dictionary, element, xDoc, 0, selfDataDescriptor);

			foreach (ElementDescriptor elementDescriptor in children)
			{
				XmlNode child = xDoc.CreateElement(dictionary[elementDescriptor.nameIndex]);
				offset = readData(reader, dictionary, child, xDoc, offset, elementDescriptor.dataDescriptor);
				element.AppendChild(child);
			}

		}
	}
}