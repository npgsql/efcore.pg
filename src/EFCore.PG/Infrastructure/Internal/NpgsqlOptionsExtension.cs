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

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal
{
    /// <summary>
    /// Represents options managed by the Npgsql.
    /// </summary>
    public class NpgsqlOptionsExtension : RelationalOptionsExtension
    {
        /// <summary>
        /// The collection of database plugins.
        /// </summary>
        [NotNull] readonly List<NpgsqlEntityFrameworkPlugin> _plugins;

        /// <summary>
        /// The name of the database for administrative operations.
        /// </summary>
        [CanBeNull]
        public string AdminDatabase { get; private set; }

        /// <summary>
        /// The backend process to target.
        /// </summary>
        public Backend Backend { get; private set; }

        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull]
        public Version Compatibility { get; private set; }

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
            => _plugins = new List<NpgsqlEntityFrameworkPlugin>();

        // NB: When adding new options, make sure to update the copy ctor below.
        /// <summary>
        /// Initializes an instance of <see cref="NpgsqlOptionsExtension"/> by copying the specified instance.
        /// </summary>
        /// <param name="copyFrom">The instance to copy.</param>
        public NpgsqlOptionsExtension([NotNull] NpgsqlOptionsExtension copyFrom) : base(copyFrom)
        {
            AdminDatabase = copyFrom.AdminDatabase;
            Backend = copyFrom.Backend;
            Compatibility = copyFrom.Compatibility;
            _plugins = new List<NpgsqlEntityFrameworkPlugin>(copyFrom._plugins);
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

        /// <summary>
        /// Returns a copy of the current instance configured to use the specified <see cref="NpgsqlEntityFrameworkPlugin"/>.
        /// </summary>
        /// <param name="plugin">The plugin to configure.</param>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithPlugin([NotNull] NpgsqlEntityFrameworkPlugin plugin)
        {
            Check.NotNull(plugin, nameof(plugin));

            var clone = (NpgsqlOptionsExtension)Clone();

            clone._plugins.Add(plugin);

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
        /// Returns a copy of the current instance configured to tartget the specified backend process.
        /// </summary>
        /// <param name="backend">The version of PostgreSQL to target.</param>
        /// <returns>
        /// A copy of the current instance configured to tartget the specified backend process.
        /// </returns>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithBackend(Backend backend)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone.Backend = backend;

            return clone;
        }

        /// <summary>
        /// Returns a copy of the current instance with the specified compatibility version.
        /// </summary>
        /// <param name="compatibility">The backend version to target.</param>
        /// <returns>
        /// A copy of the current instance with the specified compatibility version.
        /// </returns>
        [NotNull]
        public virtual NpgsqlOptionsExtension WithCompatiblity([CanBeNull] Version compatibility)
        {
            var clone = (NpgsqlOptionsExtension)Clone();

            clone.Compatibility = compatibility;

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
}
