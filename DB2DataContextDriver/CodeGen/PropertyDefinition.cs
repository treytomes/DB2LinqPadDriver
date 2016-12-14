using System.Collections.Generic;

namespace DB2DataContextDriver.CodeGen
{
	public class PropertyDefinition
	{
		public PropertyDefinition()
		{
			Attributes = new List<string>();
		}

		public List<string> Attributes { get; }

		public string Type { get; set; }

		public string Name { get; set; }

		public override string ToString()
		{
			if (Attributes.Count == 0)
			{
				return string.Format("public {0} {1} {{ get; set; }}", Type, Name);
			}
			else
			{
				return string.Format("{0} public {1} {2} {{ get; set; }}", string.Join(" ", Attributes), Type, Name);
			}
		}
	}
}
