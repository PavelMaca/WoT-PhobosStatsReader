using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Phobos.WoT
{
	public class Gun : ICloneable
	{
		#region Nested types
		public class Shot
		{
			public int Speed { get; set; }
			public int MaxDistance { get; set; }
			public float Gravity { get; set; }
			public int Penetration { get; set; }
			public Shell Shell { get; set; }
		}
		#endregion Nested types

		#region Properties
		public string Nation { get; set; }
		public string Name { get; set; }
		public int Tier { get; set; }
		public int Price { get; set; }
		public float RotationSpeed { get; set; }
		public float ReloadTime { get; set; }
		public float AimingTime { get; set; }
		public float Accuracy { get; set; }
		public int ClipSize { get; set; }
		public float ClipRate { get; set; }
		public float MinPitch { get; set; }
		public float MaxPitch { get; set; }
		public float YawStart { get; set; }
		public float YawEnd { get; set; }
		public int Weight { get; set; }

		public List<Shot> Shots { get; private set; }

		public int ApDamage { get; set; }
		public int HeDamage { get; protected set; }
		public int ApPenetration { get; set; }
		public int HePenetration { get; protected set; }

		public double RateOfFire { get; set; }
		public double ApDamagePerMinute { get { return this.ApDamage * this.RateOfFire; } }
		public double HeDamagePerMinute { get { return this.HeDamage * this.RateOfFire; } }
		public bool IsAutoLoader { get { return this.ClipRate > 0.6f; } }
		#endregion Properties

		#region Constructors
		public Gun() { this.Shots = new List<Shot>(3); }
		#endregion Constructors

		#region Methods
		public string ToString(bool detailed, bool multiLine = false)
		{
			string newLine = String.Empty;

			if (multiLine) newLine = Environment.NewLine;

			string penetration = String.Join(" / ", this.Shots.Select(s => s.Penetration));
			string damage = String.Join(" / ", this.Shots.Select(s => s.Shell.Damage));

			if (detailed) return base.ToString()
				+ "{"
				+ newLine + "Nation = " + this.Nation
				+ newLine + ", Name = " + this.Name
				+ newLine + ", Price = " + this.Price
				+ newLine + ", RotationSpeed = " + this.RotationSpeed
				+ newLine + ", ReloadTime = " + this.ReloadTime
				+ newLine + ", AimingTime = " + this.AimingTime
				+ newLine + ", Accuracy = " + this.Accuracy
				+ newLine + ", ClipSize = " + this.ClipSize
				+ newLine + ", ClipRate = " + this.ClipRate
				+ newLine + ", MinPitch = " + this.MinPitch
				+ newLine + ", MaxPitch = " + this.MaxPitch
				+ newLine + ", Penetration = " + penetration
				+ newLine + ", Damage = " + damage
				+ newLine + "}";

			return this.Name + ": " + penetration + " -> " + damage;
		}

		public override string ToString() { return this.ToString(false); }

		public object Clone() { return this.MemberwiseClone(); }
		#endregion Methods

		#region Static methods
		public static float[] GetPitch(XmlNode element)
		{
			var minPitchArray = element.ParseSingleArray("pitchLimits/minPitch");
			var maxPitchArray = element.ParseSingleArray("pitchLimits/maxPitch");

			var minPitch = minPitchArray == null ? 0 : minPitchArray.Min();
			var maxPitch = maxPitchArray == null ? 0 : maxPitchArray.Max();
			var pitchLimits = element.ParseSingleArray("pitchLimits/text()");

			//TODO: extraPitch?
			if ((pitchLimits == null) || pitchLimits.Length != 2)
			{
				pitchLimits = new float[] { minPitch, maxPitch };
			}

			return pitchLimits;
		}

		public static Gun LoadFromXml(ItemDatabase db, XmlElement element, string nation)
		{
			int tier = element.ParseInt32("level").Value;
			int? clipSize = element.ParseInt32("clip/count");

			// Pitch.
			float[] pitchLimits = Gun.GetPitch(element);

			// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx
			if (element.Name == "OQF_17pdr_Gun_Mk_VII")
			{
				tier = tier + 0;
			}
			// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx

			Gun gun = new Gun
			{
				Nation = nation,
				Name = element.Name,
				Tier = tier,
				Price = element.ParseInt32("price").Value,
				RotationSpeed = Single.Parse(element.SelectSingleNode("rotationSpeed").InnerText),
				ReloadTime = Single.Parse(element.SelectSingleNode("reloadTime").InnerText),
				AimingTime = Single.Parse(element.SelectSingleNode("aimingTime").InnerText),
				Accuracy = Single.Parse(element.SelectSingleNode("shotDispersionRadius").InnerText),
				ClipSize = clipSize.HasValue ? clipSize.Value : 0,
				ClipRate = clipSize.HasValue ? (60f / element.ParseFloat("clip/rate").Value) : 0f,
				MinPitch = pitchLimits[1] * -1,
				MaxPitch = pitchLimits[0] * -1,
				Weight = element.ParseInt32("weight").Value
			};

			// Yaw.
			float[] gunYawLimits = element.ParseLimits("turretYawLimits");
			if (gunYawLimits != null)
			{
				gun.YawStart = gunYawLimits[0];
				gun.YawEnd = gunYawLimits[1];
			}

			if (db.Shells.Count > 0)
			{
				foreach (XmlNode node in element.SelectSingleNode("shots").ChildNodes)
				{
					if (node.NodeType == XmlNodeType.Element)
					{
						XmlElement shotElement = (XmlElement)node;

						Shell shell = null;
						if (db.Shells.TryGetValue(nation+"-"+shotElement.Name, out shell))
						{
							int penetration = (int)shotElement.ParseLimits("piercingPower")[0];

							if (!shell.CostsGold)
							{
								if ((gun.ApPenetration == 0) || (gun.ApPenetration < penetration))
								{
									gun.ApDamage = shell.Damage;
									gun.ApPenetration = penetration;
								}

								if ((shell.ExplosionRadius > 0) && (gun.HeDamage < shell.Damage))
								{
									gun.HeDamage = shell.Damage;
									gun.HePenetration = penetration;
								}
							}

							gun.Shots.Add(new Shot
							{
								Speed = shotElement.ParseInt32("speed").Value,
								MaxDistance = shotElement.ParseInt32("maxDistance").Value,
								Gravity = shotElement.ParseSingle("gravity").Value,
								Penetration = penetration,
								Shell = shell
							});
						}
					}
				}
			}

			gun.Update();

			return gun;
		}

		public static void LoadFromFile(ItemDatabase db, string path, string nation = "unknown")
		{
			DataReader reader = new DataReader();
			XmlDocument doc = reader.Read(path);

			if (doc.DocumentElement.Name != "guns.xml") throw new ArgumentException("The file is not a packed XML containing gun data.", "path");

			foreach (XmlNode e in doc.DocumentElement.SelectSingleNode("shared").ChildNodes) if (e.NodeType == XmlNodeType.Element)
				{
					Gun gun = Gun.LoadFromXml(db, (XmlElement)e, nation);
					db.Guns[nation+"-"+gun.Name] = gun;
				}
		}
		#endregion Static methods

		public void Update()
		{
			if (this.ClipSize > 0) this.RateOfFire = 60d * this.ClipSize / (this.ReloadTime + (this.ClipSize - 1) * this.ClipRate);
			else this.RateOfFire = 60d / this.ReloadTime;
		}
	}
}

/*<_13.2mm_Hotchkiss_mle._1930>
      <userString>#france_vehicles:_13.2mm_Hotchkiss_mle._1930</userString>
      <description>#france_vehicles:_13.2mm_Hotchkiss_mle._1930</description>
      <icon>gui/maps/icons_vehicle_components.dds 5 1</icon>
      <tags>
      </tags>
      <level>1</level>
      <price>1500</price>
      <impulse>0.06</impulse>
      <recoil>
        <lodDist>MEDIUM</lodDist>
        <amplitude>0.2</amplitude>
        <backoffTime>0.1</backoffTime>
        <returnTime>0.1</returnTime>
      </recoil>
      <effects>shot_small</effects>
      <pitchLimits>-20 10</pitchLimits>
      <rotationSpeed>43.75</rotationSpeed>
      <reloadTime>5.71428571428571</reloadTime>
      <maxAmmo>720</maxAmmo>
      <aimingTime>1.94285714285714</aimingTime>
      <clip>
        <count>15</count>
        <rate>450</rate>
      </clip>
      <burst>
        <count>5</count>
        <rate>450</rate>
      </burst>
      <weight>70</weight>
      <maxHealth>18</maxHealth>
      <maxRegenHealth>9</maxRegenHealth>
      <repairCost>1</repairCost>
      <shotDispersionRadius>0.582857142857143</shotDispersionRadius>
      <shotDispersionFactors>
        <turretRotation>0.12</turretRotation>
        <afterShot>1.1</afterShot>
        <whileGunDamaged>2.0</whileGunDamaged>
      </shotDispersionFactors>
      <shots>
        <_13.2mm_Balle_P.>
          <defaultPortion>1.0</defaultPortion>
          <speed>800</speed>
          <gravity>9.81</gravity>
          <maxDistance>350</maxDistance>
          <piercingPower>23 16</piercingPower>
        </_13.2mm_Balle_P.>
        <_13.2mm_Balle_T.P.>
          <defaultPortion>0.0</defaultPortion>
          <speed>1000</speed>
          <gravity>9.81</gravity>
          <maxDistance>350</maxDistance>
          <piercingPower>36 15</piercingPower>
        </_13.2mm_Balle_T.P.>
      </shots>
    </_13.2mm_Hotchkiss_mle._1930>*/