using System;
using System.Collections.Generic;
using System.Text;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using Timberborn.WorldPersistence;
using UnityEngine;

namespace Hytone.Timberborn.Plugins.Floodgates.EntityAction
{
    public interface IWeatherTypeGetter: ISaveableSingleton, ILoadableSingleton, IPostLoadableSingleton
    {
        public event WeatherChanged WeatherChanging;
        public abstract WeatherTypes GetWeatherType();

        public ISingletonLoader SingletonLoader { get; init; }
        public SingletonKey WeatherTypesKey => new("WeatherTypesKey");

        public abstract void OnWeatherChanging(WeatherChange weatherChange);
    }

    public delegate void WeatherChanged(IWeatherTypeGetter weatherTypeGetter, WeatherChange weatherChange);

    public class WeatherChange : EventArgs
    {
        public WeatherTypes ChangesFrom { get;  init; }
        public WeatherTypes ChangesTo { get; init; } = new WeatherTypes();
    }

    public enum WeatherTypes
    {
        Default = 0,
        Drought = 1,
        Badtide = 2,
        Surge = 4,
        Goodtide = 8,
        Temperate = 16
    }
}
