using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace Cike.AspNetCore.Swagger;

public class EnumDescriptionSupplementSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
        {
            return;
        }
        var enumValues = Enum.GetValues(context.Type);
        StringBuilder stringBuilder = new StringBuilder(schema.Description);
        stringBuilder.Append("{");
        foreach (object value in enumValues)
        {
            StringBuilder stringBuilder2 = stringBuilder;
            StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(15, 2, stringBuilder2);
            handler.AppendFormatted<object>($"\"{value}\"");
            handler.AppendLiteral(":");
            if (enumValues.GetValue(enumValues.Length - 1)!.ToString() == value.ToString())
            {
                handler.AppendFormatted($"{(int)value}");
            }
            else
            {
                handler.AppendFormatted($"{(int)value},");
            }
            stringBuilder2.Append(ref handler);
        }
        stringBuilder.Append("}");

        schema.Description = stringBuilder.ToString();

        stringBuilder.Clear();
    }
}
