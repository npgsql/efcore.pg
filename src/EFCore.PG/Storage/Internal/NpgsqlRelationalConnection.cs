using System.Data.Common;
using System.Net.Security;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlRelationalConnection : RelationalConnection, INpgsqlRelationalConnection
    {
        /// <summary>
        /// True if the connection settings have already been validated; otherwise, false.
        /// </summary>
        bool _validated;

        ProvideClientCertificatesCallback ProvideClientCertificatesCallback { get; }
        RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; }

        /// <summary>
        ///     Indicates whether the store connection supports ambient transactions
        /// </summary>
        protected override bool SupportsAmbientTransactions => true;

        public NpgsqlRelationalConnection([NotNull] RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
            var npgsqlOptions =
                dependencies.ContextOptions.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

            ProvideClientCertificatesCallback = npgsqlOptions.ProvideClientCertificatesCallback;
            RemoteCertificateValidationCallback = npgsqlOptions.RemoteCertificateValidationCallback;
        }

        protected override DbConnection CreateDbConnection()
        {
            var conn = new NpgsqlConnection(ConnectionString);
            if (ProvideClientCertificatesCallback != null)
                conn.ProvideClientCertificatesCallback = ProvideClientCertificatesCallback;
            if (RemoteCertificateValidationCallback != null)
                conn.UserCertificateValidationCallback = RemoteCertificateValidationCallback;

            if (_validated)
                return conn;

            if (conn.Settings.MaxAutoPrepare == 0)
                Dependencies.ConnectionLogger.AutoPrepareDisabledWarning(conn, ConnectionId);

            _validated = true;
            return conn;
        }

        public INpgsqlRelationalConnection CreateMasterConnection()
        {
            var adminDb = Dependencies.ContextOptions.FindExtension<NpgsqlOptionsExtension>()?.AdminDatabase
                          ?? "postgres";
            var csb = new NpgsqlConnectionStringBuilder(ConnectionString) {
                Database = adminDb,
                Pooling = false
            };
            var masterConn = ((NpgsqlConnection)DbConnection).CloneWith(csb.ToString());
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql(masterConn);

            return new NpgsqlRelationalConnection(Dependencies.With(optionsBuilder.Options));
        }
    }
}
