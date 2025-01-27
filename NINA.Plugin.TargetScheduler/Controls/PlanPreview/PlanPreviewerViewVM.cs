using LinqKit;
using NINA.Core.MyMessageBox;
using NINA.Core.Utility;
using NINA.Plugin.TargetScheduler.Controls.Util;
using NINA.Plugin.TargetScheduler.Database;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Plugin.TargetScheduler.Util;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;

namespace NINA.Plugin.TargetScheduler.Controls.PlanPreview {

    public class PlanPreviewerViewVM : BaseVM {
        private SchedulerDatabaseInteraction database;

        public PlanPreviewerViewVM(IProfileService profileService) : base(profileService) {
            database = new SchedulerDatabaseInteraction();
            InstructionList = new ObservableCollection<TreeViewItem>();

            profileService.ProfileChanged += ProfileService_ProfileChanged;
            profileService.Profiles.CollectionChanged += ProfileService_ProfileChanged;

            InitializeCriteria();

            SetNowCommand = new RelayCommand(SetPreviewTimeNow);
            PlanPreviewCommand = new RelayCommand(RunPlanPreview);
            PlanPreviewResultsCommand = new RelayCommand(RunPlanPreviewResults);
        }

        private void ProfileService_ProfileChanged(object sender, EventArgs e) {
            InstructionList.Clear();
            SelectedProfileId = profileService.ActiveProfile.Id.ToString();
            ProfileChoices = GetProfileChoices();
        }

        private void InitializeCriteria() {
            PlanDate = DateTime.Now.Date;
            SelectedProfileId = profileService.ActiveProfile.Id.ToString();
            ProfileChoices = GetProfileChoices();

            ShowPlanPreview = true;
            ShowPlanPreviewResults = false;
        }

        private DateTime planDate = DateTime.MinValue;

        public DateTime PlanDate {
            get => planDate;
            set {
                planDate = value;
                SchedulerPlans = null;
                RaisePropertyChanged(nameof(PlanDate));
            }
        }

        private int planHours = 13;

        public int PlanHours {
            get => planHours;
            set {
                planHours = value;
                SchedulerPlans = null;
                RaisePropertyChanged(nameof(PlanHours));
            }
        }

        private int planMinutes = 0;

        public int PlanMinutes {
            get => planMinutes;
            set {
                planMinutes = value;
                SchedulerPlans = null;
                RaisePropertyChanged(nameof(PlanMinutes));
            }
        }

        private int planSeconds = 0;

        public int PlanSeconds {
            get => planSeconds;
            set {
                planSeconds = value;
                SchedulerPlans = null;
                RaisePropertyChanged(nameof(PlanSeconds));
            }
        }

        private AsyncObservableCollection<KeyValuePair<string, string>> profileChoices;

        public AsyncObservableCollection<KeyValuePair<string, string>> ProfileChoices {
            get {
                return profileChoices;
            }
            set {
                profileChoices = value;
                SchedulerPlans = null;
                RaisePropertyChanged(nameof(ProfileChoices));
            }
        }

        private string selectedProfileId;

        public string SelectedProfileId {
            get => selectedProfileId;
            set {
                selectedProfileId = value;
                SchedulerPlans = null;
                RaisePropertyChanged(nameof(SelectedProfileId));
            }
        }

        private ObservableCollection<TreeViewItem> instructionList;

        public ObservableCollection<TreeViewItem> InstructionList {
            get => instructionList;
            set {
                instructionList = value;
                RaisePropertyChanged(nameof(InstructionList));
            }
        }

        private List<SchedulerPlan> SchedulerPlans { get; set; }

        private bool showPlanPreview;

        public bool ShowPlanPreview {
            get => showPlanPreview;
            set {
                showPlanPreview = value;
                RaisePropertyChanged(nameof(ShowPlanPreview));
            }
        }

        private bool showPlanPreviewResults;

        public bool ShowPlanPreviewResults {
            get => showPlanPreviewResults;
            set {
                showPlanPreviewResults = value;
                RaisePropertyChanged(nameof(ShowPlanPreviewResults));
            }
        }

        public ICommand SetNowCommand { get; private set; }
        public ICommand PlanPreviewCommand { get; private set; }
        public ICommand PlanPreviewResultsCommand { get; private set; }

        private void LoadSchedulerPlans(DateTime atDateTime, IProfileService profileService) {
            /* While the caching here works and detects changes to the preview parameters (like date/time), it's not picking
             * up changes to the database.  For now just disable the caching ... doesn't take long to run anyway.

            if (SchedulerPlans != null) {
                return;
            }*/

            try {
                TSLogger.Debug($"running plan preview for {Utils.FormatDateTimeFull(atDateTime)}, profileId={SelectedProfileId}");

                SchedulerPlanLoader loader = new SchedulerPlanLoader(GetProfile(SelectedProfileId));
                List<IProject> projects = loader.LoadActiveProjects(database.GetContext());
                ProfilePreference profilePreference = loader.GetProfilePreferences(database.GetContext());

                ObservableCollection<TreeViewItem> list = new ObservableCollection<TreeViewItem>();
                string profileName = ProfileChoices.First(p => p.Key == selectedProfileId).Value;

                if (projects == null) {
                    TSLogger.Debug($"no active projects for preview at {atDateTime}, profileId={SelectedProfileId}");
                    InstructionList = list;

                    MyMessageBox.Show($"No active projects/targets were returned by the planner for {Utils.FormatDateTimeFull(atDateTime)} and{Environment.NewLine}profile '{profileName}' - or no active targets were found with active exposure plans.", "Oops");
                    SchedulerPlans = null;
                    return;
                }

                List<SchedulerPlan> schedulerPlans = new PreviewPlanner().GetPlanPreview(atDateTime, profileService, profilePreference, projects);
                if (schedulerPlans.Count == 0) {
                    TSLogger.Debug($"no imagable projects for preview at {atDateTime}, profileId={SelectedProfileId}");
                    InstructionList = list;

                    MyMessageBox.Show($"No imagable projects/targets were returned by the planner for {Utils.FormatDateTimeFull(atDateTime)} and{Environment.NewLine}profile '{profileName}'.", "Oops");
                    SchedulerPlans = null;
                    return;
                }

                SchedulerPlans = schedulerPlans;
                return;
            } catch (Exception ex) {
                TSLogger.Error($"failed to run plan preview: {ex.Message} {ex.StackTrace}");
                MyMessageBox.Show($"Exception running plan preview - see the TS log for details.", "Oops");
                SchedulerPlans = null;
                return;
            }
        }

        private void SetPreviewTimeNow() {
            DateTime now = DateTime.Now;
            PlanDate = now.Date;
            PlanHours = now.Hour;
            PlanMinutes = now.Minute;
            PlanSeconds = now.Second;

            RaisePropertyChanged(nameof(PlanDate));
            RaisePropertyChanged(nameof(PlanHours));
            RaisePropertyChanged(nameof(PlanMinutes));
            RaisePropertyChanged(nameof(PlanSeconds));
        }

        private void RunPlanPreview() {
            ObservableCollection<TreeViewItem> list = new ObservableCollection<TreeViewItem>();

            if (PlanDate == DateTime.MinValue || SelectedProfileId == null) {
                return;
            }

            try {
                DateTime atDateTime = PlanDate.Date.AddHours(PlanHours).AddMinutes(PlanMinutes).AddSeconds(PlanSeconds);
                LoadSchedulerPlans(atDateTime, profileService);

                if (SchedulerPlans == null || SchedulerPlans.Count == 0) {
                    return;
                }

                int lastTargetId = -1;
                string lastFilterName = null;
                TreeViewItem planItem = null;

                foreach (SchedulerPlan plan in SchedulerPlans) {
                    if (plan.IsWait) {
                        planItem = new TreeViewItem();
                        planItem.Header = $"Wait until {Utils.FormatDateTimeFull(plan.WaitForNextTargetTime)}";
                        list.Add(planItem);
                        lastTargetId = -1;
                        continue;
                    }

                    if (plan.PlanTarget.DatabaseId != lastTargetId) {
                        lastTargetId = plan.PlanTarget.DatabaseId;
                        planItem = new TreeViewItem();
                        planItem.Header = GetTargetLabel(plan);
                        planItem.IsExpanded = false;
                        list.Add(planItem);
                    }

                    foreach (IInstruction instruction in plan.PlanInstructions) {
                        TreeViewItem instructionItem = new TreeViewItem();

                        if (instruction is PlanMessage
                            || instruction is PlanBeforeNewTargetContainer
                            || instruction is PlanPostExposure) {
                            continue;
                        }

                        if (instruction is PlanSlew) {
                            instructionItem.Header = GetSlewLabel(plan.PlanTarget, (PlanSlew)instruction);
                            planItem.Items.Add(instructionItem);
                            continue;
                        }

                        if (instruction is PlanSwitchFilter) {
                            string filterName = ((PlanSwitchFilter)instruction).exposure.FilterName;
                            if (filterName != lastFilterName) {
                                lastFilterName = filterName;
                                instructionItem.Header = $"Switch Filter: {filterName}";
                                planItem.Items.Add(instructionItem);
                            }
                            continue;
                        }

                        if (instruction is PlanSetReadoutMode) {
                            int? readoutMode = ((PlanSetReadoutMode)instruction).exposure.ReadoutMode;
                            if (readoutMode != null && readoutMode > 0) {
                                instructionItem.Header = $"Set readout mode: {readoutMode}";
                                planItem.Items.Add(instructionItem);
                            }
                            continue;
                        }

                        if (instruction is PlanTakeExposure) {
                            instructionItem.Header = GetTakeExposureLabel((PlanTakeExposure)instruction);
                            planItem.Items.Add(instructionItem);
                            continue;
                        }

                        if (instruction is PlanDither) {
                            instructionItem.Header = "Dither";
                            planItem.Items.Add(instructionItem);
                            continue;
                        }

                        TSLogger.Error($"unknown instruction type in plan preview: {instruction.GetType().FullName}");
                        throw new Exception($"unknown instruction type in plan preview: {instruction.GetType().FullName}");
                    }
                }

                InstructionList = list;
                ShowPlanPreviewResults = false;
                ShowPlanPreview = true;
            } catch (Exception ex) {
                TSLogger.Error($"failed to run plan preview: {ex.Message} {ex.StackTrace}");
                InstructionList.Clear();
            }
        }

        private string planPreviewResultsLog;

        public string PlanPreviewResultsLog {
            get => planPreviewResultsLog;
            set {
                planPreviewResultsLog = value;
                RaisePropertyChanged(nameof(PlanPreviewResultsLog));
            }
        }

        private void RunPlanPreviewResults() {
            if (PlanDate == DateTime.MinValue || SelectedProfileId == null) {
                return;
            }

            try {
                DateTime atDateTime = PlanDate.Date.AddHours(PlanHours).AddMinutes(PlanMinutes).AddSeconds(PlanSeconds);
                LoadSchedulerPlans(atDateTime, profileService);

                if (SchedulerPlans == null || SchedulerPlans.Count == 0) {
                    return;
                }

                StringBuilder sb = new StringBuilder();
                foreach (SchedulerPlan plan in SchedulerPlans) {
                    sb.Append(plan.DetailsLog);
                }

                sb.AppendLine("\nRUN COMPLETE - NO MORE TARGETS AVAILABLE");
                PlanPreviewResultsLog = sb.ToString();
                ShowPlanPreview = false;
                ShowPlanPreviewResults = true;
            } catch (Exception ex) {
                TSLogger.Error($"failed to run plan preview results: {ex.Message} {ex.StackTrace}");
                PlanPreviewResultsLog = string.Empty;
            }
        }

        private AsyncObservableCollection<KeyValuePair<string, string>> GetProfileChoices() {
            Dictionary<string, string> profiles = new Dictionary<string, string>();
            profileService.Profiles.ForEach(p => {
                profiles.Add(p.Id.ToString(), p.Name);
            });

            AsyncObservableCollection<KeyValuePair<string, string>> profileChoices = new AsyncObservableCollection<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, string> entry in profiles) {
                profileChoices.Add(new KeyValuePair<string, string>(entry.Key, entry.Value));
            }

            return profileChoices;
        }

        private IProfile GetProfile(string profileId) {
            foreach (ProfileMeta profileMeta in profileService.Profiles) {
                if (profileMeta.Id.ToString() == profileId) {
                    return ProfileLoader.Load(profileService, profileMeta);
                }
            }

            TSLogger.Error($"failed to get profile for ID={profileId}");
            throw new Exception($"failed to get profile for ID={profileId}");
        }

        private string GetTargetLabel(SchedulerPlan plan) {
            string label = $"{plan.PlanTarget.Project.Name} / {plan.PlanTarget.Name}";
            return $"{label} - start: {Utils.FormatDateTimeFull(plan.StartTime)}";
        }

        private string GetSlewLabel(ITarget planTarget, PlanSlew planSlew) {
            string name = "Slew";
            string rotate = $", Rotate: {planTarget.Rotation}°";

            if (planSlew.center) {
                name = "Slew/Rotate/Center";
            }

            return $"{name}: {planTarget.Coordinates.RAString} {planTarget.Coordinates.DecString}{rotate}";
        }

        private string GetTakeExposureLabel(PlanTakeExposure instruction) {
            IExposure planExposure = instruction.exposure;
            StringBuilder sb = new StringBuilder();
            sb.Append("Take Exposure:");
            sb.Append($" {planExposure.ExposureLength} secs, ");
            sb.Append($" Gain={CameraDefault(planExposure.Gain)}, ");
            sb.Append($" Offset={CameraDefault(planExposure.Offset)}, ");
            sb.Append($" Binning={planExposure.BinningMode}");

            return sb.ToString();
        }

        private string CameraDefault(int? value) {
            return value != null ? value.ToString() : "(camera)";
        }
    }
}