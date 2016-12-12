using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DB2DataContextDriver.Reflection
{
	public static class TypeBuilderExtensions
	{
		public static FieldBuilder DefineField<T>(this TypeBuilder @this, string fieldName, FieldAttributes attributes)
		{
			return @this.DefineField(typeof(T), fieldName, attributes);
		}

		public static FieldBuilder DefineField(this TypeBuilder @this, Type type, string fieldName, FieldAttributes attributes)
		{
			return @this.DefineField(fieldName, type, attributes);
		}

		public static MethodBuilder DefineMethod<TResult>(this TypeBuilder @this, string name, MethodAttributes attributes, CallingConventions callingConvention)
		{
			return @this.DefineMethod(name, attributes, callingConvention, typeof(TResult), new Type[0]);
		}

		public static MethodBuilder DefineMethod<TArg, TResult>(this TypeBuilder @this, string name, MethodAttributes attributes, CallingConventions callingConvention)
		{
			return @this.DefineMethod(name, attributes, callingConvention, typeof(TResult), new Type[] { typeof(TArg) });
		}
	}
}