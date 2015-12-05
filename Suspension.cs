using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Phobos.WoT
{
	public class Suspension
	{
		#region Properties
		public string Nation { get; set; }
		public string Name { get; set; }
		public int Tier { get; set; }
		public int Price { get; set; }
		public float LeftTrackArmor { get; set; }
		public float RightTrackArmor { get; set; }
		public int Weight { get; set; }
		public int LoadLimit { get; set; }
		public int BrakeForce { get; set; }
		public bool RotatesAroundCenter { get; set; }
		public int RotationSpeed { get; set; }
		#endregion Properties

		#region Methods
		public string ToString(bool detailed)
		{
			if (detailed) return base.ToString()
				+ "{"
				+ "Nation = " + this.Nation
				+ ", Name = " + this.Name
				+ ", Tier = " + this.Tier
				+ ", Price = " + this.Price
				+ ", TrackArmor = " + this.LeftTrackArmor + "·" + this.RightTrackArmor
				+ ", Weight = " + this.Weight
				+ ", LoadLimit = " + this.LoadLimit
				+ ", BrakeForce = " + this.BrakeForce
				+ ", RotatesAroundCenter = " + this.RotatesAroundCenter
				+ ", RotationSpeed = " + this.RotationSpeed
				+ "}";

			return this.Name + ": " + this.LoadLimit + "/" + this.RotationSpeed;
		}

		public override string ToString() { return this.ToString(false); }
		#endregion Methods

		#region Static methods
		public static Suspension LoadFromXml(XmlElement element, string nation)
		{
			return new Suspension
			{
				Nation = nation,
				Name = element.Name,
				Tier = element.ParseInt32("level").Value,
				Price = (int)element.ParseSingle("price"),
				LeftTrackArmor = element.ParseSingle("armor/leftTrack").Value,
				RightTrackArmor = element.ParseSingle("armor/rightTrack").Value,
				Weight = element.ParseInt32("weight").Value,
				LoadLimit = element.ParseInt32("maxLoad").Value,
				BrakeForce = element.ParseInt32("brakeForce").Value,
				RotatesAroundCenter = element.ParseBool("rotationIsAroundCenter").Value,
				RotationSpeed = element.ParseInt32("rotationSpeed").Value
			};
		}
		#endregion Static methods
	}
}
/*
<Chassis_AMX40>
	<level>3</level>
  <price>1750.0</price>
	<armor>
		<leftTrack>10</leftTrack>
		<rightTrack>10</rightTrack>
	</armor>
	<maxClimbAngle>25</maxClimbAngle>
	<weight>4200</weight>
	<maxLoad>20230</maxLoad>
	<terrainResistance>1.5 1.6 2.7</terrainResistance>
	<brakeForce>45500</brakeForce>
	<rotationIsAroundCenter>false</rotationIsAroundCenter>
	<rotationSpeed>28</rotationSpeed>
	<shotDispersionFactors>
		<vehicleMovement>0.32</vehicleMovement>
		<vehicleRotation>0.32</vehicleRotation>
	</shotDispersionFactors>
	<bulkHealthFactor>3.0</bulkHealthFactor>
	<maxHealth>70</maxHealth>
	<maxRegenHealth>55</maxRegenHealth>
	<repairCost>5.0</repairCost>
</Chassis_AMX40>*/