using System;
using System.Data.Entity;
using Api.Common.Legacy.v3.Builders;
using Api.Common.Legacy.v3.Builders.Interfaces;
using Api.Common.Legacy.v3.Builders.PartBuilders;
using Api.Common.Legacy.v3.Builders.PartBuilders.Interfaces;
using Api.Common.Legacy.v4.Builders.DepartCity;
using Api.Common.Legacy.v4.Builders.DepartCity.Interfaces;
using Api.Common.Legacy.v4.Builders.Destination;
using Api.Common.Legacy.v4.Builders.Destination.Interfaces;
using Api.Common.Legacy.v4.Builders.Tour;
using Api.Common.Legacy.v4.Builders.Tour.Interfaces;
using Api.Common.Uniteller.Strategies;
using DataModel.Entities;
using DAL;
using EF.DAL;
using EntityManagementModule.Extensions;
using EntityManagementModule.Strategies.EntityStrategies.Interfaces;
using FactoryModule;
using FactoryModule.Extensions;
using FactoryModule.Interfaces;
using MailerModule.Unity.Extensions;
using Meerkat.Core;
using Meerkat.Core.File;
using Meerkat.Core.Web;
using Meerkat.File;
using Meerkat.UserProfile.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.WebApi;
using Services.AcquiredTour.Strategies;
using Services.AcquiredTour.Strategies.Contexts;
using Services.Common;
using Services.Managers.EntityManagers.User;
using Services.Managers.EntityManagers.User.Interfaces;
using Services.Managers.EntityManagers.User.Strategies.Get;
using Services.Managers.EntityManagers.User.Strategies.Get.Contexts;
using Services.Managers.EntityManagers.User.Strategies.GetAll;
using Services.Managers.EntityManagers.User.Strategies.GetAll.Contexts;
using Services.Managers.EntityManagers.User.Strategies.Update;
using Services.Managers.EntityManagers.User.Strategies.Update.Contexts;
using Services.ModuleIntegration;
using Services.Order;
using Services.TouAction;
using Services.TourProviderRepositoryProxy;
using Services.User;
using Services.User.Interfaces;
using Tools.Cache.Interfaces;
using Tools.Cache.Redis;
using Tools.Generators.Interfaces;
using Tools.Hashing;
using Tools.Hashing.Interfaces;
using Tools.Hashing.Strategies;
using Tools.Hashing.Strategies.Interfaces;
using Tools.Unity;
using TourProvider.Common.Hash.Strategies;
using TourProvider.Common.Repository;
using TourProvider.Common.Repository.Dtos.Contexts;
using TourProvider.Common.Repository.Interfaces;
using TourProvider.InnaTour.Hash.Strategies;
using Services.Facebook.Interfaces;
using Services.Facebook;
using Services.Messaging;
using Services.Order.Interfaces;
using Services.Order.Strategies;
using Services.Order.Strategies.Contexts;
using Services.Tourist.Strategies;
using Services.Tourist.Strategies.Contexts;
using TourProvider.Common.Repository.Dtos;
using TourProvider.InnaTour.Builders;
using TourProvider.InnaTour.Builders.Interfaces;
using TourProvider.InnaTour.Repository.Strategies.Country;
using TourProvider.InnaTour.Repository.Strategies.Country.Interfaces;
using TourProvider.InnaTour.Repository.Strategies.Hotel;
using TourProvider.InnaTour.Repository.Strategies.Hotel.Interfaces;
using TourProvider.InnaTour.Repository.Strategies.Meal;
using TourProvider.InnaTour.Repository.Strategies.Meal.Interfaces;
using TourProvider.InnaTour.Repository.Strategies.Resort;
using TourProvider.InnaTour.Repository.Strategies.Resort.Interfaces;
using TourProvider.InnaTour.RequestHelper.Api.Dtos.RequestContexts;
using UnitellerModule.Unity.Extensions;
using Image = TourProvider.Common.Entities.Image;

namespace Api
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static readonly Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = Container.Instance;
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        private static void RegisterTypes(IUnityContainer container)
        {
            RegisterCommonTypes(container);
            RegisterV4Types(container);
            RegisterV5Types(container);
        }

        private static void RegisterCommonTypes(IUnityContainer container)
        {
            RegisterRepositories(container);
            RegisterHashGenerator(container);
            RegisterBuilders(container);
            RegisterStrategies(container);

            container.RegisterFactoryModule();
            container.RegisterMailerModuleDependencies();
            container.RegisterUnitellerModuleDependencies<TestCheckOrderIdStrategy, TestCheckCustomerIdStrategy>();

            container.RegisterType<ICacheManager, RedisCacheManager>();
            container.RegisterType<IApplication, MeerkatApplication>();
            container.RegisterType<IFileModule, FileModule>();
        }

        private static void RegisterV4Types(IUnityContainer container)
        {
            var versionContainer = container.CreateChildContainer();
            SaveApiVersionContainer(container, versionContainer, 4);
        }

        private static void RegisterV5Types(IUnityContainer container)
        {
            var versionContainer = container.CreateChildContainer();
            SaveApiVersionContainer(container, versionContainer, 5);
        }

        private static void SaveApiVersionContainer(IUnityContainer container, IUnityContainer childContainer, int version)
        {
            container.RegisterInstance(typeof (IUnityContainer), version.ToString(), childContainer,
                new ContainerControlledLifetimeManager());
        }

        private static void RegisterRepositories(IUnityContainer container)
        {
            container.RegisterType<ITourProviderRepository, TourProviderRepositoryProxy>();
        }

        private static void RegisterHashGenerator(IUnityContainer container)
        {
            #region Meerkat dependencies

            container.RegisterType<ICache, Cache>();
            container.RegisterType<ILog, TextLog>();
            container.RegisterType<IFactory, Factory>();

            #endregion

            container.RegisterType<DbContext, EFContext>(new PerRequestLifetimeManager());
            container.RegisterType<IUserAccount, User>();
            container.RegisterType<IUnitOfWorkRepository, EFRepository>();
            container.RegisterType<IRepository, EFRepository>();
            container.RegisterType<ILog, TextLog>();
            container.RegisterType<IUserSelectionService, UserSelectionService>();
            container.RegisterType<ITourActionService, TourActionService>();
            container.RegisterType<IFilesService, FilesService>();
            container.RegisterType<IOrderService, OrderService>();
            container.RegisterType<IUserIdProvider, PrincipalUserIdProvider>();
            container.RegisterType<IMembershipProvider, MeerkatMembershipProvider>();
            container.RegisterType<IConfiguration, Configuration>();

            container.RegisterType<IHashGenerator, HashGenerator>();
            container.RegisterType(typeof(IHashGenerationStrategy<>), typeof(DefaultHashGenerationStrategy<>));
            container.RegisterType<IHashGenerationStrategy<InnaTourSearchRequestContext>, InnaTourSearchRequestHashGenerationStrategy>();
            container.RegisterType<IHashGenerationStrategy<InnaTourGetHotelInfoRequestContext>, InnaTourGetHotelInfoRequestHashGenerationStrategy>();
            container.RegisterType<IHashGenerationStrategy<InnaTourActualizationTourRequestContext>, InnaTourActualizationTourRequestHashGenerationStrategy>();
            container.RegisterType<IHashGenerationStrategy<InnaTourCountryListRequestContext>, InnaTourCountryListRequestHashGenerationStrategy>();
            container.RegisterType<IHashGenerationStrategy<InnaTourDepartCitiesRequestContext>, InnaTourDepartCitiesRequestHashGenerationStrategy>();
            container.RegisterType<IHashGenerationStrategy<InnaTourHotelsRequestContext>, InnaTourHotelsRequestHashGenerationStrategy>();
            container.RegisterType<IHashGenerationStrategy<InnaTourResortsRequestContext>, InnaTourResortsRequestHashGenerationStrategy>();
            container.RegisterType<IHashGenerationStrategy<InnaTourHotelStarsRequestContext>, InnaTourHotelStarsRequestHashGenerationStrategy>();
            container.RegisterType<IHashGenerationStrategy<InnaTourHotelMealsRequestContext>, InnaTourHotelMealsRequestHashGenerationStrategy>();
            container.RegisterType<IHashGenerationStrategy<InnaTourGetAirlinesRequestContext>, InnaTourGetAirlinesResultHashGenerationStrategy>();
            container.RegisterType<IHashGenerationStrategy<TourProviderRepositoryGetAllToursContext>, TourProviderRepositoryGetAllToursContextHashGenerationStrategy>();

            container.RegisterType<ITourProviderCacheRepository, TourProviderCacheRepository>();
            container.RegisterType<IUserManager, UserManager>();
            
            container.RegisterType(typeof(IFactoryMethod<>), typeof(FactoryMethod<>));

            container.RegisterEntityManagementModule(typeof(EntityManagementModuleIntegration<>), typeof(EntityManagementModuleIntegration<>), typeof(EntityManagementModuleIntegration<>), typeof(EntityManagementModuleIntegration<>));

            container.RegisterType<IFacebookService, FacebookService>();
            container.RegisterType<IUserRegistrationService,UserRegistrationService>();
            container.RegisterType<IChatService, ChatService>();
        }

        private static void RegisterBuilders(IUnityContainer container)
        {
            container.RegisterType(typeof(IHasCountryTitleAsyncPartBuilder), typeof(HasCountryTitleAsyncPartBuilder));
            container.RegisterType(typeof(IHasResortTitleAsyncPartBuilder), typeof(HasResortTitleAsyncPartBuilder));

            container.RegisterType(typeof(ITourActionModelAsyncBuilder), typeof(TourActionModelAsyncBuilder));

            container.RegisterType<ITourGetAllRequestStatePartBuilder, TourGetAllRequestStatePartBuilder>();

            container.RegisterType<ITourGetAllRequestResultResponseModelAsyncFluentBuilder, TourGetAllRequestResultResponseAsyncFluentModelBuilder>();
            container.RegisterType<ITourGetAllRequestInfoAsyncFluentModelBuilder, TourGetAllRequestInfoAsyncFluentModelBuilder>();

            container.RegisterType<IInnaTourSearchRequestContextBuilder, InnaTourSearchRequestContextBuilder>();

            container.RegisterType<Common.Legacy.v4.Builders.Tour.Interfaces.ITourGetAllRequestInfoAsyncFluentModelBuilder, Common.Legacy.v4.Builders.Tour.TourGetAllRequestInfoAsyncFluentModelBuilder>();
            container.RegisterType<Common.Legacy.v4.Builders.PartBuilders.Interfaces.ITourGetAllRequestStatePartBuilder, v4.Models.Builders.PartBuilders.TourGetAllRequestStatePartBuilder>();
            container.RegisterType<IDepartCityCollectionModelBuilder, DepartCityCollectionModelBuilder>();
            container.RegisterType<ITourGetAllModelBuilder, TourGetAllModelBuilder>();
            container.RegisterType<ITourGetAllResortsModelBuilder, TourGetAllResortsModelBuilder>();
            container.RegisterType<IDestinationLoadDictionariesResponseModelBuilder, DestinationLoadDictionariesResponseModelBuilder>();
        }

        private static void RegisterStrategies(IUnityContainer container)
        {
            container.RegisterType<IGetAllCountriesStrategy, GetAllCountriesStrategy>();
            container.RegisterType<IGetAllCountriesFromInnaTourStrategy, GetAllCountriesFromInnaTourStrategy>();
            container.RegisterType<IGetAllCountriesFromCacheStrategy, GetAllCountriesFromCacheStrategy>();
            container.RegisterType<IGetCountryByIdStrategy, GetCountryByIdStrategy>();
            container.RegisterType<IGetAllHotelStarsStrategy, GetAllHotelStarsStrategy>();
            container.RegisterType<IGetAllHotelStarsFromCacheStrategy, GetAllHotelStarsFromCacheStrategy>();
            container.RegisterType<IGetAllHotelsStrategy, GetAllHotelsStrategy>();
            container.RegisterType<IGetAllHotelsFromInnaTourStrategy, GetAllHotelsFromInnaTourStrategy>();
            container.RegisterType<IGetAllHotelsFromCacheStrategy, GetAllHotelsFromCacheStrategy>();
            container.RegisterType<IGetHotelByIdStrategy, GetHotelByIdStrategy>();
            container.RegisterType<IGetAllResortsStrategy, GetAllResortsStrategy>();
            container.RegisterType<IGetAllResortsFromCacheStrategy, GetAllResortsFromCacheStrategy>();
            container.RegisterType<IGetResortByIdStrategy, GetResortByIdStrategy>();
            container.RegisterType<IGetAllMealsStrategy, GetAllMealsStrategy>();

            container.RegisterType(typeof(IGetEntityStrategy<User, GetUserByEmailEntityStrategyContext>), typeof(GetUserByEmailEntityStrategy));
            container.RegisterType(typeof(IGetEntityStrategy<User, GetUserByVerificationKeyEntityStrategyContext>), typeof(GetUserByVerificationKeyEntityStrategy));
            container.RegisterType(typeof(IGetAllEntitiesStrategy<User, GetAllUserEntitiesStrategyContext>), typeof(GetAllUserEntitiesStrategy));
            container.RegisterType(typeof(IUpdateEntityStrategy<User, UpdateUserEntityStrategyContext>), typeof(UpdateUserEntityStrategy));
            container.RegisterType(typeof(IUpdateEntityStrategy<User, UpdateUserPasswordEntityStrategyContext>), typeof(UpdateUserPasswordEntityStrategy));

            container.RegisterType(typeof(ICreateEntityStrategy<Order>), typeof(OrderCreateEntityStrategy));
            container.RegisterType(typeof(IUpdateEntityStrategy<Order, OrderCreationUpdateEntityStrategyContext>), typeof(OrderUpdateEntityStrategy));
            container.RegisterType(typeof(IUpdateEntityStrategy<Order, OrderCreateResultDto>), typeof(OrderUpdateEntityStrategy));
            container.RegisterType(typeof(IUpdateEntityStrategy<Tourist, OrderCreateTouristUpdateStrategyContext>), typeof(TouristUpdateEntityStrategy));

            container.RegisterType(typeof(IGetEntityStrategy<DataModel.Entities.AcquiredTour, string>), typeof(AcquireTourGetStrategy));
            container.RegisterType(typeof(IUpdateEntityStrategy<DataModel.Entities.AcquiredTour, AcquiredTourUpdateEntityByTourAndRateKeyStrategyContext>), typeof(AcquiredTourUpdateEntityStrategy));
            container.RegisterType(typeof(IUpdateEntityStrategy<AcquiredTourImage, Image>), typeof(AcquiredImageCreationUpdateStrategy));
        }
    }
}
