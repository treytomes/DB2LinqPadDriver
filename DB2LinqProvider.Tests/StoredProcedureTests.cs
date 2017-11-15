using DB2DataContextDriver.DB2;
using System;
using System.Text.RegularExpressions;
using Xunit;
using System.Linq;
using System.Collections.Generic;

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
			tests.Can_call_stored_procedure_1();
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

		public void Can_call_stored_procedure_2()
		{
			/*
			var databaseName = "SYSTEM";
			var routineName = "GETCHILDELEMENTS";
			var paramValues = new object[] { 366 };
			*/

			var databaseName = "INVENT";
			var routineName = "GETAVAILSTOCKBYCATVND";
			var paramValues = new object[] { "CHF120", "CHD", 34, 0, 0 };

			var connectionString = $"Server=EESIBM02;DATABASE={databaseName};PWD=db2power;UID=DB2INST1;Persist Security Info=True;Connection Lifetime=60;Connection Reset=false;Min Pool Size=0;Max Pool Size=100;Pooling=true;";
			var routines = new StoredProcedureInfoList(connectionString, "DB2INST1");
			var routine = routines.Single(x => x.RoutineName == routineName);
			var parameters = routine.GetParameters().ToArray();

			using (var cn = new IBM.Data.DB2.DB2Connection(connectionString))
			{
				cn.Open();

				using (var cmd = new IBM.Data.DB2.DB2Command(routine.RoutineName, cn))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;

					for (var index = 0; index < parameters.Length; index++)
					{
						cmd.Parameters.Add(new IBM.Data.DB2.DB2Parameter(parameters[index].Name, paramValues[index]));
						cmd.Parameters[cmd.Parameters.Count - 1].Direction = parameters[index].Direction;
					}

					using (var reader = cmd.ExecuteReader())
					{
						var outputParams = cmd.Parameters.Cast<IBM.Data.DB2.DB2Parameter>()
							.Where(param => param.Direction != System.Data.ParameterDirection.Input)
							.Select(param => new KeyValuePair<string, object>(param.ParameterName, param.Value));

						foreach (var param in outputParams)
						{
							Console.WriteLine("{0}={1}", param.Key, param.Value);
						}

						while (reader.Read())
						{
							Console.WriteLine(reader.ToString());
						}
					}
				}

				cn.Close();
			}
		}

		public void Can_call_stored_procedure_1()
		{
			var connectionString = "Server=EESIBM02;DATABASE=INVENT;PWD=db2power;UID=DB2INST1;Persist Security Info=True;Connection Lifetime=60;Connection Reset=false;Min Pool Size=0;Max Pool Size=100;Pooling=true;";
			using (var cn = new IBM.Data.DB2.DB2Connection(connectionString))
			{
				cn.Open();

				using (var cmd = new IBM.Data.DB2.DB2Command("GETAVAILSTOCKBYCATVND", cn))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.Parameters.Add("INVCATALOG", "CHF120").Direction = System.Data.ParameterDirection.Input;
					cmd.Parameters.Add("INVVENDOR", "CHD").Direction = System.Data.ParameterDirection.Input;
					cmd.Parameters.Add("STORE", 34).Direction = System.Data.ParameterDirection.Input;
					cmd.Parameters.Add("CURSTORESTOCKAVAILABLE", IBM.Data.DB2.DB2Type.Integer).Direction = System.Data.ParameterDirection.Output;
					cmd.Parameters.Add("ALLSTORESTOCKAVAILABLE", IBM.Data.DB2.DB2Type.Integer).Direction = System.Data.ParameterDirection.Output;

					//cmd.ExecuteNonQuery();

					using (var reader = cmd.ExecuteReader())
					{
						foreach (IBM.Data.DB2.DB2Parameter param in cmd.Parameters)
						{
							if (param.Direction != System.Data.ParameterDirection.Input)
							{
								Console.WriteLine("{0}={1}", param.ParameterName, param.Value);
							}
						}

						while (reader.Read())
						{
							Console.WriteLine(reader.ToString());
						}
					}
				}

				cn.Close();
			}
		}
	}
}
