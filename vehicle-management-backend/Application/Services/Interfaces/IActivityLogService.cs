namespace vehicle_management_backend.Application.Services.Interfaces
{
    public interface IActivityLogService
    {
        /// <summary>
        /// Logs a Create operation with 'success' type
        /// </summary>
        Task LogCreateAsync(string message);

        /// <summary>
        /// Logs an Update operation with 'info' type
        /// </summary>
        Task LogUpdateAsync(string message);

        /// <summary>
        /// Logs a Delete operation with 'warning' type
        /// </summary>
        Task LogDeleteAsync(string message);
    }
}
