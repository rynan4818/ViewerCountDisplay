using ViewerCountDisplay.Models;
using Zenject;

namespace ViewerCountDisplay.Installers
{
    public class ViewerCountDisplayAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<ViewerCountDisplayController>().AsSingle().NonLazy();
            this.Container.BindInterfacesAndSelfTo<CatCoreManager>().AsSingle().NonLazy();
        }
    }
}
