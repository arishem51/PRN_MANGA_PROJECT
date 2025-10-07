using System;

namespace PRN_MANGA_PROJECT.Models.Entities
{
    public interface IHasTimestamps
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}


