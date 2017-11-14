using IBM.Data.DB2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DB2DataContextDriver.DB2
{
	public class StoredProcedureInfoList : IDisposable, IEnumerable<StoredProcedureInfo>
	{
		#region Constants

		private const string SQL_WO_SCHEMA = "SELECT ROUTINE_NAME, ROUTINE_DEFINITION FROM SYSIBM.ROUTINES WHERE ROUTINE_BODY='SQL' ORDER BY ROUTINE_NAME";
		private const string SQL_W_SCHEMA = "SELECT ROUTINE_NAME, ROUTINE_DEFINITION FROM SYSIBM.ROUTINES WHERE ROUTINE_BODY='SQL' AND SPECIFIC_SCHEMA='{0}' ORDER BY ROUTINE_NAME";

		#endregion

		#region Fields

		/// <summary>
		/// To detect redundant calls.
		/// </summary>
		private bool _disposedValue = false;

		private DataTable _data;

		#endregion

		#region Constructors

		public StoredProcedureInfoList(string connectionString, string schema = null)
		{
			using (var cn = new DB2Connection(connectionString))
			{
				cn.Open();

				using (var cm = cn.CreateCommand())
				{
					cm.CommandType = CommandType.Text;

					if (string.IsNullOrWhiteSpace(schema))
					{
						cm.CommandText = SQL_WO_SCHEMA;
					}
					else
					{
						cm.CommandText = string.Format(SQL_W_SCHEMA, schema);
					}

					using (var a = new DB2DataAdapter(cm))
					{
						_data = new DataTable();
						a.Fill(_data);
					}
				}
			}
		}

		public StoredProcedureInfoList(IDB2Properties properties)
			: this(properties.GetConnectionString(), properties.Schema)
		{
		}

		#endregion

		#region Methods

		public void Dispose()
		{
			if (!_disposedValue)
			{
				//_command.Dispose();
				//_command = null;

				//_connection.Close();
				//_connection.Dispose();
				//_connection = null;

				_disposedValue = true;
			}
		}

		public IEnumerator<StoredProcedureInfo> GetEnumerator()
		{
			foreach (var routine in _data.AsEnumerable())
			{
				yield return new StoredProcedureInfo(routine.Field<string>("ROUTINE_NAME"), routine.Field<string>("ROUTINE_DEFINITION"));
			}

			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
