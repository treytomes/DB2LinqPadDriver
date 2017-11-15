using LINQPad.Extensibility.DataContext;
using System.Collections.Generic;
using System.Collections;
using DB2DataContextDriver.DB2;
using System.Linq;

namespace DB2DataContextDriver
{
	public class DatabaseExplorerInfoList : IEnumerable<ExplorerItem>
	{
		#region Fields

		private IEnumerable<TableInfo> _tables;
		private IEnumerable<StoredProcedureInfo> _routines;

		#endregion

		#region Constructors

		public DatabaseExplorerInfoList(IEnumerable<TableInfo> tables, IEnumerable<StoredProcedureInfo> routines)
		{
			_tables = tables;
			_routines = routines;
		}

		public DatabaseExplorerInfoList(DB2Properties properties)
			: this(new TableInfoList(properties), new StoredProcedureInfoList(properties))
		{
		}

		#endregion

		#region Methods

		public IEnumerator<ExplorerItem> GetEnumerator()
		{
			yield return new ExplorerItem("Tables", ExplorerItemKind.Category, ExplorerIcon.Table)
			{
				IsEnumerable = true,
				Children = GetTables(_tables).ToList()
			};

			yield return new ExplorerItem("Stored Procedures", ExplorerItemKind.Category, ExplorerIcon.StoredProc)
			{
				IsEnumerable = true,
				Children = GetRoutines(_routines).ToList()
			};

			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private static IEnumerable<ExplorerItem> GetRoutines(string connectionString, string schema)
		{
			using (var routines = new StoredProcedureInfoList(connectionString, schema))
			{
				return GetRoutines(routines);
			}
		}

		private static IEnumerable<ExplorerItem> GetRoutines(IEnumerable<StoredProcedureInfo> routines)
		{
			foreach (var routine in routines)
			{
				yield return new ExplorerItem(routine.RoutineName, ExplorerItemKind.QueryableObject, ExplorerIcon.StoredProc)
				{
					Tag = routine,
					DragText = $"{routine.DotNetDragText}",
					ToolTipText = routine.RoutineName,
					SqlName = $"{routine.RoutineName}",
					IsEnumerable = false,
					Children = new List<ExplorerItem>(
						routine.GetParameters().Select(x =>
							new ExplorerItem(string.Format("{0} ({1} {2})", x.Name, x.DotNetModifier, x.DotNetType), ExplorerItemKind.Parameter, ExplorerIcon.Parameter)
							{
								SqlName = string.Format("{0} ({1}, {2})", x.Name, x.DB2Type, x.DirectionText)
							}).Concat(
							new[] {
								new ExplorerItem("Get Contents", ExplorerItemKind.ReferenceLink, ExplorerIcon.View)
								{
									DragText = routine.RoutineDefinition,
									ToolTipText = "Drag to the editor to get the contents of the stored procedure.",
									IsEnumerable = false
								}}))
				};
			}
			yield break;
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
						new ExplorerItem($"SQL Type: {column.SqlColumnType}", ExplorerItemKind.Property, ExplorerIcon.Blank),
						new ExplorerItem($".NET Type: {column.DotNetColumnType}", ExplorerItemKind.Property, ExplorerIcon.Blank),
						new ExplorerItem($"Remarks: {column.Remarks}", ExplorerItemKind.Property, ExplorerIcon.Blank)
					}
				};
			}
			yield break;
		}

		#endregion
	}
}
