using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Phobos.WoT
{
	public static class XmlNodeExtensions
	{
		public static float? ParseSingle(this XmlNode node, string xpath)
		{
			XmlNode found = node.SelectSingleNode(xpath + "/text()");
			if (found != null) return Single.Parse(found.InnerText);
			else return null;
		}

		public static int? ParseInt32(this XmlNode node, string xpath)
		{
			XmlNode found = node.SelectSingleNode(xpath);
			if (found != null) return Int32.Parse(found.InnerText);
			else return null;
		}

        public static float? ParseFloat(this XmlNode node, string xpath)
        {
            XmlNode found = node.SelectSingleNode(xpath);
            if (found != null) return float.Parse(found.InnerText);
            else return null;
        }

        public static bool? ParseBool(this XmlNode node, string xpath)
		{
			XmlNode found = node.SelectSingleNode(xpath);
			if (found != null) return found.InnerText == "true";
			else return null;
		}

		public static string[] ParseArray(this XmlNode node, string xpath)
		{
			XmlNode found = node.SelectSingleNode(xpath);
			if (found != null) return found.InnerText.Split(' ');
			else return null;
		}

		private static Regex decimalFormat = new Regex(@"-?\d+(?:\.\d+)?", RegexOptions.Compiled);
		public static float[] ParseSingleArray(this XmlNode node, string xpath)
		{
			var array = node.ParseArray(xpath);

			if (array == null)
			{
				return null;
			}

			return array.Where(v => !String.IsNullOrWhiteSpace(v)).Select(v => Single.Parse(decimalFormat.Match(v).Value)).ToArray();
		}

		public static float[] ParseLimits(this XmlNode node, string xpath)
		{
			float[] limits = node.ParseSingleArray(xpath);
			if (limits == null) return null;

			if (limits.Length != 2) throw new ArgumentException("Yaw or pitch limits are not valid!", xpath);

			return limits;
		}
	}
}