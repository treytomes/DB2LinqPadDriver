using System.Collections.Generic;
using System.Text;

namespace DB2DataContextDriver.CodeGen
{
	public class NamespaceDefinition
	{
		public string Name { get; set; }

		public List<ClassDefinition> Classes { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("namespace {0} {{\n", Name);

			foreach (var cls in Classes)
			{
				sb.AppendLine(cls.ToString());
			}

			return sb.Append("}").ToString();
		}
	}
}
