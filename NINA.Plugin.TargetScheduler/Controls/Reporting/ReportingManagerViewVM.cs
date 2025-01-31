using CommunityToolkit.Mvvm.Input;
using NINA.Core.Locale;
using NINA.Core.Utility;
using NINA.Plugin.TargetScheduler.Controls.AcquiredImages;
using NINA.Plugin.TargetScheduler.Database;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Plugin.TargetScheduler.Util;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NINA.Plugin.TargetScheduler.Controls.Reporting {

    public class ReportingManagerViewVM : BaseVM {
        private SchedulerDatabaseInteraction database;

        public ReportingManagerViewVM(IProfileService profileService) : base(profileService) {
            database = new SchedulerDatabaseInteraction();
            RefreshTableCommand = new AsyncRelayCommand(RefreshTable);
            InitializeCriteria();

            ReportRowCollection = new ReportRowCollection();
            ItemsView = CollectionViewSource.GetDefaultView(ReportRowCollection);
            ItemsView.SortDescriptions.Clear();
            ItemsView.SortDescriptions.Add(new SortDescription("AcquiredDate", ListSortDirection.Descending));
        }

        private void InitializeCriteria() {
            SearchCriteraKey = null;
            selectedTargetId = 0;
            selectedFilterId = -1;
            selectedTarget = null;
            TargetChoices = GetTargetChoices();
            FilterChoices = GetFilterChoices();
        }

        private bool tableLoading = false;

        public bool TableLoading {
            get => tableLoading;
            set {
                tableLoading = value;
                RaisePropertyChanged(nameof(TableLoading));
            }
        }

        private ICollectionView itemsView;
        public ICollectionView ItemsView { get => itemsView; set { itemsView = value; } }

        private AsyncObservableCollection<KeyValuePair<int, string>> GetTargetChoices() {
            AsyncObservableCollection<KeyValuePair<int, string>> choices = new AsyncObservableCollection<KeyValuePair<int, string>> {
                new KeyValuePair<int, string>(0, "Select")
            };

            using (var context = database.GetContext()) {
                List<Project> projects = context.GetAllProjects();
                projects.ForEach(p => {
                    p.Targets.ForEach(t => {
                        choices.Add(new KeyValuePair<int, string>(t.Id, $"{t.Project.Name} / {t.Name}"));
                    });
                });
            }

            return choices;
        }

        private AsyncObservableCollection<KeyValuePair<int, string>> targetChoices;

        public AsyncObservableCollection<KeyValuePair<int, string>> TargetChoices {
            get => targetChoices;
            set {
                targetChoices = value;
                RaisePropertyChanged(nameof(TargetChoices));
            }
        }

        private int selectedTargetId = 0;

        public int SelectedTargetId {
            get => selectedTargetId;
            set {
                selectedTargetId = value;
                SelectedTarget = selectedTargetId != 0 ? GetTarget(selectedTargetId) : null;

                SelectedFilterId = -1;
                FilterChoices = GetFilterChoices();
                RaisePropertyChanged(nameof(SelectedTargetId));
                _ = LoadRecords();
            }
        }

        private Target selectedTarget;
        public Target SelectedTarget { get => selectedTarget; set => selectedTarget = value; }

        private Target GetTarget(int selectedTargetId) {
            using (var context = database.GetContext()) {
                Target t = context.GetTargetOnly(selectedTargetId);
                t = context.GetTarget(t.ProjectId, selectedTargetId);
                t.Project = context.GetProject(t.ProjectId);
                return t;
            }
        }

        private AsyncObservableCollection<KeyValuePair<int, string>> GetFilterChoices() {
            AsyncObservableCollection<KeyValuePair<int, string>> choices = new AsyncObservableCollection<KeyValuePair<int, string>> {
                new KeyValuePair<int, string>(-1, Loc.Instance["LblAny"])
            };

            if (SelectedTargetId == 0) {
                return choices;
            }

            for (int i = 0; i < SelectedTarget.ExposurePlans.Count; i++) {
                ExposurePlan exposurePlan = SelectedTarget.ExposurePlans[i];
                choices.Add(new KeyValuePair<int, string>(i, exposurePlan.ExposureTemplate.FilterName));
            }

            return choices;
        }

        private AsyncObservableCollection<KeyValuePair<int, string>> filterChoices;

        public AsyncObservableCollection<KeyValuePair<int, string>> FilterChoices {
            get => filterChoices;
            set {
                filterChoices = value;
                RaisePropertyChanged(nameof(FilterChoices));
            }
        }

        private int selectedFilterId = -1;

        public int SelectedFilterId {
            get => selectedFilterId;
            set {
                selectedFilterId = value;
                RaisePropertyChanged(nameof(SelectedFilterId));
                _ = LoadRecords();
            }
        }

        public ICommand RefreshTableCommand { get; private set; }

        private async Task<bool> RefreshTable() {
            InitializeCriteria();
            RaisePropertyChanged(nameof(SelectedTargetId));
            RaisePropertyChanged(nameof(SelectedFilterId));
            await LoadRecords();
            return true;
        }

        private ReportRowCollection reportRowCollection;

        public ReportRowCollection ReportRowCollection {
            get => reportRowCollection;
            set {
                reportRowCollection = value;
                RaisePropertyChanged(nameof(ReportRowCollection));
            }
        }

        private ReportTableSummary reportTableSummary;

        public ReportTableSummary ReportTableSummary {
            get => reportTableSummary;
            set {
                reportTableSummary = value;
                RaisePropertyChanged(nameof(ReportTableSummary));
            }
        }

        private static Dispatcher _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

        /* TODO:
         *   - Additional summary - sounds like a table
         *
         *                           Total         Accepted      Rejected       Pending
         *                All:        2h0m            1h32m
         *                Lum:        0h8m             0h7m
         *                Red:        0h8m             0h7m
         *
         *   Should pull exposure duration from AI, not from EP(ET) which could change
         *
         *   To fit this, may want to reorg:
         *   - When you select a target, generate new table plus existing with Filter=Any
         *   - The Filter dropdown moves below the table and drives existing summary and AI rows
         *
         *
*/

        private async Task<bool> LoadRecords() {
            return await Task.Run(() => {
                if (ReportRowCollection == null || SelectedTargetId == 0) {
                    ReportTableSummary = new ReportTableSummary();
                    _dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                        ReportRowCollection.Clear();
                        ReportRowCollection.AddRange(new List<ReportRowVM>());
                    }));

                    RaisePropertyChanged(nameof(ReportRowCollection));
                    RaisePropertyChanged(nameof(ItemsView));
                    return true;
                }

                string newSearchCriteraKey = GetSearchCriteraKey();
                if (newSearchCriteraKey == SearchCriteraKey) {
                    return true;
                }

                // Slight delay allows the UI thread to update the spinner property before the dispatcher
                // thread starts ... which seems to block the UI updates.
                TableLoading = true;
                Thread.Sleep(50);

                try {
                    SearchCriteraKey = newSearchCriteraKey;

                    List<int> exposurePlanIds = new List<int>();
                    string filterName = "any";
                    if (SelectedFilterId != -1) {
                        exposurePlanIds.Add(SelectedTarget.ExposurePlans[SelectedFilterId].Id);
                        filterName = SelectedTarget.ExposurePlans[SelectedFilterId].ExposureTemplate.FilterName;
                    } else {
                        SelectedTarget.ExposurePlans.ForEach(ep => exposurePlanIds.Add(ep.Id));
                    }

                    List<AcquiredImage> acquiredImages;
                    using (var context = database.GetContext()) {
                        acquiredImages = context.AcquiredImageSet
                        .AsNoTracking()
                        .Where(ai => ai.TargetId == SelectedTargetId && exposurePlanIds.Contains(ai.ExposureId))
                        .ToList();
                    }

                    // Create an intermediate list so we can add it to the display collection via AddRange while suppressing notifications
                    List<ReportRowVM> reportRowVMs = new List<ReportRowVM>(acquiredImages.Count);
                    acquiredImages.ForEach(ai => { reportRowVMs.Add(new ReportRowVM(database, ai)); });
                    ReportTableSummary = new ReportTableSummary(acquiredImages, SelectedTarget.Project.Name, SelectedTarget.Name, filterName);

                    _dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                        ReportRowCollection.Clear();
                        ReportRowCollection.AddRange(reportRowVMs);
                    }));
                } catch (Exception ex) {
                    TSLogger.Error($"exception loading acquired images: {ex.Message} {ex.StackTrace}");
                } finally {
                    RaisePropertyChanged(nameof(ReportRowCollection));
                    RaisePropertyChanged(nameof(ItemsView));
                    TableLoading = false;
                }

                return true;
            });
        }

        private string SearchCriteraKey;

        private string GetSearchCriteraKey() {
            return $"{SelectedTargetId}_{SelectedFilterId}";
        }
    }

    public class ReportTableSummary {
        public string Title { get; private set; }
        public string DateRange { get; private set; }
        public string StarsRange { get; private set; }
        public string HFRRange { get; private set; }
        public string FWHMRange { get; private set; }
        public string EccentricityRange { get; private set; }

        public ReportTableSummary(List<AcquiredImage> acquiredImages, string projectName, string targetName, string filterName) {
            // TODO: should do fixed formatting so they line up

            Title = $"{projectName} / {targetName} ({filterName})";

            if (Common.IsEmpty(acquiredImages)) return;

            DateTime minDate = DateTime.MaxValue;
            acquiredImages.ForEach(x => { if (x.AcquiredDate < minDate) minDate = x.AcquiredDate; });
            DateTime maxDate = DateTime.MinValue;
            acquiredImages.ForEach(x => { if (x.AcquiredDate > maxDate) maxDate = x.AcquiredDate; });
            DateRange = $"{Utils.FormatDateTimeFull(minDate)}  to  {Utils.FormatDateTimeFull(maxDate)}";

            List<double> samples = GetSamples(acquiredImages, i => { return i.Metadata.DetectedStars; });
            StarsRange = $"{samples.Min()} - {samples.Max()}";

            samples = GetSamples(acquiredImages, i => { return i.Metadata.HFR; });
            HFRRange = $"{Utils.FormatDbl(samples.Min())} - {Utils.FormatDbl(samples.Max())}";

            samples = GetSamples(acquiredImages, i => { return i.Metadata.FWHM; });
            double min = samples.Min(); double max = samples.Max();
            FWHMRange = min > 0 && max > 0 ? $"{Utils.FormatDbl(min)} - {Utils.FormatDbl(max)}" : "n/a";

            samples = GetSamples(acquiredImages, i => { return i.Metadata.Eccentricity; });
            min = samples.Min(); max = samples.Max();
            EccentricityRange = min > 0 && max > 0 ? $"{Utils.FormatDbl(min)} - {Utils.FormatDbl(max)}" : "n/a";
        }

        public ReportTableSummary() {
        }

        private List<double> GetSamples(List<AcquiredImage> list, Func<AcquiredImage, double> Sample) {
            List<double> samples = new List<double>();
            list.ForEach(i => samples.Add(Sample(i)));
            return samples;
        }
    }

    public class ReportRowCollection : RangeObservableCollection<ReportRowVM> { }

    public class ReportRowVM {
        private AcquiredImage acquiredImage;
        private SchedulerDatabaseInteraction database;

        public ReportRowVM(SchedulerDatabaseInteraction database, AcquiredImage acquiredImage) {
            this.acquiredImage = acquiredImage;
            this.database = database;
        }

        public DateTime AcquiredDate { get { return acquiredImage.AcquiredDate; } }
        public string FilterName { get { return acquiredImage.FilterName; } }
        public string ExposureDuration { get { return Utils.FormatDbl(acquiredImage.Metadata.ExposureDuration); } }
        public string GradingStatus { get { return acquiredImage.GradingStatus.ToString(); } }
        public string RejectReason { get { return acquiredImage.RejectReason; } }

        public string DetectedStars { get { return Utils.FormatInt(acquiredImage.Metadata.DetectedStars); } }
        public string HFR { get { return Utils.FormatDbl(acquiredImage.Metadata.HFR); } }
        public string FWHM { get { return Utils.FormatHF(acquiredImage.Metadata.FWHM); } }
        public string Eccentricity { get { return Utils.FormatHF(acquiredImage.Metadata.Eccentricity); } }
        public string GuidingRMS { get { return Utils.FormatDbl(acquiredImage.Metadata.GuidingRMS); } }

        private ImageData imageData;

        public ImageData ImageData {
            get {
                if (imageData == null) {
                    using (var context = database.GetContext()) {
                        imageData = context.GetImageData(acquiredImage.Id);
                    }
                }

                return imageData;
            }
        }

        private ImageSource thumbnail;

        public ImageSource Thumbnail {
            get {
                if (thumbnail == null) {
                    thumbnail = ImageData != null
                            ? Thumbnails.RestoreThumbnail(imageData.Data)
                            : null;
                }

                return thumbnail;
            }
        }

        public int ThumbnailWidth { get => ImageData != null ? ImageData.Width : 0; }
        public int ThumbnailHeight { get => ImageData != null ? ImageData.Height : 0; }
    }
}