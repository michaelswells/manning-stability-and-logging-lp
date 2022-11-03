using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RobotsInc.Inspections.API.I.Json;

public class RobotJsonConverter : JsonConverter<Robot>
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(Robot);

    public override Robot Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        const string ErrorMessage = $"Json for type {nameof(Robot)} not supported by {nameof(RobotJsonConverter)}.";
        string discriminatorPropertyName =
            options.PropertyNamingPolicy?.ConvertName(nameof(Robot.RobotType))
            ?? nameof(Robot.RobotType);

        // clone original reader to parse the JSON as a JsonDocument and find the discriminator
        Utf8JsonReader readerClone = reader;
        using JsonDocument jsonDocument = JsonDocument.ParseValue(ref readerClone);
        if (!jsonDocument.RootElement.TryGetProperty(discriminatorPropertyName, out JsonElement discriminatorProperty))
        {
            throw new JsonException(ErrorMessage);
        }

        // use the discriminator property to do the correct deserialization
        string? typeDiscriminator = discriminatorProperty.GetString();
        Robot robot = typeDiscriminator switch
        {
            nameof(RobotType.ARTICULATED_ROBOT) => JsonSerializer.Deserialize<ArticulatedRobot>(ref reader, options)!,
            nameof(RobotType.AUTOMATED_GUIDED_VEHICLE) => JsonSerializer.Deserialize<AutomatedGuidedVehicle>(ref reader, options)!,
            _ => throw new JsonException(ErrorMessage)
        };

        return robot;
    }

    public override void Write(Utf8JsonWriter writer, Robot robot, JsonSerializerOptions options)
    {
        // choose correct serializer based on type of the dto
        switch (robot)
        {
            case ArticulatedRobot articulatedRobot:
                JsonSerializer.Serialize(writer, articulatedRobot, options);
                break;
            case AutomatedGuidedVehicle automatedGuidedVehicle:
                JsonSerializer.Serialize(writer, automatedGuidedVehicle, options);
                break;
            default:
                throw new JsonException($"Type of robot ({robot.GetType().FullName}) not supported by {nameof(RobotJsonConverter)}");
        }
    }
}
