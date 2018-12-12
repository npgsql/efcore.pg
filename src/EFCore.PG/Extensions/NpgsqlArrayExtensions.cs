using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

// ReSharper disable UnusedParameter.Global
// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods for <see cref="List{T}"/> supporting PostgreSQL translation.
    /// </summary>
    public static class NpgsqlArrayExtensions
    {
        #region @>

        /// <summary>
        /// Tests whether the first collection contains the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection contains the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Contains<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] first,
            [CanBeNull] [ItemCanBeNull] T[] second)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Tests whether the first collection contains the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection contains the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Contains<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] first,
            [CanBeNull] [ItemCanBeNull] List<T> second)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Tests whether the first collection contains the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection contains the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Contains<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> first,
            [CanBeNull] [ItemCanBeNull] T[] second)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Tests whether the first collection contains the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection contains the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Contains<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> first,
            [CanBeNull] [ItemCanBeNull] List<T> second)
            => throw ClientEvaluationNotSupportedException();

        #endregion

        #region <@

        /// <summary>
        /// Tests whether the first collection is contained by the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection is contained by the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainedBy<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] first,
            [CanBeNull] [ItemCanBeNull] T[] second)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Tests whether the first collection is contained by the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection is contained by the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainedBy<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] first,
            [CanBeNull] [ItemCanBeNull] List<T> second)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Tests whether the first collection is contained by the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection is contained by the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainedBy<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> first,
            [CanBeNull] [ItemCanBeNull] T[] second)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Tests whether the first collection is contained by the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection is contained by the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainedBy<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> first,
            [CanBeNull] [ItemCanBeNull] List<T> second)
            => throw ClientEvaluationNotSupportedException();

        #endregion

        #region &&

        /// <summary>
        /// Tests whether the first collection overlaps the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection overlaps the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Overlaps<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] first,
            [CanBeNull] [ItemCanBeNull] T[] second)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Tests whether the first collection overlaps the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection overlaps the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Overlaps<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] first,
            [CanBeNull] [ItemCanBeNull] List<T> second)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Tests whether the first collection overlaps the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection overlaps the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Overlaps<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> first,
            [CanBeNull] [ItemCanBeNull] T[] second)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Tests whether the first collection overlaps with the second collection.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// True if the first collection overlaps the second collection; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Overlaps<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> first,
            [CanBeNull] [ItemCanBeNull] List<T> second)
            => throw ClientEvaluationNotSupportedException();

        #endregion

        #region array_fill

        /// <summary>
        /// Initializes an array with the given
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="value">The value with which to initialize each element.</param>
        /// <param name="dimensions">The dimensions of the array.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// An array initialized with the given <paramref name="value"/> and <paramref name="dimensions"/>.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [NotNull]
        public static T[] ArrayFill<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] T value,
            [CanBeNull] [ItemNotNull] T[] dimensions)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Initializes an array with the given
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="value">The value with which to initialize each element.</param>
        /// <param name="dimensions">The dimensions of the array.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// An array initialized with the given <paramref name="value"/> and <paramref name="dimensions"/>.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [NotNull]
        public static List<T> ListFill<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] T value,
            [CanBeNull] [ItemNotNull] T[] dimensions)
            => throw ClientEvaluationNotSupportedException();

        #endregion

        #region array_dims

        /// <summary>
        /// Returns a text representation of the array dimensions.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="array">The array.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// A text representation of the array dimensions.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [NotNull]
        public static string ArrayDimensions<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] T[] array)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Returns a text representation of the list dimensions.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="list">The list.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// A text representation of the list dimensions.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [NotNull]
        public static string ArrayDimensions<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] List<T> list)
            => throw ClientEvaluationNotSupportedException();

        #endregion

        #region array_positions

        /// <summary>
        /// Finds the positions at which the <paramref name="value"/> appears in the <paramref name="array"/>.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="array">The array to search.</param>
        /// <param name="value">The value to locate.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// The positions at which the <paramref name="value"/> appears in the <paramref name="array"/>.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [NotNull]
        public static int[] ArrayPositions<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] array,
            [CanBeNull] T value)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Finds the positions at which the <paramref name="value"/> appears in the <paramref name="list"/>.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="list">The list to search.</param>
        /// <param name="value">The value to locate.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// The positions at which the <paramref name="value"/> appears in the <paramref name="list"/>.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [NotNull]
        public static int[] ArrayPositions<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> list,
            [CanBeNull] T value)
            => throw ClientEvaluationNotSupportedException();

        #endregion

        #region array_remove

        /// <summary>
        /// Removes all elements equal to the given value from the array.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="array">The array to search.</param>
        /// <param name="value">The value to locate.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// An array where all elements are not equal to the given value.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static T[] ArrayRemove<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] array,
            [CanBeNull] T value)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Removes all elements equal to the given value from the <see cref="List{T}"/>.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="list">The list to search.</param>
        /// <param name="value">The value to locate.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// A <see cref="List{T}"/> where all elements are not equal to the given value.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static List<T> ArrayRemove<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> list,
            [CanBeNull] T value)
            => throw ClientEvaluationNotSupportedException();

        #endregion

        #region array_replace

        /// <summary>
        /// Replaces each element equal to the given value with a new value.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="array">The array to search.</param>
        /// <param name="current">The value to replace.</param>
        /// <param name="replacement">The new value.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>
        /// An array where each element equal to the given value is replaced with a new value.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static T[] ArrayReplace<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] array,
            [CanBeNull] T current,
            [CanBeNull] T replacement)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Replaces each element equal to the given value with a new value.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="list">The list to search.</param>
        /// <param name="current">The value to replace.</param>
        /// <param name="replacement">The new value.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// An<see cref="List{T}"/> where each element equal to the given value is replaced with a new value.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static List<T> ArrayReplace<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> list,
            [CanBeNull] T current,
            [CanBeNull] T replacement)
            => throw ClientEvaluationNotSupportedException();

        #endregion

        #region array_to_string

        /// <summary>
        /// Concatenates elements using the supplied delimiter.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="array">The list to convert to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="array"/>.</typeparam>
        /// <returns>
        /// The string concatenation of the elements with the supplied delimiter.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static string ArrayToString<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] array,
            [CanBeNull] string delimiter)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Concatenates elements using the supplied delimiter.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="list">The list to convert to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="list"/>.</typeparam>
        /// <returns>
        /// The string concatenation of the elements with the supplied delimiter.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static string ArrayToString<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> list,
            [CanBeNull] string delimiter)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Concatenates elements using the supplied delimiter and the string representation for null elements.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="array">The list to convert to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <param name="nullString">The value used to represent a null value.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="array"/>.</typeparam>
        /// <returns>
        /// The string concatenation of the elements with the supplied delimiter and null string.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static string ArrayToString<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] T[] array,
            [CanBeNull] string delimiter,
            [CanBeNull] string nullString)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Concatenates elements using the supplied delimiter and the string representation for null elements.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="list">The list to convert to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <param name="nullString">The value used to represent a null value.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="list"/>.</typeparam>
        /// <returns>
        /// The string concatenation of the elements with the supplied delimiter and null string.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static string ArrayToString<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] [ItemCanBeNull] List<T> list,
            [CanBeNull] string delimiter,
            [CanBeNull] string nullString)
            => throw ClientEvaluationNotSupportedException();

        #endregion

        #region string_to_array

        /// <summary>
        /// Converts the input string into an array using the given delimiter.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="input">The input string of delimited values.</param>
        /// <param name="delimiter">The value that delimits the elements.</param>
        /// <typeparam name="T">The type of the elements in the resulting array.</typeparam>
        /// <returns>
        /// The array resulting from splitting the input string based on the supplied delimiter.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static T[] StringToArray<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string input,
            [CanBeNull] string delimiter)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Converts the input string into an array using the given delimiter and null string.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="input">The input string of delimited values.</param>
        /// <param name="delimiter">The value that delimits the elements.</param>
        /// <param name="nullString">The value that represents a null value.</param>
        /// <typeparam name="T">The type of the elements in the resulting array.</typeparam>
        /// <returns>
        /// The array resulting from splitting the input string based on the supplied delimiter and null string.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static T[] StringToArray<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string input,
            [CanBeNull] string delimiter,
            [CanBeNull] string nullString)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Converts the input string into a <see cref="List{T}"/> using the given delimiter.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="input">The input string of delimited values.</param>
        /// <param name="delimiter">The value that delimits the elements.</param>
        /// <typeparam name="T">The type of the elements in the resulting array.</typeparam>
        /// <returns>
        /// The list resulting from splitting the input string based on the supplied delimiter.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static List<T> StringToList<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string input,
            [CanBeNull] string delimiter)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Converts the input string into a <see cref="List{T}"/> using the given delimiter and null string.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="input">The input string of delimited values.</param>
        /// <param name="delimiter">The value that delimits the elements.</param>
        /// <param name="nullString">The value that represents a null value.</param>
        /// <typeparam name="T">The type of the elements in the resulting array.</typeparam>
        /// <returns>
        /// The list resulting from splitting the input string based on the supplied delimiter and null string.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        [CanBeNull]
        public static List<T> StringToList<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string input,
            [CanBeNull] string delimiter,
            [CanBeNull] string nullString)
            => throw ClientEvaluationNotSupportedException();

        #endregion

        #region Utilities

        /// <summary>
        /// Helper method to throw a <see cref="NotSupportedException"/> with the name of the throwing method.
        /// </summary>
        /// <param name="method">The method that throws the exception.</param>
        /// <returns>
        /// A <see cref="NotSupportedException"/>.
        /// </returns>
        [NotNull]
        static NotSupportedException ClientEvaluationNotSupportedException([CallerMemberName] string method = default)
            => new NotSupportedException($"{method} is only intended for use via SQL translation as part of an EF Core LINQ query.");

        #endregion
    }
}
