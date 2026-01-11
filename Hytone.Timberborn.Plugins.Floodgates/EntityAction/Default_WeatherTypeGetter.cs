using Timberborn.HazardousWeatherSystem;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using Timberborn.WeatherSystem;
using Timberborn.WorldPersistence;

namespace Hytone.Timberborn.Plugins.Floodgates.EntityAction
{
    internal class Default_WeatherTypeGetter : IWeatherTypeGetter
    {
        private WeatherTypes curWeatherTypes;
        private readonly WeatherService defaultWeatherService;
        private readonly HazardousWeatherService defaultHazardousWeatherService;

        private PropertyKey<WeatherTypes> weatherTypesKey = new("weatherTypesKey");

        public WeatherTypes CurWeatherTypes { get; private set; }
        public ISingletonLoader SingletonLoader { get; init; }

        public event WeatherChanged? WeatherChanging;


        public Default_WeatherTypeGetter(WeatherService DefaultWeatherService, HazardousWeatherService DefaultHazardousWeatherService, ISingletonLoader singletonLoader)
        {
            defaultWeatherService = DefaultWeatherService;
            defaultHazardousWeatherService = DefaultHazardousWeatherService;
            SingletonLoader = singletonLoader;
        }
        public void Save(ISingletonSaver singletonSaver)
        {
            IObjectSaver objectSaver = singletonSaver.GetSingleton((this as IWeatherTypeGetter).WeatherTypesKey);
            objectSaver.Set(weatherTypesKey, curWeatherTypes);
        }

        public void Load()
        {
            if (SingletonLoader.TryGetSingleton((this as IWeatherTypeGetter).WeatherTypesKey, out IObjectLoader loader))
            {
                CurWeatherTypes = (loader.Has(weatherTypesKey)) ? loader.Get(weatherTypesKey) : WeatherTypes.Default;
            }
        }
        public void PostLoad()
        {
            if ((int)CurWeatherTypes == 0)
            {
                UpdateWeatherType();
            }
            else
            {
                OnWeatherChanging(new()
                {
                    ChangesFrom = curWeatherTypes,
                    ChangesTo = curWeatherTypes
                });
            }
        }

        [OnEvent]
        public void OnHazardousWeatherStarted(HazardousWeatherStartedEvent e) => UpdateWeatherType();
        public void OnHazardousWeatherEnded(HazardousWeatherEndedEvent e) => UpdateWeatherType();
        public WeatherTypes GetWeatherType() => CurWeatherTypes;

        public void UpdateWeatherType()
        {
            CurWeatherTypes = WeatherTypes.Default;
            WeatherTypes old = CurWeatherTypes;
            if (!defaultWeatherService.IsHazardousWeather)
            {
                CurWeatherTypes |= WeatherTypes.Temperate;
            }
            else
            {
                if( defaultHazardousWeatherService.CurrentCycleHazardousWeather.GetType() == typeof(DroughtWeather) ) { CurWeatherTypes |= WeatherTypes.Drought; }
                else if (defaultHazardousWeatherService.CurrentCycleHazardousWeather.GetType() == typeof(BadtideWeather)) { CurWeatherTypes |= WeatherTypes.Badtide; }
            }
            WeatherChange change = new()
            {
                ChangesFrom = old,
                ChangesTo = CurWeatherTypes
            };
            OnWeatherChanging(change);
        }
        public void OnWeatherChanging(WeatherChange weatherChange)
        {
            WeatherChanging?.Invoke(this, weatherChange);
        }
    }
}
