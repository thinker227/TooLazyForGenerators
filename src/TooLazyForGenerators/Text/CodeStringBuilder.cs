using System.Text;

namespace TooLazyForGenerators.Text;

/// <summary>
/// A string builder adapted for building strings of code. 
/// </summary>
public sealed class CodeStringBuilder
{
    private readonly StringBuilder builder;
    private int indentationLevel;

    /// <summary>
    /// The default indentation string.
    /// </summary>
    public const string DefaultIndentation = "    ";

    private bool EndsWithNewline =>
        builder.Length == 0 || builder.EndsWithNewline();
    
    /// <summary>
    /// The newline string used to append newlines.
    /// </summary>
    public string Newline { get; }
    
    /// <summary>
    /// The indentation string used to indent the start of every line.
    /// </summary>
    public string Indentation { get; }

    /// <summary>
    /// The current indentation level.
    /// </summary>
    public int IndentationLevel => indentationLevel;

    /// <summary>
    /// Initializes a new <see cref="CodeStringBuilder"/> instance.
    /// </summary>
    /// <param name="indentation">The indentation string to indent each line with.</param>
    /// <param name="newline">The string to use as the newline appended
    /// when using <see cref="AppendLine()"/> or <see cref="AppendLine(ReadOnlySpan{char})"/>.</param>
    public CodeStringBuilder(string indentation, string newline)
    {
        if (indentation.AsSpan().ContainsNewline()) throw new ArgumentException(
            "Indentation string cannot contain newlines.",
            nameof(indentation));

            if (string.IsNullOrEmpty(newline)) throw new ArgumentException(
            "Newline string cannot be null or empty.",
            nameof(newline));
        
        if (!newline.AsSpan().IsOnlyNewlines()) throw new ArgumentException(
            "Newline string can only consist of newline characters.",
            nameof(newline));
        
        builder = new();
        Indentation = indentation;
        Newline = newline;
        indentationLevel = 0;
    }
    
    /// <summary>
    /// Initializes a new <see cref="CodeStringBuilder"/> instance
    /// using <see cref="DefaultIndentation"/> for the indentation string
    /// and <see cref="Environment.NewLine"/> for the newline string..
    /// </summary>
    public CodeStringBuilder() : this(DefaultIndentation, Environment.NewLine) {}

    public override string ToString() =>
        builder.ToString();

    private void AppendIndentation()
    {
        for (var i = 0; i < indentationLevel; i++)
        {
            builder.Append(Indentation);
        }
    }

    /// <summary>
    /// Appends a newline to the builder.
    /// </summary>
    public CodeStringBuilder AppendLine()
    {
        builder.Append(Newline);
        return this;
    }

    /// <summary>
    /// Ensures that the builder currently ends with a newline, otherwise appends one.
    /// </summary>
    public CodeStringBuilder EnsureNewline()
    {
        if (!EndsWithNewline) AppendLine();
        return this;
    }

    /// <summary>
    /// Appends a string of text to the builder and optionally a newline at the end.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <param name="appendNewLine">Whether to append a newline at the end of the text.</param>
    public CodeStringBuilder Append(ReadOnlySpan<char> text, bool appendNewLine = false)
    {
        var lines = text.EnumerateLines();

        foreach (var line in lines)
        {
            // Append indentation at the start of the current line.
            if (EndsWithNewline)
                AppendIndentation();

            builder.Append(line);
        }

        if (appendNewLine) 
            AppendLine();

        return this;
    }

    /// <summary>
    /// Appends a string of text to the builder and a newline at the end.
    /// </summary>
    /// <param name="text">The text to append.</param>
    public CodeStringBuilder AppendLine(ReadOnlySpan<char> text) =>
        Append(text, true);
    
    /// <summary>
    /// Indents the builder by the specified amount.
    /// </summary>
    /// <param name="amount">The amount of indentation levels to indent by.</param>
    public CodeStringBuilder Indent(int amount)
    {
        // If this manages to overflow then that's up to you.
        checked
        {
            var newLevel = indentationLevel + amount;
            if (newLevel < 0) newLevel = 0;
            indentationLevel = newLevel;
        }
        
        return this;
    }

    /// <summary>
    /// Indents the builder by one indentation level.
    /// </summary>
    public CodeStringBuilder Indent() =>
        Indent(1);

    /// <summary>
    /// Unindents the builder by the specified amount.
    /// </summary>
    /// <param name="amount">The amount of indentation levels to unindent by.</param>
    public CodeStringBuilder Unindent(int amount) =>
        Indent(-amount);

    /// <summary>
    /// Unindents the builder by one indentation level.
    /// </summary>
    public CodeStringBuilder Unindent() =>
        Unindent(1);

    /// <summary>
    /// Enumerates through several values and executes an action for each of them,
    /// appending a separator between each section.
    /// </summary>
    /// <param name="values">The values to enumerate.</param>
    /// <param name="separator">The separator to append between sections.</param>
    /// <param name="appendNewline">Whether to append a newline at the end of the last section.</param>
    /// <param name="buildAction">The action used to build each section.</param>
    /// <typeparam name="T">The type of the values to enumerate.</typeparam>
    public CodeStringBuilder Sections<T>(
        IEnumerable<T> values,
        ReadOnlySpan<char> separator,
        bool appendNewline,
        Action<CodeStringBuilder, T> buildAction)
    {
        var firstIteration = true;

        foreach (var value in values)
        {
            if (!firstIteration)
                Append(separator);

            buildAction(this, value);

            firstIteration = false;
        }

        if (appendNewline)
            AppendLine();

        return this;
    }

    /// <summary>
    /// Enumerates through several values and executes an action for each of them,
    /// appending a separator between each section and appending a newline at the end of the last section.
    /// </summary>
    /// <param name="values">The values to enumerate.</param>
    /// <param name="separator">The separator to append between sections.</param>
    /// <param name="buildAction">The action used to build each section.</param>
    /// <typeparam name="T">The type of the values to enumerate.</typeparam>
    public CodeStringBuilder Sections<T>(
        IEnumerable<T> values,
        ReadOnlySpan<char> separator,
        Action<CodeStringBuilder, T> buildAction) =>
        Sections(values, separator, true, buildAction);
    
    /// <summary>
    /// Indents the builder and returns an <see cref="IDisposable"/>
    /// which will unindent the builder once it is disposed. 
    /// </summary>
    /// <param name="indentationAmount">The amount of indentation levels to indent and unindent by.</param>
    /// <param name="appendNewline">Whether to append a newline at the end of the block.</param>
    /// <returns></returns>
    public IDisposable IndentedBlock(int indentationAmount = 1, bool appendNewline = false)
    {
        Indent(indentationAmount);
        return new Block(this, indentationAmount, appendNewline);
    }

    private sealed class Block : IDisposable
    {
        private readonly CodeStringBuilder builder;
        private readonly int indentationAmount;
        private readonly bool appendNewline;

        public Block(CodeStringBuilder builder, int indentationAmount, bool appendNewline)
        {
            this.builder = builder;
            this.indentationAmount = indentationAmount;
            this.appendNewline = appendNewline;
        }

        public void Dispose()
        {
            builder.Unindent(indentationAmount);
            if (appendNewline) builder.AppendLine();
        }
    }

    /// <summary>
    /// Implicitly converts a <see cref="CodeStringBuilder"/> to a <see cref="string"/>
    /// by calling <see cref="ToString"/>.
    /// </summary>
    /// <param name="builder">The builder to convert.</param>
    public static implicit operator string(CodeStringBuilder builder) => builder.ToString();
}
