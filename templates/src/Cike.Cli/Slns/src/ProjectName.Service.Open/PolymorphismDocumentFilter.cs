namespace ProjectName.Service.Open;

public class PolymorphismDocumentFilter<TBase, TKey> : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var schemaRepository = context.SchemaRepository;

        var baseType = typeof(TBase);
        var polymorphicAttr = baseType!.GetCustomAttribute<JsonPolymorphicAttribute>();

        if (polymorphicAttr is null)
        {
            return;
        }

        var derivedAttrs = baseType.GetCustomAttributes<JsonDerivedTypeAttribute>().ToList();

        if (derivedAttrs.Count == 0)
        {
            return;
        }

        //var keyType = derivedAttrs[0].TypeDiscriminator! is string ? "string" : "integer";

        OpenApiSchema keyTypeSchema;

        if (typeof(TKey) == typeof(string))
        {
            keyTypeSchema = new OpenApiSchema
            {
                Type = "string",
                Nullable = false,
            };
        }
        else
        {
            if (!schemaRepository.TryLookupByType(typeof(TKey), out var keySchemaRef))
            {
                keySchemaRef = context.SchemaGenerator.GenerateSchema(typeof(TKey), context.SchemaRepository);
            }

            keyTypeSchema = new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Id = keySchemaRef.Reference.Id,
                    Type = keySchemaRef.Reference.Type,
                },
            };
        }

        if (!schemaRepository.Schemas.TryGetValue(baseType.Name, out var baseSchema))
        {
            return;
        }

        var typePropertyName = polymorphicAttr.TypeDiscriminatorPropertyName ?? "$type";

        if (!baseSchema.Properties.TryGetValue(typePropertyName, out var typeProperty))
        {
            baseSchema.Properties = InsertProperty(typePropertyName, keyTypeSchema, baseSchema.Properties);
        }

        foreach (var attr in derivedAttrs)
        {
            var type = attr.DerivedType;

            if (!schemaRepository.Schemas.TryGetValue(type.Name, out var childSchema))
            {
                continue;
            }

            if (!childSchema.Properties.TryGetValue(typePropertyName, out var childProperty))
            {
                childSchema.Properties = InsertProperty(typePropertyName, keyTypeSchema, childSchema.Properties);
            }
        }
    }

    private IDictionary<string, OpenApiSchema> InsertProperty(string typeName, OpenApiSchema first, IEnumerable<KeyValuePair<string, OpenApiSchema>> others)
    {
        var dic = new Dictionary<string, OpenApiSchema> { { typeName, first } };

        foreach (var pair in others)
        {
            dic.Add(pair.Key, pair.Value);
        }

        return dic;
    }
}
