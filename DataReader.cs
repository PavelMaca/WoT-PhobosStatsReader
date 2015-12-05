using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace Phobos.WoT
{
	public class DataReader
	{
		private PackedSection ps = new PackedSection();
		private PrimitiveFile pf = new PrimitiveFile();

		public XmlDocument Read(string path)
		{
			string fileName = Path.GetFileName(path);

			XmlDocument xDoc = new XmlDocument();

			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			using (BinaryReader reader = new BinaryReader(fs))
			{
				Int32 header = reader.ReadInt32();

				if (header == PackedSection.Packed_Header)
				{
					reader.ReadSByte();
					List<string> dictionary = ps.readDictionary(reader);

					XmlNode xmlroot = xDoc.CreateNode(XmlNodeType.Element, fileName, "");
					ps.readElement(reader, xmlroot, xDoc, dictionary);
					xDoc.AppendChild(xmlroot);
				}
				else if (header == PrimitiveFile.BinaryHeader)
				{
					XmlNode xmlprimitives = xDoc.CreateNode(XmlNodeType.Element, "primitives", "");
					pf.ReadPrimitives(reader, xmlprimitives, xDoc);

					xDoc.AppendChild(xmlprimitives);
				}
			}

			return xDoc;
			// Phobos:
			/*txtOut.Clear();

			string[] primaryArmor = xDoc.DocumentElement.SelectSingleNode("hull/primaryArmor").InnerText.Split(' ');
			int[] hull = new int[primaryArmor.Length];

			for (int i = 0; i < primaryArmor.Length; i++) hull[i] = Int32.Parse(xDoc.DocumentElement.SelectSingleNode("hull//armor//" + primaryArmor[i]).InnerText);

			string s = "Hull: ";
			foreach (int a in hull) s += a + "-";
			s = s.Substring(0, s.Length - 1);
			s += "\r\n";
			txtOut.AppendText(s);

			int hullHp = Int32.Parse(xDoc.DocumentElement.SelectSingleNode("hull/maxHealth").InnerText);
			txtOut.AppendText("HP: " + hullHp + "\r\n");

			List<int> levels = new List<int>(2);*/
			//foreach (XmlElement e in xDoc.DocumentElement.SelectNodes("chassis/*/level")) levels.Add(Int32.Parse(e.InnerText));

			//txtOut.AppendText("Levels: " + levels.Last().ToString());
			/*tanks.Add(new Tank{ FileName = @"usa-M3_Stuart", Nation = "us", Type = "Light", Name = "M3 Stuart", Tier = 3, Hp = 240,
				ApPenetration = 56, ApDamage = 40,
				TurretFront = 38, TurretSides = 32, TurretBack = 32,
				HullFront = 38, HullSides = 25, HullBack = 25 });*/
		}
	}
}
