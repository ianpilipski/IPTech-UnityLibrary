using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IPTech.EditorTools.Tests {
    public class ProjectCleanerSessionTests {
        [Test]
        public void SelectVisibleSelectsOnlyEligibleRowsAndScriptsRequireAcknowledgement() {
            var candidate = Item("Assets/candidate.asset", CleanerStatus.Candidate);
            var script = Item("Assets/review.cs", CleanerStatus.ReviewRequired);
            script.IsScript = true;
            var session = Session(candidate, script);

            session.RefreshVisible();
            session.SelectVisible();
            Assert.That(session.Selected, Is.EquivalentTo(new[] { candidate.Guid }));
            Assert.That(session.BlockingReason(session.SelectedItems()), Is.Null);

            session.ClearSelection();
            session.Status = "All";
            session.RefreshVisible();
            session.ReviewedScripts = true;
            session.SelectVisible();
            Assert.That(session.Selected, Is.EquivalentTo(new[] { candidate.Guid, script.Guid }));
            Assert.That(session.ReviewedScripts, Is.False, "Selecting a script must reset the acknowledgement.");
            Assert.That(session.CanConfirm(session.SelectedItems()), Is.False);
            Assert.That(session.BlockingReason(session.SelectedItems()), Does.Contain("acknowledge"));

            session.ReviewedScripts = true;
            Assert.That(session.CanConfirm(session.SelectedItems()), Is.True);

            session.MarkStale();
            Assert.That(session.CanConfirm(session.SelectedItems()), Is.False);
            Assert.That(session.BlockingReason(session.SelectedItems()), Does.Contain("stale"));
        }

        [TestCase("selected", false)]
        [TestCase("selected", true)]
        [TestCase("path", false)]
        [TestCase("path", true)]
        [TestCase("type", false)]
        [TestCase("type", true)]
        [TestCase("status", false)]
        [TestCase("status", true)]
        [TestCase("bytes", false)]
        [TestCase("bytes", true)]
        [TestCase("refs", false)]
        [TestCase("refs", true)]
        public void SortsEveryColumnInBothDirections(string column, bool descending) {
            var (low, high) = SortPair();
            var session = Session(high, low);
            session.Status = "All";
            session.Selected.Add(high.Guid);

            session.SetSort(column, descending);
            session.RefreshVisible();

            Assert.That(session.SortColumn, Is.EqualTo(column));
            Assert.That(session.SortDescending, Is.EqualTo(descending));
            Assert.That(session.Visible, Is.EqualTo(descending ? new[] { high, low } : new[] { low, high }));
        }

        [Test]
        public void CompleteDeletionRemovesOnlySuccessfulDeletionsAndTheirChildren() {
            var deleted = Item("Assets/T/DeadBeef.mat", CleanerStatus.Candidate);
            var failed = Item("Assets/T/failed.mat", CleanerStatus.Candidate);
            var folder = Item("Assets/T/empty", CleanerStatus.Candidate);
            folder.IsDirectory = true;
            var child = Item("Assets/T/empty/child", CleanerStatus.Candidate);
            child.IsDirectory = true;
            var sibling = Item("Assets/T/empty-other", CleanerStatus.Candidate);
            var session = Session(deleted, failed, folder, child, sibling);
            session.Status = "All";
            session.Selected.Add(deleted.Guid);
            session.ReviewedScripts = true;

            var message = session.CompleteDeletion(new[] {
                new CleanerOutcome { AssetPath = deleted.AssetPath, Success = true, Message = "Deleted." },
                new CleanerOutcome { AssetPath = failed.AssetPath, Success = false, Message = "Unity refused deletion." },
                new CleanerOutcome { AssetPath = folder.AssetPath, Success = true, Message = "Deleted." }
            });

            Assert.That(session.Scan.Items, Is.EqualTo(new[] { failed, sibling }));
            Assert.That(session.Visible, Is.EquivalentTo(new[] { failed, sibling }));
            Assert.That(session.Selected, Is.Empty);
            Assert.That(session.ReviewedScripts, Is.False);
            Assert.That(session.Stale, Is.True);
            Assert.That(message, Does.Contain("Unity refused deletion."));
            Assert.That(session.Summary(), Does.Contain("Visible: 2").And.Contain("STALE"));
        }

        internal static ProjectCleanerSession Session(params ProjectCleanerItem[] items) {
            var session = new ProjectCleanerSession { BackupDirectory = "Temp/unused" };
            session.Normalize();
            var scan = new ProjectCleanerScan { Complete = true };
            scan.Items.AddRange(items);
            session.Scan = scan;
            return session;
        }

        internal static ProjectCleanerItem Item(string path, CleanerStatus status) {
            return new ProjectCleanerItem {
                AssetPath = path, Guid = Guid.NewGuid().ToString("N"), Category = "Test", Status = status, CanDelete = true
            };
        }

        internal static (ProjectCleanerItem low, ProjectCleanerItem high) SortPair() {
            var low = Item("Assets/T/A.asset", CleanerStatus.Candidate);
            low.Category = "Material";
            low.Bytes = 9;
            low.ReferencedBy.Add(new CleanerReference { Source = "A" });
            low.ReferencedBy.Add(new CleanerReference { Source = "A", Evidence = CleanerEvidence.AssetReference });
            var high = Item("Assets/T/Z.asset", CleanerStatus.Protected);
            high.Category = "Texture2D";
            high.Bytes = 100;
            high.ReferencedBy.Add(new CleanerReference { Source = "A" });
            high.ReferencedBy.Add(new CleanerReference { Source = "B" });
            return (low, high);
        }
    }
}
