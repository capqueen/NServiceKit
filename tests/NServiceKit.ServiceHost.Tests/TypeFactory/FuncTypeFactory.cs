using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Funq;
using NServiceKit.Configuration;

namespace NServiceKit.ServiceHost.Tests.TypeFactory
{
    /// <summary>A function type factory.</summary>
	public class FuncTypeFactory
		: ITypeFactory
	{
		private readonly Container container;
		private readonly Dictionary<Type, Func<object>> resolveFnMap = new Dictionary<Type, Func<object>>();

        /// <summary>Initializes a new instance of the NServiceKit.ServiceHost.Tests.TypeFactory.FuncTypeFactory class.</summary>
        ///
        /// <param name="container">The container.</param>
		public FuncTypeFactory(Container container)
		{
			this.container = container;
		}

        /// <summary>Creates an instance.</summary>
        ///
        /// <param name="type">The type.</param>
        ///
        /// <returns>The new instance.</returns>
		public object CreateInstance(Type type)
		{
			Func<object> resolveFn;

			if (!this.resolveFnMap.TryGetValue(type, out resolveFn))
			{
				var containerInstance = Expression.Constant(this.container);
				var resolveInstance = Expression.Call(containerInstance, "Resolve", new[] {type}, new Expression[0]);
				var resolveObject = Expression.Convert(resolveInstance, typeof (object));
				resolveFn = (Func<object>)Expression.Lambda(resolveObject, new ParameterExpression[0]).Compile();

				lock (this.resolveFnMap)
				{
					this.resolveFnMap[type] = resolveFn;
				}
			}

			return resolveFn();
		}
	}
}