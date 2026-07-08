using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain_Layer.Entities
{
    [Owned]
    public class RefreshTokens
    {
        public string Token { get; set; }
        public DateTime CreatedOn { get; set; }

        public DateTime ExpiresOn { get; set; }

        public DateTime? revokedOn { get; set; }

        public bool isExpired => DateTime.UtcNow >= ExpiresOn;

        public bool isActive => revokedOn == null && !isExpired;
    }
}
