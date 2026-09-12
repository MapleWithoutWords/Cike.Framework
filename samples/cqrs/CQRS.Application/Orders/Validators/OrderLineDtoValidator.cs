using CQRS.Application.Contracts.Orders.Dtos;
using FluentValidation;

namespace CQRS.Application.Orders.Validators;

public class OrderLineDtoValidator : AbstractValidator<OrderLineDto>
{
    public OrderLineDtoValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("商品名称不能为空。")
            .MaximumLength(200).WithMessage("商品名称不能超过200个字符。")
            .Matches(@"^[一-龥a-zA-Z0-9_-]+$").WithMessage("商品名称只支持中文、英文字母、数字、下划线和连字符。");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 999).WithMessage("数量必须在1到999之间。");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("单价必须大于0。")
            .PrecisionScale(18, 2, true).WithMessage("单价整数部分不能超过16位，小数不能超过2位。");
    }
}
