using System;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace DB2LinqProvider.Tests
{
	public class TBLORDHDR
	{
		//[LinqToDB.Mapping.PrimaryKey]
		public int ORDER_STORE { get; set; }

		//[LinqToDB.Mapping.PrimaryKey]
		public int ORDER_NUMBER { get; set; }

		public string SHIPTO_NAME { get; set; }

		public string DATE_CREATED { get; set; }
	}

	public class CUSDTA : LinqToDB.Data.DataConnection
	{
		public CUSDTA(string connectionString) : base(new LinqToDB.DataProvider.DB2.DB2DataProvider("DB2", LinqToDB.DataProvider.DB2.DB2Version.zOS), connectionString) { }

		public LinqToDB.ITable<TBLORDHDR> TBLORDHDR { get { return GetTable<TBLORDHDR>(); } }
	}

	public class QueryTests
	{
		[Fact]
		public void Can_execute_query()
		{
			using (var db = new CUSDTA("Server=EESIBM02;DATABASE=CUSDTA;PWD=db2power;UID=DB2INST1;Persist Security Info=True;Connection Lifetime=60;Connection Reset=false;Min Pool Size=0;Max Pool Size=100;Pooling=true;"))
			{

				var query = from o in db.TBLORDHDR
							where o.DATE_CREATED == "20161105"
							select o;
				foreach (var order in query)
				{
					Debug.WriteLine("Order: {0}, {1}, {2}, {3}", order.ORDER_STORE, order.ORDER_NUMBER, order.DATE_CREATED, order.SHIPTO_NAME);
				}
			}
		}
	}
}
