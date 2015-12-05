using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Phobos.WoT
{
	public enum TankType { Light, Medium, Heavy, Spg, TankDestroyer }

	public class Tank
	{
		#region Fields
		public static readonly TankType[] Types = (TankType[])Enum.GetValues(typeof(TankType));
		private Turret lastTurret = null;
		#endregion Fields

		#region Properties
		public string Nation { get; set; }
		public string Id { get; set; }
		public string ShortName { get; set; }
		public bool IsPremium { get; set; }
		public int Price { get; set; }
		/// <summary>Specifies whether the tank is visible in the ingame shop.</summary>
		public bool IsHidden { get; set; }
		public string Tags { get; set; }
		public TankType Type { get; set; }

		public int Tier { get; set; }
		public int Hp { get { return this.HullHp + this.LastTurret.Hp; } }

		public float SpeedLimitForward { get; set; }
		public float SpeedLimitBackward { get; set; }

		public float RepairCost { get; set; }
		public float CrewXpFactor { get; set; }

		public float ArmorFront { get; set; }
		public float ArmorSides { get; set; }
		public float ArmorBack { get; set; }

		public float Weight { get; set; }
		public int HullHp { get; set; }

		public bool CanFitRammer { get; set; }

		public List<Suspension> Suspensions { get; private set; }
		public List<Turret> Turrets { get; private set; }

		public Turret LastTurret { get { return (this.lastTurret == null) ? this.lastTurret = this.Turrets.Last() : this.lastTurret; } }
		#endregion Properties

		#region Constructors
		public Tank()
		{
			this.Turrets = new List<Turret>();
			this.Suspensions = new List<Suspension>();
		}
		#endregion Constructors

		#region Methods
		public string ToString(bool detailed, bool multiLine = false)
		{
			string newLine = String.Empty;

			if (multiLine) newLine = Environment.NewLine;

			if (detailed) return base.ToString()
				+ "{"
				+ newLine + "Nation = " + this.Nation
				+ newLine + ", Name = " + this.Id
				+ newLine + ", IsPremium = " + this.IsPremium
				+ newLine + ", Price = " + this.Price
				+ newLine + ", Tags = " + this.Tags
				+ newLine + ", Type = " + this.Type
				+ newLine + ", Tier = " + this.Tier
				+ newLine + ", Hp = " + this.Hp
				+ newLine + ", SpeedLimitForward = " + this.SpeedLimitForward
				+ newLine + ", SpeedLimitBackward = " + this.SpeedLimitBackward
				+ newLine + ", RepairCost = " + this.RepairCost
				+ newLine + ", CrewXpFactor = " + this.CrewXpFactor
				+ newLine + ", Armor = " + this.ArmorFront + "·" + this.ArmorSides + "·" + this.ArmorBack
				+ newLine + ", Weight = " + this.Weight
				+ newLine + ", HullHp = " + this.HullHp
				+ newLine + newLine + ", Turrets = " + String.Join(", " + newLine, this.Turrets.Select(t => t.ToString(detailed, multiLine)))
				+ newLine + "}";

			return this.Id;
		}

		public override string ToString() { return this.ToString(false); }
		#endregion Methods

		#region Static methods
		public static TankType GetTankType(string type)
		{
			switch(type.ToLowerInvariant())
			{
				case "mediumtank": return TankType.Medium;
				case "lighttank": return TankType.Light;
				case "spg": return TankType.Spg;
				case "heavytank": return TankType.Heavy;
				case "at-spg": return TankType.TankDestroyer;
			}

			throw new ArgumentException("Invalid tank type specified.", "type");
		}

		public static string GetTankType(TankType type)
		{
			switch (type)
			{
				case TankType.Medium: return "mediumTank";
				case TankType.Light: return "lightTank";
				case TankType.Spg: return "Spg";
				case TankType.Heavy: return "heavyTank";
				case TankType.TankDestroyer: return "AT-Spg";
			}

			return "heavyTank";
		}

		public static void LoadFromFile(ItemDatabase db, string path, string nation, XmlElement tankElement)
		{
			DataReader reader = new DataReader();
			XmlDocument doc = reader.Read(path);
			XmlElement element = doc.DocumentElement;

			string tags = tankElement.SelectSingleNode("tags").InnerText;
			string tagsLower = tags.ToLowerInvariant();

			if (tagsLower.Contains("observer")) return;
			string type = tags.Split(' ')[0].Split('\r')[0];

			string[] primaryArmor = element.SelectSingleNode("hull/primaryArmor").InnerText.Split(' ');

bool? isInShop = tankElement.ParseBool("notInShop");

Tank tank = new Tank
{
	Nation = nation,
	Id = tankElement.Name,
	ShortName = ItemDatabase.TankNames[tankElement.Name],
	IsPremium = tankElement.SelectSingleNode("price/gold") != null,
	IsHidden = (isInShop != null) && isInShop.Value,
	Price = tankElement.ParseInt32("price").Value,
	Tags = tags,
	CanFitRammer = tagsLower.Contains("rammer"),
	Type = Tank.GetTankType(type),
	Tier = (int)tankElement.ParseSingle("level").Value,
	SpeedLimitForward = element.ParseSingle("speedLimits/forward").Value,
	SpeedLimitBackward = element.ParseSingle("speedLimits/backward").Value,
	RepairCost = Single.Parse(element.SelectSingleNode("repairCost").InnerText),
	CrewXpFactor = Single.Parse(element.SelectSingleNode("crewXpFactor").InnerText),
	ArmorFront = element.ParseSingle("hull/armor/" + primaryArmor[0]).Value,
	ArmorSides = element.ParseSingle("hull/armor/" + primaryArmor[1]).Value,
	ArmorBack = element.ParseSingle("hull/armor/" + primaryArmor[2]).Value,
	Weight = Single.Parse(element.SelectSingleNode("hull/weight").InnerText),
	HullHp = Int32.Parse(element.SelectSingleNode("hull/maxHealth").InnerText)
};

			foreach (XmlNode node in element.SelectSingleNode("chassis").ChildNodes) if (node.NodeType == XmlNodeType.Element)
			{
				tank.Suspensions.Add(Suspension.LoadFromXml((XmlElement)node, nation));
			}

			foreach (XmlNode node in element.SelectSingleNode("turrets0").ChildNodes)
			{
				if (node.NodeType == XmlNodeType.Element) tank.Turrets.Add(Turret.LoadFromXml(tank, db, (XmlElement)node, nation));
			}

			db.Tanks[tank.Id] = tank;

			// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx
			if (tank.ShortName == "Alecto") tank.ShortName = "Alecto"+"";
			// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx
		}
		#endregion Static methods
	}
}
/*<PzV_PzIV>
    <id>201</id>
    <userString>#germany_vehicles:PzV_PzIV</userString>
    <shortUserString>#germany_vehicles:PzV_PzIV_short</shortUserString>
    <description>#germany_vehicles:PzV_PzIV_descr</description>
    <icon>gui/maps/icons_vehicle_components.dds 1 1</icon>
    <notInShop>true</notInShop>
    <price>3000<gold></gold></price>
    <tags>mediumTank enhancedTorsions5t_user mediumCaliberTankRammer_user wetCombatPack_class1_user</tags>
    <level>6</level>
  </PzV_PzIV>*/
/*
<amx40.xml>
	<speedLimits>
		<forward>50</forward>
		<backward>20</backward>
	</speedLimits>
	<repairCost>4.2</repairCost>
	<crewXpFactor>1.0</crewXpFactor>
	<camouflage><priceFactor>0.8</priceFactor></camouflage>
	<hull>
		<armor>
			<armor_1>70</armor_1>
			<armor_2>60</armor_2>
			<armor_3>65</armor_3>
			<armor_4>40</armor_4>
			<armor_5>45</armor_5>
			<armor_6>40</armor_6>
			<armor_7>25</armor_7>
			<armor_8>40</armor_8>
			<armor_9>25</armor_9>
			<armor_10>20</armor_10>
			<armor_11>50</armor_11>
			<armor_12>30<vehicleDamageFactor>0.0</vehicleDamageFactor></armor_12>
			<armor_13>15<vehicleDamageFactor>0.0</vehicleDamageFactor></armor_13>
			<surveyingDevice>40</surveyingDevice>
		</armor>
		<primaryArmor>armor_1 armor_3 armor_4</primaryArmor>
		<weight>12750</weight>
		<maxHealth>224</maxHealth>
		<ammoBayHealth>
			<maxHealth>125</maxHealth>
			<maxRegenHealth>85</maxRegenHealth>
			<repairCost>1.0</repairCost>
		</ammoBayHealth>
	</hull>
	<chassis>
		...
	</chassis>
	<turrets0>
		...
	</turrets0>
	<engines>
		<Aster>shared<unlocks><engine>Somua_LM<cost>400.0</cost></engine></unlocks></Aster>
	</engines>
	<fuelTanks>
		<Medium>shared</Medium>
	</fuelTanks>
	<radios>
		<ER_52>shared<unlocks><radio>ER_53<cost>610.0</cost></radio></unlocks></ER_52>
	</radios>
*/