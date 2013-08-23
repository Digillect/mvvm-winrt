using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Autofac;

using Digillect.Mvvm.Services;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Digillect.Mvvm.UI
{
	/// <summary>
	///     Base class for Windows Metro applications.
	/// </summary>
	public abstract class WindowsRTApplication : Application
	{
		private readonly Stack<Breadcrumb> _breadcrumbs = new Stack<Breadcrumb>();

		#region Constructors/Disposer
		/// <summary>
		///     Initializes a new instance of the <see cref="WindowsRTApplication" /> class.
		/// </summary>
		[SuppressMessage( "Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors" )]
		protected WindowsRTApplication()
		{
			//InitializeIoC();

			Suspending += ( s, e ) => HandleSuspension( e );
		}
		#endregion

		#region Public properties
		/// <summary>
		///     Gets the application root frame.
		/// </summary>
		public Frame RootFrame { get; private set; }

		/// <summary>
		///     Gets the lifetime scope used to create components.
		/// </summary>
		/// <value>
		///     The scope.
		/// </value>
		public ILifetimeScope Scope { get; private set; }
		#endregion

		#region Events and event raisers
		/// <summary>
		///     Invoked when the application is launched. Override this method to perform application initialization and to display initial content in the associated Window.
		/// </summary>
		/// <param name="args">Event data for the event.</param>
		protected override void OnLaunched( LaunchActivatedEventArgs args )
		{
			RootFrame = CreateRootFrame();

			InitializeIoC();
			HandleLaunch( args );

			RootFrame.NavigationFailed += RootFrame_NavigationFailed;

			Window.Current.Content = RootFrame;
			Window.Current.Activate();
		}
		#endregion

		#region Event handlers
		private void RootFrame_NavigationFailed( object sender, NavigationFailedEventArgs e )
		{
			HandleNavigationFailed( e );
		}
		#endregion

		#region Protected methods
		/// <summary>
		///     Creates application root frame. By default creates instance of <see cref="Windows.UI.Xaml.Controls.Frame" />, override
		///     to create instance of other type.
		/// </summary>
		/// <returns>application frame.</returns>
		protected virtual Frame CreateRootFrame()
		{
			return new Frame();
		}

		/// <summary>
		///     Handles the launch.
		/// </summary>
		/// <param name="eventArgs">
		///     The <see cref="LaunchActivatedEventArgs" /> instance containing the event data.
		/// </param>
		protected virtual void HandleLaunch( LaunchActivatedEventArgs eventArgs )
		{
		}

		/// <summary>
		///     Executes when navigation has been failed. Override to provide your own handling.
		/// </summary>
		/// <param name="eventArgs">
		///     The <see cref="Windows.UI.Xaml.Navigation.NavigationFailedEventArgs" /> instance containing the event data.
		/// </param>
		protected virtual void HandleNavigationFailed( NavigationFailedEventArgs eventArgs )
		{
		}

		/// <summary>
		///     Handles the suspension.
		/// </summary>
		/// <param name="eventArgs">
		///     The <see cref="SuspendingEventArgs" /> instance containing the event data.
		/// </param>
		protected virtual void HandleSuspension( SuspendingEventArgs eventArgs )
		{
		}

		/// <summary>
		///     Called to registers services in container.
		/// </summary>
		/// <param name="builder">The builder.</param>
		protected virtual void RegisterServices( ContainerBuilder builder )
		{
			builder.RegisterModule<WindowsRTModule>();
		}
		#endregion

		#region Miscellaneous
		private void InitializeIoC()
		{
			var builder = new ContainerBuilder();

			RegisterServices( builder );

			Scope = builder.Build();
		}
		#endregion
	}
}