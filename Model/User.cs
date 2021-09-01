using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class User
    {
        [Key]
        public long UserId { get; set; }

        public string SubscribedStockCodeSerialize { get; set; }
        public IEnumerable<string> SubscribedStockCode => SubscribedStockCodeSerialize.Split(',');
    }
}