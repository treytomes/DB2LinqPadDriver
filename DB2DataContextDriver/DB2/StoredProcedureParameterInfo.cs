using System;
using System.Text.RegularExpressions;

namespace DB2DataContextDriver.DB2
{
	public class StoredProcedureParameterInfo
	{
		#region Fields

		private static Regex _parameterRegex = new Regex(@"
^\s*(
	(?<ParameterModifier>IN|OUT|INOUT)\s+)?
	(?<ParameterName>[\w\d_]+)\s+
	(?<ParameterType>\w+(\(\d+(,\d+)?\))?
)
", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
		
		#endregion

		#region Constructors

		public StoredProcedureParameterInfo(string parameterText)
		{
			var match = _parameterRegex.Match(parameterText);
			if (!match.Success)
			{
				throw new Exception("Invalid parameter text.");
			}

			if (match.Groups["ParameterModifier"].Success)
			{
				DirectionText = match.Groups["ParameterModifier"].Value;
			}
			else
			{
				DirectionText = "IN";
			}

			if (DirectionText == "IN")
			{
				Direction = System.Data.ParameterDirection.Input;
			}
			if (DirectionText == "INOUT")
			{
				Direction = System.Data.ParameterDirection.InputOutput;
			}
			if (DirectionText == "OUT")
			{
				Direction = System.Data.ParameterDirection.Output;
			}


			Name = match.Groups["ParameterName"].Value;

			DB2Type = match.Groups["ParameterType"].Value;

			DotNetType = DB2TypeFactory.GetTypeFromDB2(DB2Type);
			DotNetModifier = DB2TypeFactory.GetModifierFromDB2(DirectionText);
		}

		#endregion

		#region Properties

		public string Name { get; }

		public string DirectionText { get; }

		public System.Data.ParameterDirection Direction { get; }

		public string DB2Type { get; }

		public string DotNetType { get; }

		public string DotNetModifier { get; }

		#endregion
	}
}
