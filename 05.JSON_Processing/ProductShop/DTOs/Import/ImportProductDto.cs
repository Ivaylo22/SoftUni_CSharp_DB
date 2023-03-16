using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductShop.DTOs.Import;

public class ImportProductDto
{
    public string Name { get; set; } = null!;

    public decimal Prica { get; set; }

    public int SellerId { get; set; }

    public int? BuyerId { get; set; }

}
