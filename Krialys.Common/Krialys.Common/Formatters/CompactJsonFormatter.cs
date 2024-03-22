﻿// Copyright 2016 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Parsing;

namespace Krialys.Common.Formatters;

public interface ICompactJsonFormatter
{
    void Format(LogEvent logEvent, TextWriter output);
}

/// <summary>
/// An <see cref="ITextFormatter"/> that writes events in a compact JSON format.
/// </summary>
public class CompactJsonFormatter : ITextFormatter, ICompactJsonFormatter
{
    private readonly JsonValueFormatter _valueFormatter;

    /// <summary>
    /// Construct a <see cref="CompactJsonFormatter"/>, optionally supplying a formatter for
    /// <see cref="LogEventPropertyValue"/>s on the event.
    /// </summary>
    /// <param name="valueFormatter">A value formatter, or null.</param>
    public CompactJsonFormatter(JsonValueFormatter valueFormatter = null)
    {
        _valueFormatter = valueFormatter ?? new JsonValueFormatter(typeTagName: "$type");
    }

    /// <summary>
    /// Format the log event into the output. Subsequent events will be newline-delimited.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    public void Format(LogEvent logEvent, TextWriter output)
    {
        FormatEvent(logEvent, output, _valueFormatter);
        output.WriteLine();
    }

    /// <summary>
    /// Format the log event into the output.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    /// <param name="valueFormatter">A value formatter for <see cref="LogEventPropertyValue"/>s on the event.</param>
    private static void FormatEvent(LogEvent logEvent, TextWriter output, JsonValueFormatter valueFormatter)
    {
        if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
        if (output == null) throw new ArgumentNullException(nameof(output));
        if (valueFormatter == null) throw new ArgumentNullException(nameof(valueFormatter));

        output.Write("{\"@t\":\"");
        //var zz = DateTime.SpecifyKind(DateTime.Parse("2022-03-05 13:19:28.167"), DateTimeKind.Utc);
        output.Write(logEvent.Timestamp.UtcDateTime.ToString("O"));//"yyyy-MM-dd HH:mm:ss.fffzzz"));

        //if (logEvent.Level != LogEventLevel.Information)
        {
            var level = logEvent.Level switch
            {
                LogEventLevel.Information => "INF",
                LogEventLevel.Warning => "WRN",
                LogEventLevel.Error => "ERR",
                LogEventLevel.Debug => "DBG",
                _ => logEvent.Level.ToString(),
            };

            output.Write(",\"@l\":\"");
            output.Write(level);
            //output.Write('\"');
        }

        output.Write("\",\"@mt\":");
        JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Text, output);

        var tokensWithFormat = logEvent.MessageTemplate.Tokens
            .OfType<PropertyToken>()
            .Where(pt => pt.Format != null);

        // Better not to allocate an array in the 99.9% of cases where this is false
        // ReSharper disable once PossibleMultipleEnumeration
        var propertyTokens = tokensWithFormat as PropertyToken[] ?? tokensWithFormat.ToArray();
        if (propertyTokens.Any())
        {
            output.Write(",\"@r\":[");
            var delim = "";
            foreach (var r in propertyTokens)
            {
                output.Write(delim);
                delim = ",";
                var space = new StringWriter();
                r.Render(logEvent.Properties, space);
                JsonValueFormatter.WriteQuotedJsonString(space.ToString(), output);
            }
            output.Write(']');
        }

        if (logEvent.Exception != null)
        {
            output.Write(",\"@x\":");
            JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
        }

        foreach (var property in logEvent.Properties)
        {
            var name = property.Key;
            if (name.Length > 0 && name[0] == '@')
            {
                // Escape first '@' by doubling
                name = '@' + name;
            }

            output.Write(',');
            JsonValueFormatter.WriteQuotedJsonString(name, output);
            output.Write(':');
            valueFormatter.Format(property.Value, output);
        }

        output.Write('}');
    }
}
