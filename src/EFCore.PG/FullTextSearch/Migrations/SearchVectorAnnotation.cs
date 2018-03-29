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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations
{
    public class SearchVectorAnnotation
    {
        public string Name { get; set; }
        public TextSearchRegconfig Config { get; set; }
        public KeyedCollection<char, SearchVectorComponentGroup> ComponentGroupsByLabel { get; set; }

        public SearchVectorAnnotation()
        {
            ComponentGroupsByLabel = new SearchVectorComponentGroup.KeyedCollection();
        }

        public string Serialize()
        {
            var builder = new StringBuilder();
            builder.Append(Name);
            builder.Append(':');
            builder.Append(Config.Name);
            builder.Append(':');
            builder.Append(Config.IsPropertyOrColumnName);

            foreach (var group in ComponentGroupsByLabel)
            {
                builder.Append(':');
                builder.Append(group.Label);
                builder.Append('-');

                foreach (var component in group.Components)
                {
                    builder.Append(component.Name);
                    builder.Append('!');
                    builder.Append(component.DefaultSqlValue);
                    builder.Append(',');
                }

                builder.Length--;
            }

            return builder.ToString();
        }

        public static SearchVectorAnnotation Deserialize([NotNull] string serialized)
        {
            if (string.IsNullOrWhiteSpace(serialized))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serialized));
            }

            var annotationParts = serialized.Split(new[] { ':' }, 5);
            if (annotationParts.Length != 4)
            {
                throw new ArgumentException("Invalid serialized value: " + serialized);
            }

            var searchVectorProperty = new SearchVectorAnnotation
            {
                Name = annotationParts[0],
                Config = new TextSearchRegconfig(annotationParts[1], annotationParts[2] == true.ToString()),
            };

            var componentGroups = annotationParts[3].Split(':').Select(
                x =>
                {
                    var componentGroupParts = x.Split(new[] { '-' }, 2);
                    if (componentGroupParts.Length != 2)
                    {
                        throw new ArgumentException("Invalid serialized value: " + serialized);
                    }

                    var components = componentGroupParts[1].Split(',')
                        .Select(
                            s =>
                            {
                                var componentParts = s.Split(new[] { '!' }, 2);
                                if (componentParts.Length != 2)
                                {
                                    throw new ArgumentException("Invalid serialized value: " + serialized);
                                }

                                return new SearchVectorComponent(componentParts[0], componentParts[1]);
                            });

                    var componentGroup = new SearchVectorComponentGroup(componentGroupParts[0][0]);
                    foreach (var component in components)
                    {
                        componentGroup.Components.Add(component);
                    }

                    return componentGroup;
                });

            foreach (var group in componentGroups)
            {
                searchVectorProperty.ComponentGroupsByLabel.Add(group);
            }

            return searchVectorProperty;
        }
    }

    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Used by extension methods.")]
    public class SearchVectorAnnotation<TEntity> : SearchVectorAnnotation where TEntity : class { }

    public static class SearchVectorPropertyExtensions
    {
        public static SearchVectorAnnotation<TEntity> Add<TEntity>(
            this SearchVectorAnnotation<TEntity> searchVector,
            Expression<Func<TEntity, string>> propertyExpression) where TEntity : class =>
            searchVector.Add(propertyExpression, NpgsqlFullTextSearchLabel.Default);

        public static SearchVectorAnnotation<TEntity> Add<TEntity>(
            this SearchVectorAnnotation<TEntity> searchVector,
            Expression<Func<TEntity, string>> propertyExpression,
            NpgsqlFullTextSearchLabel label) where TEntity : class =>
            searchVector.Add(propertyExpression.GetPropertyAccess().Name, label);

        public static TSearchVectorProperty Add<TSearchVectorProperty>(
            this TSearchVectorProperty searchVector,
            string propertyName) where TSearchVectorProperty : SearchVectorAnnotation =>
            searchVector.Add(propertyName, NpgsqlFullTextSearchLabel.Default);

        public static TSearchVectorProperty Add<TSearchVectorProperty>(
            this TSearchVectorProperty searchVector,
            string propertyName,
            NpgsqlFullTextSearchLabel label) where TSearchVectorProperty : SearchVectorAnnotation
        {
            var searchVectorComponentGroup = searchVector.ComponentGroupsByLabel[label];
            if (searchVectorComponentGroup == null)
            {
                searchVectorComponentGroup = new SearchVectorComponentGroup(label);
                searchVector.ComponentGroupsByLabel.Add(searchVectorComponentGroup);
            }

            searchVectorComponentGroup.Components.Add(new SearchVectorComponent(propertyName, string.Empty));
            return searchVector;
        }
    }
}
