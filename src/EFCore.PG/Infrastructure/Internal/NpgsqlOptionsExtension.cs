using System;
using System.Collections.Generic;
using System.Net.Security;
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
        [NotNull] readonly List<RangeMappingInfo> _rangeMappings;

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
        public IReadOnlyList<RangeMappingInfo> RangeMappings => _rangeMappings;

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
            => _rangeMappings = new List<RangeMappingInfo>();

        // NB: When adding new options, make sure to update the copy ctor below.
        /// <summary>
        /// Initializes an instance of <see cref="NpgsqlOptionsExtension"/> by copying the specified instance.
        /// </summary>
        /// <param name="copyFrom">The instance to copy.</param>
        public NpgsqlOptionsExtension([NotNull] NpgsqlOptionsExtension copyFrom) : base(copyFrom)
        {
            AdminDatabase = copyFrom.AdminDatabase;
            _rangeMappings = new List<RangeMappingInfo>(copyFrom._rangeMappings);
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

        /// <summary>
        /// Returns a copy of the current instance configured with the specified range mapping.
        /// </summary>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithRangeMapping<TSubtype>(string rangeName, string subtypeName)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone._rangeMappings.Add(new RangeMappingInfo(rangeName, typeof(TSubtype), subtypeName));

            return clone;
        }

        /// <summary>
        /// Returns a copy of the current instance configured with the specified range mapping.
        /// </summary>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithRangeMapping(string rangeName, Type subtypeClrType, string subtypeName)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone._rangeMappings.Add(new RangeMappingInfo(rangeName, subtypeClrType, subtypeName));

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

    public class RangeMappingInfo
    {
        /// <summary>The name of the PostgreSQL range type to be mapped.</summary>
        public string RangeName { get; }
        /// <summary>
        /// The CLR type of the range's subtype (or element).
        /// The actual mapped type will be an <see cref="NpgsqlRange{T}"/> over this type.
        /// </summary>
        public Type SubtypeClrType { get; }
        /// <summary>
        /// Optionally, the name of the range's PostgreSQL subtype (or element).
        /// This is usually not needed - the subtype will be inferred based on <see cref="SubtypeClrType"/>.
        /// </summary>
        public string SubtypeName { get; }

        public RangeMappingInfo(string rangeName, Type subtypeClrType, string subtypeName)
        {
            RangeName = rangeName;
            SubtypeClrType = subtypeClrType;
            SubtypeName = subtypeName;
        }

        public void Deconstruct(out string rangeName, out Type subtypeClrType, out string subtypeName)
        {
            rangeName = RangeName;
            subtypeClrType = SubtypeClrType;
            subtypeName = SubtypeName;
        }
    }
}
