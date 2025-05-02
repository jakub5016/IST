namespace EmployeeService.Core
{
    public sealed record Error(string Code, string Description)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NotFound = new("404", "Not found");
        public static Error SomethingWentWrong(string exception) => new("500", $"Unexpected error: {exception}");

    }
}
