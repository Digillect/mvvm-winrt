using Autofac;

using Digillect.Mvvm.UI;

namespace Digillect.Mvvm.Services
{
	/// <summary>
	///     Autofac module that registers default and system services.
	/// </summary>
	public class WindowsRTModule : MvvmModule
	{
		#region Protected methods
		/// <summary>
		///     Override to add registrations to the container.
		/// </summary>
		/// <param name="builder">
		///     The builder through which components can be
		///     registered.
		/// </param>
		/// <remarks>
		///     Note that the ContainerBuilder parameter is unique to this module.
		/// </remarks>
		protected override void Load( ContainerBuilder builder )
		{
			base.Load( builder );

			builder.RegisterType<NetworkAvailabilityService>().As<INetworkAvailabilityService, IStartable>().SingleInstance();
			builder.RegisterType<PageDecorationService>().As<IPageDecorationService>().SingleInstance();
			builder.RegisterType<NavigationService>().As<INavigationService, IWindowsRTNavigationService, IBreadcrumbService>().As<IStartable>().SingleInstance();

			builder.RegisterType<AuthenticationService>()
					.As<IAuthenticationService, INavigationHandler, IStartable>()
					.SingleInstance()
					.OnActivated( e => e.Instance.SetNavigationService( e.Context.Resolve<IWindowsRTNavigationService>() ) );
		}
		#endregion
	}
}