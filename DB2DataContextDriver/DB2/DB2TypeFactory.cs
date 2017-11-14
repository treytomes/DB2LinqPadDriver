using System;
using System.Linq;

namespace DB2DataContextDriver.DB2
{
	public static class DB2TypeFactory
	{
		public static string GetTypeFromDB2(string db2Type)
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

		public static string GetModifierFromDB2(string db2Modifier)
		{
			if (string.Compare(db2Modifier, "OUT", true) == 0)
			{
				return "out";
			}
			return string.Empty;
		}
	}
}
