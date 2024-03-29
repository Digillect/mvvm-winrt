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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;

using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Autofac;

using Digillect.Mvvm.Services;

namespace Digillect.Mvvm.UI
{
	/// <summary>
	/// Base for application pages.
	/// </summary>
	public class WindowsRTPage : Windows.UI.Xaml.Controls.Page, INotifyPropertyChanged
	{
		private ILifetimeScope _scope;
		private List<Control> _layoutAwareControls;
		private Breadcrumb _breadcrumb;
		private XParameters _viewParameters;
		private readonly IBreadcrumbService _breadcrumbService;

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsRTPage"/> class.
		/// </summary>
		public WindowsRTPage()
		{
			if( Windows.ApplicationModel.DesignMode.DesignModeEnabled )
			{
				return;
			}
			
			// When this page is part of the visual tree make two changes:
			// 1) Map application view state to visual state for the page
			// 2) Handle keyboard and mouse navigation requests
			Loaded += ( sender, e ) =>
			{
				StartLayoutUpdates( sender );

				// Keyboard and mouse navigation only apply when occupying the entire window
				if( ActualHeight == Window.Current.Bounds.Height && ActualWidth == Window.Current.Bounds.Width )
				{
					// Listen to the window directly so focus isn't required
					Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += CoreDispatcher_AcceleratorKeyActivated;
					Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;
				}

				// If we are restoring state during unwind - kick infrastructure creation
				if( _breadcrumb != null )
				{
					var parameter = _breadcrumb.Parameters;
					_breadcrumb = null;

					HandleNavigationToPage( parameter, true );
				}
			};

			// Undo the same changes when the page is no longer visible
			Unloaded += ( sender, e ) =>
			{
				StopLayoutUpdates( sender );

				Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= CoreDispatcher_AcceleratorKeyActivated;
				Window.Current.CoreWindow.PointerPressed -= CoreWindow_PointerPressed;
			};

			_breadcrumbService = CurrentApplication.Scope.Resolve<IBreadcrumbService>();

			if( _breadcrumbService.IsUnwinding )
			{
				_breadcrumb = _breadcrumbService.PeekBreadcrumb( GetType() );
			}

			GoBack = new RelayCommand( () =>
			{
				try
				{
					if( _scope != null )
					{
						_scope.Resolve<INavigationService>().GoBack();
					}
					else
					{
						if( Frame.CanGoBack )
						{
							Frame.GoBack();
						}
					}
				}
				catch
				{
				}
			}, () => Frame != null && Frame.CanGoBack );
		}
		#endregion

		#region Properties
		/// <summary>
		/// Autofac scope associated with this page.
		/// </summary>
		/// <value>
		/// The scope.
		/// </value>
		public ILifetimeScope Scope
		{
			get { return _scope; }
		}

		/// <summary>
		/// Gets the current application.
		/// </summary>
		/// <value>
		/// The current application.
		/// </value>
		protected static WindowsRTApplication CurrentApplication
		{
			get { return (WindowsRTApplication) Application.Current; }
		}

		public XParameters ViewParameters
		{
			get { return _viewParameters; }
		}
		#endregion

		#region INotifyPropertyChanged support
		/// <summary>
		/// Occurs when value of any of the page properties has been changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Sets the property and raises an event to notify about property value changes.
		/// </summary>
		/// <typeparam name="T">Property value type</typeparam>
		/// <param name="storage">The storage.</param>
		/// <param name="value">The value.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns><c>true</c> if storage content was changed, otherwise <c>false</c>.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed" ),
		System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#" )]
		protected bool SetProperty<T>( ref T storage, T value, [CallerMemberName] string propertyName = null )
		{
			if( Equals( storage, value ) )
			{
				return false;
			}

			storage = value;
			OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );

			return true;
		}

		/// <summary>
		/// Called when value of property <paramref name="propertyName"/> has been changed.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected void OnPropertyChanged( string propertyName )
		{
			OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}

		/// <summary>
		/// Raises the <see cref="E:PropertyChanged" /> event.
		/// </summary>
		/// <param name="eventArgs">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
		protected virtual void OnPropertyChanged( PropertyChangedEventArgs eventArgs )
		{
			var handler = PropertyChanged;

			if( handler != null )
			{
				handler( this, eventArgs );
			}
		}
		#endregion

		#region Navigation handling
		/// <summary>
		/// Called when a page becomes the active page in a frame.
		/// </summary>
		/// <param name="e">An object that contains the event data.</param>
		protected override void OnNavigatedTo( NavigationEventArgs e )
		{
			base.OnNavigatedTo( e );

			if( e == null )
			{
				return;
			}

			object parameter = null;
			var resurrection = false;

			if( e.NavigationMode == NavigationMode.New )
			{
				if( _breadcrumb != null )
				{
					parameter = _breadcrumb.Parameters;
					_breadcrumb = null;
				}
				else
				{
					parameter = e.Parameter;
				}
			}

			if( e.NavigationMode == NavigationMode.Back )
			{
				if( _scope == null )
				{
					// Most probably we're unwinding

					var breadcrumb = _breadcrumbService.PeekBreadcrumb( GetType() );

					parameter = breadcrumb.Parameters;
					resurrection = true;
				}
			}

			HandleNavigationToPage( parameter, resurrection );
		}

		private void HandleNavigationToPage( object parameter, bool resurrection )
		{
			if( _scope == null )
			{
				ProcessParameters( parameter );

				_scope = CurrentApplication.Scope.BeginLifetimeScope();

				DataContext = CreateDataContext();

				if( resurrection )
				{
					OnPageResurrected();
				}
				else
				{
					OnPageCreated();
				}

				IPageDecorationService pageDecorationService;

				if( _scope.TryResolve( out pageDecorationService ) )
				{
					pageDecorationService.AddDecoration( this );
				}
			}
			else
			{
				OnPageAwaken();
			}
		}

		/// <summary>
		/// Called when a page is no longer the active page in a frame.
		/// </summary>
		/// <param name="e">An object that contains the event data.</param>
		protected override void OnNavigatingFrom( NavigatingCancelEventArgs e )
		{
			if( e != null )
			{
				if( e.NavigationMode == NavigationMode.Back )
				{
					OnPageDestroyed();

					if( _scope != null )
					{
						IPageDecorationService pageDecorationService;

						if( _scope.TryResolve( out pageDecorationService ) )
						{
							pageDecorationService.RemoveDecoration( this );
						}

						_scope.Dispose();
						_scope = null;
					}
				}
				else
				{
					OnPageAsleep();
				}
			}

			base.OnNavigatingFrom( e );
		}
		#endregion

		#region Page Lifecycle handlers
		/// <summary>
		/// Creates data context to be set for the page. Override to create your own data context.
		/// </summary>
		/// <returns>Data context that will be set to <see cref="Windows.UI.Xaml.FrameworkElement.DataContext"/> property.</returns>
		protected virtual object CreateDataContext()
		{
			return this;
		}

		/// <summary>
		/// This method is called when page is visited for the very first time. You should perform
		/// initialization and create one-time initialized resources here.
		/// </summary>
		protected virtual void OnPageCreated()
		{
		}

		/// <summary>
		/// This method is called when page is returned from being Dormant. All resources are preserved,
		/// so most of the time you should just ignore this event.
		/// </summary>
		protected virtual void OnPageAwaken()
		{
		}

		/// <summary>
		///     This method is called when page navigated after application has been resurrected from thombstombed state.
		///     Use saved data to restore state.
		/// </summary>
		protected virtual void OnPageResurrected()
		{
		}

		/// <summary>
		/// This method is called when navigation outside of the page occures.
		/// </summary>
		protected virtual void OnPageAsleep()
		{
		}

		/// <summary>
		/// This method is called when page is being destroyed, usually after user presses Back key.
		/// </summary>
		protected virtual void OnPageDestroyed()
		{
		}
		#endregion

		protected virtual void ProcessParameters( object parameters )
		{
			if( parameters != null && !(parameters is XParameters) )
			{
				throw new ArgumentException( "Parameter of incompatible type has been passed to view." );
			}

			_viewParameters = (XParameters) parameters ?? XParameters.Empty;

			var pageType = GetType();

			foreach( var attribute in pageType.GetTypeInfo().GetCustomAttributes( typeof( ViewParameterAttribute ), true ).Cast<ViewParameterAttribute>() )
			{
				var parameterExists = _viewParameters.Contains( attribute.ParameterName );

				if( attribute.Required && !parameterExists )
				{
					throw new ViewParameterException( string.Format( "View {0} requires argument {1} of type {2}.", pageType, attribute.ParameterName, attribute.ParameterType ) );
				}

				if( parameterExists )
				{
					var parameterValue = _viewParameters.GetValue<object>( attribute.ParameterName );

					if( !attribute.ParameterType.GetTypeInfo().IsAssignableFrom( parameterValue.GetType().GetTypeInfo() ) )
					{
						throw new ViewParameterException( string.Format( "View {0} requires argument {1} of type {2}, but it is of type {3}.", pageType, attribute.ParameterName, attribute.ParameterType, parameterValue.GetType() ) );
					}
				}
			}
		}

		public IRelayCommand GoBack { get; private set; }

		#region Keyboard & Mouse
		/// <summary>
		/// Invoked on every keystroke, including system keys such as Alt key combinations, when
		/// this page is active and occupies the entire window.  Used to detect keyboard navigation
		/// between pages even when the page itself doesn't have focus.
		/// </summary>
		/// <param name="sender">Instance that triggered the event.</param>
		/// <param name="args">Event data describing the conditions that led to the event.</param>
		private void CoreDispatcher_AcceleratorKeyActivated( CoreDispatcher sender, AcceleratorKeyEventArgs args )
		{
			var virtualKey = args.VirtualKey;

			// Only investigate further when Left, Right, or the dedicated Previous or Next keys
			// are pressed
			if( (args.EventType == CoreAcceleratorKeyEventType.SystemKeyDown ||
				args.EventType == CoreAcceleratorKeyEventType.KeyDown) &&
				(virtualKey == VirtualKey.Left || virtualKey == VirtualKey.Right ||
				(int) virtualKey == 166 || (int) virtualKey == 167) )
			{
				var coreWindow = Window.Current.CoreWindow;
				const CoreVirtualKeyStates downState = CoreVirtualKeyStates.Down;
				var menuKey = (coreWindow.GetKeyState( VirtualKey.Menu ) & downState) == downState;
				var controlKey = (coreWindow.GetKeyState( VirtualKey.Control ) & downState) == downState;
				var shiftKey = (coreWindow.GetKeyState( VirtualKey.Shift ) & downState) == downState;
				var noModifiers = !menuKey && !controlKey && !shiftKey;
				var onlyAlt = menuKey && !controlKey && !shiftKey;

				if( ((int) virtualKey == 166 && noModifiers) ||
					(virtualKey == VirtualKey.Left && onlyAlt) )
				{
					// When the previous key or Alt+Left are pressed navigate back
					args.Handled = true;

					if( _scope != null )
					{
						Scope.Resolve<INavigationService>().GoBack();
					}
				}
				else
				{
					if( ((int) virtualKey == 167 && noModifiers) ||
					(virtualKey == VirtualKey.Right && onlyAlt) )
					{
						// When the next key or Alt+Right are pressed navigate forward
						args.Handled = true;
						//GoForward( this, new RoutedEventArgs() );
					}
				}
			}
		}

		/// <summary>
		/// Invoked on every mouse click, touch screen tap, or equivalent interaction when this
		/// page is active and occupies the entire window.  Used to detect browser-style next and
		/// previous mouse button clicks to navigate between pages.
		/// </summary>
		/// <param name="sender">Instance that triggered the event.</param>
		/// <param name="args">Event data describing the conditions that led to the event.</param>
		private void CoreWindow_PointerPressed( CoreWindow sender, PointerEventArgs args )
		{
			var properties = args.CurrentPoint.Properties;

			// Ignore button chords with the left, right, and middle buttons
			if( properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
				properties.IsMiddleButtonPressed )
			{
				return;
			}	// If back or foward are pressed (but not both) navigate appropriately
			var backPressed = properties.IsXButton1Pressed;
			var forwardPressed = properties.IsXButton2Pressed;
			if( backPressed ^ forwardPressed )
			{
				args.Handled = true;

				if( _scope != null )
				{
					var navigationService = _scope.Resolve<INavigationService>();

					if( backPressed )
					{
						navigationService.GoBack();
					}
					if( forwardPressed )
					{
						//GoForward( this, new RoutedEventArgs() );
					}
				}
			}
		}

		#endregion

		#region Visual state switching
		/// <summary>
		/// Invoked as an event handler, typically on the <see cref="FrameworkElement.Loaded"/>
		/// event of a <see cref="Control"/> within the page, to indicate that the sender should
		/// start receiving visual state management changes that correspond to application view
		/// state changes.
		/// </summary>
		/// <param name="sender">Instance of <see cref="Control"/> that supports visual state
		/// management corresponding to view states.</param>
		/// <remarks>The current view state will immediately be used to set the corresponding
		/// visual state when layout updates are requested.  A corresponding
		/// <see cref="FrameworkElement.Unloaded"/> event handler connected to
		/// <see cref="StopLayoutUpdates"/> is strongly encouraged.  Instances of
		/// <seealso cref="DetermineVisualState"/>
		/// <seealso cref="InvalidateVisualState"/>
		/// </remarks>
		public void StartLayoutUpdates( object sender )
		{
			var control = sender as Control;

			if( control == null )
			{
				return;
			}

			if( _layoutAwareControls == null )
			{
				// Start listening to view state changes when there are controls interested in updates
				Window.Current.SizeChanged += WindowSizeChanged;

				_layoutAwareControls = new List<Control>();
			}

			_layoutAwareControls.Add( control );

			// Set the initial visual state of the control
			VisualStateManager.GoToState( control, DetermineVisualState(), false );
		}

		private void WindowSizeChanged( object sender, WindowSizeChangedEventArgs e )
		{
			InvalidateVisualState();
		}

		/// <summary>
		/// Invoked as an event handler, typically on the <see cref="FrameworkElement.Unloaded"/>
		/// event of a <see cref="Control"/>, to indicate that the sender should start receiving
		/// visual state management changes that correspond to application view state changes.
		/// </summary>
		/// <param name="sender">Instance of <see cref="Control"/> that supports visual state
		/// management corresponding to view states.</param>
		/// <remarks>The current view state will immediately be used to set the corresponding
		/// visual state when layout updates are requested.</remarks>
		/// <seealso cref="StartLayoutUpdates"/>
		public void StopLayoutUpdates( object sender )
		{
			var control = sender as Control;

			if( control == null || _layoutAwareControls == null )
			{
				return;
			}

			_layoutAwareControls.Remove( control );

			if( _layoutAwareControls.Count == 0 )
			{
				// Stop listening to view state changes when no controls are interested in updates
				_layoutAwareControls = null;

				Window.Current.SizeChanged -= WindowSizeChanged;
			}
		}

		/// <summary>
		/// Calculate value for visual state for the page.
		/// </summary>
		/// <returns>Visual state name used to drive the
		/// <see cref="VisualStateManager"/></returns>
		/// <seealso cref="InvalidateVisualState"/>
		protected virtual string DetermineVisualState()
		{
			var windowWidth = Window.Current.Bounds.Width;
			var windowHeight = Window.Current.Bounds.Height;

			string viewState;

			if( windowWidth < 500 )
			{
				viewState = PageVisualState.Snapped.ToString();
			}
			else if( windowWidth < windowHeight )
			{
				viewState = PageVisualState.FullScreenPortrait.ToString();
			}
			else
			{
				viewState = PageVisualState.FullScreenLandscape.ToString();
			}

			return viewState;
		}

		/// <summary>
		/// Updates all controls that are listening for visual state changes with the correct
		/// visual state.
		/// </summary>
		/// <remarks>
		/// Typically used in conjunction with overriding <see cref="DetermineVisualState"/> to
		/// signal that a different value may be returned even though the view state has not
		/// changed.
		/// </remarks>
		public void InvalidateVisualState()
		{
			if( _layoutAwareControls != null )
			{
				var visualState = DetermineVisualState();

				foreach( var layoutAwareControl in _layoutAwareControls )
				{
					VisualStateManager.GoToState( layoutAwareControl, visualState, false );
				}
			}
		}
		#endregion
	}
}
