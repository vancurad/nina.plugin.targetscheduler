using NINA.Core.Utility;
using NINA.Equipment.Interfaces;
using NINA.Plugin.Interfaces;
using NINA.Plugin.TargetScheduler.Database;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
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
            InitPluginHome();

            // TODO
            /*
            if (SyncEnabled(profileService)) {
                SyncManager.Instance.Start(profileService);
            }*/

            TSLogger.Info("plugin initialized");

            // TODO: tmp just to bootstrap database
            ProfilePreference profilePreference = new SchedulerPlanLoader(profileService.ActiveProfile).GetProfilePreferences();
            profilePreference.MaxGradingSampleSize = 1000;
            profilePreference.RMSPixelThreshold = 1.2345;
            SchedulerDatabaseContext context = new SchedulerDatabaseInteraction().GetContext();
            using (context) {
                context.ProfilePreferenceSet.Add(profilePreference);
                context.SaveChanges();
            }

            return Task.CompletedTask;
        }

        private void InitPluginHome() {
            if (!Directory.Exists(Common.PLUGIN_HOME)) {
                Directory.CreateDirectory(Common.PLUGIN_HOME);
            }

            // TODO
            //SchedulerDatabaseInteraction.BackupDatabase();
        }

        public override Task Teardown() {
            /* TODO
            if (SyncManager.Instance.IsRunning) {
                SyncManager.Instance.Shutdown();
            }*/

            profileService.ProfileChanged -= ProfileService_ProfileChanged;
            TSLogger.Info("closing log");
            TSLogger.CloseAndFlush();
            return Task.CompletedTask;
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