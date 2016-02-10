using LINQPad.Extensibility.DataContext;
using System.Collections.Generic;
using System.Collections;
using DB2DataContextDriver.DB2;

namespace DB2DataContextDriver
{
	public class DatabaseExplorerInfoList : IEnumerable<ExplorerItem>
	{
		#region Fields

		private IEnumerable<TableInfo> _tables;

		#endregion

		#region Constructors

		public DatabaseExplorerInfoList(IEnumerable<TableInfo> tables)
		{
			_tables = tables;
		}

		public DatabaseExplorerInfoList(DB2Properties properties)
			: this(new TableInfoList(properties))
		{
		}

		#endregion

		#region Methods

		public IEnumerator<ExplorerItem> GetEnumerator()
		{
			return GetTables(_tables).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private static IEnumerable<ExplorerItem> GetTables(string connectionString, string schema)
		{
			using (var tables = new TableInfoList(connectionString, schema))
			{
				return GetTables(tables);
			}
		}

		private static IEnumerable<ExplorerItem> GetTables(IEnumerable<TableInfo> tables)
		{
			foreach (var table in tables)
			{
				yield return new ExplorerItem(table.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
				{
					Tag = table,
					DragText = table.Name,
					ToolTipText = table.Remarks,
					SqlName = table.Name,
					IsEnumerable = true,
					Children = new List<ExplorerItem>(GetTableColumns(table.Columns))
				};
			}
			yield break;
		}

		private static IEnumerable<ExplorerItem> GetTableColumns(IEnumerable<ColumnInfo> columns)
		{
			foreach (var column in columns)
			{
				yield return new ExplorerItem(column.Name, ExplorerItemKind.Property, column.IsPrimary ? ExplorerIcon.Key : ExplorerIcon.Column)
				{
					Tag = column,
					DragText = column.Name,
					ToolTipText = column.Remarks,
					SqlName = column.Name,
					SqlTypeDeclaration = column.SqlColumnType,
					Children = new List<ExplorerItem>()
					{
						new ExplorerItem($"SQL Type = {column.SqlColumnType}", ExplorerItemKind.Property, ExplorerIcon.Blank),
						new ExplorerItem($".NET Type = {column.DotNetColumnType}", ExplorerItemKind.Property, ExplorerIcon.Blank)
					}
				};
			}
			yield break;
		}

		#endregion
	}
}
