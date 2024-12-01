namespace Cike.FluentValidation;

public class AutoFluentValidationEndpointFilter : IEndpointFilter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly JsonOptions _jsonOptions;
    private static List<Type> _validatorEntityTypes = default!;
    private static readonly object _lock = new object();
    private static readonly ConcurrentDictionary<Type, Type> _validatorTypes = new ConcurrentDictionary<Type, Type>();
    private static readonly AdvancedJsonCamelCaseNamingPolicy _advancedJsonCamelCaseNamingPolicy = new AdvancedJsonCamelCaseNamingPolicy();

    public AutoFluentValidationEndpointFilter(IServiceCollection services, IServiceProvider serviceProvider, IOptions<JsonOptions> jsonOptions)
    {
        if (_validatorEntityTypes is null)
        {
            InitValidatorTypes(services);
        }
        _serviceProvider = serviceProvider;
        _jsonOptions = jsonOptions.Value;
    }

    private static void InitValidatorTypes(IServiceCollection services)
    {
        if (_validatorEntityTypes is not null) return;

        lock (_lock)
        {
            if (_validatorEntityTypes is not null) return;

            var validatorType = typeof(IValidator);
            _validatorEntityTypes = services.Where(service => service.ServiceType?.IsAssignableTo(validatorType) ?? false).SelectMany(service => service.ServiceType.GenericTypeArguments).ToList();
        }
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        for (var i = 0; i < context.Arguments.Count; i++)
        {
            var argument = context.Arguments[i];
            var type = argument?.GetType();

            if (type is null || !_validatorEntityTypes.Contains(type)) continue;
            var validatorType = _validatorTypes.GetOrAdd(type, t =>
            {
                return typeof(IValidator<>).MakeGenericType(type);
            });

            if (_serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var validationResult = await validator.ValidateAsync(new ValidationContext<object?>(argument), context.HttpContext.RequestAborted);

                if (!validationResult.IsValid)
                {
                    var data = validationResult.ToDictionary();

                    if (_jsonOptions.SerializerOptions.PropertyNamingPolicy?.Equals(System.Text.Json.JsonNamingPolicy.CamelCase) ?? false)
                    {
                        data = data.ToDictionary(x => _advancedJsonCamelCaseNamingPolicy.ConvertName(x.Key), x => x.Value);
                    }

                    return Results.ValidationProblem(data);
                }
            }
        }

        return await next(context);
    }
}
