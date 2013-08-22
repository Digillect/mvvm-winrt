using System;
using System.Diagnostics.Contracts;

namespace Digillect.Mvvm.UI
{
	internal class Breadcrumb
	{
		private readonly XParameters _parameters;
		private readonly Type _type;

		#region Constructors/Disposer
		/// <summary>
		///     Initializes a new instance of the Breadcrumb class.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parameters"></param>
		public Breadcrumb( Type type, XParameters parameters )
		{
			Contract.Requires<ArgumentNullException>( type != null, "type" );

			_type = type;
			_parameters = parameters;
		}
		#endregion

		#region Public Properties
		public Type Type
		{
			get { return _type; }
		}

		public XParameters Parameters
		{
			get { return _parameters; }
		}
		#endregion

		public override bool Equals( object obj )
		{
			if( obj == null || !(obj is Breadcrumb) )
			{
				return false;
			}

			var other = (Breadcrumb) obj;

			return _type == other._type && Equals( _parameters, other._parameters );
		}

		public override int GetHashCode()
		{
			int hashCode = 17 + _type.GetHashCode();

			if( _parameters != null )
			{
				foreach( var p in _parameters )
				{
					hashCode = hashCode*37 + p.Key.GetHashCode();
					hashCode = hashCode*37 + p.Value.GetHashCode();
				}
			}

			return hashCode;
		}
	}
}