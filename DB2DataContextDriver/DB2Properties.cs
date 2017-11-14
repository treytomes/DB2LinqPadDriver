using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DB2DataContextDriver
{
	/// <summary>
	/// Wrapper to expose typed properties over ConnectionInfo.DriverData.
	/// </summary>
	public class DB2Properties : IDB2Properties, IEquatable<DB2Properties>, INotifyPropertyChanged, IDataErrorInfo
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Fields

		private readonly IConnectionInfo _cxInfo;
		private readonly XElement _driverData;

		#endregion

		#region Constructors

		public DB2Properties(IConnectionInfo cxInfo)
		{
			_cxInfo = cxInfo;
			_driverData = cxInfo.DriverData;
		}

		#endregion

		#region Properties

		public bool Persist
		{
			get
			{
				return _cxInfo.Persist;
			}
			set
			{
				if (_cxInfo.Persist != value)
				{
					_cxInfo.Persist = value;
					RaisePropertyChanged();
				}
			}
		}

		public bool IsProduction
		{
			get
			{
				return _cxInfo.IsProduction;
			}
			set
			{
				if (IsProduction != value)
				{
					_cxInfo.IsProduction = value;
					RaisePropertyChanged();
				}
			}
		}

		public string ServerName
		{
			get
			{
				return (string)_driverData.Element(nameof(ServerName)) ?? string.Empty;
			}
			set
			{
				if (ServerName != value)
				{
					_driverData.SetElementValue(nameof(ServerName), value.ToUpper());
					_cxInfo.DatabaseInfo.CustomCxString = GetConnectionString();
					RaisePropertyChanged();
				}
			}
		}

		public string Database
		{
			get
			{
				return (string)_driverData.Element(nameof(Database)) ?? string.Empty;
			}
			set
			{
				if (Database != value)
				{
					_driverData.SetElementValue(nameof(Database), value.ToUpper());
					_cxInfo.DatabaseInfo.CustomCxString = GetConnectionString();
					RaisePropertyChanged();
				}
			}
		}

		public string UserId
		{
			get
			{
				return (string)_driverData.Element(nameof(UserId)) ?? string.Empty;
			}
			set
			{
				if (UserId != value)
				{
					value = value.ToUpper();
					var oldUserId = UserId;

					_driverData.SetElementValue(nameof(UserId), value);
					_cxInfo.DatabaseInfo.CustomCxString = GetConnectionString();

					// By default, set the Schema/CREATOR field to the UserId.  This will usually be correct.
					if (string.IsNullOrWhiteSpace(Schema) || (Schema == oldUserId))
					{
						Schema = value;
					}

					RaisePropertyChanged();
				}
			}
		}

		public string Schema
		{
			get
			{
				return (string)_driverData.Element(nameof(Schema)) ?? string.Empty;
			}
			set
			{
				if (Schema != value)
				{
					_driverData.SetElementValue(nameof(Schema), value.ToUpper());
					RaisePropertyChanged();
				}
			}
		}

		public string Password
		{
			get
			{
				return _cxInfo.Decrypt((string)_driverData.Element(nameof(Password)) ?? string.Empty);
			}
			set
			{
				if (Password != value)
				{
					_driverData.SetElementValue(nameof(Password), _cxInfo.Encrypt(value));
					_cxInfo.DatabaseInfo.CustomCxString = GetConnectionString();
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets an error message indicating what is wrong with this object.
		/// </summary>
		/// <remarks>
		/// An error message indicating what is wrong with this object.  The default is an empty string ("").
		/// </remarks>
		public string Error
		{
			get
			{
				// TODO: This should be a concatenation of the errors on all of the properties.
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets the error message for the property with the given name.
		/// </summary>
		/// <param name="columnName">The name of the property whose error message to get.</param>
		/// <returns>The error message for the property. The default is an empty string ("").</returns>
		public string this[string columnName]
		{
			get
			{
				if (columnName == nameof(Persist))
				{
					return string.Empty;
				}
				else if (columnName == nameof(Password))
				{
					if (string.IsNullOrWhiteSpace(Password))
					{
						return "Password must have a value.";
					}
				}
				else
				{
					var columnElement = _driverData.Element(columnName);
					if ((columnElement == null) || string.IsNullOrWhiteSpace(columnElement.Value))
					{
						return $"{columnName} must have a value.";
					}
				}
				return string.Empty;
			}
		}

		#endregion

		#region Methods

		public string GetConnectionString()
		{
			return string.Format("Server={0};DATABASE={1};PWD={2};UID={3};Persist Security Info=True;Connection Lifetime=60;Connection Reset=false;Min Pool Size=0;Max Pool Size=100;Pooling=true;",
				ServerName,
				Database,
				Password,
				UserId);
		}

		public bool Equals(DB2Properties other)
		{
			return (other != null) && (other.GetConnectionString().ToUpper() == GetConnectionString().ToUpper());
		}

		public override string ToString()
		{
			return GetConnectionString();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as DB2Properties);
		}

		public override int GetHashCode()
		{
			return GetConnectionString().GetHashCode();
		}

		private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
