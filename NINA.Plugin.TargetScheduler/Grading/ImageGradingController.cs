﻿using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.WPF.Base.Interfaces.Mediator;
using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Plugin.TargetScheduler.Grading {

    /// <summary>
    /// Manage image grading work - and the associated database updates - asynchronously.
    ///
    /// This is patterned after NINA ImageSaveController.
    /// </summary>
    public class ImageGradingController {
        private static readonly Lazy<ImageGradingController> lazy = new Lazy<ImageGradingController>(() => new ImageGradingController());
        public static ImageGradingController Instance { get => lazy.Value; }

        private Task worker;
        private CancellationTokenSource workerCTS;
        private AsyncProducerConsumerQueue<GradingWorkData> queue;
        private IImageGrader imageGrader;

        public ImageGradingController() : this(new ImageGrader()) {
        }

        public ImageGradingController(IImageGrader imageGrader) {
            this.imageGrader = imageGrader;
            queue = new AsyncProducerConsumerQueue<GradingWorkData>(5);
            workerCTS = new CancellationTokenSource();
            worker = Task.Run(DoWork);
        }

        public Task Enqueue(GradingWorkData workPackage, CancellationToken token) {
            var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(token, workerCTS.Token);
            TSLogger.Debug($"queuing image grading task: targetId={workPackage.TargetId}, exposureId={workPackage.ExposurePlanId}, imageId={workPackage.ImageSavedEventArgs.MetaData.Image.Id}");
            return queue.EnqueueAsync(workPackage, mergedCts.Token);
        }

        private async Task DoWork() {
            while (!workerCTS.IsCancellationRequested) {
                try {
                    GradingWorkData workPackage = await queue.DequeueAsync(workerCTS.Token);
                    imageGrader.Grade(workPackage);
                } catch (OperationCanceledException) {
                } catch (Exception ex) {
                    TSLogger.Error(ex);
                } finally {
                }
            }
        }

        public void Shutdown() {
            try { workerCTS?.Cancel(); } catch { }
            if (!worker.Wait(TimeSpan.FromSeconds(30))) {
                TSLogger.Error("image grading worker failed to cleanly shutdown after 30s");
            }
        }
    }

    public class GradingWorkData {
        public bool IsEmulated { get; private set; }
        public int TargetId { get; private set; }
        public int ExposurePlanId { get; private set; }
        public int AcquiredImageId { get; private set; }
        public ImageSavedEventArgs ImageSavedEventArgs { get; private set; }
        public IImageGraderPreferences GraderPreferences { get; private set; }

        public GradingWorkData(bool isEmulated, int targetId, int exposurePlanId, int acquiredImageId,
            ImageSavedEventArgs imageSavedEventArgs, IImageGraderPreferences graderPreferences) {
            IsEmulated = isEmulated;
            TargetId = targetId;
            ExposurePlanId = exposurePlanId;
            AcquiredImageId = acquiredImageId;
            ImageSavedEventArgs = imageSavedEventArgs;
            GraderPreferences = graderPreferences;
        }

        public GradingWorkData() {
        }
    }
}