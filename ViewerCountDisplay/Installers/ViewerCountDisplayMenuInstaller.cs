using ViewerCountDisplay.Models;
using Zenject;

namespace ViewerCountDisplay.Installers
{
    public class ViewerCountDisplayMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<ViewerCountDisplayController>().AsSingle().NonLazy();
        }
    }
}