using System;

namespace CompBioAnalyticsApi.DataAccess.Exceptions
{
    /// <summary>
    /// Thrown when a database operation has failed
    /// </summary>
    public class DatabaseOperationFailedException : Exception
    {
        public string Query { get; }

        public object Input { get; }

        public DatabaseOperationFailedException(string query, object input)
        {
            Query = query;
            Input = input;
        }
    }
}