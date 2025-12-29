using System.Linq;

namespace LifeOS.Domain.Common;

public interface IQuery<T>
{
    IQueryable<T> Query();
}
