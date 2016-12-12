using IBM.Data.DB2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using System.Linq;

namespace DB2DataContextDriver.DB2
{
	public class TableInfoList : IDisposable, IEnumerable<TableInfo>
	{
		#region Constants

		//private const string SQL_W_SCHEMA = "SELECT * FROM SYSIBM.SYSTABLES WHERE TYPE='T' AND CREATOR='{0}' ORDER BY NAME";
		//private const string SQL_WO_SCHEMA = "SELECT * FROM SYSIBM.SYSTABLES WHERE TYPE='T' ORDER BY NAME";

		private const string SQL_W_SCHEMA = "SELECT t.NAME as TBNAME, t.REMARKS AS TBREMARKS, COLNO, COLTYPE, KEYSEQ, c.NAME AS COLNAME, c.REMARKS AS COLREMARKS, c.LENGTH FROM SYSIBM.SYSCOLUMNS c INNER JOIN SYSIBM.SYSTABLES t ON t.NAME=c.TBNAME AND t.TYPE='T' AND CREATOR='{0}' ORDER BY t.NAME, COLNO";
		private const string SQL_WO_SCHEMA = "SELECT t.NAME as TBNAME, t.REMARKS AS TBREMARKS, COLNO, COLTYPE, KEYSEQ, c.NAME AS COLNAME, c.REMARKS AS COLREMARKS, c.LENGTH FROM SYSIBM.SYSCOLUMNS c INNER JOIN SYSIBM.SYSTABLES t ON t.NAME=c.TBNAME AND t.TYPE='T' ORDER BY t.NAME, COLNO";

		#endregion

		#region Fields

		/// <summary>
		/// To detect redundant calls.
		/// </summary>
		private bool _disposedValue = false;

		private DataTable _data;

		#endregion

		#region Constructors

		public TableInfoList(string connectionString, string schema = null)
		{
			//_connectionString = connectionString;

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

					//using (var reader = cm.ExecuteReader())
					//{
					//	_data.Load(reader);
					//}
				}
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
				//_command.Dispose();
				//_command = null;

				//_connection.Close();
				//_connection.Dispose();
				//_connection = null;

				_disposedValue = true;
			}
		}

		public IEnumerator<TableInfo> GetEnumerator()
		{
			//using (var reader = _command.ExecuteReader())
			//{
			//	while (reader.Read())
			//	{
			//		yield return new TableInfo(_connection, reader);
			//	}
			//}

			foreach (var tableGroup in _data.AsEnumerable().GroupBy(x => x.Field<string>("TBNAME")))
			{
				yield return new TableInfo(tableGroup);
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
