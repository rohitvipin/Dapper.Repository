namespace Dapper.Repository.Models
{
    /// <summary>
    /// Base model inherited to all other models
    /// </summary>
    public abstract class BaseModel : BaseModelWithCompositePrimaryKey
    {
        /// <summary>
        /// Primary key Id of the table
        /// </summary>
        public int Id { get; set; }
    }
}