using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RazerPoliceLights
{
    /// <summary>
    /// Lightweight implementation of an IoC container for simplification of mod dependencies to not use a heavyweight DLL dependency.
    /// </summary>
    public class IoC
    {
        private readonly Dictionary<Type, ImplementationType> _components = new Dictionary<Type, ImplementationType>();
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        private IoC()
        {
        }

        /// <summary>
        /// Get the IoC instance.
        /// </summary>
        public static IoC Instance { get; } = new IoC();

        /// <summary>
        /// Register a new component type.
        /// </summary>
        /// <param name="implementation">Set the implementation type.</param>
        /// <typeparam name="T">Set the type of the component.</typeparam>
        /// <returns>Returns this IoC.</returns>
        public IoC Register<T>(Type implementation)
        {
            RegisterType<T>(implementation, false);
            return this;
        }

        /// <summary>
        /// Register a new singleton type.
        /// </summary>
        /// <param name="implementation">Set the implementation type.</param>
        /// <typeparam name="T">Set the type of the component.</typeparam>
        /// <returns>Returns this IoC.</returns>
        public IoC RegisterSingleton<T>(Type implementation)
        {
            RegisterType<T>(implementation, true);
            return this;
        }

        /// <summary>
        /// Register a new instance for the given type.
        /// This should only be used in unit tests.
        /// </summary>
        /// <param name="instance">Set the instance to register.</param>
        /// <typeparam name="T">Set the type of the component.</typeparam>
        /// <returns>Returns this IoC.</returns>
        public IoC RegisterInstance<T>(object instance)
        {
            RegisterType<T>(instance.GetType(), true);
            _singletons.Add(typeof(T), instance);
            return this;
        }

        /// <summary>
        /// Unregister everything in this IoC.
        /// Should only be used for unit testing.
        /// </summary>
        public IoC UnregisterAll()
        {
            _components.Clear();
            _singletons.Clear();
            return this;
        }

        /// <summary>
        /// Get the an instance of the registered type.
        /// </summary>
        /// <typeparam name="T">Set the component type to return.</typeparam>
        /// <returns>Returns the component instance.</returns>
        public T GetInstance<T>()
        {
            return (T) GetInstance(typeof(T));
        }

        /// <summary>
        /// Verify if the given type already has an existing singleton instance.
        /// This method will only work if the given type is a registered singleton type.
        /// </summary>
        /// <typeparam name="T">Set the component type.</typeparam>
        /// <returns>Returns true if an instance already exists, else false.</returns>
        /// <exception cref="IoCException">Is thrown when the given type is not registered or not a singleton.</exception>
        public bool InstanceExists<T>()
        {
            var type = typeof(T);

            if (!_components.ContainsKey(type))
                throw new IoCException(type + " has not been registered");

            if (!_components[type].IsSingleton)
                throw new IoCException(type + " is not registered as a singleton");

            return _singletons.ContainsKey(type);
        }

        private void RegisterType<T>(Type implementation, bool isSingleton)
        {
            var type = typeof(T);

            if (!type.IsAssignableFrom(implementation))
                throw new IoCException(implementation + " does not implement given type " + type);
            if (_components.ContainsKey(type))
                throw new IoCException(type + " has already been registered");

            _components.Add(type, new ImplementationType
            {
                Type = implementation,
                IsSingleton = isSingleton
            });
        }

        private object GetInstance(Type type)
        {
            if (!_components.ContainsKey(type))
                return null;

            var component = _components[type];

            if (component.IsSingleton && _singletons.ContainsKey(type))
                return _singletons[type];

            var instance = InitializeInstanceType(component.Type);

            if (component.IsSingleton)
                _singletons.Add(type, instance);

            return instance;
        }

        private object InitializeInstanceType(Type type)
        {
            foreach (var constructor in type.GetConstructors())
            {
                if (!AreAllParametersRegistered(constructor))
                    continue;

                return constructor.Invoke(constructor.GetParameters()
                    .Select(parameterInfo => GetInstance(parameterInfo.ParameterType))
                    .ToArray());
            }

            throw new IoCException("Could not create instance for " + type);
        }

        private bool AreAllParametersRegistered(ConstructorInfo constructor)
        {
            return constructor.GetParameters().All(parameterInfo => _components.ContainsKey(parameterInfo.ParameterType));
        }

        private class ImplementationType
        {
            public Type Type { get; set; }

            public bool IsSingleton { get; set; }
        }
    }
}