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
		private string _nameSpace;
		private string _typeName;

		#endregion

		#region Constructors

		public DataContextCodeGenerator(IEnumerable<TableInfo> tables, string nameSpace, string typeName)
		{
			_tables = tables;
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
			
			nsDef.Classes.Add(GenerateDataContextClass());

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
				Name = table.Name + "Item",
				Methods = new List<string>(),

				// Create a property for each of the columns.
				Properties = table.Columns.Select(column => GenerateProperty(column)).ToList()
			};
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
			return new PropertyDefinition() { Name = column.Name, Type = column.DotNetColumnType };
		}

		#endregion
	}
}
