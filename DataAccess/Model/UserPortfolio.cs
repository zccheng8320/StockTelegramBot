using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Model
{
    public class UserPortfolio
    {
        [Key]
        public long UserId { get; set; }

        public string PortfolioSerialize { get; set; }
        public IEnumerable<string> Portfolio => PortfolioSerialize.Split(',');
    }
}