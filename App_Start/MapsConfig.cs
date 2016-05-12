using System;
using System.Linq;
using System.Reflection;
using AutoMapper;

namespace Api
{
    public class MapsConfig
    {
        private static readonly object LockObject = new object();
        private static bool _isRegistered;

        public static void Register()
        {
            if (_isRegistered == false)
                lock (LockObject)
                {
                    if (_isRegistered == false)
                    {
                        //TODO: Вынести загрузку профилей сборок в сами сборки, предусмотреть конкурентность
                        RegisterAssemblyMapProfiles("Api.Common");
                        RegisterAssemblyMapProfiles("Api.v4");
                        RegisterAssemblyMapProfiles("Api.v4.Models");
                        RegisterAssemblyMapProfiles("Api.v5");
                        RegisterAssemblyMapProfiles("Api.v5.Models");
                        RegisterAssemblyMapProfiles("Services");
                        RegisterAssemblyMapProfiles("TourProvider.Common");
                        RegisterAssemblyMapProfiles("TourProvider.InnaTour");

                        _isRegistered = true;
                    }
                }
        }

        private static void RegisterAssemblyMapProfiles(string assemblyName)
        {
            foreach (var profiles in Assembly.Load(assemblyName)
                                                        .GetTypes()
                                                        .Where(x => typeof(Profile).IsAssignableFrom(x)))
            {
                Mapper.AddProfile((Profile)Activator.CreateInstance(profiles));
            }
        }
    }
}