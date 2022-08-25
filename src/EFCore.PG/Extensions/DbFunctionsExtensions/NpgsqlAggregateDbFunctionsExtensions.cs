// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods supporting aggregate function translation for PostgreSQL.
/// </summary>
public static class NpgsqlAggregateDbFunctionsExtensions
{
    /// <summary>
    /// Collects all the input values, including nulls, into a PostgreSQL array.
    /// Corresponds to the PostgreSQL <c>array_agg</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input values to be aggregated into an array.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static T[] ArrayAgg<T>(this DbFunctions _, IEnumerable<T> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ArrayAgg)));

    /// <summary>
    /// Collects all the input values, including nulls, into a json array. Values are converted to JSON as per <c>to_json</c> or
    /// <c>to_jsonb</c>. Corresponds to the PostgreSQL <c>json_agg</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input values to be aggregated into a JSON array.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static T[] JsonAgg<T>(this DbFunctions _, IEnumerable<T> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonAgg)));

    /// <summary>
    /// Collects all the input values, including nulls, into a jsonb array. Values are converted to JSON as per <c>to_json</c> or
    /// <c>to_jsonb</c>. Corresponds to the PostgreSQL <c>jsonb_agg</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input values to be aggregated into a JSON array.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static T[] JsonbAgg<T>(this DbFunctions _, IEnumerable<T> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonbAgg)));

    /// <summary>
    /// Computes the sum of the non-null input intervals. Corresponds to the PostgreSQL <c>sum</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input values to be summed.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static TimeSpan? Sum(this DbFunctions _, IEnumerable<TimeSpan> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Sum)));

    /// <summary>
    /// Computes the average (arithmetic mean) of the non-null input intervals. Corresponds to the PostgreSQL <c>avg</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input values to be computed into an average.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static TimeSpan? Average(this DbFunctions _, IEnumerable<TimeSpan> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Average)));

    // See additional range aggregate functions in NpgsqlRangeDbfunctionsExtensions

    #region JsonObjectAgg

    /// <summary>
    /// Collects all the key/value pairs into a JSON object. Key arguments are coerced to text; value arguments are converted as per
    /// <c>to_json</c>. Values can be <see langword="null" />, but not keys.
    /// Corresponds to the PostgreSQL <c>json_object_agg</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="keyValuePairs">An enumerable of key-value pairs to be aggregated into a JSON object.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static string JsonObjectAgg<T1, T2>(this DbFunctions _, IEnumerable<(T1, T2)> keyValuePairs)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonObjectAgg)));

    /// <summary>
    /// Collects all the key/value pairs into a JSON object. Key arguments are coerced to text; value arguments are converted as per
    /// <c>to_json</c>. Values can be <see langword="null" />, but not keys.
    /// Corresponds to the PostgreSQL <c>json_object_agg</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="keyValuePairs">An enumerable of key-value pairs to be aggregated into a JSON object.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static TReturn JsonObjectAgg<T1, T2, TReturn>(this DbFunctions _, IEnumerable<(T1, T2)> keyValuePairs)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonObjectAgg)));

    /// <summary>
    /// Collects all the key/value pairs into a JSON object. Key arguments are coerced to text; value arguments are converted as per
    /// <c>to_jsonb</c>. Values can be <see langword="null" />, but not keys.
    /// Corresponds to the PostgreSQL <c>jsonb_object_agg</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="keyValuePairs">An enumerable of key-value pairs to be aggregated into a JSON object.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static string JsonbObjectAgg<T1, T2>(this DbFunctions _, IEnumerable<(T1, T2)> keyValuePairs)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonbObjectAgg)));

    /// <summary>
    /// Collects all the key/value pairs into a JSON object. Key arguments are coerced to text; value arguments are converted as per
    /// <c>to_jsonb</c>. Values can be <see langword="null" />, but not keys.
    /// Corresponds to the PostgreSQL <c>jsonb_object_agg</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="keyValuePairs">An enumerable of key-value pairs to be aggregated into a JSON object.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static TReturn JsonbObjectAgg<T1, T2, TReturn>(this DbFunctions _, IEnumerable<(T1, T2)> keyValuePairs)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonbObjectAgg)));

    #endregion JsonObjectAgg

    #region Sample standard deviation

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<byte> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<short> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<int> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<long> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<float> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<double> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<decimal> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    #endregion Sample standard deviation

    #region Population standard deviation

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<byte> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<short> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<int> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<long> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<float> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<double> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>stddev_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<decimal> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    #endregion Population standard deviation

    #region Sample variance

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<byte> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<short> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<int> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<long> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<float> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<double> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<decimal> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    #endregion Sample variance

    #region Population variance

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<byte> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<short> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<int> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<long> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<float> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<double> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to the PostgreSQL <c>var_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<decimal> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    #endregion Population variance

    #region Other statistics functions

    /// <summary>
    ///     Computes the correlation coefficient. Corresponds to the PostgreSQL <c>corr</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static double? Correlation(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Correlation)));

    /// <summary>
    ///     Computes the population covariance. Corresponds to the PostgreSQL <c>covar_pop</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static double? CovariancePopulation(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(CovariancePopulation)));

    /// <summary>
    ///     Computes the sample covariance. Corresponds to the PostgreSQL <c>covar_samp</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static double? CovarianceSample(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(CovarianceSample)));

    /// <summary>
    ///     Computes the average of the independent variable, <c>sum(X)/N</c>.
    ///     Corresponds to the PostgreSQL <c>regr_avgx</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static double? RegrAverageX(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RegrAverageX)));

    /// <summary>
    ///     Computes the average of the dependent variable, <c>sum(Y)/N</c>.
    ///     Corresponds to the PostgreSQL <c>regr_avgy</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static double? RegrAverageY(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RegrAverageY)));

    /// <summary>
    ///     Computes the number of rows in which both inputs are non-null.
    ///     Corresponds to the PostgreSQL <c>regr_count</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static long? RegrCount(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RegrCount)));

    /// <summary>
    ///     Computes the y-intercept of the least-squares-fit linear equation determined by the (X, Y) pairs.
    ///     Corresponds to the PostgreSQL <c>regr_intercept</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static double? RegrIntercept(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RegrIntercept)));

    /// <summary>
    ///     Computes the square of the correlation coefficient.
    ///     Corresponds to the PostgreSQL <c>regr_r2</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static double? RegrR2(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RegrR2)));

    /// <summary>
    ///     Computes the slope of the least-squares-fit linear equation determined by the (X, Y) pairs.
    ///     Corresponds to the PostgreSQL <c>regr_slope</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static double? RegrSlope(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RegrSlope)));

    /// <summary>
    ///     Computes the “sum of squares” of the independent variable, <c>sum(X^2) - sum(X)^2/N</c>.
    ///     Corresponds to the PostgreSQL <c>regr_sxx</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static double? RegrSXX(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RegrSXX)));

    /// <summary>
    ///     Computes the “sum of products” of independent times dependent variables, <c>sum(X*Y) - sum(X) * sum(Y)/N</c>.
    ///     Corresponds to the PostgreSQL <c>regr_sxy</c> function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    public static double? RegrSXY(this DbFunctions _, IEnumerable<(double, double)> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RegrSXY)));

    #endregion Other statistics functions
}
