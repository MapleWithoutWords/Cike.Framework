using CQRS.Application.Contracts.Orders.Dtos;
using FluentValidation;

namespace CQRS.Application.Orders.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.BuyerId)
            .GreaterThan(0).WithMessage("买家Id必须大于0。");

        RuleFor(x => x.Address)
            .NotNull().WithMessage("收货地址不能为空。")
            .SetValidator(new AddressDtoValidator());

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("订单至少包含一个订单行。");

        RuleForEach(x => x.Lines)
            .SetValidator(new OrderLineDtoValidator());
    }
}
