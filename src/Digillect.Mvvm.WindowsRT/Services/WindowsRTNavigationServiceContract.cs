﻿#region Copyright (c) 2011-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2011-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics.Contracts;

namespace Digillect.Mvvm.Services
{
	[ContractClassFor( typeof( IWindowsRTNavigationService ) )]
	internal abstract class WindowsRTNavigationServiceContract : IWindowsRTNavigationService
	{
		#region Implementation of IWindowsRTNavigationService
		public object CreateSnapshot()
		{
			Contract.Ensures( Contract.Result<object>() != null );

			return null;
		}

		public object CreateSnapshot( Action<object> guard )
		{
			Contract.Ensures( Contract.Result<object>() != null );

			return null;
		}

		public object CreateSnapshot( Action<object> guard, object tag )
		{
			Contract.Ensures( Contract.Result<object>() != null );

			return null;
		}

		public bool RollbackSnapshot( object snapshotId )
		{
			Contract.Requires<ArgumentNullException>( snapshotId != null );

			return false;
		}

		public bool RollbackSnapshot( object snapshotId, string viewName, XParameters parameters )
		{
			Contract.Requires<ArgumentNullException>( snapshotId != null );

			return false;
		}

		public void RestoreStateOrGoToLandingView( string viewName )
		{
			Contract.Requires( viewName != null, "viewName" );
		}

		public void RestoreStateOrGoToLandingView( string viewName, XParameters parameters )
		{
			Contract.Requires( viewName != null, "viewName" );
		}

		public void SaveState()
		{
		}
		#endregion

		#region Implementation of INavigationService
		public abstract void Navigate( string viewName );
		public abstract void Navigate( string viewName, XParameters parameters );
		public abstract void GoBack();
		public abstract void GoBack( string viewName );
		#endregion
	}
}