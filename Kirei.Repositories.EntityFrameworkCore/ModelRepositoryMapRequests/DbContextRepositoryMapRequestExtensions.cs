using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Kirei.Repositories
{
    /// <summary>
    /// Extensions that hang off ModelRepositoryMapRequest to map DbContext repositories to models.
    /// </summary>
    public static class DbContextRepositoryMapRequestExtensions
    {
        /// <summary>
        /// Use JsonFileRepository <typeparamref name="Repository"/> as the repository for <paramref name="request"/>.
        /// </summary>
        /// <remarks>
        /// Matches the Model to a DbModel by name.
        /// </remarks>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type UseDbContext<Context>(this ModelRepositoryMapRequest request)
            where Context : DbContext
        {
            return UseDbContext<Context>(request, optionsAction: null);
        }

        /// <summary>
        /// Use JsonFileRepository <typeparamref name="Repository"/> as the repository for <paramref name="request"/>.
        /// </summary>
        /// <remarks>
        /// Matches the Model to a DbModel by name.
        /// </remarks>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type UseDbContext<Context>(this ModelRepositoryMapRequest request, Action<DbContextOptionsBuilder> optionsAction = null)
            where Context : DbContext
        {
            if (optionsAction == null) {
                request.Services.AddDbContext<Context>();
            } else {
                request.Services.AddDbContext<Context>(options => optionsAction(options));
            }

            // Lookup a Db Model type by trying to match by name from the context's properties.
            var dbSetProperty = typeof(Context).GetProperties()
                .Where(item => item.PropertyType.IsGenericType && item.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .FirstOrDefault(item => item.PropertyType.GetGenericArguments().First().Name == request.ModelType.Name);
            
            // If we couldn't find a match then return doing nothing as this is as far as we can go.
            if (dbSetProperty == null) {
                return null;
            }

            // If we found a set, extract is type as our db model.
            var dbModelType = dbSetProperty.PropertyType.GetGenericArguments().First();

            
            // Create a repository class with the right types look up the DbSet's model type by name.
            var ret = typeof(DbContextRepository<,,>).MakeGenericType(request.ModelType, typeof(Context), dbModelType);
            return ret;
        }
        /// <summary>
        /// Use JsonFileRepository <typeparamref name="Repository"/> as the repository for <paramref name="request"/>.
        /// </summary>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type UseDbContext<Context, DbModel>(this ModelRepositoryMapRequest request)
            where Context : DbContext
        {
            return UseDbContext<Context, DbModel>(request, optionsAction: null);
        }

        /// <summary>
        /// Use JsonFileRepository <typeparamref name="Repository"/> as the repository for <paramref name="request"/>.
        /// </summary>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type UseDbContext<Context, DbModel>(this ModelRepositoryMapRequest request, Action<DbContextOptionsBuilder> optionsAction = null)
            where Context : DbContext
        {
            if (optionsAction == null) {
                request.Services.AddDbContext<Context>();
            } else {
                request.Services.AddDbContext<Context>(options => optionsAction(options));
            }
            
            // Create a repository class with the right types look up the DbSet's model type by name.
            var ret = typeof(DbContextRepository<,,>).MakeGenericType(request.ModelType, typeof(Context), typeof(DbModel));
            return ret;
        }
    }
}
