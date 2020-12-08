using System;
using System.Diagnostics.CodeAnalysis;

namespace NSubstitute.Core.Arguments
{
    public static class ArgumentMatcher
    {
        /// <summary>
        /// Enqueues a matcher for the method argument in current position and returns the value which should be
        /// passed back to the method you invoke.
        /// </summary>
        public static ref T? Enqueue<T>(IArgumentMatcher<T> argumentMatcher)
        {
            if (argumentMatcher == null) throw new ArgumentNullException(nameof(argumentMatcher));

            IArgumentMatcher nonGenericMatcher;
            if (argumentMatcher is IDescribeNonMatches)
            {
                nonGenericMatcher = new GenericToNonGenericMatcherProxyWithDescribe<T>(argumentMatcher);
            }
            else
            {
                nonGenericMatcher = new GenericToNonGenericMatcherProxy<T>(argumentMatcher);
            }

            return ref EnqueueArgSpecification<T>(new ArgumentSpecification(typeof(T), nonGenericMatcher));
        }

        internal static ref T? Enqueue<T>(IArgumentMatcher argumentMatcher)
        {
            return ref EnqueueArgSpecification<T>(new ArgumentSpecification(typeof(T), argumentMatcher));
        }

        internal static ref T? Enqueue<T>(IArgumentMatcher argumentMatcher, Action<object?> action)
        {
            return ref EnqueueArgSpecification<T>(new ArgumentSpecification(typeof(T), argumentMatcher, action));
        }

        private static ref T? EnqueueArgSpecification<T>(IArgumentSpecification specification)
        {
            SubstitutionContext.Current.ThreadContext.EnqueueArgumentSpecification(specification);
            return ref new DefaultValueContainer<T>().Value;
        }

        private class GenericToNonGenericMatcherProxy<T> : IArgumentMatcher
        {
            protected readonly IArgumentMatcher<T> _matcher;

            public GenericToNonGenericMatcherProxy(IArgumentMatcher<T> matcher)
            {
                _matcher = matcher;
            }

            public bool IsSatisfiedBy(object? argument) => _matcher.IsSatisfiedBy((T) argument!);
        }

        private class GenericToNonGenericMatcherProxyWithDescribe<T> : GenericToNonGenericMatcherProxy<T>, IDescribeNonMatches
        {
            public GenericToNonGenericMatcherProxyWithDescribe(IArgumentMatcher<T> matcher) : base(matcher)
            {
                if (matcher as IDescribeNonMatches == null) throw new ArgumentException("Should implement IDescribeNonMatches type.");
            }

            public string DescribeFor(object? argument) => ((IDescribeNonMatches) _matcher).DescribeFor(argument);
        }

        private class DefaultValueContainer<T>
        {
            public T? Value;
        }
    }
}