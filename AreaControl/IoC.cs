using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private readonly List<ComponentDefinition> _components = new List<ComponentDefinition>();
        private readonly Dictionary<ComponentDefinition, object> _singletons = new Dictionary<ComponentDefinition, object>();
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
                _singletons.Add(RegisterType<T>(instance.GetType(), true), instance);
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
        public ReadOnlyCollection<T> GetInstances<T>()
        {
            lock (_lockState)
            {
                var type = typeof(T);

                return GetInstances(type)
                    .Select(x => (T) x)
                    .ToList()
                    .AsReadOnly();
            }
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

                if (GetDefinitionsFor(type).Count == 0)
                    throw new IoCException(type + " has not been registered");

                var definition = GetSingletonDefinitionFor(type);

                if (definition == null)
                    throw new IoCException(type + " is not registered as a singleton");

                return _singletons.ContainsKey(definition);
            }
        }

        #endregion

        #region Functions

        private ComponentDefinition RegisterType<T>(Type implementation, bool isSingleton)
        {
            lock (_lockState)
            {
                var type = typeof(T);
                var key = new ComponentDefinition(type, implementation, isSingleton);

                if (!type.IsAssignableFrom(implementation))
                    throw new IoCException(implementation + " does not implement given type " + type);

                if (isSingleton && GetDefinitionsFor(type).Count > 0)
                    throw new IoCException(type + " has already been registered as a singleton or component");

                _components.Add(key);
                key.AddDerivedTypes(GetDerivedTypes(key.Type));
                return key;
            }
        }

        private IEnumerable<Type> GetDerivedTypes(Type type)
        {
            lock (_lockState)
            {
                var derivedTypes = new List<Type>();

                foreach (var derivedInterface in type.GetInterfaces())
                {
                    derivedTypes.Add(derivedInterface);
                    derivedTypes.AddRange(GetDerivedTypes(derivedInterface));
                }

                return derivedTypes;
            }
        }

        private IList<ComponentDefinition> GetDefinitionsFor(Type type)
        {
            lock (_lockState)
            {
                return _components.FindAll(x => x.Defines(type));
            }
        }

        private ComponentDefinition GetSingletonDefinitionFor(Type type)
        {
            lock (_lockState)
            {
                return _components.FirstOrDefault(x => x.Type == type && x.IsSingleton);
            }
        }

        private object GetInstance(Type type)
        {
            var instances = GetInstances(type);

            if (instances.Count > 1)
                throw new IoCException("More than one instance has been found for " + type);

            return instances.FirstOrDefault();
        }

        private List<object> GetInstances(Type type)
        {
            lock (_lockState)
            {
                var definitions = GetDefinitionsFor(type);

                if (definitions.Count == 0)
                    return new List<object>();

                var singletonDefinition = GetSingletonDefinitionFor(type);

                if (singletonDefinition != null && _singletons.ContainsKey(singletonDefinition))
                    return new List<object> {_singletons[singletonDefinition]};

                var instances = new List<object>();

                foreach (var definition in definitions)
                {
                    var instance = InitializeInstanceType(definition.ImplementationType);
                    InvokePostConstruct(instance);

                    if (definition.IsSingleton)
                        _singletons.Add(definition, instance);

                    instances.Add(instance);
                }

                return instances;
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
            return constructor.GetParameters().All(parameterInfo => _components.FirstOrDefault(x => x.Type == parameterInfo.ParameterType) != null);
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
        /// Internal placeholder of the IoC definition type info.
        /// This is the type that is being used as interface registration of an implementation.
        /// </summary>
        private class ComponentDefinition
        {
            private readonly List<Type> _derivedTypes = new List<Type>();

            public ComponentDefinition(Type type, Type implementationType, bool isSingleton)
            {
                Type = type;
                ImplementationType = implementationType;
                IsSingleton = isSingleton;
            }

            #region Properties

            /// <summary>
            /// Get the definition type.
            /// This is the key of the registration types.
            /// </summary>
            public Type Type { get; }
            
            /// <summary>
            /// Get the implementation type for this definition.
            /// </summary>
            public Type ImplementationType { get; }

            /// <summary>
            /// Get if this definition is a singleton registration.
            /// </summary>
            public bool IsSingleton { get; }

            /// <summary>
            /// Get all derived types defined by the key interface.
            /// </summary>
            public ReadOnlyCollection<Type> DerivedTypes => _derivedTypes.AsReadOnly();

            #endregion

            #region Methods

            /// <summary>
            /// Get if this key defines (implements) the given type.
            /// This will check it's own type as well as derived types.
            /// </summary>
            /// <param name="type">Set the type to check for.</param>
            /// <returns>Returns true if this definition defines the given type, else false.</returns>
            public bool Defines(Type type)
            {
                return Type == type || DerivedTypes.Contains(type);
            }

            public void AddDerivedTypes(IEnumerable<Type> types)
            {
                foreach (var type in types)
                {
                    AddDerivedType(type);
                }
            }

            public void AddDerivedType(Type type)
            {
                if (!_derivedTypes.Contains(type))
                    _derivedTypes.Add(type);
            }

            #endregion

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ComponentDefinition) obj);
            }

            public override int GetHashCode()
            {
                return Type != null ? Type.GetHashCode() : 0;
            }

            public override string ToString()
            {
                return $"{nameof(Type)}: {Type}, {nameof(ImplementationType)}: {ImplementationType}, {nameof(IsSingleton)}: {IsSingleton}, " +
                       $"{nameof(_derivedTypes)}: {_derivedTypes.Count}";
            }

            private bool Equals(ComponentDefinition other)
            {
                return Type == other.Type;
            }
        }
    }
}