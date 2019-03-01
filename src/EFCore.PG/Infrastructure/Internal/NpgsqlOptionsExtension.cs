using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Security;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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
        [NotNull] readonly List<UserRangeDefinition> _userRangeDefinitions;
        string _logFragment;

        /// <summary>
        /// The name of the database for administrative operations.
        /// </summary>
        [CanBeNull]
        public string AdminDatabase { get; private set; }

        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull]
        public Version PostgresVersion { get; private set; }

        /// <summary>
        /// The list of range mappings specified by the user.
        /// </summary>
        [NotNull]
        public IReadOnlyList<UserRangeDefinition> UserRangeDefinitions => _userRangeDefinitions;

        /// <summary>
        /// The specified <see cref="ProvideClientCertificatesCallback"/>.
        /// </summary>
        [CanBeNull]
        public ProvideClientCertificatesCallback ProvideClientCertificatesCallback { get; private set; }

        /// <summary>
        /// The specified <see cref="RemoteCertificateValidationCallback"/>.
        /// </summary>
        [CanBeNull]
        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; private set; }

        /// <summary>
        /// True if reverse null ordering is enabled; otherwise, false.
        /// </summary>
        public bool ReverseNullOrdering { get; private set; }

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
            ReverseNullOrdering = copyFrom.ReverseNullOrdering;
        }

        /// <inheritdoc />
        [NotNull]
        protected override RelationalOptionsExtension Clone() => new NpgsqlOptionsExtension(this);

        /// <inheritdoc />
        public override bool ApplyServices(IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            services.AddEntityFrameworkNpgsql();

            return true;
        }

        // The following is a hack to set the default minimum batch size to 2 in Npgsql
        // See https://github.com/aspnet/EntityFrameworkCore/pull/10091
        public override int? MinBatchSize => base.MinBatchSize ?? 2;

        /// <inheritdoc />
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.UseAdminDatabase)]
                = (AdminDatabase?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

            debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.SetPostgresVersion)]
                = (PostgresVersion?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

            debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.ReverseNullOrdering)]
                = ReverseNullOrdering.GetHashCode().ToString(CultureInfo.InvariantCulture);

            debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.RemoteCertificateValidationCallback)]
                = (RemoteCertificateValidationCallback?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

            debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.ProvideClientCertificatesCallback)]
                = (ProvideClientCertificatesCallback?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

            foreach (var rangeDefinition in _userRangeDefinitions)
            {
                debugInfo["Npgsql.EntityFrameworkCore.PostgreSQL:" + nameof(NpgsqlDbContextOptionsBuilder.MapRange) + ":" + rangeDefinition.SubtypeClrType.DisplayName()]
                    = rangeDefinition.GetHashCode().ToString(CultureInfo.InvariantCulture);
            }
        }

        [NotNull]
        public override string LogFragment
        {
            get
            {
                if (_logFragment != null)
                    return _logFragment;

                var builder = new StringBuilder(base.LogFragment);

                if (AdminDatabase != null)
                {
                    builder.Append("AdminDatabase=").Append(AdminDatabase).Append(' ');
                }

                if (PostgresVersion != null)
                {
                    builder.Append("PostgresVersion=").Append(PostgresVersion).Append(' ');
                }

                if (ProvideClientCertificatesCallback != null)
                {
                    builder.Append("ProvideClientCertificatesCallback ");
                }

                if (RemoteCertificateValidationCallback != null)
                {
                    builder.Append("RemoteCertificateValidationCallback ");
                }

                if (ReverseNullOrdering)
                {
                    builder.Append("ReverseNullOrdering ");
                }

                if (UserRangeDefinitions.Count > 0)
                {
                    builder.Append("UserRangeDefinitions=[");
                    foreach (var item in UserRangeDefinitions)
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

        #endregion Authentication
    }

    public class UserRangeDefinition
    {
        /// <summary>
        /// The name of the PostgreSQL range type to be mapped.
        /// </summary>
        [NotNull]
        public string RangeName { get; }

        /// <summary>
        /// The PostgreSQL schema in which the range is defined. If null, the default schema is used
        /// (which is public unless changed on the model).
        /// </summary>
        [CanBeNull]
        public string SchemaName { get; }

        /// <summary>
        /// The CLR type of the range's subtype (or element).
        /// The actual mapped type will be an <see cref="NpgsqlRange{T}"/> over this type.
        /// </summary>
        [NotNull]
        public Type SubtypeClrType { get; }

        /// <summary>
        /// Optionally, the name of the range's PostgreSQL subtype (or element).
        /// This is usually not needed - the subtype will be inferred based on <see cref="SubtypeClrType"/>.
        /// </summary>
        [CanBeNull]
        public string SubtypeName { get; }

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

        public void Deconstruct(
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
