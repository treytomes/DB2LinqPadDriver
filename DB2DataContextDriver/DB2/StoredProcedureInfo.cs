using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DB2DataContextDriver.DB2
{
	public class StoredProcedureInfo
	{
		#region Fields

		private static Regex _storedProcRegex = new Regex(@"
^\s*CREATE(\s+OR\s+REPLACE)?\s+(PROCEDURE|FUNCTION)\s+
	((?<Schema>[\w\d]+)\.)?
	((?<RoutineName>[\w\d_]+))\s*\(
		(?<Parameters>(\s*
			((?<Parameter>
				\s*((?<ParameterModifier>IN|OUT|INOUT)\s+)?
				(?<ParameterName>[\w\d_]+)\s+
				(?<ParameterType>\w+\s*(\(\d+(,\d+)?\))?)
			)\s*,)*

			((?<Parameter>
				\s*((?<ParameterModifier>IN|OUT|INOUT)\s+)?
				(?<ParameterName>[\w\d_]+)\s+
				(?<ParameterType>\w+\s*(\(\d+(,\d+)?\))?)
			)\s*)*
		)
	)\)
", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

		#endregion

		#region Constructors

		public StoredProcedureInfo(string routineName, string routineDefinition)
		{
			RoutineName = routineName;
			RoutineDefinition = routineDefinition;
		}

		#endregion

		#region Properties

		public string RoutineName { get; }

		public string RoutineDefinition { get; }

		#endregion

		#region Methods

		public IEnumerable<StoredProcedureParameterInfo> GetParameters()
		{
			var match = _storedProcRegex.Match(RoutineDefinition);
			if (!match.Success)
			{
				throw new Exception("Invalid stored procedure text.");
			}

			foreach (Capture parameter in match.Groups["Parameter"].Captures)
			{
				yield return new StoredProcedureParameterInfo(parameter.Value);
			}
		}

		#endregion
	}
}
