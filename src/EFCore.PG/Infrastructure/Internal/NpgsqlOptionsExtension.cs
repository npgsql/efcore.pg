using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal
{
    /// <summary>
    /// Represents options managed by the Npgsql.
    /// </summary>
    public class NpgsqlOptionsExtension : RelationalOptionsExtension
    {
        DbContextOptionsExtensionInfo _info;
        [NotNull] readonly List<UserRangeDefinition> _userRangeDefinitions;

        /// <summary>
        /// The name of the database for administrative operations.
        /// </summary>
        [CanBeNull]
        public virtual string AdminDatabase { get; private set; }

        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull]
        public virtual Version PostgresVersion { get; private set; }

        /// <summary>
        /// The list of range mappings specified by the user.
        /// </summary>
        [NotNull]
        public virtual IReadOnlyList<UserRangeDefinition> UserRangeDefinitions => _userRangeDefinitions;

        /// <summary>
        /// The specified <see cref="ProvideClientCertificatesCallback"/>.
        /// </summary>
        [CanBeNull]
        public virtual ProvideClientCertificatesCallback ProvideClientCertificatesCallback { get; private set; }

        /// <summary>
        /// The specified <see cref="RemoteCertificateValidationCallback"/>.
        /// </summary>
        [CanBeNull]
        public virtual RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; private set; }

        /// <summary>
        /// The specified <see cref="ProvidePasswordCallback"/>.
        /// </summary>
        [CanBeNull]
        public virtual ProvidePasswordCallback ProvidePasswordCallback { get; private set; }

        /// <summary>
        /// True if reverse null ordering is enabled; otherwise, false.
        /// </summary>
        public virtual bool ReverseNullOrdering { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="NpgsqlOptionsExtension"/> with the default settings.
        /// </summary>
        public NpgsqlOptionsExtension()
            => _userRangeDefinitions = new List<UserRangeDefinition>();

        // NB: When adding new options, make sure to update the copy ctor below.
        /// <summary>
        /// Initializes an instance of <see cref="NpgsqlOptionsExtension"/> by copying the specified instance.
        /// </summary>
        /// <param name="copyFrom">The instance to copy.</param>
        public NpgsqlOptionsExtension([NotNull] NpgsqlOptionsExtension copyFrom) : base(copyFrom)
        {
            AdminDatabase = copyFrom.AdminDatabase;
            _userRangeDefinitions = new List<UserRangeDefinition>(copyFrom._userRangeDefinitions);
            PostgresVersion = copyFrom.PostgresVersion;
            ProvideClientCertificatesCallback = copyFrom.ProvideClientCertificatesCallback;
            RemoteCertificateValidationCallback = copyFrom.RemoteCertificateValidationCallback;
            ProvidePasswordCallback = copyFrom.ProvidePasswordCallback;
            ReverseNullOrdering = copyFrom.ReverseNullOrdering;
        }

        // The following is a hack to set the default minimum batch size to 2 in Npgsql
        // See https://github.com/aspnet/EntityFrameworkCore/pull/10091
        public override int? MinBatchSize => base.MinBatchSize ?? 2;

        /// <summary>
        /// Returns a copy of the current instance configured with the specified range mapping.
        /// </summary>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithUserRangeDefinition<TSubtype>(
            [NotNull] string rangeName,
            [CanBeNull] string schemaName = null,
            [CanBeNull] string subtypeName = null)
            => WithUserRangeDefinition(rangeName, schemaName, typeof(TSubtype), subtypeName);

        /// <summary>
        /// Returns a copy of the current instance configured with the specified range mapping.
        /// </summary>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithUserRangeDefinition(
            [NotNull] string rangeName,
            [CanBeNull] string schemaName,
            [NotNull] Type subtypeClrType,
            [CanBeNull] string subtypeName)
        {
            Check.NotEmpty(rangeName, nameof(rangeName));
            Check.NotNull(subtypeClrType, nameof(subtypeClrType));

            var clone = (NpgsqlOptionsExtension)Clone();

            clone._userRangeDefinitions.Add(new UserRangeDefinition(rangeName, schemaName, subtypeClrType, subtypeName));

            return clone;
        }

        /// <summary>
        /// Returns a copy of the current instance configured to use the specified administrative database.
        /// </summary>
        /// <param name="adminDatabase">The name of the database for administrative operations.</param>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithAdminDatabase([CanBeNull] string adminDatabase)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone.AdminDatabase = adminDatabase;

            return clone;
        }

        /// <summary>
        /// Returns a copy of the current instance with the specified PostgreSQL version.
        /// </summary>
        /// <param name="postgresVersion">The backend version to target.</param>
        /// <returns>
        /// A copy of the current instance with the specified PostgreSQL version.
        /// </returns>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithPostgresVersion([CanBeNull] Version postgresVersion)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone.PostgresVersion = postgresVersion;

            return clone;
        }

        /// <summary>
        /// Returns a copy of the current instance configured with the specified value..
        /// </summary>
        /// <param name="reverseNullOrdering">True to enable reverse null ordering; otherwise, false.</param>
        [NotNull]
        internal virtual NpgsqlOptionsExtension WithReverseNullOrdering(bool reverseNullOrdering)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone.ReverseNullOrdering = reverseNullOrdering;

            return clone;
        }

        #region Authentication

        /// <summary>
        /// Returns a copy of the current instance with the specified <see cref="ProvideClientCertificatesCallback"/>.
        /// </summary>
        /// <param name="callback">The specified callback.</param>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithProvideClientCertificatesCallback([CanBeNull] ProvideClientCertificatesCallback callback)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone.ProvideClientCertificatesCallback = callback;

            return clone;
        }

        /// <summary>
        /// Returns a copy of the current instance with the specified <see cref="RemoteCertificateValidationCallback"/>.
        /// </summary>
        /// <param name="callback">The specified callback.</param>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithRemoteCertificateValidationCallback([CanBeNull] RemoteCertificateValidationCallback callback)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone.RemoteCertificateValidationCallback = callback;

            return clone;
        }

        /// <summary>
        /// Returns a copy of the current instance with the specified <see cref="ProvidePasswordCallback"/>.
        /// </summary>
        /// <param name="callback">The specified callback.</param>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithProvidePasswordCallback([CanBeNull] ProvidePasswordCallback callback)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone.ProvidePasswordCallback = callback;

            return clone;
        }

        #endregion Authentication

        #region Infrastructure

        /// <inheritdoc />
        [NotNull]
        protected override RelationalOptionsExtension Clone() => new NpgsqlOptionsExtension(this);

        /// <inheritdoc />
        public override void ApplyServices(IServiceCollection services)
            => services.AddEntityFrameworkNpgsql();

        /// <inheritdoc />
        public override DbContextOptionsExtensionInfo Info
            => _info ??= new ExtensionInfo(this);

        sealed class ExtensionInfo : RelationalExtensionInfo
        {
            long? _serviceProviderHash;
            string _logFragment;

            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            new NpgsqlOptionsExtension Extension => (NpgsqlOptionsExtension)base.Extension;

            public override bool IsDatabaseProvider => true;

            [NotNull]
            public override string LogFragment
            {
                get
                {
                    if (_logFragment != null)
                        return _logFragment;

                    var builder = new StringBuilder(base.LogFragment);

                    if (Extension.AdminDatabase != null)
                    {
                        builder.Append(nameof(Extension.AdminDatabase)).Append("=").Append(Extension.AdminDatabase).Append(' ');
                    }

                    if (Extension.PostgresVersion != null)
                    {
                        builder.Append(nameof(Extension.PostgresVersion)).Append("=").Append(Extension.PostgresVersion).Append(' ');
                    }

                    if (Extension.ProvideClientCertificatesCallback != null)
                    {
                        builder.Append(nameof(Extension.ProvideClientCertificatesCallback)).Append(" ");
                    }

                    if (Extension.RemoteCertificateValidationCallback != null)
                    {
                        builder.Append(nameof(Extension.RemoteCertificateValidationCallback)).Append(" ");
                    }

                    if (Extension.ProvidePasswordCallback != null)
                    {
                        builder.Append(nameof(Extension.ProvidePasswordCallback)).Append(" ");
                    }

                    if (Extension.ReverseNullOrdering)
                    {
                        builder.Append(nameof(Extension.ReverseNullOrdering)).Append(" ");;
                    }

                    if (Extension.UserRangeDefinitions.Count > 0)
                    {
                        builder.Append(nameof(Extension.UserRangeDefinitions)).Append("=[");
                        foreach (var item in Extension.UserRangeDefinitions)
                        {
                            builder.Append(item.SubtypeClrType).Append("=>");

                            if (item.SchemaName != null)
                                builder.Append(item.SchemaName).Append(".");

                            builder.Append(item.RangeName);

                            if (item.SubtypeName != null)
                                builder.Append("(").Append(item.SubtypeName).Append(")");

                            builder.Append(";");
                        }

                        builder.Length = builder.Length -1;
                        builder.Append("] ");
                    }

                    return _logFragment = builder.ToString();
                }
            }

            public override long GetServiceProviderHashCode()
            {
                unchecked
                {
                    if (_serviceProviderHash == null)
                    {
                        _serviceProviderHash = Extension._userRangeDefinitions.Aggregate(
                            base.GetServiceProviderHashCode(),
                            (h, ud) => (h * 397) ^ ud.GetHashCode());
                        _serviceProviderHash = (_serviceProviderHash * 397) ^ Extension.AdminDatabase?.GetHashCode() ?? 0L;
                        _serviceProviderHash = (_serviceProviderHash * 397) ^ (Extension.PostgresVersion?.GetHashCode() ?? 0L);
                        _serviceProviderHash = (_serviceProviderHash * 397) ^ (Extension.ProvideClientCertificatesCallback?.GetHashCode() ?? 0L);
                        _serviceProviderHash = (_serviceProviderHash * 397) ^ (Extension.RemoteCertificateValidationCallback?.GetHashCode() ?? 0L);
                        _serviceProviderHash = (_serviceProviderHash * 397) ^ (Extension.ProvidePasswordCallback?.GetHashCode() ?? 0L);
                        _serviceProviderHash = (_serviceProviderHash * 397) ^ Extension.ReverseNullOrdering.GetHashCode();
                    }

                    return _serviceProviderHash.Value;
                }
            }

            /// <inheritdoc />
            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.UseAdminDatabase)]
                    = (Extension.AdminDatabase?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

                debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.SetPostgresVersion)]
                    = (Extension.PostgresVersion?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

                debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.ReverseNullOrdering)]
                    = Extension.ReverseNullOrdering.GetHashCode().ToString(CultureInfo.InvariantCulture);

                debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.RemoteCertificateValidationCallback)]
                    = (Extension.RemoteCertificateValidationCallback?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

                debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.ProvideClientCertificatesCallback)]
                    = (Extension.ProvideClientCertificatesCallback?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

                debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.ProvidePasswordCallback)]
                    = (Extension.ProvidePasswordCallback?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

                foreach (var rangeDefinition in Extension._userRangeDefinitions)
                {
                    debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.MapRange) + ":" + rangeDefinition.SubtypeClrType.Name]
                        = rangeDefinition.GetHashCode().ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        #endregion Infrastructure
    }

    public class UserRangeDefinition : IEquatable<UserRangeDefinition>
    {
        /// <summary>
        /// The name of the PostgreSQL range type to be mapped.
        /// </summary>
        [NotNull]
        public virtual string RangeName { get; }

        /// <summary>
        /// The PostgreSQL schema in which the range is defined. If null, the default schema is used
        /// (which is public unless changed on the model).
        /// </summary>
        [CanBeNull]
        public virtual string SchemaName { get; }

        /// <summary>
        /// The CLR type of the range's subtype (or element).
        /// The actual mapped type will be an <see cref="NpgsqlRange{T}"/> over this type.
        /// </summary>
        [NotNull]
        public virtual Type SubtypeClrType { get; }

        /// <summary>
        /// Optionally, the name of the range's PostgreSQL subtype (or element).
        /// This is usually not needed - the subtype will be inferred based on <see cref="SubtypeClrType"/>.
        /// </summary>
        [CanBeNull]
        public virtual string SubtypeName { get; }

        public UserRangeDefinition(
            [NotNull] string rangeName,
            [CanBeNull] string schemaName,
            [NotNull] Type subtypeClrType,
            [CanBeNull] string subtypeName)
        {
            RangeName = Check.NotEmpty(rangeName, nameof(rangeName));
            SchemaName = schemaName;
            SubtypeClrType = Check.NotNull(subtypeClrType, nameof(subtypeClrType));
            SubtypeName = subtypeName;
        }

        public override int GetHashCode()
            => HashCode.Combine(RangeName, SchemaName, SubtypeClrType, SubtypeName);

        public override bool Equals(object obj) => obj is UserRangeDefinition urd && Equals(urd);

        public virtual bool Equals(UserRangeDefinition other)
            => ReferenceEquals(this, other) ||
               !(other is null) &&
               RangeName == other.RangeName &&
               SchemaName == other.SchemaName &&
               SubtypeClrType == other.SubtypeClrType &&
               SubtypeName == other.SubtypeName;

        public virtual void Deconstruct(
            [NotNull] out string rangeName,
            [CanBeNull] out string schemaName,
            [NotNull] out Type subtypeClrType,
            [CanBeNull] out string subtypeName)
        {
            rangeName = RangeName;
            schemaName = SchemaName;
            subtypeClrType = SubtypeClrType;
            subtypeName = SubtypeName;
        }
    }
}
