using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using LifeOS.Persistence.Contexts;

namespace LifeOS.Persistence.Repositories;

public class ImageRepository(LifeOSDbContext dbContext) : EfRepositoryBase<Image, LifeOSDbContext>(dbContext), IImageRepository
{
}
