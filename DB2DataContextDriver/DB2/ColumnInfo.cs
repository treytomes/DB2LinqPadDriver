using IBM.Data.DB2;
using System;
using System.Data;
using System.Linq;

namespace DB2DataContextDriver.DB2
{
	public class ColumnInfo
	{
		#region Constructors

		//internal ColumnInfo(DB2DataReader reader)
		//{
		//	ColumnNumber = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("COLNO")));
		//	SqlColumnType = Convert.ToString(reader.GetValue(reader.GetOrdinal("COLTYPE")));
		//	DotNetColumnType = GetTypeFromDB2(SqlColumnType);
		//	IsPrimary = !string.IsNullOrWhiteSpace(Convert.ToString(reader.GetValue(reader.GetOrdinal("KEYSEQ"))));
		//	Name = Convert.ToString(reader.GetValue(reader.GetOrdinal("NAME")));
		//	Remarks = Convert.ToString(reader.GetValue(reader.GetOrdinal("REMARKS")));
		//}

		internal ColumnInfo(DataRow data)
		{
			var baseColumnType = Convert.ToString(data.Field<object>("COLTYPE")).Trim();

			ColumnNumber = Convert.ToInt32(data.Field<object>("COLNO"));
			SqlColumnType = $"{baseColumnType} [{data.Field<object>("LENGTH")}]";
            DotNetColumnType = DB2TypeFactory.GetTypeFromDB2(baseColumnType);
			IsPrimary = !string.IsNullOrWhiteSpace(Convert.ToString(data.Field<object>("KEYSEQ")));
			Name = Convert.ToString(data.Field<object>("COLNAME"));
			Remarks = Convert.ToString(data.Field<object>("COLREMARKS"));
		}

		#endregion

		#region Properties

		public int ColumnNumber { get; private set; }

		public string SqlColumnType { get; private set; }

		public string DotNetColumnType { get; private set; }

		public bool IsPrimary { get; private set; }

		public string Name { get; private set; }

		public string Remarks { get; private set; }

		#endregion
	}
}
