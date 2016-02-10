using IBM.Data.DB2;
using System;
using System.Collections.Generic;

namespace DB2DataContextDriver.DB2
{
	public class TableInfo
	{
		private DB2Connection _connection;

		internal TableInfo(DB2Connection connection, DB2DataReader reader)
		{
			_connection = connection;

			Name = Convert.ToString(reader.GetValue(reader.GetOrdinal("NAME")));
			Remarks = Convert.ToString(reader.GetValue(reader.GetOrdinal("REMARKS")));
		}

		public string Name { get; private set; }

		public string Remarks { get; private set; }

		public IEnumerable<ColumnInfo> Columns
		{
			get
			{
				return new ColumnInfoList(_connection, Name);
			}
		}
	}
}
