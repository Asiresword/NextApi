using System;
using System.Threading.Tasks;
using Abitech.NextApi.Server.EfCore.Tests.Base;
using Abitech.NextApi.Server.EfCore.Tests.Entity;
using Abitech.NextApi.Server.EfCore.Tests.Repository;
using DeepEqual.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Abitech.NextApi.Server.EfCore.Tests
{
    public class EfCoreTest
    {
        public IServiceScope Services => GetServices();

        [Fact]
        public async Task BasicLogicCheck()
        {
            using (var scope = Services)
            {
                var provider = scope.ServiceProvider;
                var repo = provider.GetService<TestEntityRepository>();
                var unitOfWork = provider.GetService<TestUnitOfWork>();

                // check adding 
                {
                    var entityToAdd = new TestEntity()
                    {
                        Name = "TestEntityToAdd"
                    };

                    await repo.AddAsync(entityToAdd);
                    await unitOfWork.Commit();
                    // should contain id after commit
                    Assert.True(entityToAdd.Id > 0);
                    var entityAdded = await repo.GetByIdAsync(entityToAdd.Id);
                    entityToAdd.ShouldDeepEqual(entityAdded);
                }
                // check update
                {
                    var entityToUpdate = await repo.GetByIdAsync(1);
                    // check that previously added entity
                    Assert.Equal("TestEntityToAdd", entityToUpdate.Name);
                    var newName = "TestEntityUpdate";
                    entityToUpdate.Name = newName;
                    repo.Update(entityToUpdate);
                    await unitOfWork.Commit();
                    var updatedEntity = await repo.GetByIdAsync(entityToUpdate.Id);
                    updatedEntity.ShouldDeepEqual(entityToUpdate);
                }
                // check delete
                {
                    Assert.True(await repo.GetAll().AnyAsync());
                    repo.Delete(e => e.Id == 1);
                    await unitOfWork.Commit();
                    Assert.False(await repo.GetAll().AnyAsync());
                }
            }
        }

        [Fact]
        public async Task CheckKeyPredicatesNotSupportedException()
        {
            using (var scope = Services)
            {
                var provider = scope.ServiceProvider;
                var repo = provider.GetService<TestEntityPredicatesRepository>();
                var unitOfWork = provider.GetService<TestUnitOfWork>();
                
                // create entity 
                var id = "id1";
                await repo.AddAsync(new TestEntityKeyPredicate
                {
                    Id = id,
                    Description = "testException"
                });
                await unitOfWork.Commit();

                // should throw a NotSupportedException (cause this entity type is not implements IEntity<TKey> interface)
                await Assert.ThrowsAsync<NotSupportedException>(() => repo.GetByIdAsync(id));
            }
        }


        private IServiceScope GetServices()
        {
            if (_services != null)
                return _services.CreateScope();
            var builder = new ServiceCollection();
            builder.AddDbContext<TestDbContext>(options => { options.UseInMemoryDatabase(Guid.NewGuid().ToString()); });
            builder.AddTransient<TestEntityRepository>();
            builder.AddTransient<TestEntityPredicatesRepository>();
            builder.AddTransient<TestUnitOfWork>();
            _services = builder.BuildServiceProvider();

            // ensure db created
            using (var scope = _services.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var context = provider.GetService<TestDbContext>();
                context.Database.EnsureCreated();
            }

            return _services.CreateScope();
        }

        private IServiceProvider _services;
    }
}