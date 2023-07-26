namespace Mwh.Sample.Domain.Models
{
    public sealed class EmployeeDtoValidationException : Exception
    {
        public EmployeeDtoValidationException()
            : base("Employee validation failed.")
        {
        }

        public void AddError(string key, string message)
        {
            Data.Add(key, message);
        }

        public void ThrowIfErrors()
        {
            if (Data.Count > 0)
            {
                throw this;
            }
        }
    }
}
