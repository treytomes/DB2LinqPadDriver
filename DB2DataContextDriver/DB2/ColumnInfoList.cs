using IBM.Data.DB2;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace DB2DataContextDriver.DB2
{
	public class ColumnInfoList : IEnumerable<ColumnInfo>
	{
		#region Constants

		private const string SQL = "SELECT * FROM SYSIBM.SYSCOLUMNS WHERE TBNAME='{0}' ORDER BY COLNO";

		#endregion

		#region Fields

		/// <summary>
		/// To detect redundant calls.
		/// </summary>
		private bool _disposedValue = false;

		/// <summary>
		/// This connection is owned by TableInfoList.
		/// </summary>
		private DB2Connection _connection;

		private DB2Command _command;

		#endregion

		#region Constructors

		internal ColumnInfoList(DB2Connection connection, string tableName)
		{
			_connection = connection;

			_command = _connection.CreateCommand();
			_command.CommandType = CommandType.Text;
			_command.CommandText = string.Format(SQL, tableName);
		}

		#endregion

		#region Methods

		public void Dispose()
		{
			if (!_disposedValue)
			{
				_command.Dispose();
				_command = null;

				_disposedValue = true;
			}
		}

		public IEnumerator<ColumnInfo> GetEnumerator()
		{
			using (var reader = _command.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return new ColumnInfo(reader);
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
