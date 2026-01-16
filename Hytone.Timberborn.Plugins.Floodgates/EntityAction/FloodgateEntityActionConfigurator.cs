using Bindito.Core;
using Bindito.Core.Internal;
using Hytone.Timberborn.Plugins.Floodgates.EntityAction.WaterPumps;
using Hytone.Timberborn.Plugins.Floodgates.EntityAction.WaterSourceRegulators;
// using Timberborn.IrrigationSystem;
using Timberborn.TemplateInstantiation;
using Timberborn.WaterBuildings;
using Timberborn.WaterSourceSystem;

// using UnityEngine.InputSystem;

namespace Hytone.Timberborn.Plugins.Floodgates.EntityAction
{

    [Context("Game")]
    public class FloodgateEntityActionConfigurator : Configurator
    {
        protected override void Configure()
        {
            Bind<FloodgateTriggerMonoBehaviour>().AsTransient();
            Bind<WaterPumpMonobehaviour>().AsTransient();
            Bind<WaterSourceRegulatorMonobehaviour>().AsTransient();
            Bind<StreamGaugeMonoBehaviour>().AsTransient();
            Bind<StreamGaugeFloodgateLinkSerializer>().AsSingleton();
            Bind<WaterpumpStreamGaugeLinkSerializer>().AsSingleton();
            Bind<WaterSourceRegulatorLinkSerializer>().AsSingleton();
            Bind<EventListeners>().AsSingleton();
            MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        private static TemplateModule ProvideTemplateModule()
        {
            TemplateModule.Builder builder = new TemplateModule.Builder();
            builder.AddDecorator<Floodgate, FloodgateTriggerMonoBehaviour>();
            builder.AddDecorator<StreamGauge, StreamGaugeMonoBehaviour>();
            builder.AddDecorator<WaterSourceRegulator, WaterSourceRegulatorMonobehaviour>();
            builder.AddDecorator<WaterInput, WaterPumpMonobehaviour>();
            builder.AddDecorator<WaterOutput, WaterPumpMonobehaviour>();
            return builder.Build();
        }
    }

    // [HarmonyPatch(typeof(EntityService), "Instantiate", typeof(BaseComponent), typeof(Guid))]
    // class MinWindStrengthPatch
    // {
    //     public static void Postfix(BaseComponent __result)
    //     {
    //         if ((__result.GetComponent<WaterInput>() != null || __result.GetComponent<WaterOutput>() != null)
    //             && __result.Name.ToLower().Contains("shower") == false)
    //         {
    //             var baseInstantiator = DependencyContainer.GetInstance<BaseInstantiator>();
    //             baseInstantiator.AddComponent<WaterPumpMonobehaviour>(__result.GameObjectFast);
    //         }
    //     }
    // }

}
