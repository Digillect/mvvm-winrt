using System.Reflection;

namespace Digillect.Mvvm.Services
{
	/// <summary>
	///     Service that supports navigation between views
	/// </summary>
	public interface INavigationServiceContext
	{
		/// <summary>
		///     Gets the main assembly containing views.
		/// </summary>
		/// <returns></returns>
		Assembly GetMainAssemblyContainingViews();

		/// <summary>
		///     Gets the root namespace of assembly.
		/// </summary>
		/// <returns></returns>
		string GetRootNamespace();
	}
}