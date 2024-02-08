// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Exceptions;

namespace Voting.Stimmregister.EVoting.Domain.Converters;

// Parsing DateOnly is only supported from .NET 7 onwards
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!DateOnly.TryParse(reader.GetString() ?? string.Empty, out var date))
        {
            throw new EVotingValidationException("The date has an invalid format.", ProcessStatusCode.DateOfBirthDoesNotMatch);
        }

        return date;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O"));
    }
}
