using System.Collections.Generic;
using System.Text;

namespace DB2DataContextDriver.CodeGen
{
	public class ClassDefinition
	{
		public string Name { get; set; }

		public string Inherits { get; set; }

		public List<string> Methods { get; set; }

		public List<PropertyDefinition> Properties { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			if (string.IsNullOrWhiteSpace(Inherits))
			{
				sb.AppendFormat("public class {0} {{\n", Name);
			}
			else
			{
				sb.AppendFormat("public class {0} : {1} {{\n", Name, Inherits);
			}

			if (Methods != null)
			{
				foreach (var method in Methods)
				{
					sb.AppendLine(method.ToString());
				}
			}

			if (Properties != null)
			{
				foreach (var property in Properties)
				{
					sb.AppendLine(property.ToString());
				}
			}

			return sb.Append("}\n").ToString();
		}
	}
}
