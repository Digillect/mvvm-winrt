using System;

namespace Digillect.Mvvm.UI
{
	/// <summary>
	/// Provides infrastructure for page backed up with <see cref="Digillect.Mvvm.EntityViewModel{TModel}"/>.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <typeparam name="TViewModel">The type of the view model.</typeparam>
	/// <remarks>Instance of this class performs lookup of the query string upon navigation to find and extract parameter with
	/// name <code>Id</code> that is used as entity id for view model. If that parameter is not found then <see cref="System.ArgumentException"/> will be thrown.</remarks>
	public class EntityPage<TEntity, TViewModel> : ViewModelPage<TViewModel>
		where TEntity: XObject
		where TViewModel: EntityViewModel<TEntity>
	{
		protected override void ProcessParameters( object parameters )
		{
			base.ProcessParameters( parameters );

			var key = ViewParameters.GetValue<XKey>( "Key" );

			if( key == null )
			{
				throw new ViewParameterException( "Entity key is not passed to the view." );
			}
		}
	}
}
