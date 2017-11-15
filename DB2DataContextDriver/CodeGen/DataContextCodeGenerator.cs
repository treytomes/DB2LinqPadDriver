using DB2DataContextDriver.DB2;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2DataContextDriver.CodeGen
{
	public class DataContextCodeGenerator
	{
		#region Fields

		private IEnumerable<TableInfo> _tables;
		private IEnumerable<StoredProcedureInfo> _routines;
		private string _nameSpace;
		private string _typeName;

		#endregion

		#region Constructors

		public DataContextCodeGenerator(IEnumerable<TableInfo> tables, IEnumerable<StoredProcedureInfo> routines, string nameSpace, string typeName)
		{
			_tables = tables;
			_routines = routines;
			_nameSpace = nameSpace;
			_typeName = typeName;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Generate the source code for the data context.
		/// </summary>
		public string Generate()
		{
			var nsDef = new NamespaceDefinition()
			{
				Name = _nameSpace,

				// Add classes for each of the tables.
				Classes = _tables.Select(table => GenerateTableClass(table)).ToList()
			};

			nsDef.Classes.Add(GenerateDataContextClassV2());

			return new StringBuilder()
				.AppendLine(Using("IBM.Data.DB2"))
				.AppendLine(Using("System.Collections.Generic"))
				.AppendLine(Using("System.Data"))
				.AppendLine(nsDef.ToString())
				.ToString();
		}

		private ClassDefinition GenerateTableClass(TableInfo table)
		{
			return new ClassDefinition()
			{
				Name = table.Name, // + "Item",
				Methods = new List<string>(),

				// Create a property for each of the columns.
				Properties = table.Columns.Select(column => GenerateProperty(column)).ToList()
			};
		}

		private ClassDefinition GenerateDataContextClassV2()
		{
			var ctxDef = new ClassDefinition()
			{
				Name = _typeName,
				Inherits = "LinqToDB.Data.DataConnection",
				Methods = new List<string>()
				{
					// This is the constructor.
					$"public {_typeName}(string connectionString) : base(new LinqToDB.DataProvider.DB2.DB2DataProvider(\"DB2\", LinqToDB.DataProvider.DB2.DB2Version.zOS), connectionString) {{ }}"
				}
			};

			// Create an accessor for each of our tables.  This accessor will use a data reader to iterate over the table's rows.
			foreach (var table in _tables)
			{
				ctxDef.Methods.Add($"public LinqToDB.ITable<{table.Name}> {table.Name} {{ get {{ return GetTable<{table.Name}>(); }} }}");
			}

			// Create a method for each stored procedure.
			foreach (var routine in _routines)
			{
				ctxDef.Methods.Add(GenerateMethod(routine));
			}

			return ctxDef;
		}

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

		private string Using(string nameSpace)
		{
			return $"using {nameSpace};";
		}

		private PropertyDefinition GenerateProperty(ColumnInfo column)
		{
			var property = new PropertyDefinition()
			{
				Name = column.Name,
				Type = column.DotNetColumnType
			};
			if (column.IsPrimary)
			{
				property.Attributes.Add("[LinqToDB.Mapping.PrimaryKey]");
			}
			return property;
		}

		private string GenerateMethod(StoredProcedureInfo routine)
		{
			var sb = new StringBuilder();

			//sb.Append($"public object {routine.RoutineName}(");

			//// Generate the parameter list.
			//foreach (var param in parameters)
			//{
			//	sb.Append($"{param.DotNetModifier} {param.DotNetType} {param.Name}");
			//	if (param != parameters.Last())
			//	{
			//		sb.Append(", ");
			//	}
			//}

			sb.AppendLine($"public IBM.Data.DB2.DB2DataReader {routine.DotNetDragText} {{");

			// Generate the method body.
			sb.AppendLine($"\t\tusing (var cmd = new IBM.Data.DB2.DB2Command(\"{routine.RoutineName}\", Connection as IBM.Data.DB2.DB2Connection)) {{");
			sb.AppendLine("\t\t\tcmd.CommandType = System.Data.CommandType.StoredProcedure;");

			var parameters = routine.GetParameters().ToArray();
			for (var index = 0; index < parameters.Length; index++)
			{
				if (parameters[index].Direction == System.Data.ParameterDirection.Output)
				{
					sb.AppendLine($"\t\t\tcmd.Parameters.Add(new IBM.Data.DB2.DB2Parameter(\"{parameters[index].Name}\", IBM.Data.DB2.DB2Type.{DB2TypeFactory.GetDB2TypeFromString(parameters[index].DB2Type)})).Direction = System.Data.ParameterDirection.{parameters[index].Direction};");
					//sb.AppendLine($"\tcmd.Parameters[cmd.Parameters.Count - 1].Direction = System.Data.ParameterDirection.{parameters[index].Direction};");
				}
				else
				{
					sb.AppendLine($"\t\t\tcmd.Parameters.Add(new IBM.Data.DB2.DB2Parameter(\"{parameters[index].Name}\", {parameters[index].Name})).Direction = System.Data.ParameterDirection.{parameters[index].Direction};");
					//sb.AppendLine($"\tcmd.Parameters[cmd.Parameters.Count - 1].Direction = System.Data.ParameterDirection.{parameters[index].Direction};");
				}
			}

			sb.AppendLine("\t\t\tvar reader = cmd.ExecuteReader();");

			var outputParams = parameters.Where(x => x.Direction != System.Data.ParameterDirection.Input);
			foreach (var param in outputParams)
			{
				sb.AppendLine($"\t\t\t{param.Name}=({param.DotNetType})(cmd.Parameters[\"{param.Name}\"].Value);");
			}

			sb.AppendLine("\t\t\treturn reader; } }");

			return sb.ToString();
		}

		#endregion
	}
}
