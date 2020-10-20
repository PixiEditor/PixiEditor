using PixiEditor.Models.Controllers;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class MouseMovementControllerTests
    {
        [Fact]
        public void TestThatStartRecordingMouseMovChangesStartsRecordingAndInvokesEvent()
        {
            var eventInvoked = false;
            var controller = new MouseMovementController();
            controller.StartedRecordingChanges += (sender, e) => eventInvoked = true;

            controller.StartRecordingMouseMovementChanges(false);

            Assert.True(controller.IsRecordingChanges);
            Assert.True(eventInvoked);
            Assert.False(controller.ClickedOnCanvas);
        }

        [Fact]
        public void TestThatRecordMouseMovementChangeRecordsMouseMovementChange()
        {
            var controller = new MouseMovementController();
            controller.StartRecordingMouseMovementChanges(false);
            controller.RecordMouseMovementChange(new Coordinates(5, 5));

            Assert.NotEmpty(controller.LastMouseMoveCoordinates);
            Assert.Equal(new Coordinates(5, 5), controller.LastMouseMoveCoordinates[0]);
            Assert.True(controller.IsRecordingChanges);
        }

        [Fact]
        public void TestThatMouseMovedRaisesEvent()
        {
            var eventRaised = false;
            var position = new Coordinates(5, 5);
            var args = new MouseMovementEventArgs(new Coordinates());

            var controller = new MouseMovementController();
            controller.MousePositionChanged += (s, e) =>
            {
                eventRaised = true;
                args = e;
            };

            controller.MouseMoved(position);

            Assert.True(eventRaised);
            Assert.Equal(position, args.NewPosition);
        }

        [Fact]
        public void TestStopRecordingChangesStopsRecording()
        {
            var controller = new MouseMovementController();

            controller.StartRecordingMouseMovementChanges(true);
            controller.StopRecordingMouseMovementChanges();

            Assert.False(controller.IsRecordingChanges);
            Assert.False(controller.ClickedOnCanvas);
        }

        [Fact]
        public void TestThatRecordChangesNotRecords()
        {
            var controller = new MouseMovementController();
            controller.RecordMouseMovementChange(new Coordinates(5, 10));

            Assert.False(controller.IsRecordingChanges);
            Assert.Empty(controller.LastMouseMoveCoordinates);
        }
    }
}