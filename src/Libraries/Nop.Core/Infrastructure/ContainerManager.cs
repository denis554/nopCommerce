﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Autofac;
using Autofac.Integration.Mvc;
using AutofacContrib.Startable;
using Nop.Core.Infrastructure.AutoFac;
using Nop.Core.Plugins;
using System.Web.Mvc;

namespace Nop.Core.Infrastructure
{
    public class ContainerManager
    {
        private IContainer _container;

        public ContainerManager(IContainer container)
        {
            _container = container;
        }

        public IContainer Container
        {
            get { return _container; }
        }

        public void AddComponent<TService>(string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponent<TService, TService>(key, lifeStyle);
        }

        public void AddComponent(Type service, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponent(service, service, key, lifeStyle);
        }

        public void AddComponent<TService, TImplementation>(string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponent(typeof(TService), typeof(TImplementation), key, lifeStyle);
        }

        public void AddComponent(Type service, Type implementation, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            UpdateContainer(x =>
            {
                var serviceTypes = new List<Type> { service };
                if (typeof(IAutoStart).IsAssignableFrom(serviceTypes[0]))
                    serviceTypes.Add(typeof(IAutoStart));

                if (service.IsGenericType)
                {
                    var temp = x.RegisterGeneric(implementation).As(
                        serviceTypes.ToArray()).PerLifeStyle(lifeStyle);
                    if (!string.IsNullOrEmpty(key))
                    {
                        temp.Keyed(key, service);
                    }
                }
                else
                {
                    var temp = x.RegisterType(implementation).As(
                        serviceTypes.ToArray()).PerLifeStyle(lifeStyle);
                    if (!string.IsNullOrEmpty(key))
                    {
                        temp.Keyed(key, service);
                    }
                }
            });
        }

        public void AddComponentInstance<TService>(object instance, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponentInstance(typeof(TService), instance, key, lifeStyle);
        }

        public void AddComponentInstance(Type service, object instance, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            UpdateContainer(x =>
            {
                var registration = x.RegisterInstance(instance).Keyed(key, service).As(service).PerLifeStyle(lifeStyle);

                if (typeof(IAutoStart).IsAssignableFrom(instance.GetType()))
                    registration.As<IAutoStart>();
            });
        }

        public void AddComponentInstance(object instance, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponentInstance(instance.GetType(), instance, key, lifeStyle);
        }

        public void AddComponentWithParameters<TService, TImplementation>(IDictionary<string, string> properties, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponentWithParameters(typeof(TService), typeof(TImplementation), properties);
        }

        public void AddComponentWithParameters(Type service, Type implementation, IDictionary<string, string> properties, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            UpdateContainer(x =>
            {
                var serviceTypes = new List<Type> { service };
                if (typeof(IAutoStart).IsAssignableFrom(serviceTypes[0]))
                    serviceTypes.Add(typeof(IAutoStart));

                var temp = x.RegisterType(implementation).As(serviceTypes.ToArray()).
                    WithParameters(
                        properties.Select(y => new NamedParameter(y.Key, y.Value)));
                if (!string.IsNullOrEmpty(key))
                {
                    temp.Keyed(key, service);
                }
            });
        }

        public T Resolve<T>(string key = "") where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                return Scope().Resolve<T>();
            }
            return Scope().ResolveKeyed<T>(key);
        }

        public object Resolve(Type type)
        {
            return Scope().Resolve(type);

        }

        public T[] ResolveAll<T>(string key = "")
        {
            if (string.IsNullOrEmpty(key))
            {
                return Scope().Resolve<IEnumerable<T>>().ToArray();
            }
            return Scope().ResolveKeyed<IEnumerable<T>>(key).ToArray();
        }

        public void StartComponents()
        {
            _container.Resolve<IStarter>().Start();
        }

        public void UpdateContainer(Action<ContainerBuilder> action)
        {
            var builder = new ContainerBuilder();
            action.Invoke(builder);
            builder.Update(_container);
        }

        public ILifetimeScope Scope()
        {
            try
            {
                return RequestLifetimeHttpModule.GetLifetimeScope(Container, null);
            }
            catch
            {
                return Container;
            }
        }
    }
    public static class ContainerManagerExtensions
    {
        public static Autofac.Builder.IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PerLifeStyle<TLimit, TActivatorData, TRegistrationStyle>(this Autofac.Builder.IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, ComponentLifeStyle lifeStyle)
        {
            switch (lifeStyle)
            {
                case ComponentLifeStyle.LifetimeScope:
                    if (HttpContext.Current != null)
                    {
                        return builder.InstancePerHttpRequest();
                    }
                    else
                    {
                        return builder.InstancePerLifetimeScope();
                    }
                case ComponentLifeStyle.Transient:
                    return builder.InstancePerDependency();
                case ComponentLifeStyle.Singleton:
                    return builder.SingleInstance();
                default:
                    return builder.SingleInstance();
            }
        }
    }
}
