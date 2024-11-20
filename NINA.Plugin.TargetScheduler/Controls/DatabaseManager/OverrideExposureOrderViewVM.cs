using CommunityToolkit.Mvvm.Input;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NINA.Plugin.TargetScheduler.Controls.DatabaseManager {

    public class OverrideExposureOrderViewVM : BaseVM {
        private static object lockObj = new object();
        private TargetViewVM targetViewVM;

        public OverrideExposureOrderViewVM(TargetViewVM targetViewVM, IProfileService profileService) : base(profileService) {
            this.targetViewVM = targetViewVM;

            MoveItemUpCommand = new RelayCommand<object>(MoveItemUp);
            MoveItemDownCommand = new RelayCommand<object>(MoveItemDown);
            CopyItemCommand = new RelayCommand<object>(CopyItem);
            DeleteItemCommand = new RelayCommand<object>(DeleteItem);
            InsertDitherCommand = new RelayCommand<object>(InsertDither);

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        public ICommand MoveItemUpCommand { get; private set; }
        public ICommand MoveItemDownCommand { get; private set; }
        public ICommand CopyItemCommand { get; private set; }
        public ICommand DeleteItemCommand { get; private set; }
        public ICommand InsertDitherCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private OverrideExposureOrderOld overrideExposureOrderOld;

        public OverrideExposureOrderOld OverrideExposureOrderOld {
            get => overrideExposureOrderOld;
            set {
                overrideExposureOrderOld = value;
                DisplayOverrideExposureOrder = overrideExposureOrderOld.GetDisplayList();
                RaisePropertyChanged(nameof(OverrideExposureOrderOld));
            }
        }

        private ObservableCollection<OverrideItemOld> displayOverrideExposureOrder;

        public ObservableCollection<OverrideItemOld> DisplayOverrideExposureOrder {
            get => displayOverrideExposureOrder;
            set {
                displayOverrideExposureOrder = value;
                RaisePropertyChanged(nameof(DisplayOverrideExposureOrder));
            }
        }

        private void MoveItemUp(object obj) {
            OverrideItemOld item = obj as OverrideItemOld;
            lock (lockObj) {
                int idx = GetItemIndex(item);
                OverrideExposureOrderOld.OverrideItems.RemoveAt(idx);

                if (idx == 0) {
                    OverrideExposureOrderOld.OverrideItems.Add(item);
                } else {
                    OverrideExposureOrderOld.OverrideItems.Insert(idx - 1, item);
                }

                DisplayOverrideExposureOrder = OverrideExposureOrderOld.GetDisplayList();
            }
        }

        private void MoveItemDown(object obj) {
            OverrideItemOld item = obj as OverrideItemOld;
            lock (lockObj) {
                int idx = GetItemIndex(item);

                if (idx == OverrideExposureOrderOld.OverrideItems.Count - 1) {
                    OverrideExposureOrderOld.OverrideItems.RemoveAt(idx);
                    OverrideExposureOrderOld.OverrideItems.Insert(0, item);
                } else {
                    OverrideExposureOrderOld.OverrideItems.RemoveAt(idx);
                    OverrideExposureOrderOld.OverrideItems.Insert(idx + 1, item);
                }

                DisplayOverrideExposureOrder = OverrideExposureOrderOld.GetDisplayList();
            }
        }

        private void CopyItem(object obj) {
            OverrideItemOld item = obj as OverrideItemOld;
            lock (lockObj) {
                OverrideExposureOrderOld.OverrideItems.Insert(GetItemIndex(item) + 1, item.Clone());
                DisplayOverrideExposureOrder = OverrideExposureOrderOld.GetDisplayList();
            }
        }

        private void DeleteItem(object obj) {
            OverrideItemOld item = obj as OverrideItemOld;
            lock (lockObj) {
                OverrideExposureOrderOld.OverrideItems.RemoveAt(GetItemIndex(item));
                DisplayOverrideExposureOrder = OverrideExposureOrderOld.GetDisplayList();
            }
        }

        private void InsertDither(object obj) {
            if (obj != null) {
                OverrideItemOld current = obj as OverrideItemOld;
                lock (lockObj) {
                    OverrideExposureOrderOld.OverrideItems.Insert(GetItemIndex(current) + 1, new OverrideItemOld());
                    DisplayOverrideExposureOrder = OverrideExposureOrderOld.GetDisplayList();
                }
            } else {
                lock (lockObj) {
                    OverrideExposureOrderOld.OverrideItems.Add(new OverrideItemOld());
                    DisplayOverrideExposureOrder = OverrideExposureOrderOld.GetDisplayList();
                }
            }
        }

        private int GetItemIndex(OverrideItemOld find) {
            return OverrideExposureOrderOld.OverrideItems.IndexOf(find);
        }

        private void Save() {
            targetViewVM.SaveOverrideExposureOrder(OverrideExposureOrderOld);
            targetViewVM.ShowOverrideExposureOrderPopup = false;
        }

        private void Cancel() {
            targetViewVM.ShowOverrideExposureOrderPopup = false;
        }
    }
}