using System;

using Digillect.Mvvm.UI;

namespace Digillect.Mvvm.Services
{
	internal interface IBreadcrumbService
	{
		#region Public properties
		bool IsUnwinding { get; }
		#endregion

		#region Public methods
		Breadcrumb PeekBreadcrumb( Type viewType );
		Breadcrumb PopBreadcrumb( Type viewType );
		void PushBreadcrumb( Type viewType, XParameters parameters );
		#endregion
	}
}