using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Phobos.WoT
{
	public class Turret
	{
		private static string[] defaultPrimaryArmor = new string[] { "armor_1", "armor_3", "armor_4" };

		#region Fields
		private IEnumerable<Gun> apGuns = null;
		private IEnumerable<Gun> heGuns = null;
		#endregion Fields

		#region Properties
		public string Name { get; set; }

		public int Tier { get; set; }
		public float Price { get; set; }

		public float YawStart { get; set; }
		public float YawEnd { get; set; }

		/// <summary>Specifies whether the turret is internal (mostly true for SPGs and TDs).</summary>
		public bool IsInternal { get; set; }
		public float ArmorFront { get; set; }
		public float ArmorSides { get; set; }
		public float ArmorBack { get; set; }

		public float Weight { get; set; }
		public int Hp { get; set; }
		public float RotationSpeed { get; set; }
		public float ViewRange { get; set; }

		public bool NotInShop { get; set; }
		public int EmblemSlotCount { get; set; }

		public List<Gun> Guns { get; private set; }
		public IEnumerable<Gun> ApGuns { get { return (this.apGuns == null) ? this.apGuns = this.Guns.Where(g => g.ApDamage > 0) : this.apGuns; } }
		public IEnumerable<Gun> HeGuns { get { return (this.heGuns == null) ? this.heGuns = this.Guns.Where(g => g.HeDamage > 0) : this.heGuns; } }
		#endregion Properties

		#region Constructors
		public Turret() { this.Guns = new List<Gun>(); }
		#endregion Constructors

		#region Methods
		public string ToString(bool detailed, bool multiLine = false)
		{
			string newLine = String.Empty;

			if (multiLine) newLine = Environment.NewLine;

			if (detailed) return base.ToString()
				+ "{"
				+ newLine + "Name = " + this.Name
				+ newLine + ", Tier = " + this.Tier
				+ newLine + ", Price = " + this.Price
				+ newLine + ", YawStart = " + this.YawStart
				+ newLine + ", YawEnd = " + this.YawEnd
				+ newLine + ", IsInternal = " + this.IsInternal
				+ newLine + ", ArmorFront = " + this.ArmorFront
				+ newLine + ", ArmorSides = " + this.ArmorSides
				+ newLine + ", ArmorBack = " + this.ArmorBack
				+ newLine + ", Hp = " + this.Hp
				+ newLine + ", RotationSpeed = " + this.RotationSpeed
				+ newLine + ", ViewRange = " + this.ViewRange
				+ newLine + newLine + ", Guns = " + String.Join(", ", this.Guns.Select(g => g.ToString(detailed, multiLine)))
				+ newLine + "}";

			return this.Name;
		}

		public override string ToString() { return this.ToString(false); }
		#endregion Methods

		#region Static methods
		

		public static Turret LoadFromXml(Tank tank, ItemDatabase db, XmlElement element, string nation)
		{
			if (element.Name.Contains("UE"))
			{
				element.IsEmpty = false;
			}

			float[] yawLimits = element.ParseLimits("yawLimits");

			Turret turret = new Turret
			{
				Name = element.Name,
				Tier = (int)element.ParseSingle("level").Value,
				Price = element.ParseSingle("price").Value,
				YawStart = yawLimits == null ? -360 : yawLimits[0],
				YawEnd = yawLimits == null ? 360 : yawLimits[1],
				Weight = element.ParseSingle("weight").Value,
				Hp = element.ParseInt32("maxHealth").Value,
				RotationSpeed = element.ParseSingle("rotationSpeed").Value,
				ViewRange = element.ParseSingle("circularVisionRadius").Value,

				NotInShop = element.ParseBool("notInShop") ?? false,
				EmblemSlotCount = element.SelectSingleNode("emblemSlots").ChildNodes.Count - 1
			};

			bool firstYaw = true;

			// Guns.
			if (db.Guns.Count > 0)
			{
				foreach (XmlNode node in element.SelectSingleNode("guns").ChildNodes)
				{
					if (node.NodeType == XmlNodeType.Element)
					{
						XmlElement gunElement = (XmlElement)node;
						int? clipCount = gunElement.ParseInt32("clip/count");
						float? clipRate = gunElement.ParseSingle("clip/rate");

						// Pitch.
						float[] pitchLimits = Gun.GetPitch(gunElement);

						// Yaw.
						float[] gunYawLimits = gunElement.ParseLimits("turretYawLimits");

						Gun gun = null;
						if (db.Guns.TryGetValue(nation+"-"+gunElement.Name, out gun))
						{
							float? cRate = gunElement.ParseFloat("clip/rate");

							gun = (Gun)gun.Clone();
							gun.ReloadTime = gunElement.ParseSingle("reloadTime") ?? gun.ReloadTime;
							gun.AimingTime = gunElement.ParseSingle("aimingTime") ?? gun.AimingTime;
							gun.ClipSize = gunElement.ParseInt32("clip/count") ?? gun.ClipSize;
							gun.ClipRate = cRate == null ? gun.ClipRate : (60f / cRate.Value);
							gun.Update();

							// Pitch.
							if (pitchLimits != null)
							{
								gun.MinPitch = pitchLimits[1] * -1;
								gun.MaxPitch = pitchLimits[0] * -1;
							}

							// Yaw.
							if (gunYawLimits != null)
							{
								gun.YawStart = gunYawLimits[0];
								gun.YawEnd = gunYawLimits[1];
							}

							if ((gun.YawStart != 0) && (gun.YawEnd != 0))
							{
								if (firstYaw)
								{
									turret.YawStart = gun.YawStart;
									turret.YawEnd = gun.YawEnd;
									firstYaw = false;
								}
								else
								{
									turret.YawStart = Math.Max(turret.YawStart, gun.YawStart);
									turret.YawEnd = Math.Min(turret.YawEnd, gun.YawEnd);
								}
							}
							
							turret.Guns.Add(gun);
						}
					}
				}
			}

			XmlNode primaryArmorNode = element.SelectSingleNode("primaryArmor");
			//turret.IsInternal = (primaryArmorNode == null) || (turret.YawStart > -40) || (turret.YawEnd < 40);
			turret.IsInternal = turret.NotInShop && ((turret.YawStart > -40) || (turret.YawEnd < 40) || (turret.EmblemSlotCount <= 0));

			// Armor.
			if (!turret.IsInternal)
			{
				turret.IsInternal = false;
				string[] primaryArmor = (primaryArmorNode == null) ? defaultPrimaryArmor : primaryArmorNode.InnerText.Split(' ');

				turret.ArmorFront = element.ParseSingle("armor/" + primaryArmor[0]).Value;
				turret.ArmorSides = element.ParseSingle("armor/" + primaryArmor[1]).Value;
				float? value = element.ParseSingle("armor/" + primaryArmor[2]); turret.ArmorBack = value ?? 0;
			}

			return turret;
		}
		#endregion Static methods
	}
}
/*
	<turrets0>
		<Turret_1_AMX40>
			<level>3</level>
			<price>1850.0</price>
			<yawLimits>-180 180</yawLimits>//gun aim for TDs
			<armor>
				<armor_1>70</armor_1>
				<armor_2>70</armor_2>
				<armor_3>60</armor_3>
				<armor_4>60</armor_4>
				<armor_5>40</armor_5>
				<armor_6>30</armor_6>
				<armor_7>20</armor_7>
				<armor_8>0</armor_8>
				<surveyingDevice>40</surveyingDevice>
			</armor>
			<primaryArmor>armor_1 armor_3 armor_4</primaryArmor>
			<weight>2250</weight>
			<maxHealth>56</maxHealth>
			<rotationSpeed>32</rotationSpeed>
			<turretRotatorHealth>
				<maxHealth>80</maxHealth>
				<maxRegenHealth>40</maxRegenHealth>
				<repairCost>1.0</repairCost>
			</turretRotatorHealth>
			<circularVisionRadius>320</circularVisionRadius>
			<surveyingDeviceHealth>
				<maxHealth>60</maxHealth>
				<maxRegenHealth>30</maxRegenHealth>
				<repairCost>1.0</repairCost>
			</surveyingDeviceHealth>
			<guns>
				<_47mm_SA34>shared<maxAmmo>156</maxAmmo>
					<armor>
						<armor_1>70<vehicleDamageFactor>0.0</vehicleDamageFactor></armor_1>
						<armor_2>60<vehicleDamageFactor>0.0</vehicleDamageFactor></armor_2>
						<armor_3>40<vehicleDamageFactor>0.0</vehicleDamageFactor></armor_3>
						<gun>10</gun>
					</armor>
					<reloadTime>2.3</reloadTime>
					<aimingTime>1.7</aimingTime>
					<shotDispersionFactors>
						<turretRotation>0.14</turretRotation>
						<afterShot>3.5</afterShot>
						<whileGunDamaged>2.0</whileGunDamaged>
					</shotDispersionFactors>
					<shotDispersionRadius>0.53</shotDispersionRadius>
				</_47mm_SA34>
			</guns>
		</Turret_1_AMX40>
	</turrets0>
*/