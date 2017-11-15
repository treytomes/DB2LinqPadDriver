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
			if (string.Compare(db2Modifier, "INOUT", true) == 0)
			{
				return "ref";
			}
			return string.Empty;
		}

		public static IBM.Data.DB2.DB2Type GetDB2TypeFromString(string db2Type)
		{
			if (db2Type.Contains("CHAR"))
			{
				return IBM.Data.DB2.DB2Type.Char;
			}
			if (db2Type == "XML")
			{
				return IBM.Data.DB2.DB2Type.Xml;
			}
			if (db2Type.Contains("INT"))
			{
				return IBM.Data.DB2.DB2Type.Integer;
			}
			if (db2Type == "ROWID")
			{
				return IBM.Data.DB2.DB2Type.RowId;
			}
			if (db2Type.Contains("DECIMAL"))
			{
				return IBM.Data.DB2.DB2Type.Decimal;
			}
			if (db2Type.Contains("NUMERIC"))
			{
				return IBM.Data.DB2.DB2Type.Numeric;
			}
			if (db2Type.Contains("DECFLOAT"))
			{
				return IBM.Data.DB2.DB2Type.DecimalFloat;
			}
			if (db2Type.Contains("REAL"))
			{
				return IBM.Data.DB2.DB2Type.Real;
			}
			if (db2Type.Contains("DOUBLE"))
			{
				return IBM.Data.DB2.DB2Type.Double;
			}
			if (db2Type.Contains("DATE"))
			{
				return IBM.Data.DB2.DB2Type.Date;
			}

			throw new Exception($"I don't understand this type: {db2Type}");
		}
	}
}
