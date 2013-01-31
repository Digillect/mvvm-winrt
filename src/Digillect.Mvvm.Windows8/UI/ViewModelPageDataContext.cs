using System;

namespace Digillect.Mvvm.UI
{
	/// <summary>
	/// Instances of this class are used by <see cref="Digillect.Mvvm.UI.ViewModelPage{TViewModel}"/> and descendants to provide data binding support.
	/// </summary>
	public class ViewModelPageDataContext : PageDataContext
	{
		/// <summary>
		/// Factory delegate to create instances of this class through Autofac.
		/// </summary>
		/// <param name="page">The page.</param>
		/// <param name="viewModel">The view model.</param>
		/// <returns>Instance of context.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible" )]
		public new delegate ViewModelPageDataContext Factory( Page page, ViewModel viewModel );
		private readonly ViewModel _viewModel;

		#region Constructors/Disposer
		/// <summary>
		/// Initializes a new instance of the <see cref="ViewModelPageDataContext"/> class.
		/// </summary>
		/// <param name="page">The page used in this context.</param>
		/// <param name="viewModel">The view model used in this context.</param>
		public ViewModelPageDataContext( Page page, ViewModel viewModel )
			: base( page )
		{
			_viewModel = viewModel;
		}

		#endregion

		#region Public Properties
		/// <summary>
		/// Gets the view model.
		/// </summary>
		public ViewModel ViewModel
		{
			get { return _viewModel; }
		}
		#endregion
	}
}
