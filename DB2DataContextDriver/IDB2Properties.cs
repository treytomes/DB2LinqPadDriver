namespace DB2DataContextDriver
{
	/// <summary>
	/// This makes the DB2Properties class testable.
	/// </summary>
	public interface IDB2Properties
	{
		//bool Persist { get; set; }
		//bool IsProduction { get; set; }
		//string ServerName { get; set; }
		//string Database { get; set; }
		//string UserId { get; set; }
		string Schema { get; set; }
		//string Password { get; set; }

		string GetConnectionString();
	}
}