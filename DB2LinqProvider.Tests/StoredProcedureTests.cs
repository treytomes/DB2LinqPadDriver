using DB2DataContextDriver.DB2;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace DB2LinqProvider.Tests
{
	public class StoredProcedureTests
	{
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

		public static void Main()
		{
			var tests = new StoredProcedureTests();
			tests.Can_generate_functions();
		}

		[Fact]
		public void Can_generate_functions()
		{
			var list = new StoredProcedureInfoList("Server=EESIBM02;DATABASE=INVENT;PWD=db2power;UID=DB2INST1;Persist Security Info=True;Connection Lifetime=60;Connection Reset=false;Min Pool Size=0;Max Pool Size=100;Pooling=true;", "DB2INST1");

			foreach (var item in list)
			{
				Console.WriteLine(item.RoutineName);

				var match = _storedProcRegex.Match(item.RoutineDefinition);
				if (!match.Success)
				{
					throw new Exception($"Failed to match on stored procedure: {item.RoutineName}");
				}
				else
				{
					for (var n = 0; n < match.Groups["Parameter"].Captures.Count; n++)
					{
						Console.WriteLine("\t{0} : {1}",
							match.Groups["ParameterName"].Captures[n].Value,
							//match.Groups["ParameterModifier"]?.Captures[n]?.Value,
							match.Groups["ParameterType"].Captures[n].Value);
					}

				}

				//Console.WriteLine("\t" + item.RoutineDefinition);
			}
		}
	}
}
