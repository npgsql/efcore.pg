#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using System.Collections.Generic;
using System.Net.Security;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal
{
    public class NpgsqlOptionsExtension : RelationalOptionsExtension
    {
        readonly List<NpgsqlEntityFrameworkPlugin> _plugins;

        public string AdminDatabase { get; private set; }
        public bool ReverseNullOrdering { get; private set; }

        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull]
        public Version PostgresVersion { get; private set; }

        /// <summary>
        /// The collection of database plugins.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<NpgsqlEntityFrameworkPlugin> Plugins => _plugins;

        /// <summary>
        /// The specified <see cref="ProvideClientCertificatesCallback"/>.
        /// </summary>
        [CanBeNull]
        public ProvideClientCertificatesCallback ProvideClientCertificatesCallback { get; private set; }

        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="NpgsqlOptionsExtension"/> with the default settings.
        /// </summary>
        public NpgsqlOptionsExtension() => _plugins = new List<NpgsqlEntityFrameworkPlugin>();

        // NB: When adding new options, make sure to update the copy ctor below.

        public NpgsqlOptionsExtension([NotNull] NpgsqlOptionsExtension copyFrom)
            : base(copyFrom)
        {
            AdminDatabase = copyFrom.AdminDatabase;
            ReverseNullOrdering = copyFrom.ReverseNullOrdering;
            PostgresVersion = copyFrom.PostgresVersion;
            ProvideClientCertificatesCallback = copyFrom.ProvideClientCertificatesCallback;
            RemoteCertificateValidationCallback = copyFrom.RemoteCertificateValidationCallback;
            _plugins = new List<NpgsqlEntityFrameworkPlugin>(copyFrom._plugins);
        }

        protected override RelationalOptionsExtension Clone() => new NpgsqlOptionsExtension(this);

        public override bool ApplyServices(IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            services.AddEntityFrameworkNpgsql();

            return true;
        }

        public virtual NpgsqlOptionsExtension WithPlugin(NpgsqlEntityFrameworkPlugin plugin)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone._plugins.Add(plugin);

            return clone;
        }

        public virtual NpgsqlOptionsExtension WithAdminDatabase(string adminDatabase)
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

        public virtual NpgsqlOptionsExtension WithProvideClientCertificatesCallback(ProvideClientCertificatesCallback callback)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone.ProvideClientCertificatesCallback = callback;

            return clone;
        }

        public virtual NpgsqlOptionsExtension WithRemoteCertificateValidationCallback(RemoteCertificateValidationCallback callback)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone.RemoteCertificateValidationCallback = callback;

            return clone;
        }

        #endregion Authentication
    }
}
