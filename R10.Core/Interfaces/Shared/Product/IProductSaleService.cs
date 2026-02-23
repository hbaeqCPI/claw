using System;
using System.Collections.Generic;
using System.Text;
using R10.Core.Entities;
using System.Threading.Tasks;


namespace R10.Core.Interfaces
{
    public interface IProductSaleService :IChildEntityService<Product, ProductSale>
    {
        //Task<bool> Update(object key, int parentId, string systemType, string userName,
        //    IEnumerable<ProductSale> updated,
        //    IEnumerable<ProductSale> added,
        //    IEnumerable<ProductSale> deleted);
    }
}
