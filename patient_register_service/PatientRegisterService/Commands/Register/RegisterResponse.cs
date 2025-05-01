namespace PatientRegisterService.Commands.Register
{
    public record RegisterResponse(bool IsSuccessful, List<ValidationError>? ValidationErrors)
    {
        public static RegisterResponse Success() => new(true, []);
        public static RegisterResponse Failure(List<ValidationError> errors) => new(false, errors);
    }
    public record ValidationError(string Property, string Message);

}
