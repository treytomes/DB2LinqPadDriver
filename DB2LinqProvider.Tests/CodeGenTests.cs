using DB2DataContextDriver;
using DB2DataContextDriver.CodeGen;
using DB2DataContextDriver.DB2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DB2LinqProvider.Tests
{
	public class CodeGenTests
	{
		private string _typeName;
		private IEnumerable<TableInfo> _tables;

		//public static void Main()
		//{
		//	var tests = new CodeGenTests();
		//	tests.Can_generate_data_context_code();
		//}

		[Fact]
		public void Can_generate_data_context_code()
		{
			_typeName = "MyDataContext";

			var properties = new TestDB2Properties();

			_tables = new TableInfoList(properties);

			var path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName()).Replace(".tmp", ".txt");
			File.WriteAllText(path, GenerateDataContextClass().ToString());

			var process = Process.Start(path);
		}

		/// <remarks>
		/// Pulled from DataContextCodeGenerator.cs
		/// </remarks>
		private ClassDefinition GenerateDataContextClass()
		{
			var ctxDef = new ClassDefinition()
			{
				Name = _typeName,
				Methods = new List<string>()
				{
					// This is the constructor.
					$"public {_typeName}(string connectionString) {{ ConnectionString = connectionString; }}"
				},
				Properties = new List<PropertyDefinition>()
				{
					new PropertyDefinition() { Name = "ConnectionString", Type = "string" }
				}
			};

			// Create an accessor for each of our tables.  This accessor will use a data reader to iterate over the table's rows.
			foreach (var table in _tables)
			{
				var sb = new StringBuilder()
					.AppendLine($"public IEnumerable<{table.Name}Item> {table.Name} {{ get {{")
					.AppendLine("using(var cn=new DB2Connection(ConnectionString)) { cn.Open();")
					.AppendLine("using(var cm=cn.CreateCommand()) {")
					.AppendLine("cm.CommandType=CommandType.Text;")
					.AppendLine($"cm.CommandText=\"SELECT * FROM {table.Name}\";")
					.AppendLine("using (var reader = cm.ExecuteReader()) { while (reader.Read()) {")
					.AppendLine($"var item = new {table.Name}Item();");

				foreach (var column in table.Columns)
				{
					sb.AppendLine($"item.{column.Name}=({column.DotNetColumnType})System.Convert.ChangeType(reader.GetValue(reader.GetOrdinal(\"{column.Name}\")), typeof({column.DotNetColumnType}));");
				}

				sb.AppendLine("yield return item; } } } cn.Close(); } } }");
				ctxDef.Methods.Add(sb.ToString());
			}
			return ctxDef;
		}

		private class TestDB2Properties : IDB2Properties
		{
			public TestDB2Properties()
			{
				Schema = "DB2INST1";
			}

			public string Schema { get; set; }

			public string GetConnectionString()
			{
				return string.Format("Server={0};DATABASE={1};PWD={2};UID={3};Persist Security Info=True;Connection Lifetime=60;Connection Reset=false;Min Pool Size=0;Max Pool Size=100;Pooling=true;",
					"EESIBM02",
					"SYSTEM",
					"db2power",
					Schema);
			}
		}
	}
}
