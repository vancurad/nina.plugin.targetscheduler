using NINA.Core.Utility;
using NINA.Equipment.Interfaces;
using NINA.Plugin.Interfaces;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NINA.Plugin.TargetScheduler {

    [Export(typeof(IPluginManifest))]
    public class TargetScheduler : PluginBase, INotifyPropertyChanged {
        private IPluginOptionsAccessor pluginSettings;
        private IProfileService profileService;
        private IApplicationMediator applicationMediator;
        private IFramingAssistantVM framingAssistantVM;
        private IDeepSkyObjectSearchVM deepSkyObjectSearchVM;
        private IPlanetariumFactory planetariumFactory;

        [ImportingConstructor]
        public TargetScheduler(IProfileService profileService,
            IOptionsVM options,
            IApplicationMediator applicationMediator,
            IFramingAssistantVM framingAssistantVM,
            IDeepSkyObjectSearchVM deepSkyObjectSearchVM,
            IPlanetariumFactory planetariumFactory) {
            if (Properties.Settings.Default.UpdateSettings) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Properties.Settings.Default);
            }

            this.pluginSettings = new PluginOptionsAccessor(profileService, Guid.Parse(this.Identifier));
            this.profileService = profileService;
            this.applicationMediator = applicationMediator;
            this.framingAssistantVM = framingAssistantVM;
            this.deepSkyObjectSearchVM = deepSkyObjectSearchVM;
            this.planetariumFactory = planetariumFactory;

            profileService.ProfileChanged += ProfileService_ProfileChanged;
        }

        public override Task Initialize() {
            return base.Initialize();
        }

        public override Task Teardown() {
            return base.Teardown();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ProfileService_ProfileChanged(object? sender, EventArgs e) {
            // TODO:
            //AssistantManagerVM = new AssistantManagerVM(profileService, applicationMediator, framingAssistantVM, deepSkyObjectSearchVM, planetariumFactory);
            //PlanPreviewerViewVM = new PlanPreviewerViewVM(profileService);
            //AcquiredImagesManagerViewVM = new AcquiredImagesManagerViewVM(profileService);

            //RaisePropertyChanged(nameof(AssistantManagerVM));
            //RaisePropertyChanged(nameof(PlanPreviewerViewVM));
            //RaisePropertyChanged(nameof(AcquiredImagesManagerViewVM));

            if (profileService.ActiveProfile != null) {
                profileService.ActiveProfile.AstrometrySettings.PropertyChanged -= ProfileService_ProfileChanged;
                profileService.ActiveProfile.AstrometrySettings.PropertyChanged += ProfileService_ProfileChanged;

                // TODO:
                /*
                if (SyncManager.Instance.IsRunning) {
                    SyncManager.Instance.Shutdown();
                    if (SyncEnabled(profileService)) {
                        SyncManager.Instance.Start(profileService);
                    }
                }*/
            }
        }
    }
}