using CQRS.Application.Contracts.Orders.Dtos;
using FluentValidation;

namespace CQRS.Application.Orders.Validators;

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Province)
            .NotEmpty().WithMessage("省份不能为空。")
            .MaximumLength(50).WithMessage("省份不能超过50个字符。")
            .Matches(@"^[一-龥a-zA-Z0-9_-]+$").WithMessage("省份只支持中文、英文字母、数字、下划线和连字符。");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("城市不能为空。")
            .MaximumLength(50).WithMessage("城市不能超过50个字符。")
            .Matches(@"^[一-龥a-zA-Z0-9_-]+$").WithMessage("城市只支持中文、英文字母、数字、下划线和连字符。");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("街道不能为空。")
            .MaximumLength(200).WithMessage("街道不能超过200个字符。");

        RuleFor(x => x.ZipCode)
            .MaximumLength(10).WithMessage("邮编不能超过10个字符。")
            .Matches(@"^\d{6}$").When(x => !string.IsNullOrEmpty(x.ZipCode)).WithMessage("邮编必须是6位数字。");
    }
}
