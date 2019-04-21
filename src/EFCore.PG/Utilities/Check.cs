using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Utilities
{
    [DebuggerStepThrough]
    internal static class Check
    {
        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>([NoEnumeration] T value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>(
            [NoEnumeration] T value,
            [InvokerParameterName] [NotNull] string parameterName,
            [NotNull] string propertyName)
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(parameterName, nameof(parameterName));
                NotEmpty(propertyName, nameof(propertyName));

                throw new ArgumentException(CoreStrings.ArgumentPropertyNull(propertyName, parameterName));
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static IReadOnlyList<T> NotEmpty<T>(IReadOnlyList<T> value, [InvokerParameterName] [NotNull] string parameterName)
        {
            NotNull(value, parameterName);

            if (value.Count == 0)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(AbstractionsStrings.CollectionArgumentIsEmpty(parameterName));
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotEmpty(string value, [InvokerParameterName] [NotNull] string parameterName)
        {
            Exception e = null;
            if (ReferenceEquals(value, null))
            {
                e = new ArgumentNullException(parameterName);
            }
            else if (value.Trim().Length == 0)
            {
                e = new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            }

            if (e != null)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }

        public static string NullButNotEmpty([CanBeNull] string value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (!ReferenceEquals(value, null)
                && (value.Length == 0))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            }

            return value;
        }

        public static IReadOnlyCollection<T> NullButNotEmpty<T>([CanBeNull] IReadOnlyCollection<T> value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (!ReferenceEquals(value, null)
                && (value.Count == 0))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            }

            return value;
        }

        public static IReadOnlyList<T> HasNoNulls<T>(IReadOnlyList<T> value, [InvokerParameterName] [NotNull] string parameterName)
            where T : class
        {
            NotNull(value, parameterName);

            if (value.Any(e => e == null))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(parameterName);
            }

            return value;
        }
    }
}
