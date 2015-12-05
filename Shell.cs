using System;
using System.Xml;

namespace Phobos.WoT
{
	public enum ShellKind { AP, APCR, HE, HEAT, AT }

	public class Shell
	{
		public string Nation { get; set; }
		public string Name { get; set; }
		public int Price { get; set; }
		public ShellKind Kind { get; set; }
		public float Caliber { get; set; }
		public float ExplosionRadius { get; set; }
		public bool IsTracer { get; set; }
		public int Damage { get; set; }
		public int DeviceDamage { get; set; }
		public bool CostsGold { get; set; }

		public string ToString(bool detailed)
		{
			if (detailed) return base.ToString()
				+ "{"
				+ "Nation = " + this.Nation
				+ ", Name = " + this.Name
				+ ", Price = " + this.Price
				+ ", Kind = " + ItemDatabase.ShellTypes[this.Kind]
				+ ", Caliber = " + this.Caliber
				+ ", ExplosionRadius = " + this.ExplosionRadius
				+ ", IsTracer = " + this.IsTracer
				+ ", ApDamage = " + this.Damage
				+ ", DeviceDamage = " + this.DeviceDamage
				+ ", CostsGold = " + this.CostsGold
				+ "}";

			return this.Name + ": " + this.Damage;
		}

		public override string ToString() { return this.ToString(false); }

		public static ShellKind GetShellKind(string shellKind)
		{
			if (shellKind == "ARMOR_PIERCING") return ShellKind.AP;
			if (shellKind == "ARMOR_PIERCING_CR") return ShellKind.APCR;
			if (shellKind == "HIGH_EXPLOSIVE") return ShellKind.HE;
			if (shellKind == "HOLLOW_CHARGE") return ShellKind.HEAT;
			if (shellKind == "AT-Spg") return ShellKind.AT;

			throw new ArgumentException("\"" + shellKind + "\" is not a valid shell kind!", "shellKind");
		}

		public static Shell LoadFromXml(XmlElement element, string nation)
		{
			float? explosionRadius = element.ParseSingle("explosionRadius");

			return new Shell
			{
				Nation = nation,
				Name = element.Name,
				Price = Int32.Parse(element.SelectSingleNode("price").InnerText),
				Kind = GetShellKind(element.SelectSingleNode("kind").InnerText),
				Caliber = Single.Parse(element.SelectSingleNode("caliber").InnerText),
				ExplosionRadius = explosionRadius ?? 0f,
				IsTracer = element.SelectSingleNode("isTracer").InnerText == "true",
				Damage = Int32.Parse(element.SelectSingleNode("damage/armor").InnerText),
				DeviceDamage = Int32.Parse(element.SelectSingleNode("damage/devices").InnerText),
				CostsGold = element.SelectSingleNode("price").InnerXml.ToLowerInvariant().Contains("gold")
			};
		}

		public static void LoadFromFile(ItemDatabase db, string path, string nation = "unknown")
		{
			DataReader reader = new DataReader();
			XmlDocument doc = reader.Read(path);

			if (doc.DocumentElement.Name != "shells.xml") throw new ArgumentException("The file is not a packed XML containing shell data.", "path");

			doc.DocumentElement.RemoveChild(doc.DocumentElement.SelectSingleNode("icons"));

			foreach (XmlNode e in doc.DocumentElement.ChildNodes) if (e.NodeType == XmlNodeType.Element)
			{
				Shell shell = Shell.LoadFromXml((XmlElement)e, nation);
				db.Shells[nation+"-"+shell.Name] = shell;
			}
		}
	}
}
/*<_75mm_Prf1926>
    <id>0</id>
    <userString>#france_vehicles:_75mm_Prf1926</userString>
    <description>#france_vehicles:_75mm_Prf1926_descr</description>
    <icon>ap</icon>
    <price>46</price>
    <kind>ARMOR_PIERCING</kind>
    <caliber>75</caliber>
    <isTracer>true</isTracer>
    <effects>mainArmorPiercing</effects>
    <damage>
      <armor>110</armor>
      <devices>100</devices>
    </damage>
  </_75mm_Prf1926>*/