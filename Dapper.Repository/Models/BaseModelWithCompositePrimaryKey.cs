using System;

namespace Dapper.Repository.Models
{
    public abstract class BaseModelWithCompositePrimaryKey
    {
        /// <summary>
        /// Whether the record is active or not
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// User who has created the record
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Created time of the record
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// User who has updated the record
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Modified time of the record
        /// </summary>
        public DateTime? ModifiedTime { get; set; }
    }
}