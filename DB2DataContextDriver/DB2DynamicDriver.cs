using System;
using System.Collections.Generic;
using System.Reflection;
using LINQPad.Extensibility.DataContext;
using IBM.Data.DB2;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using LINQPad;

namespace DB2DataContextDriver
{
	/// <summary>
	/// Sample dynamic driver. This lets users connect to an ADO.NET Data Services URI, builds the
	/// type data context dynamically, and returns objects for the Schema Explorer.
	/// </summary>
	public class DB2DynamicDriver : DynamicDataContextDriver
	{
		#region Properties

		/// <summary>User-friendly name for your driver.</summary>
		public override string Name
		{
			get
			{
				return "DB2 Data Context";
			}
		}

		/// <summary>Your name.</summary>
		public override string Author
		{
			get
			{
				return "Trey Tomes";
			}
		}

		#endregion

		#region Methods

		/// <summary>Returns the text to display in the root Schema Explorer node for a given connection info.</summary>
		public override string GetConnectionDescription(IConnectionInfo cxInfo)
		{
			var props = new DB2Properties(cxInfo);
			return $"{props.Database}.{props.Schema}";
		}

		/// <summary>
		/// Displays a dialog prompting the user for connection details. The isNewConnection
		/// parameter will be true if the user is creating a new connection rather than editing an
		/// existing connection. This should return true if the user clicked OK. If it returns false,
		/// any changes to the IConnectionInfo object will be rolled back.
		/// </summary>
		public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
		{
			return new ConnectionDialog(cxInfo).ShowDialog().GetValueOrDefault(false);
		}

		/// <summary>
		/// Builds an assembly containing a typed data context, and returns data for the Schema Explorer.
		/// </summary>
		/// <param name="cxInfo">Connection information, as entered by the user.</param>
		/// <param name="assemblyToBuild">Name and location of the target assembly to build.</param>
		/// <param name="nameSpace">The suggested namespace of the typed data context.  You must update this parameter if you don't use the suggested namespace.</param>
		/// <param name="typeName">The suggested type name of the typed data context.  You must update this parameter if you don't use the suggested type name.</param>
		/// <returns>Schema which will be subsequently loaded into the Schema Explorer.</returns>
		public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
		{
			return SchemaBuilder.GetSchemaAndBuildAssembly(new DB2Properties(cxInfo), assemblyToBuild, ref nameSpace, ref typeName, GetDriverFolder());
		}

		/// <summary>
		/// Returns the names & types of the parameter(s) that should be passed into your data
		/// context's constructor.  Typically this is a connection string or a DbConnection. The number
		/// of parameters and their types need not be fixed - they may depend on custom flags in the
		/// connection's DriverData. The default is no parameters.
		/// </summary>
		public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
		{
			// Pass the chosen database's connection string into the DB2Connection's constructor.
			return new object[] { new DB2Properties(cxInfo).GetConnectionString() };
		}

		/// <summary>
		/// Returns the argument values to pass into your data context's constructor, based on a given IConnectionInfo.
		/// This must be consistent with GetContextConstructorParameters.
		/// </summary>
		public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
		{
			// We are using the DB2Connection constructor with a single string parameter called "connectionString".
			return new[] { new ParameterDescriptor("connectionString", "System.String") };
		}

		/// <summary>
		/// This virtual method is called after a data context object has been instantiated, in
		/// preparation for a query. You can use this hook to perform additional initialization work.
		/// </summary>
		public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
		{
			// At this point, context is set to a ready-to-go DB2Connection.

			// NOTE: A useful application of overriding InitializeContext is to set up population of the SQL translation tab.
			executionManager.SqlTranslationWriter.WriteLine($"Executing object of type {context.GetType()}: {context.ToString()}");
			executionManager.SqlTranslationWriter.WriteLine($"Properties: {string.Join(", ", context.GetType().GetProperties().Select(x => x.Name))}");
			executionManager.SqlTranslationWriter.WriteLine($"Methods: {string.Join(", ", context.GetType().GetMethods().Select(x => x.Name))}");

			// TODO: How can I grab a copy of the SQL I'm executing?

			base.InitializeContext(cxInfo, context, executionManager);
		}

		/// <summary>
		/// This virtual method is called after a query has completed. You can use this hook to
		/// perform cleanup activities such as disposing of the context or other objects.
		/// </summary>
		public override void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
		{
			base.TearDownContext(cxInfo, context, executionManager, constructorArguments);
		}

		/// <summary>
		/// This method is called after the query's main thread has finished running the user's code,
		/// but before the query has stopped. If you've spun up threads that are still writing results, you can 
		/// use this method to wait out those threads.
		/// </summary>
		public override void OnQueryFinishing(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
		{
			base.OnQueryFinishing(cxInfo, context, executionManager);
		}

		/// <summary>
		/// Returns a list of additional assemblies to reference when building queries. To refer to
		/// an assembly in the GAC, specify its fully qualified name, otherwise specified the assembly's full
		/// location on the hard drive. Assemblies in the same folder as the driver, however, don't require a
		/// folder name. If you're unable to find the necessary assemblies, throw an exception, with a message
		/// indicating the problem assembly.
		/// </summary>
		public override IEnumerable<string> GetAssembliesToAdd() // TODO: What replaces this?
		{
			// We need the following assembly for compilation and autocompletion:
			return new[] { "IBM.Data.DB2.dll" };
		}

		/// <summary>
		/// Returns a list of additional namespaces that should be imported automatically into all 
		/// queries that use this driver. This should include the commonly used namespaces of your ORM or
		/// querying technology.
		/// </summary>
		public override IEnumerable<string> GetNamespacesToAdd() // TODO: What replaces this?
		{
			// Import the commonly used namespaces as a courtesy to the user:
			return new[] { "IBM.Data.DB2" };
		}

		/// <summary>
		/// Returns a list of namespace imports that should be removed to improve the autocompletion
		/// experience. This might include System.Data.Linq if you're not using LINQ to SQL.
		/// </summary>
		public override IEnumerable<string> GetNamespacesToRemove() // TODO: What replaces this?
		{
			return base.GetNamespacesToRemove();
		}

		/// <summary>Returns true if two <see cref="IConnectionInfo"/> objects are semantically equal.</summary>
		public override bool AreRepositoriesEquivalent(IConnectionInfo r1, IConnectionInfo r2)
		{
			return new DB2Properties(r1).Equals(new DB2Properties(r2));
		}

		/// <summary>Returns the time that the schema was last modified. If unknown, return null.</summary>
		public virtual DateTime? GetLastSchemaUpdate(IConnectionInfo cxInfo)
		{
			// Return the last date/time that the database was updated if known, otherwise null.
			return null;
		}

		/// <summary>
		/// Instantiates a database connection for queries whose languages is set to 'SQL'.
		/// By default, this calls cxInfo.DatabaseInfo.GetCxString to obtain a connection string, 
		/// then GetProviderFactory to obtain a connection object. You can override this if you want 
		/// more control over creating the connection or connection string.
		/// </summary>
		/// <remarks>
		/// I want to make sure we create a DB2Connection here; not just any SQL connection.
		/// </remarks>
		public override IDbConnection GetIDbConnection(IConnectionInfo cxInfo)
		{
			return new DB2Connection(new DB2Properties(cxInfo).GetConnectionString());
		}

		public override void ExecuteESqlQuery(IConnectionInfo cxInfo, string query)
		{
			throw new Exception("ESQL queries are not supported for this type of connection");
		}

		#endregion
	}
}
