using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AreaControl
{
    /// <summary>
    /// Lightweight implementation of an IoC container for simplification of mod dependencies to not use a heavyweight DLL dependency.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class IoC
    {
        private static readonly string PostConstructName = typeof(PostConstruct).FullName;

        private readonly Dictionary<Type, ImplementationType> _components = new Dictionary<Type, ImplementationType>();
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
        private readonly object _lockState = new object();

        #region Constructors

        private IoC()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the IoC instance.
        /// </summary>
        public static IoC Instance { get; } = new IoC();

        #endregion

        #region Methods

        /// <summary>
        /// Register a new component type.
        /// </summary>
        /// <param name="implementation">Set the implementation type.</param>
        /// <typeparam name="T">Set the type of the component.</typeparam>
        /// <returns>Returns this IoC.</returns>
        public IoC Register<T>(Type implementation)
        {
            lock (_lockState)
            {
                RegisterType<T>(implementation, false);
                return this;
            }
        }

        /// <summary>
        /// Register a new singleton type.
        /// </summary>
        /// <param name="implementation">Set the implementation type.</param>
        /// <typeparam name="T">Set the type of the component.</typeparam>
        /// <returns>Returns this IoC.</returns>
        public IoC RegisterSingleton<T>(Type implementation)
        {
            lock (_lockState)
            {
                RegisterType<T>(implementation, true);
                return this;
            }
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
            lock (_lockState)
            {
                RegisterType<T>(instance.GetType(), true);
                _singletons.Add(typeof(T), instance);
                return this;
            }
        }

        /// <summary>
        /// Unregister everything in this IoC.
        /// Should only be used for unit testing.
        /// </summary>
        public IoC UnregisterAll()
        {
            lock (_lockState)
            {
                _components.Clear();
                _singletons.Clear();
                return this;
            }
        }

        /// <summary>
        /// Get the an instance of the registered type.
        /// </summary>
        /// <typeparam name="T">Set the component type to return.</typeparam>
        /// <returns>Returns the component instance.</returns>
        public T GetInstance<T>()
        {
            lock (_lockState)
            {
                return (T) GetInstance(typeof(T));
            }
        }

        /// <summary>
        /// Get one or more instance of the required type.
        /// </summary>
        /// <typeparam name="T">Set the instance type to return.</typeparam>
        /// <returns>Returns one or more instance(s) of the required type of found, otherwise, it will return an empty list if none found.</returns>
        public List<T> GetInstances<T>()
        {
            return null;
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
            lock (_lockState)
            {
                var type = typeof(T);

                if (!_components.ContainsKey(type))
                    throw new IoCException(type + " has not been registered");

                if (!_components[type].IsSingleton)
                    throw new IoCException(type + " is not registered as a singleton");

                return _singletons.ContainsKey(type);
            }
        }

        #endregion

        #region Functions

        private void RegisterType<T>(Type implementation, bool isSingleton)
        {
            lock (_lockState)
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
        }

        private object GetInstance(Type type)
        {
            lock (_lockState)
            {
                if (!_components.ContainsKey(type))
                    return null;

                var component = _components[type];

                if (component.IsSingleton && _singletons.ContainsKey(type))
                    return _singletons[type];

                var instance = InitializeInstanceType(component.Type);

                InvokePostConstruct(instance);

                if (component.IsSingleton)
                    _singletons.Add(type, instance);

                return instance;
            }
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

        private static void InvokePostConstruct(object instance)
        {
            var postConstructMethod = instance.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(x => x.GetCustomAttribute<PostConstruct>() != null);

            if (postConstructMethod == null)
                return;

            var methodParams = postConstructMethod.GetParameters();

            if (methodParams.Length > 0)
                throw new IoCException(PostConstructName + " doesn't allow parameterized methods");

            try
            {
                postConstructMethod.Invoke(instance, new object[] { });
            }
            catch (Exception ex)
            {
                throw new IoCException(PostConstructName + " failed with error:" + Environment.NewLine + ex.Message, ex);
            }
        }

        private bool AreAllParametersRegistered(ConstructorInfo constructor)
        {
            return constructor.GetParameters().All(parameterInfo => _components.ContainsKey(parameterInfo.ParameterType));
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Indicates that the method must be executed after instantiation of the instance managed by this IoC.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class PostConstruct : Attribute
        {
        }

        /// <summary>
        /// Internal placeholder of the IoC implementation type info.
        /// </summary>
        private class ImplementationType
        {
            public Type Type { get; set; }

            public bool IsSingleton { get; set; }
        }
    }
}