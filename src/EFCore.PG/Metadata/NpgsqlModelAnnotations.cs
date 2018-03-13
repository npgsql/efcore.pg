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

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class NpgsqlModelAnnotations : RelationalModelAnnotations, INpgsqlModelAnnotations
    {
        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

        public NpgsqlModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        public NpgsqlModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        #region HiLo

        public virtual string HiLoSequenceName
        {
            get => (string)Annotations.Metadata[NpgsqlAnnotationNames.HiLoSequenceName];
            [param: CanBeNull]
            set => SetHiLoSequenceName(value);
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceSchema
        {
            get => (string)Annotations.Metadata[NpgsqlAnnotationNames.HiLoSequenceSchema];
            [param: CanBeNull]
            set => SetHiLoSequenceSchema(value);
        }

        protected virtual bool SetHiLoSequenceSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(value, nameof(value)));

        #endregion

        #region Value Generation Strategy

        public virtual NpgsqlValueGenerationStrategy? ValueGenerationStrategy
        {
            get => (NpgsqlValueGenerationStrategy?)Annotations.Metadata[NpgsqlAnnotationNames.ValueGenerationStrategy];
            set => SetValueGenerationStrategy(value);
        }

        protected virtual bool SetValueGenerationStrategy(NpgsqlValueGenerationStrategy? value)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value);

        #endregion

        #region PostgreSQL Extensions

        public virtual PostgresExtension GetOrAddPostgresExtension([NotNull] string name)
            => PostgresExtension.GetOrAddPostgresExtension((IMutableModel)Model, name);

        public virtual IReadOnlyList<IPostgresExtension> PostgresExtensions
            => PostgresExtension.GetPostgresExtensions(Model).ToList();

        #endregion

        #region Database Template

        public virtual string DatabaseTemplate
        {
            get => (string)Annotations.Metadata[NpgsqlAnnotationNames.DatabaseTemplate];
            [param: CanBeNull]
            set => SetDatabaseTemplate(value);
        }

        protected virtual bool SetDatabaseTemplate([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.DatabaseTemplate,
                Check.NullButNotEmpty(value, nameof(value)));

        #endregion

        #region Tablespace

        public virtual string Tablespace
        {
            get => (string)Annotations.Metadata[NpgsqlAnnotationNames.Tablespace];
            [param: CanBeNull]
            set => SetTablespace(value);
        }

        protected virtual bool SetTablespace([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.Tablespace,
                Check.NullButNotEmpty(value, nameof(value)));

        #endregion
    }
}
