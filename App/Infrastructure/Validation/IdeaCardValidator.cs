using App.Data.Entities;
using FluentValidation;

namespace App.Infrastructure.Validation;

public class IdeaCardValidator : AbstractValidator<IdeaCard>
{
    public IdeaCardValidator()
    {
        RuleFor(x => x.Tytul)
            .NotEmpty().WithMessage("Podaj tytuł pomysłu.")
            .MinimumLength(5).WithMessage("Tytuł jest zbyt krótki.")
            .MaximumLength(200);

        RuleFor(x => x.Streszczenie)
            .MaximumLength(400);

        RuleFor(x => x.Problem)
            .MinimumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Problem))
            .WithMessage("Opis problemu powinien mieć co najmniej 30 znaków.")
            .MaximumLength(4000);

        RuleFor(x => x.GrupaDocelowa).MaximumLength(1000);
        RuleFor(x => x.Rozwiazanie)
            .MinimumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Rozwiazanie))
            .MaximumLength(4000);
        RuleFor(x => x.Rezultaty).MaximumLength(2000);
        RuleFor(x => x.Zasoby).MaximumLength(1000);
        RuleFor(x => x.Ryzyka).MaximumLength(2000);
        RuleFor(x => x.Tagi).MaximumLength(500);
    }
}
