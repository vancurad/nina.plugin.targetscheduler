using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces;
using NINA.Plugin.Interfaces;
using NINA.Plugin.TargetScheduler.Controls.AcquiredImages;
using NINA.Plugin.TargetScheduler.Controls.DatabaseManager;
using NINA.Plugin.TargetScheduler.Controls.PlanPreview;
using NINA.Plugin.TargetScheduler.Database;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Grading;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Plugin.TargetScheduler.SyncService.Sync;
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

        // Plugin specific image file patterns
        public static readonly ImagePattern FlatSessionIdImagePattern = new ImagePattern("$$TSSESSIONID$$", "Session identifier for working with TS lights and flats", "Target Scheduler");

        public static readonly ImagePattern ProjectNameImagePattern = new ImagePattern("$$TSPROJECTNAME$$", "TS project name (if available)", "Target Scheduler");

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

            options.AddImagePattern(FlatSessionIdImagePattern);
            options.AddImagePattern(ProjectNameImagePattern);
        }

        public override Task Initialize() {
            InitPluginHome();

            if (SyncEnabled(profileService)) {
                SyncManager.Instance.Start(profileService);
            }

            TSLogger.Info("plugin initialized");
            return Task.CompletedTask;
        }

        private void InitPluginHome() {
            if (!Directory.Exists(Common.PLUGIN_HOME)) {
                Directory.CreateDirectory(Common.PLUGIN_HOME);
            }

            SchedulerDatabaseInteraction.BackupDatabase();
        }

        public static bool SyncEnabled(IProfileService profileService) {
            ProfilePreference profilePreference = new SchedulerPlanLoader(profileService.ActiveProfile).GetProfilePreferences();
            return profilePreference.EnableSynchronization;
        }

        private DatabaseManagerVM databaseManagerVM;

        public DatabaseManagerVM DatabaseManagerVM {
            get => databaseManagerVM;
            set {
                databaseManagerVM = value;
                RaisePropertyChanged(nameof(DatabaseManagerVM));
            }
        }

        private PlanPreviewerViewVM planPreviewerViewVM;

        public PlanPreviewerViewVM PlanPreviewerViewVM {
            get => planPreviewerViewVM;
            set {
                planPreviewerViewVM = value;
                RaisePropertyChanged(nameof(PlanPreviewerViewVM));
            }
        }

        private AcquiredImagesManagerViewVM acquiredImagesManagerViewVM;

        public AcquiredImagesManagerViewVM AcquiredImagesManagerViewVM {
            get => acquiredImagesManagerViewVM;
            set {
                acquiredImagesManagerViewVM = value;
                RaisePropertyChanged(nameof(AcquiredImagesManagerViewVM));
            }
        }

        private bool databaseManagerIsExpanded = false;

        public bool DatabaseManagerIsExpanded {
            get { return databaseManagerIsExpanded; }
            set {
                databaseManagerIsExpanded = value;
                if (value && DatabaseManagerVM == null) {
                    DatabaseManagerVM = new DatabaseManagerVM(profileService, applicationMediator, framingAssistantVM, deepSkyObjectSearchVM, planetariumFactory);
                }
            }
        }

        private bool planPreviewIsExpanded = false;

        public bool PlanPreviewIsExpanded {
            get { return planPreviewIsExpanded; }
            set {
                planPreviewIsExpanded = value;
                if (value && PlanPreviewerViewVM == null) {
                    PlanPreviewerViewVM = new PlanPreviewerViewVM(profileService);
                }
            }
        }

        private bool acquiredImagesManagerIsExpanded = false;

        public bool AcquiredImagesManagerIsExpanded {
            get { return acquiredImagesManagerIsExpanded; }
            set {
                acquiredImagesManagerIsExpanded = value;
                if (value && AcquiredImagesManagerViewVM == null) {
                    AcquiredImagesManagerViewVM = new AcquiredImagesManagerViewVM(profileService);
                }
            }
        }

        private void ProcessExited(object sender, EventArgs e) {
            TSLogger.Warning($"process exited");
        }

        public override Task Teardown() {
            ImageGradingController.Instance.Shutdown();

            if (SyncManager.Instance.IsRunning) {
                SyncManager.Instance.Shutdown();
            }

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
            DatabaseManagerVM = new DatabaseManagerVM(profileService, applicationMediator, framingAssistantVM, deepSkyObjectSearchVM, planetariumFactory);
            PlanPreviewerViewVM = new PlanPreviewerViewVM(profileService);
            AcquiredImagesManagerViewVM = new AcquiredImagesManagerViewVM(profileService);

            RaisePropertyChanged(nameof(DatabaseManagerVM));
            RaisePropertyChanged(nameof(PlanPreviewerViewVM));
            RaisePropertyChanged(nameof(AcquiredImagesManagerViewVM));

            if (profileService.ActiveProfile != null) {
                profileService.ActiveProfile.AstrometrySettings.PropertyChanged -= ProfileService_ProfileChanged;
                profileService.ActiveProfile.AstrometrySettings.PropertyChanged += ProfileService_ProfileChanged;

                if (SyncManager.Instance.IsRunning) {
                    SyncManager.Instance.Shutdown();
                    if (SyncEnabled(profileService)) {
                        SyncManager.Instance.Start(profileService);
                    }
                }
            }
        }
    }
}