namespace DB2DataContextDriver.CodeGen
{
	public class PropertyDefinition
	{
		public string Type { get; set; }

		public string Name { get; set; }

		public override string ToString()
		{
			return string.Format("public {0} {1} {{ get; set; }}", Type, Name);
		}
	}
}
