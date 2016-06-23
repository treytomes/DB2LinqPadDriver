using IBM.Data.DB2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DB2DataContextDriver.DB2
{
	public class TableInfo
	{
		//private DB2Connection _connection;
		private IGrouping<string, DataRow> _data;

		//internal TableInfo(DB2Connection connection, DB2DataReader reader)
		//{
		//	_connection = connection;

		//	Name = Convert.ToString(reader.GetValue(reader.GetOrdinal("NAME")));
		//	Remarks = Convert.ToString(reader.GetValue(reader.GetOrdinal("REMARKS")));
		//}

		internal TableInfo(IGrouping<string, DataRow> data)
		{
			_data = data;

			Name = data.First().Field<string>("TBNAME");
			Remarks = data.First().Field<string>("TBREMARKS");
		}

		public string Name { get; private set; }

		public string Remarks { get; private set; }

		public IEnumerable<ColumnInfo> Columns
		{
			get
			{
				//return new ColumnInfoList(_connection, Name);
				return new ColumnInfoList(_data);
			}
		}
	}
}
