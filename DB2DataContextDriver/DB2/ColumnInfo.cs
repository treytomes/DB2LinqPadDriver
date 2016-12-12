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
            DotNetColumnType = GetTypeFromDB2(baseColumnType);
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

		#region Methods

		private static string GetTypeFromDB2(string db2Type)
		{
			db2Type = db2Type.ToUpper().Trim();
			// See also: https://www-01.ibm.com/support/knowledgecenter/SSEPEK_10.0.0/com.ibm.db2z10.doc.intro/src/tpc/db2z_distincttypes.dita
			if (db2Type.Contains("CHAR") || (db2Type == "XML"))
			{
				// Handles all VARCHAR, CHARACTER and XML types.
				return typeof(string).FullName;
			}
			else if (db2Type.Contains("INT") || (db2Type == "ROWID"))
			{
				// Handles SMALLINT, INTEGER, BIGINT, etc.
				return typeof(int).FullName;
			}
			else if (new[] { "DECIMAL", "NUMERIC", "DECFLOAT", "REAL", "DOUBLE" }.Contains(db2Type))
			{
				return typeof(double).FullName;
			}
			else if (db2Type == "DATE")
			{
				return typeof(DateTime).FullName;
			}

			return typeof(object).FullName;
		}

		#endregion
	}
}
