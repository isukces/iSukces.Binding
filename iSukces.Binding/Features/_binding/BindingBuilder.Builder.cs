using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public partial class BindingBuilder
    {
        public BindingBuilder WithListenerTag(object listenerTag)
        {
            ListenerTag = listenerTag;
            return this;
        }

        private sealed class Builder
        {
            public Builder(BindingBuilder src)
            {
                _src            = src;
                _bindingManager = src.GetBindingManager();
            }

            internal IDisposable Create<TTarget>([NotNull] TTarget target, string propertyName, BindingMode mode)
            {
                _src.Mode = mode;
                return Create(target, propertyName);
            }

            internal IDisposable Create<TTarget>([NotNull] TTarget target, string propertyName)
            {
                if (target == null) throw new ArgumentNullException(nameof(target));
                IPropertyInfo propertyInfo;
                if (propertyName.Contains("."))
                    throw new NotSupportedException("Property name cannot have dots.");

                void SureInfo()
                {
                    var reg = _bindingManager.PropertyInfoProviderRegistry;
                    if (reg is null)
                    {
                        const string message = nameof(_wrapper.BindingManager) + " doesn't have " +
                                               nameof(IPropertyInfoProviderRegistry) + ".";
                        throw new NullReferenceException(message);
                    }

                    propertyInfo = reg.FindProvider(target.GetType(), propertyName, _src.UpdateSourceTrigger);
                    if (propertyInfo is null)
                        throw new NotSupportedException(); // !!!!!!!!!!!!!!!!
                }

                var mode = _src.Mode;
                if (mode == BindingMode.Default)
                {
                    SureInfo();
                    if ((propertyInfo.Features & BindingFeatures.TwoWayBindingByDefault) != 0)
                        mode = BindingMode.TwoWay;
                    else
                        mode = BindingMode.OneWay;
                }

                SureInfo();
                var accessor = propertyInfo.Create(target);

                void Listener([NotNull] IValueInfo info)
                {
                    // update target property when source changed
                    accessor.PropertySetValue(info.Value);
                }

                switch (mode)
                {
                    case BindingMode.TwoWay:
                        return CreateListener(Listener, accessor.PropertyType, TwoWayBindingFactoryMethod);
                    case BindingMode.OneWay:
                        return CreateListener(Listener, accessor.PropertyType, null);
                    default: 
                        throw new ArgumentOutOfRangeException(mode.ToString());
                }

                IDisposable TwoWayBindingFactoryMethod(IDisposable disconnectFromBindingManager,
                    ListerInfo info, BindingValueWrapper wrapper)
                {
                    if (_src.Validators is not IReadOnlyList<BindingValidator> bindingValidators)
                        bindingValidators = _src.Validators?.ToArray() ?? Array.Empty<BindingValidator>();
                    
                    var eventSubscription = accessor.SubscribePropertyNotification(o =>
                    {
                       

                        var updateSourceResult = wrapper.UpdateSource(o, info, bindingValidators);

                        if (BindingTracker.Instance.LoggingEnabled)
                        {
                            BindingTracker.Instance.LogSourceUpdated(updateSourceResult, info);
                            BindingTracker.Instance.NotifyDataModelChanged();
                        }

                        return updateSourceResult;
                    });
                    // if (bindingValidators.Count > 0)
                    {
                        var m = _bindingManager.AddUpdateSourceAction(() =>
                        {
                            eventSubscription.ForceUpdate(false);
                        });
                        disconnectFromBindingManager = new CombinedDisposable(disconnectFromBindingManager, m);
                    }
                    return new CombinedDisposable(
                        disconnectFromBindingManager,
                        eventSubscription);
                }
            }


            internal IDisposable CreateListener(ListenerDelegate listener,
                Type typeAcceptedByListener, aaaFunc factory)
            {
                var disposables = new DisposableHolder();
                var wrapper     = _src._wrapper.CreateAccessor(_src.Path);
                var info = new ListerInfo(listener, typeAcceptedByListener,
                    _src.Converter,
                    _src.ConverterParameter,
                    _src.CultureInfo,
                    _src.ListenerDispatcher,
                    _src.StringFormat,
                    _src.ListenerTag);
                disposables.RemoveFromListerer = wrapper.AddListenerAction(info);

                var disconnectFromBindingManager = new DisposableAction(() =>
                {
                    if (disposables.MainDisposing is null)
                        return;
                    _bindingManager.RemoveDisposable(disposables.MainDisposing);
                    disposables.RemoveFromListerer?.Dispose();
                });

                if (factory is not null)
                    disposables.MainDisposing = factory(disconnectFromBindingManager, info, wrapper);
                if (disposables.MainDisposing is null)
                    disposables.MainDisposing = disconnectFromBindingManager;
                _bindingManager.AddDisposable(disposables.MainDisposing);
                return disposables.MainDisposing;
            }

           
            /*
            public ITwoWayBinding CreateTwoWayBinding(ListenerDelegate listener, Type typeAcceptedByListener)
            {
                var disposable = CreateListener(listener, typeAcceptedByListener,
                    (disconnectFromBindingManager, info, wrapper) =>
                    {
                        var result = new TwoWayBinding(disconnectFromBindingManager, obj =>
                        {
                            return wrapper.UpdateSource(obj, info);
                        });
                        return result;
                    });
                return (ITwoWayBinding)disposable;
            }

            public IDisposable CreateTwoWayBinding<TTarget>([NotNull] TTarget target, string propertyName)
            {
                _src.Mode = BindingMode.TwoWay;
                return Create(target, propertyName);
            }
            */


            private readonly BindingManager _bindingManager;

            private readonly BindingBuilder _src;

            public ITwoWayBinding CreateTwoWayBinding(ListenerDelegate listener, Type typeAcceptedByListener)
            {
                var disposable = CreateListener(listener, typeAcceptedByListener,
                    (disconnectFromBindingManager, info, wrapper) =>
                    {
                        var result = new TwoWayBinding(disconnectFromBindingManager, obj =>
                        {
                            return wrapper.UpdateSource(obj, info, null);
                        });
                        return result;
                    });
                return (ITwoWayBinding)disposable;
            }
        }
    }

    internal delegate IDisposable aaaFunc(IDisposable disconnectFromBindingManager, ListerInfo info,
        BindingValueWrapper wrapper);
}
