using HarmonyLib;
using Hytone.Timberborn.Plugins.Floodgates.EntityAction;
using ModdableWeathers.Services;
using ModdableWeathers.Weathers;
using Timberborn.Persistence;
using Timberborn.WorldPersistence;
using UnityEngine;

namespace MW_compat_SFC
{
    internal class MW_WeatherTypeGetter : IWeatherTypeGetter
    {
        private readonly ModdableWeatherService moddableWeatherService;
        private readonly ModdableWaterStrengthModifierService waterStrengthModifierService;
        private readonly ModdableWaterContaminationModifierService waterContaminationModifierService;
        private readonly ModdableWeatherRegistry weatherRegistry;
        public float WaterStrengthModifierTarget;
        public float WaterContaminationModifierTarget;

        //private ISingletonSaver singletonSaver;
        public ISingletonLoader SingletonLoader { get; init; }

        private WeatherTypes curWeatherTypes;
        private PropertyKey<float> waterStrengthKey = new("waterStrengthKey");
        private PropertyKey<float> waterContaminationKey = new("waterContaminationKey");
        private PropertyKey<WeatherTypes> weatherTypesKey = new("weatherTypesKey");



        public event WeatherChanged? WeatherChanging;

        public WeatherTypes CurWeatherTypes { get => curWeatherTypes; private set => curWeatherTypes = value; }

        public MW_WeatherTypeGetter(ModdableWeatherService ModdableWeatherService,
                                    ModdableWaterStrengthModifierService WaterStrengthModifierService,
                                    ModdableWaterContaminationModifierService WaterContaminationModifierService,
                                    ModdableWeatherRegistry WeatherRegistry,
                                    //ISingletonSaver saver,
                                    ISingletonLoader loader)
        {
            //singletonSaver = saver;
            SingletonLoader = loader;
            moddableWeatherService = ModdableWeatherService;
            waterStrengthModifierService = WaterStrengthModifierService;
            waterContaminationModifierService = WaterContaminationModifierService;
            weatherRegistry = WeatherRegistry;
            weatherRegistry.Weathers.Do(x => {
                Debug.unityLogger.Log("{MW_WeatherTypeGetter}{ctor} Weather " + x.Id + " in MW_WeatherRegistry");

            });
        }

        /*private void WaterContaminationModifierService_OnTickingChanged(bool obj)
        {
            if (obj)
            {
                WaterContaminationModifierTarget = waterContaminationModifierService.TargetModifier;
                Debug.unityLogger.Log("{MW_WeatherTypeGetter}{WaterContaminationModifierService_OnTickingChanged} Weather possibly changing / target modifier was likely changed / CALL UpdateWeatherType");
                UpdateWeatherType();
            }
        }

        private void WaterStrengthModifierService_OnTickingChanged(bool obj)
        {
            if (obj)
            {
                WaterStrengthModifierTarget = waterStrengthModifierService.TargetModifier;
                Debug.unityLogger.Log("{MW_WeatherTypeGetter}{WaterStrengthModifierService_OnTickingChanged} Weather possibly changing / target modifier was likely changed / CALL UpdateWeatherType");
                UpdateWeatherType();
            }
        }*/

        private void OnWeatherChanged(IModdableWeather weather, bool active, bool onLoad)
        {
            if (!active)
            {
                return;
            }
            WaterStrengthModifierTarget = waterStrengthModifierService.TargetModifier;
            WaterContaminationModifierTarget = waterContaminationModifierService.TargetModifier;
            Debug.unityLogger.Log("{MW_WeatherTypeGetter}{OnWeatherChanged} Weather Changing / into [ " + weather.Id + " ] / CALL UpdateWeatherType");
            UpdateWeatherType();
        }
        public void PostLoad()
        {
            Debug.unityLogger.Log("{MW_WeatherTypeGetter}{PostLoad}");
            //waterStrengthModifierService.OnTickingChanged += WaterStrengthModifierService_OnTickingChanged;
            //waterContaminationModifierService.OnTickingChanged += WaterContaminationModifierService_OnTickingChanged;
            weatherRegistry.Weathers.Do(x => {
                x.WeatherChanged += OnWeatherChanged;
                Debug.unityLogger.Log("{MW_WeatherTypeGetter}{PostLoad} Weather " + x.Id + ", registering handler {OnWeatherChanged}");

            });
            if ((int)CurWeatherTypes == 0)
            {
                WaterStrengthModifierTarget = waterStrengthModifierService.TargetModifier;
                WaterContaminationModifierTarget = waterContaminationModifierService.TargetModifier;
                UpdateWeatherType();
            }
            else
            {
                OnWeatherChanging(weatherChange: new()
                {
                    ChangesFrom = CurWeatherTypes,
                    ChangesTo = CurWeatherTypes
                });
            }
        }

        /*[OnEvent]
        public void OnWeatherTransitioned(WeatherTransitionedEvent weatherTransitionedEvent)
        {
            var prev_weather = weatherTransitionedEvent.From.GetValueOrDefault().Weather;
            var new_weather = weatherTransitionedEvent.To.Weather;
            Debug.unityLogger.Log("{MW_WeatherTypeGetter}{OnWeatherTranistioned} Weather Changing from / " + prev_weather.Id + " / into [ " + new_weather.Id + " ] / CALL UpdateWeatherType");
            WaterStrengthModifierTarget = waterStrengthModifierService.TargetModifier;
            WaterContaminationModifierTarget = waterContaminationModifierService.TargetModifier;
            UpdateWeatherType();
        }*/

        public WeatherTypes GetWeatherType() => CurWeatherTypes;
        public void UpdateWeatherType()
        {
            WeatherTypes old = CurWeatherTypes;
            CurWeatherTypes = new();
            CurWeatherTypes = WeatherTypes.Default;
            if (WaterStrengthModifierTarget <= 0) { CurWeatherTypes |= WeatherTypes.Drought; }
            if (WaterContaminationModifierTarget > 0) { CurWeatherTypes |= WeatherTypes.Badtide; }
            if (WaterContaminationModifierTarget < 0) { CurWeatherTypes |= WeatherTypes.Goodtide; }
            if (WaterStrengthModifierTarget > 1) { CurWeatherTypes |= WeatherTypes.Surge; }
            if (!moddableWeatherService.IsHazardousWeather)
            {
                CurWeatherTypes |= WeatherTypes.Temperate;
            }
            WeatherChange change = new()
            {
                ChangesFrom = old,
                ChangesTo = CurWeatherTypes
            };
            //Save();
            OnWeatherChanging(change);
        }

        public void OnWeatherChanging(WeatherChange weatherChange)
        {
            Debug.unityLogger.Log("{MW_WeatherTypeGetter}{Weather-Changing event invokation} Weather Changing from / " + weatherChange.ChangesFrom.ToString() + " / into [ " + weatherChange.ChangesTo.ToString() + " ]");

            WeatherChanging?.Invoke(this, weatherChange);
        }
        public void Save(ISingletonSaver singletonSaver)
        {
            IObjectSaver objectSaver = singletonSaver.GetSingleton((this as IWeatherTypeGetter).WeatherTypesKey);
            objectSaver.Set(weatherTypesKey, curWeatherTypes);
            objectSaver.Set(waterContaminationKey, WaterContaminationModifierTarget);
            objectSaver.Set(waterStrengthKey, WaterStrengthModifierTarget);
        }

        public void Load()
        {
            if (SingletonLoader.TryGetSingleton((this as IWeatherTypeGetter).WeatherTypesKey, out IObjectLoader loader))
            {
                CurWeatherTypes = (loader.Has(weatherTypesKey)) ? loader.Get(weatherTypesKey) : WeatherTypes.Default;
                WaterContaminationModifierTarget = loader.Has(waterContaminationKey) ? loader.Get(waterContaminationKey) : waterContaminationModifierService.TargetModifier;
                WaterStrengthModifierTarget = loader.Has(waterStrengthKey) ? loader.Get(waterStrengthKey) : waterStrengthModifierService.TargetModifier;
            }
        }
    }
}
