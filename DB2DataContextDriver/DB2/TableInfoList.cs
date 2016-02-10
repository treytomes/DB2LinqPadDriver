using IBM.Data.DB2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;

namespace DB2DataContextDriver.DB2
{
	public class TableInfoList : IDisposable, IEnumerable<TableInfo>
	{
		#region Constants

		private const string SQL_W_SCHEMA = "SELECT * FROM SYSIBM.SYSTABLES WHERE TYPE='T' AND CREATOR='{0}' ORDER BY NAME";
		private const string SQL_WO_SCHEMA = "SELECT * FROM SYSIBM.SYSTABLES WHERE TYPE='T' ORDER BY NAME";

		#endregion

		#region Fields

		/// <summary>
		/// To detect redundant calls.
		/// </summary>
		private bool _disposedValue = false;

		private string _connectionString;
		private DB2Connection _connection;
		private DB2Command _command;

		#endregion

		#region Constructors

		public TableInfoList(string connectionString, string schema = null)
		{
			_connectionString = connectionString;

			_connection = new DB2Connection(_connectionString);
			_connection.Open();

			_command = _connection.CreateCommand();
			_command.CommandType = CommandType.Text;

			if (string.IsNullOrWhiteSpace(schema))
			{
				_command.CommandText = SQL_WO_SCHEMA;
			}
			else
			{
				_command.CommandText = string.Format(SQL_W_SCHEMA, schema);
			}
		}

		public TableInfoList(DB2Properties properties)
			: this(properties.GetConnectionString(), properties.Schema)
		{
		}

		#endregion

		#region Methods

		public void Dispose()
		{
			if (!_disposedValue)
			{
				_command.Dispose();
				_command = null;

				_connection.Close();
				_connection.Dispose();
				_connection = null;

				_disposedValue = true;
			}
		}

		public IEnumerator<TableInfo> GetEnumerator()
		{
			using (var reader = _command.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return new TableInfo(_connection, reader);
				}
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
