using PixiEditor.Models.Controllers;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class MouseMovementControllerTests
    {
        /*[Fact]
        public void TestThatStartRecordingMouseMovChangesStartsRecordingAndInvokesEvent()
        {
            bool eventInvoked = false;
            ToolSessionController controller = new ToolSessionController();
            controller.StartedRecordingChanges += (sender, e) => eventInvoked = true;

            controller.StartRecordingMouseMovementChanges(false);

            Assert.True(controller.IsRecordingChanges);
            Assert.True(eventInvoked);
            Assert.False(controller.ClickedOnCanvas);
        }

        [Fact]
        public void TestThatRecordMouseMovementChangeRecordsMouseMovementChange()
        {
            ToolSessionController controller = new ToolSessionController();
            controller.StartRecordingMouseMovementChanges(false);
            controller.RecordMouseMovementChange(new Coordinates(5, 5));

            Assert.NotEmpty(controller.LastMouseMoveCoordinates);
            Assert.Equal(new Coordinates(5, 5), controller.LastMouseMoveCoordinates[0]);
            Assert.True(controller.IsRecordingChanges);
        }

        [Fact]
        public void TestThatMouseMovedRaisesEvent()
        {
            bool eventRaised = false;
            Coordinates position = new Coordinates(5, 5);
            MouseMovementEventArgs args = new MouseMovementEventArgs(default(Coordinates));

            ToolSessionController controller = new ToolSessionController();
            controller.PreciseMousePositionChanged += (s, e) =>
            {
                eventRaised = true;
                args = e;
            };

            controller.OnMouseMove(position);

            Assert.True(eventRaised);
            Assert.Equal(position, args.NewPosition);
        }

        [Fact]
        public void TestStopRecordingChangesStopsRecording()
        {
            ToolSessionController controller = new ToolSessionController();

            controller.StartRecordingMouseMovementChanges(true);
            controller.StopRecordingMouseMovementChanges();

            Assert.False(controller.IsRecordingChanges);
            Assert.False(controller.ClickedOnCanvas);
        }

        [Fact]
        public void TestThatRecordChangesNotRecords()
        {
            ToolSessionController controller = new ToolSessionController();
            controller.RecordMouseMovementChange(new Coordinates(5, 10));

            Assert.False(controller.IsRecordingChanges);
            Assert.Empty(controller.LastMouseMoveCoordinates);
        }*/
    }
}