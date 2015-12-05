using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phobos.WoT
{
	public class TankStats
	{
		public static bool OnlyBasicStats { get; set; }

		#region Properties
		// Tank.
		public string Id { get; protected set; }
		public string Name { get; protected set; }
		public TankType Type { get; protected set; }
		public string Nation { get; protected set; }

		public int Tier { get; protected set; }
		public int Hp { get; protected set; }

		public int HullFront { get; protected set; }
		public int HullSides { get; protected set; }
		public int HullBack { get; protected set; }

		public float Speed { get; protected set; }

		public bool IsPremium { get; protected set; }
		public bool IsHidden { get; protected set; }

		public float Weight { get; protected set; }

		public bool CanFitRammer { get; protected set; }

		// Suspension
		public int LoadLimit { get; protected set; }
		public int BrakeForce { get; protected set; }
		public bool RotatesAroundCenter { get; protected set; }
		public int RotationSpeed { get; protected set; }

		// Turret.
		public bool IsTurretInternal { get; protected set; }
		public float TurretRotationSpeed { get; protected set; }
		public float ViewRange { get; protected set; }

		public int TurretFront { get; protected set; }
		public int TurretSides { get; protected set; }
		public int TurretBack { get; protected set; }

		// Gun
		public Gun ApGun { get; protected set; }
		public Gun HeGun { get; protected set; }

		public string Hull { get { return this.HullFront + "·" + this.HullSides + "·" + this.HullBack; } }
		public string Turret { get { return this.TurretFront + "·" + this.TurretSides + "·" + this.TurretBack; } }
		public string FileName { get { return this.Nation + "-" + this.Id + ".png"; } }

		// If a howitzer gun is available and has much higher damage than the AP one, use it instead.
		public bool IsUsingHe { get { return (this.ApGun == null) || ((this.HeGun != null) && (this.HeGun.HeDamage > (3 * this.ApGun.ApDamage)) && (this.HeGun.HeDamagePerMinute > 1.22 * this.ApGun.ApDamagePerMinute) && (this.ApGun.ApPenetration <= 2 * this.HeGun.HePenetration)); } }

		public bool IsAutoloader { get { return ((this.ApGun != null) && this.ApGun.IsAutoLoader) || ((this.HeGun != null) && this.HeGun.IsAutoLoader); } }
		#endregion Properties

		#region Constructors
		public TankStats(Tank tank)
		{
			// Tank.
			this.Id = tank.Id;
			this.Name = ItemDatabase.TankNames[tank.Id];
			////////////////////////////////////////////////
			//if (this.Name == "T71") this.Name += "";
			////////////////////////////////////////////////
			this.Type = tank.Type;
			this.Nation = tank.Nation;

			this.Tier = tank.Tier;
			this.Hp = tank.Hp;

			this.HullFront = (int)tank.ArmorFront;
			this.HullSides = (int)tank.ArmorSides;
			this.HullBack = (int)tank.ArmorBack;

			this.Speed = tank.SpeedLimitForward;

			this.IsPremium = tank.IsPremium;
			this.IsHidden = tank.IsHidden;

			this.CanFitRammer = tank.CanFitRammer;

			// Suspension
			Suspension s = tank.Suspensions.Last();
			this.LoadLimit = s.LoadLimit;
			this.BrakeForce = s.BrakeForce;
			this.RotatesAroundCenter = s.RotatesAroundCenter;
			this.RotationSpeed = s.RotationSpeed;

			// Turret.
			Turret turret = tank.LastTurret;

			this.IsTurretInternal = turret.IsInternal;
			this.TurretRotationSpeed = turret.RotationSpeed;
			this.ViewRange = turret.ViewRange;

			this.TurretFront = (int)turret.ArmorFront;
			this.TurretSides = (int)turret.ArmorSides;
			this.TurretBack = (int)turret.ArmorBack;

			// AP gun.
			if ((tank.Type != TankType.Spg) && turret.ApGuns.Any())
			{
				Gun penGun = turret.ApGuns.OrderByDescending(g => g.ApPenetration).ThenByDescending(g => g.ApDamage).ThenByDescending(g => g.RateOfFire).First();
				Gun dmgGun = turret.ApGuns.Where(g => g.ApPenetration >= 0.78f * penGun.ApPenetration).OrderByDescending(g => g.ApDamage).ThenByDescending(g => g.ApPenetration).ThenByDescending(g => g.ApDamagePerMinute).First();

				if (penGun == dmgGun)
				{
					this.ApGun = penGun;
				}
				else
				{
					double penetrationRatio = (double)penGun.ApPenetration / (double)dmgGun.ApPenetration;
					double damagePerMinuteRatio = penGun.ApDamagePerMinute / dmgGun.ApDamagePerMinute;
					double damageRatio = penGun.ApDamage / (double)dmgGun.ApDamage;
					double accuracyRatio = dmgGun.Accuracy / penGun.Accuracy;
					double efficiency = 0.45f * penetrationRatio + (0.21f * damageRatio + 0.14f * damagePerMinuteRatio) + 0.20f * accuracyRatio;

					this.ApGun = (efficiency >= 1f) ? penGun : dmgGun;
				}
			}

			// HE gun.
			if (turret.HeGuns.Any())
			{
				this.HeGun = turret.HeGuns.OrderByDescending(g => g.HeDamage).ThenByDescending(g => g.HePenetration).ThenByDescending(g => g.RateOfFire).First();
			}

			this.Weight = tank.Weight + s.Weight + turret.Weight + (this.IsUsingHe ? this.HeGun.Weight : this.ApGun.Weight);
		}
		#endregion Constructors

		#region Methods
		public static double CalculateRealStatValue(double value, double crewSkill, bool progressive = false)
		{
			//Reload time is a degressive stat. Thus we calculate 2.286d * 0.875d / (0.00375d * crewSkill + 0.5). We obtain as a result our effective (rounded) reload time of 2.47s. 
			//progressive stat. Thus we calculate 262m / 0.875 * (0.00375d * crewSkill + 0.5).
			double skill = 0.00375d * crewSkill + 0.5;
			return progressive ? value / 0.875d * skill : value * 0.875d / skill;
		}

		/// <summary>Compares the current tank with another one by its stats.</summary>
		/// <param name="other">The stats of the other tank.</param>
		/// <param name="sb">A <see cref="T:System.Text.StringBuilder"/> to write the differences to.</param>
		/// <param name="comparedStats">The tank stats that should be compared. If none are specified, all are compared. Optional.</param>
		/// <returns><c>true</c> if the tank stats are different; otherwise, <c>false</c>.</returns>
		public bool CompareTo(TankStats other, StringBuilder sb, params string[] comparedStats)
		{
			bool isDifferent = false;
			List<string> differences = new List<string>();

			// Tank.
			CheckChanges("type", ItemDatabase.TypeNames[this.Type], ItemDatabase.TypeNames[other.Type], differences, ref isDifferent, comparedStats);
			CheckChanges("nation", this.Nation, other.Nation, differences, ref isDifferent, comparedStats);

			CheckChanges("tier", this.Tier, other.Tier, differences, ref isDifferent, comparedStats);
			CheckChanges("HP", this.Hp, other.Hp, differences, ref isDifferent, comparedStats);
			CheckChanges("weight", this.Weight, other.Weight, differences, ref isDifferent, comparedStats);

			CheckChanges("hull", this.Hull, other.Hull, differences, ref isDifferent, comparedStats);
			CheckChanges("speed", this.Speed, other.Speed, differences, ref isDifferent, comparedStats);
			CheckChanges("is premium", this.IsPremium, other.IsPremium, differences, ref isDifferent, comparedStats);
			CheckChanges("is hidden", this.IsHidden, other.IsHidden, differences, ref isDifferent, comparedStats);

			if (!OnlyBasicStats)
			{
				// Suspension.
				CheckChanges("load limit", this.LoadLimit, other.LoadLimit, differences, ref isDifferent, comparedStats);
				CheckChanges("brake force", this.BrakeForce, other.BrakeForce, differences, ref isDifferent, comparedStats);
				CheckChanges("rotates around center", this.RotatesAroundCenter, other.RotatesAroundCenter, differences, ref isDifferent, comparedStats);
				CheckChanges("tank rotation speed", this.RotationSpeed, other.RotationSpeed, differences, ref isDifferent, comparedStats);


				// Turret.
				CheckChanges("no turret", this.IsTurretInternal, other.IsTurretInternal, differences, ref isDifferent, comparedStats);
				CheckChanges("turret rotation speed", this.TurretRotationSpeed, other.TurretRotationSpeed, differences, ref isDifferent, comparedStats);
			}

			CheckChanges("view range", this.ViewRange, other.ViewRange, differences, ref isDifferent, comparedStats);
			CheckChanges("turret", this.Turret, other.Turret, differences, ref isDifferent, comparedStats);
			

			// AP Gun.
			if ((this.ApGun != null) && (other.ApGun != null))
			{
				CheckChanges("AP penetration", this.ApGun.ApPenetration, other.ApGun.ApPenetration, differences, ref isDifferent, comparedStats);
				CheckChanges("AP damage", this.ApGun.ApDamage, other.ApGun.ApDamage, differences, ref isDifferent, comparedStats);
				if (!OnlyBasicStats)
				{
					CheckChanges("AP accuracy", this.ApGun.Accuracy, other.ApGun.Accuracy, differences, ref isDifferent, comparedStats);
					CheckChanges("AP aim time", this.ApGun.AimingTime, other.ApGun.AimingTime, differences, ref isDifferent, comparedStats);
					CheckChanges("AP reload time", this.ApGun.ReloadTime, other.ApGun.ReloadTime, differences, ref isDifferent, comparedStats);

					CheckChanges("AP rotation speed", this.ApGun.RotationSpeed, other.ApGun.RotationSpeed, differences, ref isDifferent, comparedStats);

					if (this.ApGun.IsAutoLoader)
					{
						CheckChanges("AP clip size", this.ApGun.ClipSize, other.ApGun.ClipSize, differences, ref isDifferent, comparedStats);
						CheckChanges("AP clip rate", this.ApGun.ClipRate, other.ApGun.ClipRate, differences, ref isDifferent, comparedStats);
					}
				}
			}

			// HE Gun.
			if ((this.HeGun != null) && (other.HeGun != null))
			{
				CheckChanges("HE penetration", this.HeGun.HePenetration, other.HeGun.HePenetration, differences, ref isDifferent, comparedStats);
				CheckChanges("HE damage", this.HeGun.HeDamage, other.HeGun.HeDamage, differences, ref isDifferent, comparedStats);
				if (!OnlyBasicStats)
				{
					CheckChanges("HE accuracy", this.HeGun.Accuracy, other.HeGun.Accuracy, differences, ref isDifferent, comparedStats);
					CheckChanges("HE aim time", this.HeGun.AimingTime, other.HeGun.AimingTime, differences, ref isDifferent, comparedStats);
					CheckChanges("HE reload time", this.HeGun.ReloadTime, other.HeGun.ReloadTime, differences, ref isDifferent, comparedStats);

					CheckChanges("HE rotation speed", this.HeGun.RotationSpeed, other.HeGun.RotationSpeed, differences, ref isDifferent, comparedStats);
				}

				if (this.HeGun.IsAutoLoader)
				{
					CheckChanges("HE clip size", this.HeGun.ClipSize, other.HeGun.ClipSize, differences, ref isDifferent, comparedStats);
					CheckChanges("HE clip rate", this.HeGun.ClipRate, other.HeGun.ClipRate, differences, ref isDifferent, comparedStats);
				}
			}

			if (isDifferent)
			{
				sb.AppendLine(this.Name);
				foreach (string difference in differences) sb.AppendLine(difference);
				sb.AppendLine();
				sb.AppendLine();
			}

			return isDifferent;
		}

		/// <summary>Renders the tank stats to the specified <see cref="T:System.Text.StringBuilder"/>.</summary>
		/// <param name="stats">The <see cref="T:System.Text.StringBuilder"/> to render the stats to.</param>
		public void Render(StringBuilder stats)
		{
			string nation = ItemDatabase.NationAdjectives[this.Nation];
			string type = ItemDatabase.TypeNames[this.Type];

			stats.AppendFormat("{0} (tier {1} {2}{3} {4}):{5}", this.Name, this.Tier, (this.IsHidden ? "hidden " : ""), nation, type, Environment.NewLine);

			if (OnlyBasicStats)
			{
				//name, HP, PEN - HE if arty, avg. damage, reload time, armor, tier, premium status, nationality, view range.
				RenderStat("HP", this.Hp, stats);
				if (this.Type == TankType.Spg)
				{
					RenderStat("HE penetration", this.HeGun.HePenetration, stats);
					RenderStat("HE damage", this.HeGun.HeDamage, stats);
					RenderStat("HE reload time", this.HeGun.ReloadTime, stats);
				}
				else
				{
					RenderStat("AP penetration", this.ApGun.ApPenetration, stats);
					RenderStat("AP damage", this.ApGun.ApDamage, stats);
					RenderStat("AP reload time", this.ApGun.ReloadTime, stats);
				}

				if (!this.IsTurretInternal) RenderStat("turret", this.Turret, stats);
				RenderStat("hull", this.Hull, stats);

				RenderStat("is premium", this.IsPremium, stats);
				RenderStat("view range", this.ViewRange, stats);
			}
			else
			{
				// Tank.
				RenderStat("HP", this.Hp, stats);
				RenderStat("weight", this.Weight, stats);
				RenderStat("hull", this.Hull, stats);
				RenderStat("speed", this.Speed, stats);
				RenderStat("is premium", this.IsPremium, stats);

				// Suspension.
				RenderStat("load limit", this.LoadLimit, stats);
				RenderStat("brake force", this.BrakeForce, stats);
				RenderStat("rotates around center", this.RotatesAroundCenter, stats);
				RenderStat("tank rotation speed", this.RotationSpeed, stats);


				// Turret.
				//RenderStat("no turret", this.IsTurretInternal, stats);
				RenderStat("turret rotation speed", this.TurretRotationSpeed, stats);

				RenderStat("view range", this.ViewRange, stats);
				if (!this.IsTurretInternal) RenderStat("turret", this.Turret, stats);

				// AP Gun.
				if (this.ApGun != null)
				{
					RenderStat("AP penetration", this.ApGun.ApPenetration, stats);
					RenderStat("AP damage", this.ApGun.ApDamage, stats);

					RenderStat("AP accuracy", this.ApGun.Accuracy, stats);
					RenderStat("AP aim time", this.ApGun.AimingTime, stats);
					RenderStat("AP reload time", this.ApGun.ReloadTime, stats);

					RenderStat("AP gun rotation speed", this.ApGun.RotationSpeed, stats);
					RenderStat("AP damage per minute", this.ApGun.ApDamagePerMinute, stats);

					if (this.ApGun.IsAutoLoader)
					{
						RenderStat("AP clip size", this.ApGun.ClipSize, stats);
						RenderStat("AP clip rate", this.ApGun.ClipRate, stats);
					}
				}

				// HE Gun.
				if (this.HeGun != null)
				{
					RenderStat("HE penetration", this.HeGun.HePenetration, stats);
					RenderStat("HE damage", this.HeGun.HeDamage, stats);

					RenderStat("HE accuracy", this.HeGun.Accuracy, stats);
					RenderStat("HE aim time", this.HeGun.AimingTime, stats);
					RenderStat("HE reload time", this.HeGun.ReloadTime, stats);

					RenderStat("HE gun rotation speed", this.HeGun.RotationSpeed, stats);
					RenderStat("HE damage per minute", this.HeGun.HeDamagePerMinute, stats);

					if (this.HeGun.IsAutoLoader)
					{
						RenderStat("HE clip size", this.HeGun.ClipSize, stats);
						RenderStat("HE clip rate", this.HeGun.ClipRate, stats);
					}
				}

				stats.AppendLine("  file name: " + this.FileName);
			}

			stats.AppendLine();
			stats.AppendLine();
		}

		/// <summary>Checks whether the specified parameter has changed.</summary>
		/// <typeparam name="T">The type of the checked parameter.</typeparam>
		/// <param name="paramName">The name of the checked parameter.</param>
		/// <param name="param1">The value of the first parameter.</param>
		/// <param name="param2">The value of the second parameter.</param>
		/// <param name="differences">A collection of stat differences of a tank.</param>
		/// <param name="isDifferent">Set to <c>true</c> if the two parameters are different; otherwise, unchanged.</param>
		/// <param name="comparedStats">The tank stats that should be compared. If none are specified, all are compared.</param>
		private static void CheckChanges<T>(string paramName, T param1, T param2, List<string> differences, ref bool isDifferent, string[] visibleStats) where T : IEquatable<T>
		{
			if ((param1 == null) && (param2 == null)) return;
			if (param1.Equals(param2)) return;
			if ((visibleStats.Length > 0) && !visibleStats.Contains(paramName)) return;

			differences.Add(String.Format("  {0}: {1} -> {2}", paramName, ParamToString(param1), ParamToString(param2)));

			isDifferent = true;
		}

		/// <summary>Renders the specified tank stat parameter to the specified <see cref="T:System.Text.StringBuilder"/>.</summary>
		/// <typeparam name="T">The type of the rendered parameter.</typeparam>
		/// <param name="paramName">The name of the rendered parameter.</param>
		/// <param name="param">The value of the rendered parameter.</param>
		/// <param name="stats">A <see cref="T:System.Text.StringBuilder"/> to render the tank stat to.</param>
		private static void RenderStat<T>(string paramName, T param, StringBuilder stats) { stats.AppendFormat("  {0}: {1}{2}", paramName, ParamToString(param), Environment.NewLine); }

		private static string ParamToString<T>(T param) { return param.GetType() == typeof(bool) ? ((bool)(object)param ? "yes" : "no") : param.ToString(); }
		#endregion Methods
	}
}