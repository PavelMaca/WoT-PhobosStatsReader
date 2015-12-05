using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Phobos.WoT
{
	public class ItemDatabase
	{
		#region Static fields and properties
		private static readonly Dictionary<string, string> shortTankNames = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		private static readonly Dictionary<string, string> tinyTankNames = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		public static readonly Dictionary<string, string> IdRenamesFrom = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		public static readonly Dictionary<string, string> IdRenamesTo = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

		public static readonly string[] Nations = new string[] { "france", "germany", "china", "usa", "ussr", "uk", "japan" };
		//public static readonly string[] Types = new string[] { "mediumTank", "lightTank", "Spg", "heavyTank", "AT-Spg" };
		public static readonly Dictionary<string, string> NationAdjectives = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		public static readonly Dictionary<TankType, string> TypeNames = new Dictionary<TankType, string>();
		public static readonly Dictionary<ShellKind, string> ShellTypes = new Dictionary<ShellKind, string>();
		public static Dictionary<string, string> TankNames { get; private set; }
		#endregion Static fields and properties

		#region Properties
		public readonly Dictionary<string, Shell> Shells = new Dictionary<string, Shell>(StringComparer.InvariantCultureIgnoreCase);
		public readonly Dictionary<string, Gun> Guns = new Dictionary<string, Gun>(StringComparer.InvariantCultureIgnoreCase);
		public readonly Dictionary<string, Tank> Tanks = new Dictionary<string, Tank>(StringComparer.InvariantCultureIgnoreCase);
		#endregion Properties

		#region Constructors
		static ItemDatabase()
		{
			ItemDatabase.TankNames = ItemDatabase.shortTankNames;

			ItemDatabase.ShellTypes[ShellKind.AP] = "Armor-Piercing";
			ItemDatabase.ShellTypes[ShellKind.APCR] = "AP Composite-Rigid";
			ItemDatabase.ShellTypes[ShellKind.HE] = "High-Explosive";
			ItemDatabase.ShellTypes[ShellKind.HEAT] = "High-Explosive Anti-Tank";
			ItemDatabase.ShellTypes[ShellKind.AT] = "";

			ItemDatabase.NationAdjectives["france"] = "french";
			ItemDatabase.NationAdjectives["germany"] = "german";
			ItemDatabase.NationAdjectives["china"] = "chinese";
			ItemDatabase.NationAdjectives["usa"] = "american";
			ItemDatabase.NationAdjectives["ussr"] = "russian";
			ItemDatabase.NationAdjectives["uk"] = "british";
			ItemDatabase.NationAdjectives["japan"] = "japanese";

			ItemDatabase.TypeNames[TankType.Medium] = "medium tank";
			ItemDatabase.TypeNames[TankType.Light] = "light tank";
			ItemDatabase.TypeNames[TankType.Spg] = "artillery";
			ItemDatabase.TypeNames[TankType.Heavy] = "heavy tank";
			ItemDatabase.TypeNames[TankType.TankDestroyer] = "tank destroyer";

			ItemDatabase.InitializeShortTankNames();
			ItemDatabase.InitializeTinyTankNames();
		}

		public ItemDatabase(string itemDefsPath)
		{
			// Load shells.
			for (int i = 0; i < Nations.Length; i++)
			{
				string path = Path.Combine(itemDefsPath, @"vehicles\", Nations[i], @"components\shells.xml");
				if (File.Exists(path)) Shell.LoadFromFile(this, path, Nations[i]);
			}

			// Load guns.
			for (int i = 0; i < Nations.Length; i++)
			{
				string path = Path.Combine(itemDefsPath, @"vehicles\", Nations[i], @"components\guns.xml");
				if (File.Exists(path)) Gun.LoadFromFile(this, path, Nations[i]);
			}

			// Load tanks.
			for (int i = 0; i < Nations.Length; i++)
			{
				string nation = Nations[i];
				string path = Path.Combine(itemDefsPath, @"vehicles\", Nations[i], @"list.xml");
				if (!File.Exists(path)) continue;

				DataReader reader = new DataReader();
				XmlDocument doc = reader.Read(path);

				if (doc.DocumentElement.Name != "list.xml") throw new ArgumentException("The file is not a packed XML containing tank data.", "path");

				foreach (XmlNode node in doc.DocumentElement.ChildNodes) if (node.NodeType == XmlNodeType.Element)
					{
						XmlElement tankElement = (XmlElement)node;

						if (!ItemDatabase.TankNames.ContainsKey(tankElement.Name))
						{
							ItemDatabase.TankNames[tankElement.Name] = tankElement.Name;
							Console.WriteLine(tankElement.Name);
						}

						path = Path.Combine(itemDefsPath, @"vehicles\", Nations[i], tankElement.Name + ".xml");
						if (File.Exists(path)) Tank.LoadFromFile(this, path, nation, tankElement);
					}
			}
		}
		#endregion Constructors

		#region Methods
		public IEnumerable<TankStats> GetTankStats() { return this.Tanks.Values.Select(tank => new TankStats(tank)); }
		public static IEnumerable<TankStats> GetTankStats(string itemDefsPath) { return (new ItemDatabase(itemDefsPath)).Tanks.Values.Select(tank => new TankStats(tank)); }

		public static void UseShortTankNames() { ItemDatabase.TankNames = ItemDatabase.shortTankNames; }
		public static void UseTinyTankNames() { ItemDatabase.TankNames = ItemDatabase.tinyTankNames; }

		private static void AddRename(string from, string to)
		{
			ItemDatabase.IdRenamesFrom[from] = to;
			ItemDatabase.IdRenamesTo[to] = from;
			ItemDatabase.shortTankNames[to] = ItemDatabase.shortTankNames[from];
		}
		#endregion Methods

		#region TankNames
		private static void InitializeShortTankNames()
		{
			#region 7.0
			ItemDatabase.shortTankNames["D2"] = "D2";
			ItemDatabase.shortTankNames["RenaultFT"] = "Renault";
			ItemDatabase.shortTankNames["RenaultBS"] = "BS";
			ItemDatabase.shortTankNames["B1"] = "B1";
			ItemDatabase.shortTankNames["Hotchkiss_H35"] = "Hotchkiss 35";
			ItemDatabase.shortTankNames["D1"] = "D1";
			ItemDatabase.shortTankNames["_105_leFH18B2"] = "leFH18B2";
			ItemDatabase.shortTankNames["FCM_36Pak40"] = "FCM 36";
			ItemDatabase.shortTankNames["ARL_44"] = "ARL 44";
			ItemDatabase.shortTankNames["AMX40"] = "AMX 40";
			ItemDatabase.shortTankNames["AMX_50_100"] = "AMX 50 100";
			ItemDatabase.shortTankNames["Lorraine39_L_AM"] = "Lorr.39 L AM";
			ItemDatabase.shortTankNames["Bat_Chatillon25t"] = "BatChat.25t";
			ItemDatabase.shortTankNames["AMX_50_120"] = "AMX 50 120";
			ItemDatabase.shortTankNames["AMX_105AM"] = "AMX 105AM";
			ItemDatabase.shortTankNames["AMX_13F3AM"] = "AMX 13 F3 AM";
			ItemDatabase.shortTankNames["AMX_13_90"] = "AMX 13 90";
			ItemDatabase.shortTankNames["AMX_13_75"] = "AMX 13 75";
			ItemDatabase.shortTankNames["Lorraine40t"] = "Lorraine 40 t";
			ItemDatabase.shortTankNames["AMX38"] = "AMX 38";
			ItemDatabase.shortTankNames["F10_AMX_50B"] = "AMX 50B";
			ItemDatabase.shortTankNames["AMX_12t"] = "AMX 12t";
			ItemDatabase.shortTankNames["BDR_G1B"] = "BDR G1B";
			ItemDatabase.shortTankNames["AMX_M4_1945"] = "AMX M4 1945";
			ItemDatabase.shortTankNames["Lorraine155_50"] = "Lorr.155 50";
			ItemDatabase.shortTankNames["Lorraine155_51"] = "Lorr.155 51";
			ItemDatabase.shortTankNames["RenaultFT_AC"] = "Renault AC";
			ItemDatabase.shortTankNames["RenaultUE57"] = "Ren. UE 57";
			ItemDatabase.shortTankNames["Somua_Sau_40"] = "Somua SAu-40";
			ItemDatabase.shortTankNames["S_35CA"] = "S-35 CA";
			ItemDatabase.shortTankNames["AMX_AC_Mle1946"] = "AMX AC Mle.46";
			ItemDatabase.shortTankNames["AMX50_Foch"] = "AMX 50 Foch";
			ItemDatabase.shortTankNames["ARL_V39"] = "ARL V39";
			ItemDatabase.shortTankNames["Bat_Chatillon155"] = "BatChat.155";
			ItemDatabase.shortTankNames["AMX_AC_Mle1948"] = "AMX AC Mle.48";
			ItemDatabase.shortTankNames["PzIV"] = "Pz 4";
			ItemDatabase.shortTankNames["Hummel"] = "Hummel";
			ItemDatabase.shortTankNames["PzVI"] = "Tiger";
			ItemDatabase.shortTankNames["Pz35t"] = "Pz 35t";
			ItemDatabase.shortTankNames["StuGIII"] = "StuG III";
			ItemDatabase.shortTankNames["PzV"] = "Panther";
			ItemDatabase.shortTankNames["JagdPzIV"] = "JgPz 4";
			ItemDatabase.shortTankNames["Hetzer"] = "Hetzer";
			ItemDatabase.shortTankNames["PzII"] = "PzII";
			ItemDatabase.shortTankNames["VK3601H"] = "VK 36.01 H";
			ItemDatabase.shortTankNames["VK3001H"] = "VK 30.01 H";
			ItemDatabase.shortTankNames["Bison_I"] = "Bison";
			ItemDatabase.shortTankNames["Ltraktor"] = "Ltraktor";
			ItemDatabase.shortTankNames["Pz38t"] = "Pz38t";
			ItemDatabase.shortTankNames["PanzerJager_I"] = "PzJäger I";
			ItemDatabase.shortTankNames["JagdPanther"] = "JgPanther";
			ItemDatabase.shortTankNames["VK3002DB"] = "VK 30.02 D";
			ItemDatabase.shortTankNames["PzIII"] = "Pz 3";
			ItemDatabase.shortTankNames["Sturmpanzer_II"] = "StPz II";
			ItemDatabase.shortTankNames["PzIII_A"] = "Pz 3 AU A";
			ItemDatabase.shortTankNames["PzVIB_Tiger_II"] = "Tiger II";
			ItemDatabase.shortTankNames["VK1602"] = "Leopard";
			ItemDatabase.shortTankNames["Grille"] = "Grille";
			ItemDatabase.shortTankNames["Wespe"] = "Wespe";
			ItemDatabase.shortTankNames["PzII_Luchs"] = "Luchs";
			ItemDatabase.shortTankNames["PzIII_IV"] = "Pz 3/4";
			ItemDatabase.shortTankNames["G20_Marder_II"] = "Marder II";
			ItemDatabase.shortTankNames["Maus"] = "Maus";
			ItemDatabase.shortTankNames["VK3001P"] = "VK 30.01 P";
			ItemDatabase.shortTankNames["VK4502P"] = "VK 45.02 P";
			ItemDatabase.shortTankNames["Ferdinand"] = "Ferdinand";
			ItemDatabase.shortTankNames["JagdTiger"] = "JgTiger";
			ItemDatabase.shortTankNames["Pz38_NA"] = "Pz 38 nA";
			ItemDatabase.shortTankNames["Panther_II"] = "PANTHER II";
			ItemDatabase.shortTankNames["G_Tiger"] = "GW Tiger";
			ItemDatabase.shortTankNames["G_Panther"] = "GW Panther";
			ItemDatabase.shortTankNames["G_E"] = "GW Typ E";
			ItemDatabase.shortTankNames["E-100"] = "E-100";
			ItemDatabase.shortTankNames["E-75"] = "E-75";
			ItemDatabase.shortTankNames["VK2801"] = "VK 28.01";
			ItemDatabase.shortTankNames["E-50"] = "E-50";
			ItemDatabase.shortTankNames["VK4502A"] = "VK 45.02 A";
			ItemDatabase.shortTankNames["PzVI_Tiger_P"] = "Tiger (P)";
			ItemDatabase.shortTankNames["PzV_PzIV"] = "Pz 5-4";
			ItemDatabase.shortTankNames["PzII_J"] = "Pz 2 AU J";
			ItemDatabase.shortTankNames["S35_captured"] = "Pz S35 739 (f)";
			ItemDatabase.shortTankNames["B-1bis_captured"] = "Pz B2 740 (f)";
			ItemDatabase.shortTankNames["H39_captured"] = "Pz 38H735 (f)";
			ItemDatabase.shortTankNames["PzV_PzIV_ausf_Alfa"] = "Pz 5-4 A";
			ItemDatabase.shortTankNames["Lowe"] = "Löwe";
			ItemDatabase.shortTankNames["T-25"] = "T-25";
			ItemDatabase.shortTankNames["T-15"] = "T-15";
			ItemDatabase.shortTankNames["PzIV_Hydro"] = "Pz 4 Hydro";
			ItemDatabase.shortTankNames["JagdTiger_SdKfz_185"] = "JgTiger 88";
			ItemDatabase.shortTankNames["White_Tiger"] = "White Tiger";
			ItemDatabase.shortTankNames["DickerMax"] = "Dicker Max";
			ItemDatabase.shortTankNames["Ch01_Type59"] = "Type 59";
			ItemDatabase.shortTankNames["Ch02_Type62"] = "Type 62";
			ItemDatabase.shortTankNames["Ch01_Type59_Gold"] = "Type 59 G";
			ItemDatabase.shortTankNames["Ch03_WZ-111"] = "WZ-111";
			ItemDatabase.shortTankNames["T14"] = "T14";
			ItemDatabase.shortTankNames["M3_Stuart"] = "M3 Stuart";
			ItemDatabase.shortTankNames["T1_Cunningham"] = "T1 Cunning.";
			ItemDatabase.shortTankNames["M6"] = "M6";
			ItemDatabase.shortTankNames["M4_Sherman"] = "M4 Sherman";
			ItemDatabase.shortTankNames["M4A3E8_Sherman"] = "M4A3E8";
			ItemDatabase.shortTankNames["T20"] = "T20";
			ItemDatabase.shortTankNames["M2_lt"] = "M2 Light";
			ItemDatabase.shortTankNames["T57"] = "T57";
			ItemDatabase.shortTankNames["T23"] = "T23";
			ItemDatabase.shortTankNames["T30"] = "T30";
			ItemDatabase.shortTankNames["T34_hvy"] = "T34";
			ItemDatabase.shortTankNames["M3_Grant"] = "M3 Lee";
			ItemDatabase.shortTankNames["T1_hvy"] = "T1 Heavy";
			ItemDatabase.shortTankNames["M7_Priest"] = "M7 Priest";
			ItemDatabase.shortTankNames["T29"] = "T29";
			ItemDatabase.shortTankNames["M41"] = "M41";
			ItemDatabase.shortTankNames["T32"] = "T32";
			ItemDatabase.shortTankNames["M37"] = "M37";
			ItemDatabase.shortTankNames["M2_med"] = "M2 MT";
			ItemDatabase.shortTankNames["M5_Stuart"] = "M5 Stuart";
			ItemDatabase.shortTankNames["M7_med"] = "M7";
			ItemDatabase.shortTankNames["T2_med"] = "T2 Medium";
			ItemDatabase.shortTankNames["Pershing"] = "Pershing";
			ItemDatabase.shortTankNames["T18"] = "T18";
			ItemDatabase.shortTankNames["T82"] = "T82";
			ItemDatabase.shortTankNames["M10_Wolverine"] = "Wolverine";
			ItemDatabase.shortTankNames["M36_Slagger"] = "Jackson";
			ItemDatabase.shortTankNames["M40M43"] = "M40/M43";
			ItemDatabase.shortTankNames["T40"] = "T40";
			ItemDatabase.shortTankNames["M12"] = "M12";
			ItemDatabase.shortTankNames["T28"] = "T28";
			ItemDatabase.shortTankNames["T92"] = "T92";
			ItemDatabase.shortTankNames["T95"] = "T95";
			ItemDatabase.shortTankNames["M46_Patton"] = "M46 Patton";
			ItemDatabase.shortTankNames["T25_AT"] = "T25 AT";
			ItemDatabase.shortTankNames["M103"] = "M103";
			ItemDatabase.shortTankNames["M24_Chaffee"] = "Chaffee";
			ItemDatabase.shortTankNames["Sherman_Jumbo"] = "M4A3E2";
			ItemDatabase.shortTankNames["M8A1"] = "M8A1";
			ItemDatabase.shortTankNames["T49"] = "T49";
			ItemDatabase.shortTankNames["T110"] = "T110E5";
			ItemDatabase.shortTankNames["T25_2"] = "T25/2";
			ItemDatabase.shortTankNames["T28_Prototype"] = "T28 Proto.";
			ItemDatabase.shortTankNames["M18_Hellcat"] = "Hellcat";
			ItemDatabase.shortTankNames["T26_E4_SuperPershing"] = "T26E4";//T26E4 Super Pershing
			ItemDatabase.shortTankNames["T2_lt"] = "T2 Light";
			ItemDatabase.shortTankNames["Ram-II"] = "Ram-II";
			ItemDatabase.shortTankNames["MTLS-1G14"] = "MTLS-1G14";
			ItemDatabase.shortTankNames["M4A2E4"] = "M4A2E4";
			ItemDatabase.shortTankNames["M6A2E1"] = "M6A2E1";
			ItemDatabase.shortTankNames["M22_Locust"] = "M22 Locust";
			ItemDatabase.shortTankNames["T-34"] = "T-34";
			ItemDatabase.shortTankNames["SU-85"] = "SU-85";
			ItemDatabase.shortTankNames["IS"] = "IS";
			ItemDatabase.shortTankNames["BT-7"] = "BT-7";
			ItemDatabase.shortTankNames["BT-2"] = "BT-2";
			ItemDatabase.shortTankNames["KV"] = "KV";
			ItemDatabase.shortTankNames["T-28"] = "T-28";
			ItemDatabase.shortTankNames["S-51"] = "S-51";
			ItemDatabase.shortTankNames["A-20"] = "A-20";
			ItemDatabase.shortTankNames["SU-152"] = "SU-152";
			ItemDatabase.shortTankNames["T-34-85"] = "T-34-85";
			ItemDatabase.shortTankNames["KV-1s"] = "KV-1s";
			ItemDatabase.shortTankNames["T-46"] = "T-46";
			ItemDatabase.shortTankNames["MS-1"] = "MS-1";
			ItemDatabase.shortTankNames["SU-100"] = "SU-100";
			ItemDatabase.shortTankNames["SU-18"] = "SU-18";
			ItemDatabase.shortTankNames["SU-14"] = "SU-14";
			ItemDatabase.shortTankNames["T-44"] = "T-44";
			ItemDatabase.shortTankNames["T-26"] = "T-26";
			ItemDatabase.shortTankNames["SU-5"] = "SU-5";
			ItemDatabase.shortTankNames["AT-1"] = "AT-1";
			ItemDatabase.shortTankNames["IS-3"] = "IS-3";
			ItemDatabase.shortTankNames["SU-8"] = "SU-8";
			ItemDatabase.shortTankNames["KV-3"] = "KV-3";
			ItemDatabase.shortTankNames["IS-4"] = "IS-4";
			ItemDatabase.shortTankNames["SU-76"] = "SU-76";
			ItemDatabase.shortTankNames["T-43"] = "T-43";
			ItemDatabase.shortTankNames["GAZ-74b"] = "SU-85B";
			ItemDatabase.shortTankNames["IS-7"] = "IS-7";
			ItemDatabase.shortTankNames["ISU-152"] = "ISU-152";
			ItemDatabase.shortTankNames["SU-26"] = "SU-26";
			ItemDatabase.shortTankNames["T-54"] = "T-54";
			ItemDatabase.shortTankNames["Object_704"] = "Object 704";
			ItemDatabase.shortTankNames["Object_212"] = "Object 212";
			ItemDatabase.shortTankNames["Object_261"] = "Object 261";
			ItemDatabase.shortTankNames["KV-13"] = "KV-13";
			ItemDatabase.shortTankNames["Object252"] = "IS-6";
			ItemDatabase.shortTankNames["T-50"] = "T-50";
			ItemDatabase.shortTankNames["T_50_2"] = "T-50-2";
			ItemDatabase.shortTankNames["KV2"] = "KV-2";
			ItemDatabase.shortTankNames["ST_I"] = "ST-I";
			ItemDatabase.shortTankNames["KV4"] = "KV-4";
			ItemDatabase.shortTankNames["T150"] = "T-150";
			ItemDatabase.shortTankNames["IS8"] = "IS-8";
			ItemDatabase.shortTankNames["KV1"] = "KV-1";
			ItemDatabase.shortTankNames["KV-220"] = "KV-220";
			ItemDatabase.shortTankNames["Matilda_II_LL"] = "Matilda";
			ItemDatabase.shortTankNames["Churchill_LL"] = "Churchill";
			ItemDatabase.shortTankNames["BT-SV"] = "BT-SV";
			ItemDatabase.shortTankNames["Valentine_LL"] = "Valentine";
			ItemDatabase.shortTankNames["M3_Stuart_LL"] = "M3 Stuart";
			ItemDatabase.shortTankNames["A-32"] = "A-32";
			ItemDatabase.shortTankNames["KV-5"] = "KV-5";
			ItemDatabase.shortTankNames["T-127"] = "T-127";
			ItemDatabase.shortTankNames["SU_85I"] = "SU-85I";
			ItemDatabase.shortTankNames["KV-220_action"] = "KV-220";
			ItemDatabase.shortTankNames["Tetrarch_LL"] = "Tetrarch";
			#endregion 7.0

			#region 7.5 - 8.5
			// Added in 7.5
			ItemDatabase.shortTankNames["AMX_50Fosh_155"] = "AMX Foch 155";
			ItemDatabase.shortTankNames["ELC_AMX"] = "ELC AMX";
			ItemDatabase.shortTankNames["JagdPantherII"] = "JgPanther 2";
			ItemDatabase.shortTankNames["JagdPz_E100"] = "JgPz E-100";
			ItemDatabase.shortTankNames["E50_Ausf_M"] = "E-50 M";
			ItemDatabase.shortTankNames["T110E4"] = "T110E4";
			ItemDatabase.shortTankNames["T110E3"] = "T110E3";
			ItemDatabase.shortTankNames["M48A1"] = "M48 Patton";
			ItemDatabase.shortTankNames["Object268"] = "Object 268";
			ItemDatabase.shortTankNames["T62A"] = "T-62A";
			ItemDatabase.shortTankNames["GB68_Matilda_Black_Prince"] = "Matilda BP";

			// Added in 8.0
			ItemDatabase.shortTankNames["PzIV_schmalturm"] = "Pz 4 Sturm";
			ItemDatabase.shortTankNames["Panther_M10"] = "Panther M10";
			ItemDatabase.shortTankNames["SU-101"] = "SU-101";
			ItemDatabase.shortTankNames["SU100M1"] = "SU-100M1";
			ItemDatabase.shortTankNames["SU122_54"] = "SU-122-54";
			ItemDatabase.shortTankNames["Object263"] = "Object 263";
			ItemDatabase.shortTankNames["SU122_44"] = "SU-122-44";

			// Added in 8.1
			ItemDatabase.shortTankNames["FCM_50t"] = "FCM 50t";
			ItemDatabase.shortTankNames["T1_E6"] = "T1 E6";
			ItemDatabase.shortTankNames["GB01_Medium_Mark_I"] = "Medium I";
			ItemDatabase.shortTankNames["GB05_Vickers_Medium_Mk_II"] = "Medium II";
			ItemDatabase.shortTankNames["GB07_Matilda"] = "Matilda";
			ItemDatabase.shortTankNames["GB21_Cromwell"] = "Cromwell";
			ItemDatabase.shortTankNames["GB20_Crusader"] = "Crusader";
			ItemDatabase.shortTankNames["GB06_Vickers_Medium_Mk_III"] = "Medium III";
			ItemDatabase.shortTankNames["GB08_Churchill_I"] = "Churchill I";
			ItemDatabase.shortTankNames["GB10_Black_Prince"] = "Black Prince";
			ItemDatabase.shortTankNames["GB11_Caernarvon"] = "Caernarvon";
			ItemDatabase.shortTankNames["GB12_Conqueror"] = "Conqueror";
			ItemDatabase.shortTankNames["GB09_Churchill_VII"] = "Churchill  7";
			ItemDatabase.shortTankNames["GB04_Valentine"] = "Valentine";
			ItemDatabase.shortTankNames["GB03_Cruiser_Mk_I"] = "Cruiser I";
			ItemDatabase.shortTankNames["GB22_Comet"] = "Comet";
			ItemDatabase.shortTankNames["GB24_Centurion_Mk3"] = "Cent. 7/1";
			ItemDatabase.shortTankNames["GB23_Centurion"] = "Centurion";
			ItemDatabase.shortTankNames["GB13_FV215b"] = "FV215b";
			ItemDatabase.shortTankNames["GB60_Covenanter"] = "Covenanter";
			ItemDatabase.shortTankNames["GB69_Cruiser_Mk_II"] = "Cruiser II";
			ItemDatabase.shortTankNames["GB70_FV4202_105"] = "FV4202";
			ItemDatabase.shortTankNames["GB59_Cruiser_Mk_IV"] = "Cruiser IV";
			ItemDatabase.shortTankNames["GB58_Cruiser_Mk_III"] = "Cruiser III";
			ItemDatabase.shortTankNames["GB71_AT_15A"] = "AT-15A";
			ItemDatabase.shortTankNames["GB63_TOG_II"] = "TOG II";

			// Added in 8.2
			ItemDatabase.shortTankNames["T69"] = "T69";
			ItemDatabase.shortTankNames["T57_58"] = "T57 Heavy";
			ItemDatabase.shortTankNames["T21"] = "T21";
			ItemDatabase.shortTankNames["T54E1"] = "T54E1";
			ItemDatabase.shortTankNames["T71"] = "T71";
			ItemDatabase.shortTankNames["SU100Y"] = "SU-100Y";

			// Added in 8.3
			ItemDatabase.shortTankNames["Ch04_T34_1"] = "T-34-1";
			ItemDatabase.shortTankNames["Ch06_Renault_NC31"] = "NC-31";
			ItemDatabase.shortTankNames["Ch05_T34_2"] = "T-34-2";
			ItemDatabase.shortTankNames["Ch18_WZ-120"] = "WZ-120";
			ItemDatabase.shortTankNames["Ch12_111_1_2_3"] = "WZ-111 1-4";
			ItemDatabase.shortTankNames["Ch07_Vickers_MkE_Type_BT26"] = "VAE Type B";
			//ItemDatabase.shortTankNames["Ch13_111_4_5"] = "WZ-111 5A";
			ItemDatabase.shortTankNames["Ch11_110"] = "110";
			ItemDatabase.shortTankNames["Ch09_M5"] = "M5A1";
			ItemDatabase.shortTankNames["Ch16_WZ_131"] = "WZ-131";
			ItemDatabase.shortTankNames["Ch10_IS2"] = "IS-2";
			ItemDatabase.shortTankNames["Ch17_WZ131_1_WZ132"] = "WZ-132";
			ItemDatabase.shortTankNames["Ch19_121"] = "121";
			ItemDatabase.shortTankNames["Ch08_Type97_Chi_Ha"] = "Chi-Ha";
			ItemDatabase.shortTankNames["Ch21_T34"] = "Type T-34";
			ItemDatabase.shortTankNames["Ch15_59_16"] = "59-16";
			ItemDatabase.shortTankNames["Ch20_Type58"] = "Type 58";
			ItemDatabase.shortTankNames["Ch22_113"] = "113";

			// Added in 8.4
			ItemDatabase.shortTankNames["PzI_ausf_C"] = "Pz. 1 C";
			ItemDatabase.shortTankNames["PzI"] = "Pz 1";
			ItemDatabase.shortTankNames["Pz_II_AusfG"] = "Pz 2 G";
			ItemDatabase.shortTankNames["PzIII_training"] = "Pz 3 TRN";
			ItemDatabase.shortTankNames["PzVIB_Tiger_II_training"] = "Tiger II TRN";
			ItemDatabase.shortTankNames["PzV_training"] = "Pz 4 TRN";
			ItemDatabase.shortTankNames["Ch04_T34_1_training"] = "T-34-1 TRN";
			ItemDatabase.shortTankNames["Sexton_I"] = "Sexton";
			ItemDatabase.shortTankNames["M4A3E8_Sherman_training"] = "M4A3E8 TRN";
			ItemDatabase.shortTankNames["T-34-85_training"] = "T-34-85 TRN";
			ItemDatabase.shortTankNames["GB42_Valentine_AT"] = "Valentine AT";
			ItemDatabase.shortTankNames["GB39_Universal_CarrierQF2"] = "UC 2-pdr";
			ItemDatabase.shortTankNames["GB72_AT15"] = "AT 15";
			ItemDatabase.shortTankNames["GB73_AT2"] = "AT 2";
			ItemDatabase.shortTankNames["GB57_Alecto"] = "Alecto";
			ItemDatabase.shortTankNames["GB48_FV215b_183"] = "FV215b 183";
			ItemDatabase.shortTankNames["GB74_AT8"] = "AT 8";
			ItemDatabase.shortTankNames["GB40_Gun_Carrier_Churchill"] = "Churchill GC";
			ItemDatabase.shortTankNames["GB75_AT7"] = "AT 7";
			ItemDatabase.shortTankNames["GB32_Tortoise"] = "Tortoise";

			// Added in 8.5
			ItemDatabase.shortTankNames["VK2001DB"] = "VK 20.01 D";
			ItemDatabase.shortTankNames["Indien_Panzer"] = "Indien-Pz.";
			ItemDatabase.shortTankNames["VK3002DB_V1"] = "VK 30.01 D";
			ItemDatabase.shortTankNames["Auf_Panther"] = "Auf.Panther";
			ItemDatabase.shortTankNames["Leopard1"] = "Leopard 1";
			ItemDatabase.shortTankNames["Pro_Ag_A"] = "Leopard PT A";
			ItemDatabase.shortTankNames["VK7201"] = "VK 72.01";
			ItemDatabase.shortTankNames["M60"] = "M60";
			ItemDatabase.shortTankNames["T-70"] = "T-70";
			ItemDatabase.shortTankNames["T-60"] = "T-60";
			ItemDatabase.shortTankNames["Object_907"] = "Object 907";
			ItemDatabase.shortTankNames["T80"] = "T-80";
			ItemDatabase.shortTankNames["GB51_Excelsior"] = "Excelsior";
			ItemDatabase.shortTankNames["GB78_Sexton_I"] = "Sexton I";

			// Added in 8.6
			ItemDatabase.shortTankNames["Bat_Chatillon155_58"] = "B-C 155 58";
			ItemDatabase.shortTankNames["Bat_Chatillon155_55"] = "B-C 155 55";
			ItemDatabase.shortTankNames["AMX_Ob_Am105"] = "AMX 105 AM";
			ItemDatabase.shortTankNames["GW_Mk_VIe"] = "G.Pz. Mk. 6";
			ItemDatabase.shortTankNames["GW_Tiger_P"] = "GW Tiger P";
			ItemDatabase.shortTankNames["Pz_Sfl_IVb"] = "Pz.Sfl IVb";
			ItemDatabase.shortTankNames["E-25"] = "E-25";
			ItemDatabase.shortTankNames["Ch23_112"] = "112";
			ItemDatabase.shortTankNames["M53_55"] = "M53/M55";
			ItemDatabase.shortTankNames["M44"] = "M44";
			ItemDatabase.shortTankNames["SU14_1"] = "SU-14";
			ItemDatabase.shortTankNames["SU122A"] = "SU-122A";
			#endregion 7.5 - 8.6

			#region 8.7 - 9.3
			// Added in 8.7
			ItemDatabase.shortTankNames["Ch14_T34_3"] = "T-34-3";
			ItemDatabase.shortTankNames["GB25_Loyd_Carrier"] = "Loyd GC";
			ItemDatabase.shortTankNames["GB26_Birch_Gun"] = "Birch Gun";
			ItemDatabase.shortTankNames["GB27_Sexton"] = "Sexton II";
			ItemDatabase.shortTankNames["GB28_Bishop"] = "Bishop";
			ItemDatabase.shortTankNames["GB29_Crusader_5inch"] = "Crusader SP";
			ItemDatabase.shortTankNames["GB30_FV3805"] = "FV3805";
			ItemDatabase.shortTankNames["GB31_Conqueror_Gun"] = "Conqueror GC";
			ItemDatabase.shortTankNames["GB77_FV304"] = "FV304";
			ItemDatabase.shortTankNames["GB79_FV206"] = "FV207";
			ItemDatabase.shortTankNames["LTP"] = "LTP";
			ItemDatabase.shortTankNames["MT25"] = "MT-25";

			// Added in 8.8
			ItemDatabase.shortTankNames["DW_II"] = "D.W. 2";
			ItemDatabase.shortTankNames["VK3002M"] = "VK 30.02 M";
			ItemDatabase.shortTankNames["Ch24_Type64"] = "Type 64";
			ItemDatabase.shortTankNames["A43"] = "A-43";
			ItemDatabase.shortTankNames["A44"] = "A-44";
			ItemDatabase.shortTankNames["Object416"] = "Object 416";
			ItemDatabase.shortTankNames["Object_140"] = "Object 140";
			ItemDatabase.shortTankNames["T44_122"] = "Т-44-122";
			ItemDatabase.shortTankNames["T44_85"] = "Т-44-85";
			ItemDatabase.shortTankNames["Chi_Nu_Kai"] = "Chi-Nu Kai";

			// Added in 8.9
			ItemDatabase.shortTankNames["Sturer_Emil"] = "Pz.Sfl. 5";
			ItemDatabase.shortTankNames["Marder_III"] = "Marder 38T";
			ItemDatabase.shortTankNames["Nashorn"] = "Nashorn";
			ItemDatabase.shortTankNames["Pz_Sfl_IVc"] = "Pz.Sfl. 4C";
			ItemDatabase.shortTankNames["Waffentrager_IV"] = "WT Pz. 4";
			ItemDatabase.shortTankNames["RhB_Waffentrager"] = "Rhm.-B. WT";
			ItemDatabase.shortTankNames["Waffentrager_E100"] = "WT E-100";
			ItemDatabase.shortTankNames["T7_Combat_Car"] = "T7 CC";
			ItemDatabase.shortTankNames["GB76_Mk_VIC"] = "Light Mk. VIC";

			// Added in 8.10
			ItemDatabase.shortTankNames["Object_430"] = "Object 430";
			ItemDatabase.shortTankNames["R104_Object_430_II"] = "Obj. 430 II";
			ItemDatabase.shortTankNames["SU76I"] = "SU76i";
			ItemDatabase.shortTankNames["Chi_Ni"] = "Chi-Ni";
			ItemDatabase.shortTankNames["NC27"] = "R. Otsu";
			ItemDatabase.shortTankNames["Ha_Go"] = "Ha-Go";
			ItemDatabase.shortTankNames["Chi_Ri"] = "Chi-Ri";
			ItemDatabase.shortTankNames["Chi_Nu"] = "Chi-Nu";
			ItemDatabase.shortTankNames["Chi_To"] = "Chi-To";
			ItemDatabase.shortTankNames["Chi_Ha"] = "Chi-Ha";
			ItemDatabase.shortTankNames["Ke_Ni"] = "Ke-Ni";
			ItemDatabase.shortTankNames["STA_1"] = "STA-1";
			ItemDatabase.shortTankNames["Ke_Ho"] = "Ke-Ho";
			ItemDatabase.shortTankNames["Type_61"] = "Type 61";
			ItemDatabase.shortTankNames["ST_B1"] = "STB-1";
			ItemDatabase.shortTankNames["Ke_Ni_B"] = "Ke-Ni B";
			ItemDatabase.shortTankNames["Chi_He"] = "Chi-He";

			// Added in 8.11
			ItemDatabase.shortTankNames["G100_Gtraktor_Krupp"] = "G100";
			ItemDatabase.shortTankNames["T23E3"] = "T23E3";
			ItemDatabase.shortTankNames["T95_E6"] = "T95E6";

			// Added in 9.0
			ItemDatabase.shortTankNames["G101_StuG_III"] = "StuG III B";
			ItemDatabase.shortTankNames["StuG_40_AusfG"] = "StuG III G";
			ItemDatabase.shortTankNames["PzIII_AusfJ"] = "Pz 3";
			ItemDatabase.shortTankNames["Pz_IV_AusfA"] = "Pz 4 A";
			ItemDatabase.shortTankNames["Pz_IV_AusfD"] = "Pz 4 D";
			ItemDatabase.shortTankNames["Pz_IV_AusfH"] = "Pz 4 H";

			// Added in 9.1
			ItemDatabase.shortTankNames["T62A_sport"] = "T-62A Sport";
			ItemDatabase.shortTankNames["Te_Ke"] = "Te-Ke";

			// Added in 9.3
			ItemDatabase.shortTankNames["G103_RU_251"] = "Ru 251";
			ItemDatabase.shortTankNames["R106_KV85"] = "KV-85";
			ItemDatabase.shortTankNames["R108_T34_85M"] = "T34-85M";
			ItemDatabase.shortTankNames["G103_RU_251"] = "Ru 251";
			ItemDatabase.shortTankNames["Env_Artillery"] = "Env Art";
			ItemDatabase.shortTankNames["T67"] = "T67";
			ItemDatabase.shortTankNames["T37"] = "T37";
			ItemDatabase.shortTankNames["M41_Bulldog"] = "M41";
			ItemDatabase.shortTankNames["M24_Chaffee_GT"] = "M24 GT";
			ItemDatabase.shortTankNames["R109_T54S"] = "Т-54 lw";
			ItemDatabase.shortTankNames["R107_LTB"] = "LTTB";
			#endregion 8.7 - 9.3

			// Added in 9.4
			ItemDatabase.shortTankNames["G106_PzKpfwPanther_AusfF"] = "Panther 8.8";
			ItemDatabase.shortTankNames["G108_PzKpfwII_AusfD"] = "Pz 2 D";
			ItemDatabase.shortTankNames["G104_Stug_IV"] = "StuG IV";
			ItemDatabase.shortTankNames["G105_T-55_NVA_DDR"] = "Т-55 NVA";
			ItemDatabase.shortTankNames["G04_PzVI_Tiger_IA"] = "Tiger I";
			ItemDatabase.shortTankNames["T95_E2"] = "T95E2";
			ItemDatabase.shortTankNames["A104_M4A3E8A"] = "M4A3E8A Fury";
			ItemDatabase.shortTankNames["A101_M56"] = "M56 Scorpion";
			ItemDatabase.shortTankNames["A102_T28_concept"] = "T28 Concept";
			ItemDatabase.shortTankNames["R110_Object_260"] = "Object 260";

			// Added in 9.5
			ItemDatabase.shortTankNames["F68_AMX_Chasseur_de_char_46"] = "AMX CdC 46";
			ItemDatabase.shortTankNames["F69_AMX13_57_100"] = "AMX 13 57 100";
			ItemDatabase.shortTankNames["F00_AMX_50Foch_155"] = "Foch 155";

			ItemDatabase.shortTankNames["J18_STA_2_3"] = "STA-2";
			ItemDatabase.shortTankNames["A00_T110E5"] = "T110E5";
			ItemDatabase.shortTankNames["R111_ISU130"] = "ISU 130";
			ItemDatabase.shortTankNames["GB14_M2"] = "M2";
			ItemDatabase.shortTankNames["GB15_Stuart_I"] = "Stuart I";
			ItemDatabase.shortTankNames["GB17_Grant_I"] = "Grant I";
			ItemDatabase.shortTankNames["GB19_Sherman_Firefly"] = "Firefly";
			ItemDatabase.shortTankNames["GB50_Sherman_III"] = "Sherman III";
			ItemDatabase.shortTankNames["GB81_FV4004"] = "FV4004";
			ItemDatabase.shortTankNames["GB44_Archer"] = "Archer";
			ItemDatabase.shortTankNames["GB83_FV4005"] = "FV4005";
			ItemDatabase.shortTankNames["GB41_Challenger"] = "Challenger";
			ItemDatabase.shortTankNames["GB45_Achilles_IIC"] = "Achilles";
			ItemDatabase.shortTankNames["GB80_Charioteer"] = "Charioteer";

			ItemDatabase.shortTankNames["AMX_AC_Mle1948_IGR"] = ItemDatabase.shortTankNames["AMX_AC_Mle1948"];
			ItemDatabase.shortTankNames["AMX_13_90_IGR"] = ItemDatabase.shortTankNames["AMX_13_90"];
			ItemDatabase.shortTankNames["AMX_50_100_IGR"] = ItemDatabase.shortTankNames["AMX_50_100"];
			ItemDatabase.shortTankNames["_105_leFH18B2_IGR"] = ItemDatabase.shortTankNames["_105_leFH18B2"];
			ItemDatabase.shortTankNames["S_35CA_IGR"] = ItemDatabase.shortTankNames["S_35CA"];
			ItemDatabase.shortTankNames["ELC_AMX_IGR"] = ItemDatabase.shortTankNames["ELC_AMX"];
			ItemDatabase.shortTankNames["Bat_Chatillon25t_IGR"] = ItemDatabase.shortTankNames["Bat_Chatillon25t"];
			ItemDatabase.shortTankNames["JagdTiger_SdKfz_185_IGR"] = ItemDatabase.shortTankNames["JagdTiger_SdKfz_185"];
			ItemDatabase.shortTankNames["Panther_II_IGR"] = ItemDatabase.shortTankNames["Panther_II"];
			ItemDatabase.shortTankNames["PzVIB_Tiger_II_IGR"] = ItemDatabase.shortTankNames["PzVIB_Tiger_II"];
			ItemDatabase.shortTankNames["PzVI_IGR"] = ItemDatabase.shortTankNames["PzVI"];
			ItemDatabase.shortTankNames["StuG_40_AusfG_IGR"] = ItemDatabase.shortTankNames["StuG_40_AusfG"];
			ItemDatabase.shortTankNames["E-25_IGR"] = ItemDatabase.shortTankNames["E-25"];
			ItemDatabase.shortTankNames["PzV_IGR"] = ItemDatabase.shortTankNames["PzV"];
			ItemDatabase.shortTankNames["Ferdinand_IGR"] = ItemDatabase.shortTankNames["Ferdinand"];
			ItemDatabase.shortTankNames["Maus_IGR"] = ItemDatabase.shortTankNames["Maus"];
			ItemDatabase.shortTankNames["Ch17_WZ131_1_WZ132_IGR"] = ItemDatabase.shortTankNames["Ch17_WZ131_1_WZ132"];
			ItemDatabase.shortTankNames["Ch11_110_IGR"] = ItemDatabase.shortTankNames["Ch11_110"];
			ItemDatabase.shortTankNames["Ch19_121_IGR"] = ItemDatabase.shortTankNames["Ch19_121"];
			ItemDatabase.shortTankNames["T71_IGR"] = ItemDatabase.shortTankNames["T71"];
			ItemDatabase.shortTankNames["T69_IGR"] = ItemDatabase.shortTankNames["T69"];
			ItemDatabase.shortTankNames["T34_hvy_IGR"] = ItemDatabase.shortTankNames["T34_hvy"];
			ItemDatabase.shortTankNames["Sherman_Jumbo_IGR"] = ItemDatabase.shortTankNames["Sherman_Jumbo"];
			ItemDatabase.shortTankNames["T67_IGR"] = ItemDatabase.shortTankNames["T67"];
			ItemDatabase.shortTankNames["Pershing_IGR"] = ItemDatabase.shortTankNames["Pershing"];
			ItemDatabase.shortTankNames["T29_IGR"] = ItemDatabase.shortTankNames["T29"];
			ItemDatabase.shortTankNames["M4_Sherman_IGR"] = ItemDatabase.shortTankNames["M4_Sherman"];
			ItemDatabase.shortTankNames["M48A1_IGR"] = ItemDatabase.shortTankNames["M48A1"];
			ItemDatabase.shortTankNames["ISU-152_IGR"] = ItemDatabase.shortTankNames["ISU-152"];
			ItemDatabase.shortTankNames["T-44_IGR"] = ItemDatabase.shortTankNames["T-44"];
			ItemDatabase.shortTankNames["R00_T_50_2"] = ItemDatabase.shortTankNames["T_50_2"];
			ItemDatabase.shortTankNames["IS-3_IGR"] = ItemDatabase.shortTankNames["IS-3"];
			ItemDatabase.shortTankNames["R106_KV85_IGR"] = ItemDatabase.shortTankNames["R106_KV85"];
			ItemDatabase.shortTankNames["KV2_IGR"] = ItemDatabase.shortTankNames["KV2"];
			ItemDatabase.shortTankNames["SU-152_IGR"] = ItemDatabase.shortTankNames["SU-152"];
			ItemDatabase.shortTankNames["KV-5_IGR"] = ItemDatabase.shortTankNames["KV-5"];
			ItemDatabase.shortTankNames["Valentine_LL_IGR"] = ItemDatabase.shortTankNames["Valentine_LL"];
			ItemDatabase.shortTankNames["IS-7_IGR"] = ItemDatabase.shortTankNames["IS-7"];
			ItemDatabase.shortTankNames["GB23_Centurion_IGR"] = ItemDatabase.shortTankNames["GB23_Centurion"];
			ItemDatabase.shortTankNames["GB11_Caernarvon_IGR"] = ItemDatabase.shortTankNames["GB11_Caernarvon"];
			ItemDatabase.shortTankNames["GB21_Cromwell_IGR"] = ItemDatabase.shortTankNames["GB21_Cromwell"];
			ItemDatabase.shortTankNames["GB77_FV304_IGR"] = ItemDatabase.shortTankNames["GB77_FV304"];
			ItemDatabase.shortTankNames["GB72_AT15_IGR"] = ItemDatabase.shortTankNames["GB72_AT15"];
			ItemDatabase.shortTankNames["GB08_Churchill_I_IGR"] = ItemDatabase.shortTankNames["GB08_Churchill_I"];
			ItemDatabase.shortTankNames["GB13_FV215b_IGR"] = ItemDatabase.shortTankNames["GB13_FV215b"];
			ItemDatabase.shortTankNames["STA_1_IGR"] = ItemDatabase.shortTankNames["STA_1"];
			ItemDatabase.shortTankNames["Chi_Ri_IGR"] = ItemDatabase.shortTankNames["Chi_Ri"];
			ItemDatabase.shortTankNames["ST_B1_IGR"] = ItemDatabase.shortTankNames["ST_B1"];

			ItemDatabase.AddRename("T-34", "R04_T-34");
			ItemDatabase.AddRename("T37", "A94_T37");

			// Added in 9.6
			ItemDatabase.shortTankNames["G107_PzKpfwIII_AusfK"] = "PZ 3 K";
			ItemDatabase.shortTankNames["A78_M4_Improved"] = "M4 Imp.";
			ItemDatabase.AddRename("Object_704", "R53_Object_704");
			ItemDatabase.shortTankNames["R113_Object_730"] = "Object 730";
			ItemDatabase.shortTankNames["R112_T54_45"] = "T-54-45";
			ItemDatabase.shortTankNames["GB70_N_FV4202_105"] = "FV 4202 N";//GB70_FV4202_105

			// Added in 9.7
			ItemDatabase.shortTankNames["F11_Renault_G1R"] = "Renault G1";
			ItemDatabase.shortTankNames["F44_Somua_S35"] = "Somua S35";
			ItemDatabase.shortTankNames["F70_SARL42"] = "SARL 42";
			ItemDatabase.shortTankNames["F50_FCM36_20t"] = "FCM 36";
			ItemDatabase.shortTankNames["F72_AMX_30"] = "AMX 30 B";
			ItemDatabase.shortTankNames["F71_AMX_30_prototype"] = "AMX 30 1er";//AMX 30 1er prototype
			ItemDatabase.shortTankNames["F49_RenaultR35"] = "Renault R35";
			ItemDatabase.shortTankNames["F69_AMX13_57_100_GrandFinal"] = "AMX 13 57 GF";
			ItemDatabase.shortTankNames["G109_Steyr_WT"] = "Steyr WT";
			ItemDatabase.shortTankNames["A35_Pershing"] = "M26 Pershing";
			ItemDatabase.shortTankNames["A35_Pershing_IGR"] = "A35 Pershing";
			ItemDatabase.shortTankNames["R90_IS_4M"] = "IS-4M";
			ItemDatabase.shortTankNames["R71_IS_2B"] = "IS-2B";
			ItemDatabase.shortTankNames["R117_T34_85_Rudy"] = "T-34-85 R";
			ItemDatabase.shortTankNames["R116_ISU122C_Berlin"] = "ISU-122S";
			ItemDatabase.shortTankNames["R114_Object_244"] = "Object 244";
			ItemDatabase.shortTankNames["GB52_A45"] = "FV201 (A45)";
			ItemDatabase.shortTankNames["GB85_Cromwell_Berlin"] = "Cromwell B";

			// Added in 9.8
			ItemDatabase.AddRename("AMX_50_120", "F09_AMX_50_120");
			ItemDatabase.AddRename("Hummel", "G02_Hummel");
			ItemDatabase.AddRename("T32", "A12_T32");
			ItemDatabase.AddRename("M10_Wolverine", "A30_M10_Wolverine");
			ItemDatabase.AddRename("Sherman_Jumbo", "A36_Sherman_Jumbo");
			ItemDatabase.AddRename("Sherman_Jumbo_IGR", "A36_Sherman_Jumbo_IGR");
			ItemDatabase.AddRename("T26_E4_SuperPershing", "A80_T26_E4_SuperPershing");
			ItemDatabase.AddRename("M48A1", "A84_M48A1");
			ItemDatabase.AddRename("M48A1_IGR", "A84_M48A1_IGR");
			ItemDatabase.AddRename("SU-85", "R02_SU-85");
			ItemDatabase.AddRename("IS-3", "R19_IS-3");
			ItemDatabase.AddRename("IS-3_IGR", "R19_IS-3_IGR");
			ItemDatabase.AddRename("Leopard1", "G89_Leopard1");

			ItemDatabase.shortTankNames["F10_AMX_50B_fallout"] = "AMX 50 B (D)";
			ItemDatabase.shortTankNames["G112_KanonenJagdPanzer"] = "KanonenJgPz";
			ItemDatabase.shortTankNames["G58_VK4502P"] = "VK 45.02 B7";
			ItemDatabase.shortTankNames["Ch25_121_mod_1971B"] = "121B";
			ItemDatabase.shortTankNames["Ch26_59_Patton"] = "59-Patton";
			ItemDatabase.shortTankNames["T110_fallout"] = "T110E5 D";
			ItemDatabase.shortTankNames["R115_IS-3_auto"] = "IS-3 Auto";
			ItemDatabase.shortTankNames["Object_907A"] = "Object 907A";
			ItemDatabase.shortTankNames["R105_BT_7A"] = "BT-7 SPG";
			ItemDatabase.shortTankNames["Object_140_fallout"] = "Object 140 D";
			ItemDatabase.shortTankNames["J19_Tiger_I_Jpn"] = "Tiger";

			// Added in 9.9
			ItemDatabase.AddRename("AMX_50_100", "F08_AMX_50_100");
			ItemDatabase.AddRename("FCM_50t", "F65_FCM_50t");
			ItemDatabase.AddRename("AMX_50_100_IGR", "F08_AMX_50_100_IGR");
			ItemDatabase.AddRename("VK3601H", "G15_VK3601H");
			ItemDatabase.AddRename("Panther_M10", "G78_Panther_M10");
			ItemDatabase.AddRename("M46_Patton", "A63_M46_Patton");
			ItemDatabase.AddRename("T23E3", "A86_T23E3");
			ItemDatabase.AddRename("T110E4", "A83_T110E4");
			ItemDatabase.AddRename("T110E3", "A85_T110E3");
			ItemDatabase.AddRename("M6A2E1", "A45_M6A2E1");
			ItemDatabase.AddRename("T62A", "R87_T62A");

			ItemDatabase.shortTankNames["F73_M4A1_Revalorise"] = "M4A1 Rev";
			ItemDatabase.shortTankNames["G114_Skorpian"] = "Skorpion";//Rheinmetall Skorpion
			ItemDatabase.shortTankNames["G113_SP_I_C"] = "SP I C";
			ItemDatabase.shortTankNames["Bat_Chatillon25t_fallout"] = "B.Chat 25t F";
			ItemDatabase.shortTankNames["G89_Leopard1_fallout"] = "Leopard 1 F";
			ItemDatabase.shortTankNames["T57_58_fallout"] = "T57 Heavy F";
			ItemDatabase.shortTankNames["R119_Object_777"] = "Object 777";
			ItemDatabase.shortTankNames["R118_T28_F30"] = "T28 F30";
			ItemDatabase.shortTankNames["Object268_fallout"] = "Obj. 268 F";
			ItemDatabase.shortTankNames["IS-7_fallout"] = "IS-7 F";

			// Added in 9.10
			ItemDatabase.AddRename("ARL_44", "F06_ARL_44");
			ItemDatabase.AddRename("AMX_M4_1945", "F07_AMX_M4_1945");
			ItemDatabase.AddRename("Pz35t", "G07_Pz35t");
			ItemDatabase.AddRename("JagdPzIV", "G17_JagdPzIV");
			ItemDatabase.AddRename("PanzerJager_I", "G21_PanzerJager_I");
			ItemDatabase.AddRename("Sturmpanzer_II", "G22_Sturmpanzer_II");
			ItemDatabase.AddRename("VK1602", "G26_VK1602");
			ItemDatabase.AddRename("JagdTiger", "G44_JagdTiger");
			ItemDatabase.AddRename("PzII_J", "G36_PzII_J");
			ItemDatabase.AddRename("M2_lt", "A02_M2_lt");
			ItemDatabase.AddRename("M41", "A18_M41");
			ItemDatabase.AddRename("T110", "A69_T110E5");
			ItemDatabase.AddRename("T-28", "R06_T-28");
			ItemDatabase.AddRename("SU-76", "R24_SU-76");
			ItemDatabase.AddRename("T71", "A103_T71E1");
			ItemDatabase.AddRename("Pro_Ag_A", "G91_Pro_Ag_A");
			ItemDatabase.AddRename("SU122A", "R100_SU122A");
			ItemDatabase.AddRename("T71_IGR", "A103_T71E1_IGR");


			ItemDatabase.shortTankNames["A106_M48A2_120"] = "M48A2";
			ItemDatabase.shortTankNames["G115_Typ_205_4_Jun"] = "VK 100.01 (P)";
			ItemDatabase.shortTankNames["R120_T22SR_A22"] = "T-22 sr.";
			ItemDatabase.shortTankNames["R87_T62A_fallout"] = "T62A F";
			ItemDatabase.shortTankNames["J20_Type_2605"] = "Type 5 Heavy";
			ItemDatabase.shortTankNames["J25_Type_4"] = "Type 4 Heavy";
			ItemDatabase.shortTankNames["J22_Type_95"] = "Type 95 Heavy";
			ItemDatabase.shortTankNames["J21_Type_91"] = "Type 91 Heavy";
			ItemDatabase.shortTankNames["J27_O_I_120"] = "O-Ho";
			ItemDatabase.shortTankNames["J28_O_I_100"] = "O-Ni";
			ItemDatabase.shortTankNames["J24_Mi_To_130_tons"] = "O-I";
			ItemDatabase.shortTankNames["J23_Mi_To"] = "O-I Exp";
			ItemDatabase.shortTankNames["J26_Type_89"] = "Type 89";


			//http://wiki.wargaming.net/en/Tank:R117_T34_85_Rudy
			//ItemDatabase.AddRename("aaaaaaaaaaaaaaaaaaaa", "zzzzzzzzzzzzzzzzz");
			//ItemDatabase.shortTankNames["xxxxxxxxxxxxxxxxxxxx"] = "yyyyyyyyyyyyy";
		}

		private static void InitializeTinyTankNames()
		{
			foreach (var kvp in ItemDatabase.shortTankNames) ItemDatabase.tinyTankNames.Add(kvp.Key, kvp.Value);

			#region 7.0
			ItemDatabase.tinyTankNames["_105_leFH18B2"] = "105 leFH";
			ItemDatabase.tinyTankNames["AMX_13F3AM"] = "AMX 13 F3";
			ItemDatabase.tinyTankNames["AMX50_Foch"] = "AMX Foch";
			ItemDatabase.tinyTankNames["AMX_50Fosh_155"] = "AMX F 155";
			ItemDatabase.tinyTankNames["AMX_AC_Mle1946"] = "AMX AC46";
			ItemDatabase.tinyTankNames["AMX_AC_Mle1948"] = "AMX AC48";
			ItemDatabase.tinyTankNames["Bat_Chatillon25t"] = "B.Ch.25t";
			ItemDatabase.tinyTankNames["Bat_Chatillon155"] = "B.Ch.155";
			ItemDatabase.tinyTankNames["Hotchkiss_H35"] = "H 35";
			ItemDatabase.tinyTankNames["Lorraine39_L_AM"] = "Lor 39 L";
			ItemDatabase.tinyTankNames["Lorraine40t"] = "Lorr 40t";
			ItemDatabase.tinyTankNames["Lorraine155_50"] = "Lor 155 50";
			ItemDatabase.tinyTankNames["Lorraine155_51"] = "Lor 155 51";
			ItemDatabase.tinyTankNames["RenaultFT_AC"] = "Ren FT AC";
			ItemDatabase.tinyTankNames["Somua_Sau_40"] = "Som S-40";
			ItemDatabase.tinyTankNames["DickerMax"] = "Dic Max";
			ItemDatabase.tinyTankNames["JagdPantherII"] = "JgPant 2";
			ItemDatabase.tinyTankNames["S35_captured"] = "Pz S35 .f";
			ItemDatabase.tinyTankNames["VK3002DB"] = "VK 30.02 D";
			ItemDatabase.tinyTankNames["M10_Wolverine"] = "M10 Wolv";
			ItemDatabase.tinyTankNames["M18_Hellcat"] = "M18 Hell";
			ItemDatabase.tinyTankNames["M24_Chaffee"] = "M24 Chaff";
			ItemDatabase.tinyTankNames["M36_Slagger"] = "M36 Jack";
			ItemDatabase.tinyTankNames["M4_Sherman"] = "M4";
			ItemDatabase.tinyTankNames["M46_Patton"] = "M46 Patt";
			ItemDatabase.tinyTankNames["G_Panther"] = "GW PANTHER";
			ItemDatabase.tinyTankNames["Object_704"] = "Obj 704";
			ItemDatabase.tinyTankNames["Object_212"] = "Obj 212";
			ItemDatabase.tinyTankNames["Object_261"] = "Obj 261";
			ItemDatabase.tinyTankNames["Object268"] = "Obj 268";
			ItemDatabase.tinyTankNames["AMX_M4_1945"] = "AMX M4";
			ItemDatabase.tinyTankNames["B-1bis_captured"] = "Pz B2 .f";
			ItemDatabase.tinyTankNames["H39_captured"] = "Pz H38 .f";
			ItemDatabase.tinyTankNames["JagdPanther"] = "JGPANTHER";
			ItemDatabase.tinyTankNames["M22_Locust"] = "M22";
			ItemDatabase.tinyTankNames["JagdTiger_SdKfz_185"] = "JgTg 88";
			ItemDatabase.tinyTankNames["PzIV_Hydro"] = "Pz 4 Hyd";
			ItemDatabase.tinyTankNames["PzII_Luchs"] = "Luchs";
			#endregion 7.0

			// Added in 8.0
			ItemDatabase.tinyTankNames["PzIV_schmalturm"] = "PZ 4 S";
			ItemDatabase.tinyTankNames["Panther_M10"] = "PANT. M10";
			ItemDatabase.tinyTankNames["Object263"] = "Obj 263";

			// Added in 8.1
			ItemDatabase.tinyTankNames["GB08_Churchill_I"] = "Church. I";
			ItemDatabase.tinyTankNames["GB10_Black_Prince"] = "BP";
			ItemDatabase.tinyTankNames["GB11_Caernarvon"] = "Caern.";
			ItemDatabase.tinyTankNames["GB09_Churchill_VII"] = "Church.7";
			ItemDatabase.tinyTankNames["GB03_Cruiser_Mk_I"] = "Cruis. I";
			ItemDatabase.tinyTankNames["GB12_Conqueror"] = "Conquer.";
			ItemDatabase.tinyTankNames["GB23_Centurion"] = "Centuri.";

			// Added in 8.2
		}
		#endregion TankNames
	}
}