/* MIT License

Copyright (c) 2016 JetBrains http://www.jetbrains.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */


// ReSharper disable InheritdocConsiderUsage

#pragma warning disable 1591
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InconsistentNaming

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked method builds string by the format pattern and (optional) arguments.
/// The parameter, which contains the format string, should be given in constructor. The format string
/// should be in <see cref="string.Format(IFormatProvider,string,object[])"/>-like form.
/// </summary>
/// <example><code>
/// [StringFormatMethod("message")]
/// void ShowError(string message, params object[] args) { /* do something */ }
///
/// void Foo() {
///   ShowError("Failed: {0}"); // Warning: Non-existing argument in format string
/// }
/// </code></example>
[AttributeUsage(
    AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate)]
internal sealed class StringFormatMethodAttribute : Attribute
{
    /// <param name="formatParameterName">
    /// Specifies which parameter of an annotated method should be treated as the format string
    /// </param>
    public StringFormatMethodAttribute(string formatParameterName)
    {
        FormatParameterName = formatParameterName;
    }

    public string FormatParameterName { get; }
}

/// <summary>
/// Use this annotation to specify a type that contains static or const fields
/// with values for the annotated property/field/parameter.
/// The specified type will be used to improve completion suggestions.
/// </summary>
/// <example><code>
/// namespace TestNamespace
/// {
///   public class Constants
///   {
///     public static int INT_CONST = 1;
///     public const string STRING_CONST = "1";
///   }
///
///   public class Class1
///   {
///     [ValueProvider("TestNamespace.Constants")] public int myField;
///     public void Foo([ValueProvider("TestNamespace.Constants")] string str) { }
///
///     public void Test()
///     {
///       Foo(/*try completion here*/);//
///       myField = /*try completion here*/
///     }
///   }
/// }
/// </code></example>
[AttributeUsage(
    AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = true)]
internal sealed class ValueProviderAttribute : Attribute
{
    public ValueProviderAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

/// <summary>
/// Indicates that the integral value falls into the specified interval.
/// It's allowed to specify multiple non-intersecting intervals.
/// Values of interval boundaries are inclusive.
/// </summary>
/// <example><code>
/// void Foo([ValueRange(0, 100)] int value) {
///   if (value == -1) { // Warning: Expression is always 'false'
///     ...
///   }
/// }
/// </code></example>
[AttributeUsage(
    AttributeTargets.Parameter
    | AttributeTargets.Field
    | AttributeTargets.Property
    | AttributeTargets.Method
    | AttributeTargets.Delegate,
    AllowMultiple = true)]
internal sealed class ValueRangeAttribute : Attribute
{
    public object From { get; }
    public object To { get; }

    public ValueRangeAttribute(long from, long to)
    {
        From = from;
        To = to;
    }

    public ValueRangeAttribute(ulong from, ulong to)
    {
        From = from;
        To = to;
    }

    public ValueRangeAttribute(long value)
    {
        From = To = value;
    }

    public ValueRangeAttribute(ulong value)
    {
        From = To = value;
    }
}

/// <summary>
/// Indicates that the integral value never falls below zero.
/// </summary>
/// <example><code>
/// void Foo([NonNegativeValue] int value) {
///   if (value == -1) { // Warning: Expression is always 'false'
///     ...
///   }
/// }
/// </code></example>
[AttributeUsage(
    AttributeTargets.Parameter
    | AttributeTargets.Field
    | AttributeTargets.Property
    | AttributeTargets.Method
    | AttributeTargets.Delegate)]
internal sealed class NonNegativeValueAttribute : Attribute
{
}

/// <summary>
/// Indicates that the function argument should be a string literal and match one
/// of the parameters of the caller function. For example, ReSharper annotates
/// the parameter of <see cref="System.ArgumentNullException"/>.
/// </summary>
/// <example><code>
/// void Foo(string param) {
///   if (param is null)
///     throw new ArgumentNullException("par"); // Warning: Cannot resolve symbol
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class InvokerParameterNameAttribute : Attribute
{
}

/// <summary>
/// Describes dependency between method input and output.
/// </summary>
/// <syntax>
/// <p>Function Definition Table syntax:</p>
/// <list>
/// <item>FDT      ::= FDTRow [;FDTRow]*</item>
/// <item>FDTRow   ::= Input =&gt; Output | Output &lt;= Input</item>
/// <item>Input    ::= ParameterName: Value [, Input]*</item>
/// <item>Output   ::= [ParameterName: Value]* {halt|stop|void|nothing|Value}</item>
/// <item>Value    ::= true | false | null | notnull | canbenull</item>
/// </list>
/// If the method has a single input parameter, its name could be omitted.<br/>
/// Using <c>halt</c> (or <c>void</c>/<c>nothing</c>, which is the same) for the method output
/// means that the method doesn't return normally (throws or terminates the process).<br/>
/// Value <c>canbenull</c> is only applicable for output parameters.<br/>
/// You can use multiple <c>[ContractAnnotation]</c> for each FDT row, or use single attribute
/// with rows separated by semicolon. There is no notion of order rows, all rows are checked
/// for applicability and applied per each program state tracked by the analysis engine.<br/>
/// </syntax>
/// <examples><list>
/// <item><code>
/// [ContractAnnotation("=&gt; halt")]
/// public void TerminationMethod()
/// </code></item>
/// <item><code>
/// [ContractAnnotation("null &lt;= param:null")] // reverse condition syntax
/// public string GetName(string surname)
/// </code></item>
/// <item><code>
/// [ContractAnnotation("s:null =&gt; true")]
/// public bool IsNullOrEmpty(string s) // string.IsNullOrEmpty()
/// </code></item>
/// <item><code>
/// // A method that returns null if the parameter is null,
/// // and not null if the parameter is not null
/// [ContractAnnotation("null =&gt; null; notnull =&gt; notnull")]
/// public object Transform(object data)
/// </code></item>
/// <item><code>
/// [ContractAnnotation("=&gt; true, result: notnull; =&gt; false, result: null")]
/// public bool TryParse(string s, out Person result)
/// </code></item>
/// </list></examples>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal sealed class ContractAnnotationAttribute : Attribute
{
    public ContractAnnotationAttribute(string contract)
        : this(contract, false)
    {
    }

    public ContractAnnotationAttribute(string contract, bool forceFullStates)
    {
        Contract = contract;
        ForceFullStates = forceFullStates;
    }

    public string Contract { get; }

    public bool ForceFullStates { get; }
}

/// <summary>
/// Indicates whether the marked element should be localized.
/// </summary>
/// <example><code>
/// [LocalizationRequiredAttribute(true)]
/// class Foo {
///   string str = "my string"; // Warning: Localizable string
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.All)]
internal sealed class LocalizationRequiredAttribute : Attribute
{
    public LocalizationRequiredAttribute()
        : this(true)
    {
    }

    public LocalizationRequiredAttribute(bool required)
    {
        Required = required;
    }

    public bool Required { get; }
}

/// <summary>
/// Indicates that the value of the marked type (or its derivatives)
/// cannot be compared using '==' or '!=' operators and <c>Equals()</c>
/// should be used instead. However, using '==' or '!=' for comparison
/// with <c>null</c> is always permitted.
/// </summary>
/// <example><code>
/// [CannotApplyEqualityOperator]
/// class NoEquality { }
///
/// class UsesNoEquality {
///   void Test() {
///     var ca1 = new NoEquality();
///     var ca2 = new NoEquality();
///     if (ca1 is not null) { // OK
///       bool condition = ca1 == ca2; // Warning
///     }
///   }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct)]
internal sealed class CannotApplyEqualityOperatorAttribute : Attribute
{
}

/// <summary>
/// When applied to a target attribute, specifies a requirement for any type marked
/// with the target attribute to implement or inherit specific type or types.
/// </summary>
/// <example><code>
/// [BaseTypeRequired(typeof(IComponent)] // Specify requirement
/// class ComponentAttribute : Attribute { }
///
/// [Component] // ComponentAttribute requires implementing IComponent interface
/// class MyComponent : IComponent { }
/// </code></example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[BaseTypeRequired(typeof(Attribute))]
internal sealed class BaseTypeRequiredAttribute : Attribute
{
    public BaseTypeRequiredAttribute(Type baseType)
    {
        BaseType = baseType;
    }

    public Type BaseType { get; }
}

/// <summary>
/// Indicates that the marked symbol is used implicitly (e.g. via reflection, in external library),
/// so this symbol will not be reported as unused (as well as by other usage inspections).
/// </summary>
[AttributeUsage(AttributeTargets.All)]
internal sealed class UsedImplicitlyAttribute : Attribute
{
    public UsedImplicitlyAttribute()
        : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default)
    {
    }

    public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags)
        : this(useKindFlags, ImplicitUseTargetFlags.Default)
    {
    }

    public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags)
        : this(ImplicitUseKindFlags.Default, targetFlags)
    {
    }

    public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
    {
        UseKindFlags = useKindFlags;
        TargetFlags = targetFlags;
    }

    public ImplicitUseKindFlags UseKindFlags { get; }

    public ImplicitUseTargetFlags TargetFlags { get; }
}

/// <summary>
/// Can be applied to attributes, type parameters, and parameters of a type assignable from <see cref="System.Type"/> .
/// When applied to an attribute, the decorated attribute behaves the same as <see cref="UsedImplicitlyAttribute"/>.
/// When applied to a type parameter or to a parameter of type <see cref="System.Type"/>,  indicates that the corresponding type
/// is used implicitly.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.GenericParameter | AttributeTargets.Parameter)]
internal sealed class MeansImplicitUseAttribute : Attribute
{
    public MeansImplicitUseAttribute()
        : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default)
    {
    }

    public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags)
        : this(useKindFlags, ImplicitUseTargetFlags.Default)
    {
    }

    public MeansImplicitUseAttribute(ImplicitUseTargetFlags targetFlags)
        : this(ImplicitUseKindFlags.Default, targetFlags)
    {
    }

    public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
    {
        UseKindFlags = useKindFlags;
        TargetFlags = targetFlags;
    }

    [UsedImplicitly] public ImplicitUseKindFlags UseKindFlags { get; }

    [UsedImplicitly] public ImplicitUseTargetFlags TargetFlags { get; }
}

/// <summary>
/// Specify the details of implicitly used symbol when it is marked
/// with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>.
/// </summary>
[Flags]
internal enum ImplicitUseKindFlags
{
    Default = Access | Assign | InstantiatedWithFixedConstructorSignature,

    /// <summary>Only entity marked with attribute considered used.</summary>
    Access = 1,

    /// <summary>Indicates implicit assignment to a member.</summary>
    Assign = 2,

    /// <summary>
    /// Indicates implicit instantiation of a type with fixed constructor signature.
    /// That means any unused constructor parameters won't be reported as such.
    /// </summary>
    InstantiatedWithFixedConstructorSignature = 4,

    /// <summary>Indicates implicit instantiation of a type.</summary>
    InstantiatedNoFixedConstructorSignature = 8,
}

/// <summary>
/// Specify what is considered to be used implicitly when marked
/// with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>.
/// </summary>
[Flags]
internal enum ImplicitUseTargetFlags
{
    Default = Itself,
    Itself = 1,

    /// <summary>Members of entity marked with attribute are considered used.</summary>
    Members = 2,

    /// <summary> Inherited entities are considered used. </summary>
    WithInheritors = 4,

    /// <summary>Entity marked with attribute and all its members considered used.</summary>
    WithMembers = Itself | Members
}

/// <summary>
/// Indicates that IEnumerable passed as a parameter is not enumerated.
/// Use this annotation to suppress the 'Possible multiple enumeration of IEnumerable' inspection.
/// </summary>
/// <example><code>
/// static void ThrowIfNull&lt;T&gt;([NoEnumeration] T v, string n) where T : class
/// {
///   // custom check for null but no enumeration
/// }
///
/// void Foo(IEnumerable&lt;string&gt; values)
/// {
///   ThrowIfNull(values, nameof(values));
///   var x = values.ToList(); // No warnings about multiple enumeration
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class NoEnumerationAttribute : Attribute
{
}

/// <summary>
/// Indicates that the marked parameter is a regular expression pattern.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class RegexPatternAttribute : Attribute
{
}