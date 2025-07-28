using Audio;
using Configs;
using Managers;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] BounceConfig bounceConfig;
    [SerializeField] AudioPool audioPoolPrefab;

    public override void InstallBindings()
    {

        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<SfxSignal>();

        // Singletons
        Container.Bind<GameManager>()
            .FromComponentInHierarchy() // finds the one in the scene
            .AsSingle()
            .NonLazy();

        // ScriptableObjects
        if (bounceConfig != null)
            Container.BindInstance(bounceConfig);

        // Audio
        // Spawn AudioPool once 
        Container.Bind<AudioPool>()
            .FromComponentInNewPrefab(audioPoolPrefab)
            .AsSingle()
            .NonLazy();

        Container.BindSignal<SfxSignal>()
            .ToMethod<AudioPool>(x => x.HandleSignal)
            .FromResolve();

    }
}